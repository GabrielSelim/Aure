#!/bin/bash

# 🔍 Script para Verificar Portas em Uso no Servidor
# Execute este script ANTES do deploy para evitar conflitos

echo "🔍 Verificando portas em uso no servidor..."
echo "=============================================="
echo

# Verificar portas TCP em uso (usar ss se netstat não disponível)
echo "📊 Portas TCP em uso:"
echo "Porta  | Processo"
echo "-------|------------------"

if command -v netstat >/dev/null 2>&1; then
    netstat -tlnp | grep LISTEN | awk '{print $4 " | " $7}' | sed 's/.*://' | sort -n | uniq
elif command -v ss >/dev/null 2>&1; then
    ss -tlnp | grep LISTEN | awk '{print $5 " | " $7}' | sed 's/.*://' | sort -n | uniq
else
    echo "❌ Nem netstat nem ss disponíveis. Use: sudo apt install net-tools"
    echo "Ou verificar manualmente: sudo lsof -i :5000"
fi

echo
echo "🐳 Containers Docker ativos:"
docker ps --format "table {{.Names}}\t{{.Ports}}\t{{.Status}}"

echo
echo "⚠️  Portas que a API Aure precisa:"
echo "   • 5000 - API Principal"
echo "   • 5432 - PostgreSQL (bind localhost)"
echo "   • 6379 - Redis (bind localhost)" 
echo "   • 8080 - Adminer (bind localhost, apenas dev)"

echo
echo "🔧 Se alguma porta estiver ocupada, edite docker-compose.yml:"
echo "   ports:"
echo "     - \"NOVA_PORTA:5000\"  # Ex: 5001:5000"

echo
echo "💡 Comandos úteis:"
echo "   Ver processo na porta: sudo lsof -i :PORTA"
echo "   Verificar com ss: sudo ss -tlnp | grep :PORTA"
echo "   Matar processo: sudo kill -9 PID"
echo "   Ver todos os containers: docker ps -a"
echo "   Instalar ferramentas: sudo apt install net-tools lsof"