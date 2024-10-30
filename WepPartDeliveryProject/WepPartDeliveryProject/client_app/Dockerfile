# Stage 1: Build the application
FROM node:18-alpine AS builder

# Set the working directory in the container
WORKDIR /app

# Copy package.json and package-lock.json to install dependencies
COPY package.json ./

# Install the dependencies
RUN npm install

# Copy the rest of the application files
COPY . .

# Start the Next.js application
CMD ["npm", "run", "dev"]
