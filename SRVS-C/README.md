# Local Server Replication Guide

## Prerequisites
- Install NodeJS v22.16.0 ([Download Node.js](https://nodejs.org/en))
- Visual Studio Code (recommended) or any code editor

## NodeJS Setup
1. Open Terminal, verify installation by running:
```bash
node --version
npm --version
```
### Install required modules:
```bash
npm install
```

## Server Execution
1. Start the server:
```bash
npm start
```
2. For client connections:
- Ensure devices are on the same local network
- Provide server IP and port to clients

## Configuration
1. Modify src/Server.ts for:
- Minimum client requirements
- Connection port
- Database properties