@echo off
title Unity MCP Server
echo Starting Unity MCP Server on port 8090...
C:\Users\1\.local\bin\uvx.exe --from mcpforunityserver==10.1.2 mcp-for-unity --transport http --http-port 8090
pause
