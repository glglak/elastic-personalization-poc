// Simple Elasticsearch initialization script to be run with Node.js
// Usage: node InitializeElasticsearch.js [elasticsearch_url] [index_name]

const http = require('http');
const https = require('https');

const elasticsearchUrl = process.argv[2] || 'http://localhost:9200';
const indexName = process.argv[3] || 'content';

const url = new URL(elasticsearchUrl);
const client = url.protocol === 'https:' ? https : http;

console.log(`Initializing Elasticsearch index '${indexName}' at ${elasticsearchUrl}`);

// Delete index if exists
const deleteRequest = {
  hostname: url.hostname,
  port: url.port,
  path: `/${indexName}`,
  method: 'DELETE'
};

client.request(deleteRequest, (res) => {
  console.log(`DELETE ${indexName} status: ${res.statusCode}`);
  
  // Create index with mappings
  const createRequest = {
    hostname: url.hostname,
    port: url.port,
    path: `/${indexName}`,
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json'
    }
  };

  const createBody = JSON.stringify({
    settings: {
      number_of_shards: 1,
      number_of_replicas: 0,
      analysis: {
        analyzer: {
          content_analyzer: {
            type: 'custom',
            tokenizer: 'standard',
            filter: ['lowercase', 'asciifolding', 'stop', 'snowball']
          }
        }
      }
    },
    mappings: {
      properties: {
        id: { type: 'keyword' },
        title: { 
          type: 'text',
          analyzer: 'content_analyzer',
          boost: 2.0,
          fields: {
            keyword: { type: 'keyword' }
          }
        },
        description: { 
          type: 'text',
          analyzer: 'content_analyzer',
          boost: 1.5
        },
        body: { 
          type: 'text',
          analyzer: 'content_analyzer'
        },
        createdAt: { type: 'date' },
        tags: { 
          type: 'keyword',
          boost: 1.5
        },
        categories: { 
          type: 'keyword',
          boost: 1.5
        },
        creatorId: { type: 'integer' }
      }
    }
  });

  const createReq = client.request(createRequest, (res) => {
    console.log(`CREATE ${indexName} status: ${res.statusCode}`);
    
    let data = '';
    res.on('data', (chunk) => {
      data += chunk;
    });
    
    res.on('end', () => {
      console.log('Create index response:', data);
      
      // Index sample documents
      indexSampleDocuments();
    });
  });

  createReq.on('error', (e) => {
    console.error(`Error creating index: ${e.message}`);
  });

  createReq.write(createBody);
  createReq.end();
}).on('error', (e) => {
  console.error(`Error deleting index: ${e.message}`);
});

// Function to index sample documents
function indexSampleDocuments() {
  const sampleDocs = [
    {
      id: '1',
      title: 'Introduction to Elasticsearch',
      description: 'Learn the basics of Elasticsearch',
      body: 'Elasticsearch is a distributed, RESTful search and analytics engine capable of addressing a growing number of use cases. As the heart of the Elastic Stack, it centrally stores your data for lightning-fast search, relevance, and powerful analytics.',
      createdAt: new Date().toISOString(),
      tags: ['elasticsearch', 'search', 'database'],
      categories: ['Database', 'Search'],
      creatorId: 1
    },
    {
      id: '2',
      title: 'Advanced .NET Core Development',
      description: 'Take your .NET skills to the next level',
      body: 'In this article, we explore advanced concepts in .NET Core development including dependency injection, middleware, and microservices architecture. We\'ll also cover best practices for performance optimization and application security.',
      createdAt: new Date().toISOString(),
      tags: ['dotnet', 'csharp', 'programming'],
      categories: ['Programming', 'Web Development'],
      creatorId: 2
    },
    {
      id: '3',
      title: 'Building Personalized Content Feeds',
      description: 'Learn how to create personalized experiences',
      body: 'Personalization is key to user engagement. This article explains how to build personalized content feeds using a combination of user preferences, interests, and interaction history. We\'ll demonstrate practical examples using Elasticsearch and SQL Server.',
      createdAt: new Date().toISOString(),
      tags: ['personalization', 'recommendation', 'user-experience'],
      categories: ['User Experience', 'Personalization'],
      creatorId: 1
    },
    {
      id: '4',
      title: 'SQL Server Performance Tuning',
      description: 'Optimize your database queries',
      body: 'This guide covers essential techniques for SQL Server optimization including index management, query analysis, and execution plans. Learn how to improve database performance and reduce resource usage through practical examples and case studies.',
      createdAt: new Date().toISOString(),
      tags: ['sql', 'database', 'performance'],
      categories: ['Database', 'Performance'],
      creatorId: 3
    },
    {
      id: '5',
      title: 'Getting Started with Docker',
      description: 'Containerize your applications',
      body: 'Docker is a platform for developing, shipping, and running applications in containers. This beginner-friendly guide will take you through the basics of Docker, including containers, images, and Docker Compose for multi-container applications.',
      createdAt: new Date().toISOString(),
      tags: ['docker', 'containerization', 'devops'],
      categories: ['DevOps', 'Containers'],
      creatorId: 2
    }
  ];

  console.log(`Indexing ${sampleDocs.length} sample documents...`);

  // Build bulk request
  let bulkBody = '';
  sampleDocs.forEach(doc => {
    bulkBody += JSON.stringify({ index: { _index: indexName, _id: doc.id } }) + '\n';
    bulkBody += JSON.stringify(doc) + '\n';
  });

  const bulkRequest = {
    hostname: url.hostname,
    port: url.port,
    path: '/_bulk',
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-ndjson'
    }
  };

  const bulkReq = client.request(bulkRequest, (res) => {
    console.log(`BULK INDEX status: ${res.statusCode}`);
    
    let data = '';
    res.on('data', (chunk) => {
      data += chunk;
    });
    
    res.on('end', () => {
      console.log('Bulk index response:', data);
      console.log('Elasticsearch initialization completed successfully!');
    });
  });

  bulkReq.on('error', (e) => {
    console.error(`Error indexing documents: ${e.message}`);
  });

  bulkReq.write(bulkBody);
  bulkReq.end();
}
