## 📘 Function Documentation: `SendQueueMessage`

### 🧠 Overview
Azure Function endpoint for SendQueueMessage operations

---

### 🔗 Endpoint
```http
POST /sendqueuemessage
```

### 🔐 Authentication
| Header | Value |
| -- | -- |
| x-api-key | your-api-key |

### 📦 Request Body
```json
{
  "title": "Sample Title",
  "slug": "sample-slug",
  "description": "Sample description",
  "content": "Sample content",
  "status": "Published"
}
```

### 📤 Response (200 OK)
```json
{
  "id": "sample-id",
  "slug": "sample-slug",
  "title": "Sample Title",
  "status": "Published",
  "lastModified": "2025-08-07T13:48:40.4333258Z"
}
```

### ❌ Error Responses
| Status Code | Message Example |
| -- | -- |
| 400 Bad Request | ["Title is required", "Author slug is required"] |
| 401 Unauthorized | "Invalid API key" |
| 500 Internal Server Error | "An unexpected error occurred" |

### 🧪 Testing Example (curl)
```bash
curl -X POST \
  http://localhost:7071/sendqueuemessage \
  -H "x-api-key: your-api-key" \
  -H "Content-Type: application/json" \
  -d '{ ... }'

```

### 🧠 Common Issues
- Missing required fields will trigger 400 errors
- Ensure API key is valid and scoped correctly
- Check that referenced entities exist

### 🔗 Related Endpoints
| Method | Endpoint | Description |
| -- | -- | -- |

#Function #Payload #ErrorHandling
