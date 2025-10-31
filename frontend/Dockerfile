# === Build stage ===
FROM node:20-alpine AS builder
WORKDIR /app

# Copy package files
COPY package*.json ./
RUN npm install

# Copy all files
COPY . .

# Build the application
RUN npm run build

# === Runtime stage ===
FROM node:20-alpine
WORKDIR /app

# Copy built files
COPY --from=builder /app ./

# Expose port 3000
EXPOSE 3000

# Start the application
CMD ["npm", "run", "start"]

