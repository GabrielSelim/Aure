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
                GetLogisticaPreset(),
                GetContabilidadePreset(),
                GetEngenhariaPreset(),
                GetDesignPreset(),
                GetEducacaoPreset(),
                GetSaudePreset(),
                GetJuridicoPreset(),
                GetTIPreset(),
                GetManutencaoPreset(),
                GetLimpezaPreset(),
                GetSegurancaPreset()
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
                "contabilidade" => GetContabilidadePreset(),
                "engenharia" => GetEngenhariaPreset(),
                "design" => GetDesignPreset(),
                "educacao" => GetEducacaoPreset(),
                "saude" => GetSaudePreset(),
                "juridico" => GetJuridicoPreset(),
                "ti" => GetTIPreset(),
                "manutencao" => GetManutencaoPreset(),
                "limpeza" => GetLimpezaPreset(),
                "seguranca" => GetSegurancaPreset(),
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

        private ContractTemplatePresetResponse GetContabilidadePreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "contabilidade",
                Nome = "Serviços de Contabilidade e Consultoria Fiscal",
                Descricao = "Modelo para contadores e consultores fiscais",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços de Contabilidade e Consultoria Fiscal",
                    DescricaoServico = "a prestação de serviços contábeis, fiscais e de assessoria tributária, a serem executados por profissional qualificado denominado CONTRATADO",
                    LocalPrestacaoServico = "nas dependências da CONTRATANTE, remotamente, ou em escritório do CONTRATADO, conforme demanda",
                    DetalhamentoServicos = new List<string>
                    {
                        "Escrituração contábil e fiscal conforme legislação vigente",
                        "Elaboração de balancetes, balanços patrimoniais e demonstrações financeiras",
                        "Apuração e cálculo de impostos federais, estaduais e municipais",
                        "Emissão de guias de pagamento (DARF, GPS, DAS, etc.)",
                        "Entrega de obrigações acessórias (SPED, DCTF, EFD, DIRF, etc.)",
                        "Assessoria em planejamento tributário e redução de carga fiscal",
                        "Consultoria sobre regime tributário mais adequado ao negócio",
                        "Atendimento a fiscalizações e defesas administrativas",
                        "Gestão de folha de pagamento e encargos trabalhistas (quando aplicável)",
                        "Orientação sobre regularização fiscal e abertura/encerramento de empresas"
                    },
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Manter sigilo absoluto sobre informações contábeis e fiscais da CONTRATANTE",
                        "Entregar documentos e declarações dentro dos prazos legais",
                        "Alertar a CONTRATANTE sobre vencimentos, pendências e mudanças na legislação",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários",
                        "Manter registro ativo no CRC (Conselho Regional de Contabilidade)",
                        "Utilizar sistemas certificados e seguros para processamento de dados"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer documentos fiscais, extratos bancários e informações necessárias com antecedência",
                        "Manter organização e guarda de documentos originais",
                        "Comunicar imediatamente sobre mudanças societárias, operacionais ou tributárias"
                    }
                }
            };
        }

        private ContractTemplatePresetResponse GetEngenhariaPreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "engenharia",
                Nome = "Serviços de Engenharia e Projetos",
                Descricao = "Modelo para engenheiros civis, eletricistas, mecânicos e projetos técnicos",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços de Engenharia e Elaboração de Projetos",
                    DescricaoServico = "a prestação de serviços técnicos de engenharia, elaboração de projetos, laudos e acompanhamento de obras, a serem executados por profissional qualificado denominado CONTRATADO",
                    LocalPrestacaoServico = "em campo, no escritório da CONTRATANTE, remotamente, ou em escritório do CONTRATADO, conforme natureza do serviço",
                    DetalhamentoServicos = new List<string>
                    {
                        "Elaboração de projetos técnicos (arquitetônicos, estruturais, elétricos, hidráulicos, etc.)",
                        "Análise de viabilidade técnica e econômica de empreendimentos",
                        "Fiscalização e acompanhamento de obras e reformas",
                        "Emissão de laudos técnicos, ART (Anotação de Responsabilidade Técnica) e pareceres",
                        "Medições de obra, orçamentos e cronogramas físico-financeiros",
                        "Vistorias técnicas e inspeções prediais",
                        "Assessoria em regularização de edificações junto a órgãos competentes",
                        "Elaboração de memoriais descritivos e especificações técnicas",
                        "Consultoria em segurança do trabalho e gestão de riscos",
                        "Atualização e revisão de projetos conforme normas ABNT e legislação local"
                    },
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Executar serviços conforme normas técnicas, ABNT e legislação vigente",
                        "Emitir ART ou RRT junto ao CREA/CAU conforme exigido",
                        "Manter registro profissional ativo no conselho de classe",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários",
                        "Manter sigilo sobre projetos e informações técnicas da CONTRATANTE",
                        "Responsabilizar-se tecnicamente por projetos e laudos emitidos"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer plantas, documentos, levantamentos e informações técnicas necessárias",
                        "Garantir acesso aos locais de vistoria e execução de serviços",
                        "Aprovar etapas de projetos e autorizar revisões em tempo hábil"
                    }
                }
            };
        }

        private ContractTemplatePresetResponse GetDesignPreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "design",
                Nome = "Serviços de Design e Criação Visual",
                Descricao = "Modelo para designers gráficos, UX/UI e ilustradores",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços de Design Gráfico e Criação Visual",
                    DescricaoServico = "a prestação de serviços criativos de design, identidade visual, interfaces digitais e materiais gráficos, a serem executados por profissional qualificado denominado CONTRATADO",
                    LocalPrestacaoServico = "remotamente, nas dependências da CONTRATANTE, ou em estúdio próprio do CONTRATADO, conforme projeto",
                    DetalhamentoServicos = new List<string>
                    {
                        "Criação de identidade visual (logotipos, paleta de cores, tipografia, manual de marca)",
                        "Design de materiais impressos (folders, cartões, banners, anúncios)",
                        "Desenvolvimento de layouts para redes sociais e campanhas digitais",
                        "Design de interfaces (UI) e experiência do usuário (UX) para sites e aplicativos",
                        "Criação de ilustrações, ícones e elementos gráficos customizados",
                        "Diagramação de apresentações, relatórios e documentos corporativos",
                        "Edição e tratamento de imagens fotográficas",
                        "Mockups e protótipos de produtos e embalagens",
                        "Animações e motion graphics para vídeos e mídias digitais",
                        "Consultoria em branding e estratégia visual"
                    },
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Entregar arquivos editáveis e finalizados nos formatos solicitados",
                        "Garantir originalidade das criações e ausência de plágio",
                        "Realizar até 2 rodadas de revisões por projeto, conforme briefing acordado",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários",
                        "Manter sigilo sobre projetos, estratégias e materiais da CONTRATANTE",
                        "Entregar arquivos dentro dos prazos acordados"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer briefing detalhado, referências visuais e materiais necessários",
                        "Aprovar propostas criativas e solicitar ajustes em até 48 horas",
                        "Respeitar prazos de resposta para não atrasar entregas",
                        "Creditar o CONTRATADO quando aplicável em publicações dos materiais criados"
                    }
                }
            };
        }

        private ContractTemplatePresetResponse GetEducacaoPreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "educacao",
                Nome = "Serviços de Educação e Treinamento",
                Descricao = "Modelo para instrutores, professores e consultores educacionais",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços de Educação, Treinamento e Capacitação",
                    DescricaoServico = "a prestação de serviços educacionais, ministrando aulas, treinamentos, cursos e workshops, a serem executados por profissional qualificado denominado CONTRATADO",
                    LocalPrestacaoServico = "presencialmente nas dependências da CONTRATANTE, online via plataformas digitais, ou em locais acordados",
                    DetalhamentoServicos = new List<string>
                    {
                        "Planejamento e elaboração de conteúdo didático e materiais de apoio",
                        "Ministração de aulas, palestras, workshops e treinamentos corporativos",
                        "Elaboração de avaliações, exercícios e atividades práticas",
                        "Acompanhamento e feedback individualizado aos participantes",
                        "Gravação de videoaulas e conteúdo digital (quando aplicável)",
                        "Aplicação de metodologias ativas e inovadoras de ensino",
                        "Emissão de certificados de conclusão aos participantes",
                        "Relatórios de desempenho e progresso dos alunos/treinandos",
                        "Atualização constante do conteúdo conforme tendências e necessidades",
                        "Suporte tira-dúvidas fora do horário de aula (via e-mail/chat)"
                    },
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Preparar e ministrar aulas com qualidade, pontualidade e profissionalismo",
                        "Manter domínio técnico e atualização sobre os temas lecionados",
                        "Respeitar horários, cronogramas e carga horária acordados",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários",
                        "Manter sigilo sobre conteúdo proprietário e informações da CONTRATANTE",
                        "Fornecer materiais didáticos e recursos necessários para as aulas"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer infraestrutura adequada (sala, equipamentos, plataforma online)",
                        "Divulgar e organizar turmas com antecedência mínima acordada",
                        "Disponibilizar lista de participantes e informações relevantes",
                        "Garantir ambiente adequado e respeitoso para execução das aulas"
                    }
                }
            };
        }

        private ContractTemplatePresetResponse GetSaudePreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "saude",
                Nome = "Serviços de Saúde e Bem-Estar",
                Descricao = "Modelo para profissionais de saúde (fisioterapeutas, nutricionistas, psicólogos)",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços de Saúde, Terapia e Bem-Estar",
                    DescricaoServico = "a prestação de serviços de saúde, terapias, consultas e acompanhamento, a serem executados por profissional qualificado denominado CONTRATADO",
                    LocalPrestacaoServico = "em clínica, consultório, nas dependências da CONTRATANTE, online, ou em domicílio, conforme modalidade acordada",
                    DetalhamentoServicos = new List<string>
                    {
                        "Realização de consultas, avaliações e anamnese de pacientes/clientes",
                        "Elaboração de planos terapêuticos, programas de tratamento ou dietas personalizadas",
                        "Acompanhamento periódico e reavaliações conforme evolução do quadro",
                        "Aplicação de técnicas terapêuticas específicas da área de atuação",
                        "Orientações sobre hábitos saudáveis, prevenção e autocuidado",
                        "Emissão de relatórios, laudos e atestados quando aplicável",
                        "Registro adequado em prontuário conforme legislação e normas éticas",
                        "Encaminhamento a outros profissionais quando necessário",
                        "Participação em reuniões multidisciplinares (quando aplicável)",
                        "Atendimento humanizado e respeitoso, observando código de ética profissional"
                    },
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Manter registro profissional ativo no conselho de classe (CRF, CRN, CRP, etc.)",
                        "Seguir protocolos, diretrizes técnicas e código de ética da profissão",
                        "Manter sigilo absoluto sobre dados e informações dos pacientes/clientes",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários",
                        "Manter apólice de seguro de responsabilidade civil profissional",
                        "Respeitar horários de atendimento e prazos de retorno"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer infraestrutura adequada e equipamentos necessários (quando aplicável)",
                        "Garantir privacidade e ambiente adequado para atendimentos",
                        "Respeitar autonomia técnica e decisões profissionais do CONTRATADO"
                    }
                }
            };
        }

        private ContractTemplatePresetResponse GetJuridicoPreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "juridico",
                Nome = "Serviços Jurídicos e Consultoria Legal",
                Descricao = "Modelo para advogados e consultores jurídicos",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços Jurídicos e Consultoria Legal",
                    DescricaoServico = "a prestação de serviços de assessoria jurídica, consultoria legal e representação em processos, a serem executados por profissional qualificado denominado CONTRATADO",
                    LocalPrestacaoServico = "no escritório da CONTRATANTE, remotamente, em tribunais, ou no escritório do CONTRATADO, conforme natureza do serviço",
                    DetalhamentoServicos = new List<string>
                    {
                        "Assessoria jurídica preventiva e consultiva em questões legais",
                        "Elaboração e revisão de contratos, termos e documentos legais",
                        "Análise de conformidade legal (compliance) de processos e operações",
                        "Representação em processos judiciais e administrativos",
                        "Defesa e acompanhamento de ações trabalhistas, cíveis, tributárias, etc.",
                        "Emissão de pareceres jurídicos sobre questões específicas",
                        "Negociação e mediação de conflitos extrajudiciais",
                        "Orientação sobre legislação aplicável ao negócio da CONTRATANTE",
                        "Acompanhamento de due diligence em operações societárias",
                        "Registro de marcas, patentes e propriedade intelectual (quando aplicável)"
                    },
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Manter inscrição ativa na OAB (Ordem dos Advogados do Brasil)",
                        "Atuar com zelo, diligência e observância ao Código de Ética da OAB",
                        "Manter sigilo absoluto sobre informações confidenciais e estratégicas",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários",
                        "Informar a CONTRATANTE sobre prazos processuais e andamento de demandas",
                        "Manter apólice de seguro de responsabilidade civil profissional"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer documentos, informações e acesso necessário para atuação",
                        "Comunicar imediatamente sobre notificações, citações ou intimações recebidas",
                        "Respeitar autonomia técnica e decisões estratégicas do CONTRATADO"
                    }
                }
            };
        }

        private ContractTemplatePresetResponse GetTIPreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "ti",
                Nome = "Serviços de TI e Infraestrutura",
                Descricao = "Modelo para profissionais de infraestrutura, redes e suporte técnico",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços de Tecnologia da Informação e Infraestrutura",
                    DescricaoServico = "a prestação de serviços de infraestrutura de TI, suporte técnico, administração de sistemas e redes, a serem executados por profissional qualificado denominado CONTRATADO",
                    LocalPrestacaoServico = "nas dependências da CONTRATANTE, remotamente, ou em datacenters, conforme natureza do serviço",
                    DetalhamentoServicos = new List<string>
                    {
                        "Instalação, configuração e manutenção de servidores e infraestrutura",
                        "Administração de redes locais (LAN) e redes corporativas (WAN)",
                        "Gerenciamento de ambientes cloud (AWS, Azure, Google Cloud)",
                        "Configuração e monitoramento de sistemas de segurança (firewall, antivírus, IDS)",
                        "Backup, recuperação de dados e disaster recovery",
                        "Suporte técnico N2/N3 para usuários e sistemas",
                        "Implementação de políticas de segurança da informação",
                        "Monitoramento proativo de serviços e disponibilidade",
                        "Atualizações de sistemas operacionais e aplicações corporativas",
                        "Documentação técnica de infraestrutura e procedimentos"
                    },
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Manter certificações técnicas relevantes (quando aplicável)",
                        "Garantir disponibilidade para atendimento emergencial conforme SLA acordado",
                        "Manter sigilo absoluto sobre infraestrutura, acessos e dados da CONTRATANTE",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários",
                        "Documentar alterações e manter registros de atividades realizadas",
                        "Seguir políticas de segurança da informação da CONTRATANTE"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer acessos necessários (VPN, credenciais, documentação)",
                        "Disponibilizar equipamentos e licenças de software necessárias",
                        "Comunicar mudanças em infraestrutura e políticas com antecedência"
                    }
                }
            };
        }

        private ContractTemplatePresetResponse GetManutencaoPreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "manutencao",
                Nome = "Serviços de Manutenção Predial e Reparos",
                Descricao = "Modelo para eletricistas, encanadores, pintores e manutenção geral",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços de Manutenção Predial e Reparos",
                    DescricaoServico = "a prestação de serviços de manutenção preventiva e corretiva, reparos e conservação predial, a serem executados por profissional qualificado denominado CONTRATADO",
                    LocalPrestacaoServico = "nas dependências da CONTRATANTE ou em locais por ela designados",
                    DetalhamentoServicos = new List<string>
                    {
                        "Manutenção preventiva e corretiva de instalações elétricas, hidráulicas e sanitárias",
                        "Reparos em alvenaria, gesso, pintura e acabamentos",
                        "Troca e instalação de luminárias, tomadas, interruptores e disjuntores",
                        "Conserto de vazamentos, entupimentos e problemas hidráulicos",
                        "Manutenção de portas, janelas, fechaduras e ferragens",
                        "Pequenas reformas e adaptações conforme necessidade",
                        "Vistoria periódica de instalações e equipamentos prediais",
                        "Pintura de paredes internas e externas, grades e portões",
                        "Jardinagem básica e manutenção de áreas externas (quando acordado)",
                        "Atendimento emergencial para problemas urgentes"
                    },
                    ClausulaAjudaCusto = "<p class=\"clausula\">Cláusula Sétima A</p><p>Será concedida ajuda de custo para aquisição de pequenos materiais e ferramentas, no valor de <strong>R$ 300,00</strong> (trezentos reais) mensais, mediante apresentação de notas fiscais dos itens adquiridos.</p>",
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Utilizar ferramentas e equipamentos adequados e em boas condições",
                        "Zelar pela limpeza e organização após execução dos serviços",
                        "Comunicar imediatamente problemas que requeiram intervenção especializada",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários",
                        "Observar normas de segurança do trabalho (NR-10, NR-12, etc.)",
                        "Manter postura profissional e respeitosa com colaboradores e clientes"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer materiais de maior custo (tintas, canos, fios, etc.) quando necessário",
                        "Garantir acesso aos locais de trabalho e informações sobre instalações",
                        "Aprovar orçamentos prévios para serviços de maior complexidade"
                    }
                }
            };
        }

        private ContractTemplatePresetResponse GetLimpezaPreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "limpeza",
                Nome = "Serviços de Limpeza e Conservação",
                Descricao = "Modelo para profissionais de limpeza e conservação predial",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços de Limpeza, Higienização e Conservação",
                    DescricaoServico = "a prestação de serviços de limpeza, higienização, desinfecção e conservação de ambientes, a serem executados por profissional qualificado denominado CONTRATADO",
                    LocalPrestacaoServico = "nas dependências da CONTRATANTE, incluindo escritórios, áreas comuns, banheiros, refeitórios e demais ambientes designados",
                    DetalhamentoServicos = new List<string>
                    {
                        "Limpeza diária de pisos, removendo sujeiras, manchas e resíduos",
                        "Higienização e desinfecção de banheiros, pias, sanitários e vestiários",
                        "Limpeza de superfícies (mesas, bancadas, prateleiras, janelas)",
                        "Recolhimento e destinação adequada de lixo e resíduos recicláveis",
                        "Limpeza de vidros, espelhos e divisórias",
                        "Aspiração de carpetes, tapetes e estofados",
                        "Reposição de materiais de higiene (papel higiênico, sabonete, álcool gel)",
                        "Limpeza periódica de áreas externas (varrição, lavagem)",
                        "Limpeza profunda semanal ou quinzenal conforme cronograma",
                        "Utilização de produtos de limpeza adequados e não agressivos"
                    },
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Utilizar uniformes limpos e identificação durante execução dos serviços",
                        "Utilizar EPIs adequados (luvas, máscaras, etc.)",
                        "Zelar pelo patrimônio e objetos da CONTRATANTE durante a limpeza",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários",
                        "Manter sigilo sobre informações e documentos visualizados durante o trabalho",
                        "Cumprir horários e frequência estabelecidos"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer produtos de limpeza, materiais de higiene e equipamentos necessários",
                        "Disponibilizar local adequado para guarda de materiais e equipamentos",
                        "Comunicar com antecedência sobre eventos ou necessidades especiais de limpeza"
                    }
                }
            };
        }

        private ContractTemplatePresetResponse GetSegurancaPreset()
        {
            return new ContractTemplatePresetResponse
            {
                Tipo = "seguranca",
                Nome = "Serviços de Segurança Patrimonial",
                Descricao = "Modelo para vigilantes e profissionais de segurança patrimonial",
                Configuracao = new ContractTemplateConfigRequest
                {
                    TituloServico = "Serviços de Segurança Patrimonial e Vigilância",
                    DescricaoServico = "a prestação de serviços de segurança, vigilância, controle de acesso e proteção patrimonial, a serem executados por profissional qualificado denominado CONTRATADO",
                    LocalPrestacaoServico = "nas dependências da CONTRATANTE, em portarias, estacionamentos, perímetros e áreas designadas",
                    DetalhamentoServicos = new List<string>
                    {
                        "Vigilância e monitoramento contínuo das instalações da CONTRATANTE",
                        "Controle de acesso de pessoas, veículos e materiais",
                        "Rondas periódicas em áreas internas e externas",
                        "Operação de sistemas de CFTV, alarmes e centrais de monitoramento",
                        "Registro de ocorrências, entrada e saída de visitantes em livro/sistema",
                        "Inspeção de bolsas, pacotes e pertences conforme política de segurança",
                        "Abertura e fechamento de portões, portas e cancelas",
                        "Comunicação imediata de situações suspeitas ou emergências",
                        "Apoio em evacuações de emergência e primeiros socorros básicos",
                        "Relacionamento cortês e profissional com funcionários, visitantes e fornecedores"
                    },
                    ObrigacoesContratado = new List<string>
                    {
                        "Cumprir integralmente as obrigações previstas no objeto do presente contrato",
                        "Portar curso de formação de vigilantes e registro na Polícia Federal (quando exigido)",
                        "Utilizar uniforme completo, limpo e identificação durante o serviço",
                        "Manter postura ética, atenta e proativa em situações de risco",
                        "Emitir Notas Fiscais de Serviços conforme previsto na Cláusula Nona deste contrato",
                        "Responsabilizar-se exclusivamente por todos os encargos trabalhistas, previdenciários, fiscais e tributários",
                        "Manter sigilo absoluto sobre rotinas, acessos e informações da CONTRATANTE",
                        "Cumprir escalas de trabalho, pontualidade e não abandonar posto sem autorização"
                    },
                    ObrigacoesContratante = new List<string>
                    {
                        "Efetuar o pagamento nas condições e prazos estabelecidos neste contrato",
                        "Fornecer uniforme, equipamentos de comunicação (rádio, celular) e EPIs",
                        "Disponibilizar local adequado para descanso e necessidades básicas",
                        "Orientar sobre procedimentos de segurança e protocolos específicos da empresa"
                    }
                }
            };
        }
    }
}
