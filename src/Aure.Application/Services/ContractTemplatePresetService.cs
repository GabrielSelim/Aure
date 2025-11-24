using Aure.Application.DTOs.Contract;

namespace Aure.Application.Services
{
    public class ContractTemplatePresetService
    {
        public List<ContractTemplatePresetResponse> GetAllPresets()
        {
            return new List<ContractTemplatePresetResponse>
            {
                GetSoftwarePreset(),
                GetVendasPreset(),
                GetConsultoriaPreset(),
                GetMarketingPreset(),
                GetLogisticaPreset()
            };
        }

        public ContractTemplatePresetResponse? GetPresetByTipo(string tipo)
        {
            return tipo.ToLower() switch
            {
                "software" => GetSoftwarePreset(),
                "vendas" => GetVendasPreset(),
                "consultoria" => GetConsultoriaPreset(),
                "marketing" => GetMarketingPreset(),
                "logistica" => GetLogisticaPreset(),
                _ => null
            };
        }

        private ContractTemplatePresetResponse GetSoftwarePreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "software",
                Nome = "Serviços de Gestão e Análise de Negócios (Software)",
                Descricao = "Modelo para empresas de tecnologia e desenvolvimento de software",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços de Gestão e Análise de Negócios",
                    DescricaoServico = "a prestação, a serem executados por especialista qualificado denominado CONTRATADO, de Serviços de Gestão e Análise de Negócios",
                    LocalPrestacaoServico = "na sede da CONTRATANTE ou em Sede de Cliente da CONTRATANTE, conforme demanda e necessidade da CONTRATANTE",
                    DetalhamentoServicos = new List<string>
                    {
                        "Análise detalhada e refinamento dos requisitos técnicos e funcionais dos softwares da CONTRATANTE, garantindo alinhamento com os objetivos estratégicos do negócio",
                        "Realização de auditorias de qualidade de software, testes automatizados e manuais, contribuindo para a excelência técnica dos produtos desenvolvidos pela CONTRATANTE",
                        "Resolução de dúvidas e o acompanhamento de feedbacks de clientes, assegurando a satisfação e o sucesso contínuo das entregas realizadas",
                        "Elaborar relatórios detalhados de produtividade das UST's da CONTRATANTE, auxiliando na identificação de oportunidades de melhoria",
                        "Supervisão periódica dos serviços prestados aos clientes da CONTRATANTE, assegurando conformidade com os padrões estabelecidos",
                        "Atendimento ágil e eficaz às solicitações de analistas de negócios e gestores da CONTRATANTE, contribuindo para o fluxo de trabalho eficiente",
                        "Orientação sobre as melhores práticas e ferramentas de desenvolvimento de sistemas e softwares, mantendo a CONTRATANTE atualizada com as últimas inovações do mercado",
                        "Fornecimento de informações claras e acessíveis sobre as necessidades específicas dos clientes da CONTRATANTE, auxiliando na tomada de decisões estratégicas",
                        "Disponibilização para viagens técnicas quando necessário, para atender demandas específicas da CONTRATANTE ou de seus clientes",
                        "Disposição para viagens voltadas à apresentação de Softwares, Certificações e demais inovações desenvolvidas pela CONTRATANTE"
                    },
                    ClausulaAjudaCusto = null,
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Observar e cumprir todas as orientações e normas estabelecidas pela CONTRATANTE, inclusive regras de confidencialidade e uso de dados",
                        "Prestar os serviços com zelo, qualidade, eficiência e excelência profissional",
                        "Manter a CONTRATANTE informada sobre o andamento dos serviços, eventuais dificuldades ou necessidades de ajustes",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários decorrentes da execução dos serviços",
                        "Não divulgar informações confidenciais obtidas durante a prestação dos serviços",
                        "Comunicar imediatamente à CONTRATANTE qualquer situação que possa comprometer a prestação dos serviços"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer as informações, documentos e recursos necessários para a adequada prestação dos serviços",
                        "Orientar e supervisionar os serviços prestados pelo CONTRATADO",
                        "Notificar o CONTRATADO sobre qualquer não conformidade ou necessidade de ajuste nos serviços prestados",
                        "Manter canal de comunicação aberto e eficiente com o CONTRATADO"
                    }
                }
            };
        }

        private ContractTemplatePresetResponse GetVendasPreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "vendas",
                Nome = "Serviços de Vendas e Representação Comercial",
                Descricao = "Modelo para empresas de vendas, comércio e representação",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços de Vendas e Representação Comercial",
                    DescricaoServico = "a prestação, a serem executados por profissional qualificado denominado CONTRATADO, de Serviços de Vendas e Representação Comercial",
                    LocalPrestacaoServico = "na sede da CONTRATANTE, em estabelecimentos de clientes ou em visitas comerciais externas, conforme demanda e necessidade da CONTRATANTE",
                    DetalhamentoServicos = new List<string>
                    {
                        "Prospecção ativa de novos clientes, identificando oportunidades de negócio e expandindo a carteira de clientes da CONTRATANTE",
                        "Apresentação detalhada de produtos e serviços aos clientes, destacando diferenciais competitivos e benefícios",
                        "Negociação de propostas comerciais, buscando alcançar os melhores resultados para a CONTRATANTE e satisfação dos clientes",
                        "Elaboração de relatórios de vendas e acompanhamento de metas estabelecidas pela CONTRATANTE",
                        "Atendimento pós-venda, garantindo a satisfação do cliente e fidelização da base de clientes",
                        "Participação em feiras, eventos e ações promocionais organizadas ou indicadas pela CONTRATANTE",
                        "Manutenção de relacionamento constante com clientes ativos, identificando novas necessidades e oportunidades",
                        "Coleta de feedback do mercado sobre produtos, preços e concorrência, auxiliando na estratégia comercial",
                        "Utilização adequada de sistemas de CRM e ferramentas de gestão comercial indicadas pela CONTRATANTE",
                        "Representação da marca da CONTRATANTE com profissionalismo e ética comercial"
                    },
                    ClausulaAjudaCusto = "<p class=\"clausula\">Cláusula Sétima A</p><p>Poderá ser concedida ajuda de custo para despesas com transporte, alimentação e hospedagem em viagens comerciais, mediante prévia aprovação da CONTRATANTE e apresentação de comprovantes, no valor máximo de <strong>R$ 500,00</strong> (quinhentos reais) por viagem.</p>",
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Observar e cumprir todas as políticas comerciais, orientações e normas estabelecidas pela CONTRATANTE",
                        "Prestar os serviços com profissionalismo, ética e comprometimento com as metas estabelecidas",
                        "Manter a CONTRATANTE informada sobre o andamento das atividades comerciais e resultados obtidos",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários",
                        "Não representar concorrentes diretos da CONTRATANTE durante a vigência deste contrato",
                        "Zelar pela imagem e reputação da CONTRATANTE perante o mercado"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer materiais promocionais, catálogos e amostras necessários para a atividade comercial",
                        "Orientar e supervisionar os serviços prestados pelo CONTRATADO",
                        "Definir metas e políticas comerciais de forma clara",
                        "Avaliar e aprovar despesas de viagens comerciais conforme previsto"
                    }
                }
            };
        }

        private ContractTemplatePresetResponse GetConsultoriaPreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "consultoria",
                Nome = "Serviços de Consultoria Empresarial",
                Descricao = "Modelo para consultores e prestadores de serviços especializados",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços de Consultoria Empresarial",
                    DescricaoServico = "a prestação, a serem executados por consultor especializado denominado CONTRATADO, de Serviços de Consultoria Empresarial",
                    LocalPrestacaoServico = "na sede da CONTRATANTE, remotamente ou em localidades indicadas pela CONTRATANTE, conforme demanda e necessidade",
                    DetalhamentoServicos = new List<string>
                    {
                        "Análise diagnóstica da situação atual da CONTRATANTE, identificando pontos fortes, fracos e oportunidades de melhoria",
                        "Elaboração de planos estratégicos e operacionais alinhados aos objetivos da CONTRATANTE",
                        "Assessoria na implementação de processos, metodologias e boas práticas de gestão",
                        "Realização de treinamentos e capacitações para equipes da CONTRATANTE",
                        "Elaboração de relatórios técnicos, pareceres e documentos especializados",
                        "Acompanhamento da execução de projetos, monitorando indicadores e propondo ajustes quando necessário",
                        "Benchmarking e estudos de mercado para subsidiar decisões estratégicas",
                        "Orientação sobre conformidade regulatória e legislação aplicável ao negócio da CONTRATANTE",
                        "Suporte na elaboração de propostas, projetos e apresentações técnicas",
                        "Disponibilidade para reuniões periódicas e atendimento a demandas emergenciais"
                    },
                    ClausulaAjudaCusto = null,
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Prestar os serviços com alto padrão de qualidade técnica e ética profissional",
                        "Manter sigilo absoluto sobre informações confidenciais e estratégicas da CONTRATANTE",
                        "Fornecer relatórios periódicos sobre o andamento dos trabalhos",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários",
                        "Não prestar serviços similares a concorrentes diretos da CONTRATANTE durante a vigência do contrato",
                        "Respeitar prazos acordados e comunicar antecipadamente eventuais impedimentos"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer acesso a informações, documentos e sistemas necessários para a consultoria",
                        "Disponibilizar pessoal e recursos para apoio às atividades do CONTRATADO",
                        "Avaliar e fornecer feedback sobre os trabalhos realizados",
                        "Manter canal de comunicação eficiente com o CONTRATADO"
                    }
                }
            };
        }

        private ContractTemplatePresetResponse GetMarketingPreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "marketing",
                Nome = "Serviços de Marketing e Comunicação",
                Descricao = "Modelo para profissionais de marketing, publicidade e comunicação",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços de Marketing e Comunicação",
                    DescricaoServico = "a prestação, a serem executados por profissional especializado denominado CONTRATADO, de Serviços de Marketing e Comunicação",
                    LocalPrestacaoServico = "na sede da CONTRATANTE, remotamente ou em eventos e ações externas, conforme demanda e necessidade da CONTRATANTE",
                    DetalhamentoServicos = new List<string>
                    {
                        "Desenvolvimento de estratégias de marketing digital e tradicional alinhadas aos objetivos da CONTRATANTE",
                        "Criação de conteúdo para redes sociais, blogs, sites e materiais de comunicação institucional",
                        "Planejamento e execução de campanhas publicitárias e promocionais",
                        "Gestão de redes sociais, incluindo programação de posts, engajamento e análise de métricas",
                        "Elaboração de materiais gráficos, peças publicitárias e apresentações institucionais",
                        "Análise de métricas e resultados de campanhas, propondo otimizações e melhorias",
                        "Pesquisa de mercado e análise de concorrência para posicionamento estratégico",
                        "Assessoria de imprensa e relacionamento com veículos de comunicação",
                        "Apoio na organização de eventos, lançamentos e ações promocionais",
                        "Desenvolvimento de identidade visual e branding conforme necessidades da CONTRATANTE"
                    },
                    ClausulaAjudaCusto = null,
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Prestar os serviços com criatividade, originalidade e alinhamento à identidade da CONTRATANTE",
                        "Respeitar prazos de entrega de materiais e campanhas acordados",
                        "Fornecer relatórios periódicos de desempenho das ações de marketing",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários",
                        "Não utilizar conteúdos criados para a CONTRATANTE em outros projetos sem autorização prévia",
                        "Zelar pela reputação e imagem da CONTRATANTE em todas as comunicações"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer briefings claros e materiais necessários para criação de conteúdo",
                        "Aprovar ou solicitar ajustes em materiais e campanhas em tempo hábil",
                        "Fornecer acesso a plataformas e ferramentas necessárias para execução dos serviços",
                        "Manter comunicação eficiente e feedback construtivo sobre os trabalhos"
                    }
                }
            };
        }

        private ContractTemplatePresetResponse GetLogisticaPreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "logistica",
                Nome = "Serviços de Logística e Transporte",
                Descricao = "Modelo para prestadores de serviços de logística, transporte e entrega",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços de Logística e Transporte",
                    DescricaoServico = "a prestação, a serem executados por profissional qualificado denominado CONTRATADO, de Serviços de Logística e Transporte",
                    LocalPrestacaoServico = "em rotas e localidades definidas pela CONTRATANTE, incluindo armazéns, centros de distribuição e endereços de entrega",
                    DetalhamentoServicos = new List<string>
                    {
                        "Coleta, transporte e entrega de mercadorias conforme cronograma estabelecido pela CONTRATANTE",
                        "Manuseio adequado de produtos, garantindo integridade e segurança durante todo o processo",
                        "Conferência de documentos fiscais, notas e comprovantes de entrega",
                        "Utilização de sistemas de rastreamento e comunicação em tempo real sobre status das entregas",
                        "Cumprimento rigoroso de prazos e rotas pré-estabelecidas",
                        "Atendimento a clientes finais com profissionalismo e cordialidade",
                        "Reporte imediato de ocorrências, avarias ou impossibilidade de entrega",
                        "Manutenção preventiva e conservação de veículos e equipamentos utilizados",
                        "Cumprimento de normas de trânsito, segurança e legislação vigente",
                        "Disponibilidade para rotas emergenciais ou alterações de última hora conforme necessidade"
                    },
                    ClausulaAjudaCusto = "<p class=\"clausula\">Cláusula Sétima A</p><p>Será concedida ajuda de custo para despesas com combustível, pedágios e manutenção de veículo, no valor de <strong>R$ 800,00</strong> (oitocentos reais) mensais, desde que apresentados os comprovantes e relatórios de quilometragem rodada.</p>",
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Utilizar veículo próprio em boas condições de uso e com documentação regularizada",
                        "Manter apólice de seguro vigente para o veículo e carga transportada",
                        "Prestar os serviços com pontualidade, eficiência e segurança",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários",
                        "Comunicar imediatamente a CONTRATANTE sobre acidentes, avarias ou impossibilidade de cumprir rota",
                        "Zelar pela imagem da CONTRATANTE perante clientes e fornecedores"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer roteiros, documentos fiscais e instruções necessárias para entregas",
                        "Aprovar e reembolsar despesas previstas em ajuda de custo",
                        "Notificar com antecedência sobre alterações em rotas ou cronogramas",
                        "Fornecer suporte em casos de emergência ou dificuldades operacionais"
                    }
                }
            };
        }
    }
}
