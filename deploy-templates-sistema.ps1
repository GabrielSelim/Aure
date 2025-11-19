ssh root@5.189.174.61 <<'ENDSSH'
cd /root/Aure
git pull origin main
docker-compose up -d --build
sleep 30

docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production <<'EOF'
ALTER TABLE contracttemplates ALTER COLUMN company_id DROP NOT NULL;
EOF

echo "Deploy concluido! Aguarde 10 segundos para a API reiniciar..."
sleep 10
curl -s https://aureapi.gabrielsanztech.com.br/health
ENDSSH
