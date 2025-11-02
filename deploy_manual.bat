@echo off
echo Conectando ao servidor...
ssh root@5.189.174.61 "cd /root/Aure && docker-compose down && docker-compose up -d --build && docker ps && docker logs --tail 20 aure-api-aure-gabriel"
pause
