using System;
using System.Data;
using System.Text;

/// <summary>
/// Summary description for myDados
/// </summary>


namespace BriskPtf
{
public partial class dados
{
    #region Objetos Estratégia

    public DataSet getProjetosOE(int codigoObjeticoEstrategico, int codigoEntidade, string where, string selectDePermissao)
    {
        string comandoSQL = string.Format(@"
                SELECT  rp.CodigoProjeto, p.NomeProjeto, rp.PercentualRealizacao AS Concluido
                    ,   u.NomeUsuario AS Responsavel, RTRIM(LTRIM(rp.CorGeral)) AS Status
                    ,   poe.CodigoObjetivoEstrategico, poe.Prioridade
                    ,   CASE    WHEN poe.Prioridade = 'A' THEN 'Alta' 
                                WHEN poe.Prioridade = 'M' THEN 'Media'
                                ELSE 'Baixa' END AS DescricaoPrioridade
                    ,   CASE    WHEN poe.Prioridade = 'A' THEN 0
                                WHEN poe.Prioridade = 'M' THEN 1
                                ELSE 2 END AS Ordem
                    ,   poe.CodigoIndicador
                    ,   ind.NomeIndicador
                    {5}

                FROM        {0}.{1}.ResumoProjeto               AS rp
                INNER JOIN  {0}.{1}.Projeto                     AS p    ON p.CodigoProjeto      = rp.CodigoProjeto
                LEFT JOIN   {0}.{1}.Usuario                     AS u    ON u.CodigoUsuario      = p.CodigoGerenteProjeto
                INNER JOIN  {0}.{1}.ProjetoObjetivoEstrategico  AS poe  ON poe.CodigoProjeto    = rp.CodigoProjeto
                LEFT JOIN   {0}.{1}.Indicador                   As ind  ON poe.CodigoIndicador  = ind.CodigoIndicador
                LEFT JOIN   {0}.{1}.ParametroConfiguracaoSistema AS pcs ON (pcs.Parametro = 'MostraProjetoEncerradoObjetivoEstrategico' and pcs.CodigoEntidade = p.CodigoEntidade)


                WHERE poe.CodigoObjetivoEstrategico = {2}
                  AND p.DataExclusao IS NULL
                  AND (pcs.Valor = 'S' OR p.CodigoStatusProjeto <> 6)
                  {4}

                ORDER BY Ordem, p.NomeProjeto
            ", bancodb, Ownerdb, codigoObjeticoEstrategico, codigoEntidade, where
             , selectDePermissao.Equals("") ? "" : ", " + selectDePermissao);
        return getDataSet(comandoSQL);
    }

    public DataSet getProjetosPlanosAcaoOE(int codigoObjeticoEstrategico, int codigoEntidade, int codigoUsuario, string where)
    {
        int codigoTipoAssociacao = getCodigoTipoAssociacao("OB");

        string comandoSQL = string.Format(@"
                SELECT  rp.CodigoProjeto AS Codigo
                      , p.NomeProjeto AS Descricao
                      , rp.PercentualRealizacao AS Concluido
                      , u.NomeUsuario AS Responsavel
                      , p.CodigoGerenteProjeto as CodigoResponsavel
                      , RTRIM(LTRIM(rp.CorGeral)) AS Status
                      , poe.Prioridade
                      , poe.CodigoIndicador
                      , {0}.{1}.f_VerificaAcessoConcedido({4}, {3}, p.CodigoProjeto, NULL, 'PR', 0, NULL, 'PR_Acs') AS Permissao
                      , '{7}' as Tipo
                      , CASE    WHEN poe.Prioridade = 'A' THEN 0
                                WHEN poe.Prioridade = 'M' THEN 1
                                ELSE 2 END AS Ordem
                      , lo.PesoObjetoLink
                FROM        {0}.{1}.ResumoProjeto               AS rp
                INNER JOIN  {0}.{1}.Projeto                     AS p    ON p.CodigoProjeto      = rp.CodigoProjeto
                LEFT JOIN   {0}.{1}.Usuario                     AS u    ON u.CodigoUsuario      = p.CodigoGerenteProjeto
                INNER JOIN  {0}.{1}.ProjetoObjetivoEstrategico  AS poe  ON poe.CodigoProjeto    = rp.CodigoProjeto
                LEFT JOIN   {0}.{1}.Indicador                   As ind  ON poe.CodigoIndicador  = ind.CodigoIndicador
                LEFT JOIN   {0}.{1}.ParametroConfiguracaoSistema AS pcs ON (pcs.Parametro = 'MostraProjetoEncerradoObjetivoEstrategico' and pcs.CodigoEntidade = p.CodigoEntidade)
                LEFT JOIN   {0}.{1}.LinkObjeto                   AS lo ON (lo.CodigoObjeto = poe.CodigoObjetivoEstrategico 
				                                                                    AND lo.CodigoTipoObjeto = dbo.f_GetCodigoTipoAssociacao('OB')
				                                                                    AND lo.CodigoObjetoPai = 0
																					AND lo.CodigoObjetoLink = p.CodigoProjeto
																					AND lo.CodigoTipoObjetoLink  = dbo.f_GetCodigoTipoAssociacao('PR')
				                                                                    AND lo.CodigoObjetoPaiLink = 0
																					AND lo.CodigoTipoLink = (select CodigoTipoLink  from TipoLinkObjeto where IniciaisTipoLink = 'AS'))  
              WHERE poe.CodigoObjetivoEstrategico = {2}
                  AND p.DataExclusao IS NULL
                  AND p.CodigoEntidade = {3}
                  AND (pcs.Valor = 'S' OR p.CodigoStatusProjeto <> 6)
                  {6}
                UNION
                 SELECT tdl.[CodigoToDoList] as Codigo
                      , tdl.[NomeToDoList]
                      , {0}.{1}.f_GetConcluidoPA(tdl.CodigoToDoList) / 100.00
                      , usu.[NomeUsuario]
                      , tdl.[CodigoUsuarioResponsavelToDoList]
                      , {0}.{1}.f_GetDesempenhoPA(tdl.CodigoToDoList, {3})
                      , null
                      , null
                      , null
                      , 'toDoList'
                      , 3
                      , lo.PesoObjetoLink
                FROM        {0}.{1}.[ToDoList]          AS [tdl]
                INNER JOIN  {0}.{1}.[TipoAssociacao]    AS [ta]     ON ( ta.[CodigoTipoAssociacao]  = tdl.[CodigoTipoAssociacao] )
                INNER JOIN  {0}.{1}.[Usuario]           AS [usu]    ON ( usu.[CodigoUsuario]        = tdl.[CodigoUsuarioResponsavelToDoList] )
                LEFT JOIN   {0}.{1}.LinkObjeto                   AS lo ON (lo.CodigoObjeto = tdl.CodigoObjetoAssociado 
				                                                                    AND lo.CodigoTipoObjeto = dbo.f_GetCodigoTipoAssociacao('OB')
				                                                                    AND lo.CodigoObjetoPai = 0
																					AND lo.CodigoObjetoLink = tdl.CodigoToDoList
																					AND lo.CodigoTipoObjetoLink  = dbo.f_GetCodigoTipoAssociacao('TD')
				                                                                    AND lo.CodigoObjetoPaiLink = 0
																					AND lo.CodigoTipoLink = (select CodigoTipoLink  from TipoLinkObjeto where IniciaisTipoLink = 'AS'))  
                WHERE   tdl.[CodigoTipoAssociacao]  = {5}
                  AND   tdl.[CodigoObjetoAssociado] = {2}
                  AND   tdl.DataExclusao            IS NULL
                ORDER BY Ordem, Descricao
                
            ", bancodb, Ownerdb, codigoObjeticoEstrategico, codigoEntidade, codigoUsuario, codigoTipoAssociacao, where, "projeto");

        return getDataSet(comandoSQL);
    }

    public DataSet getProjetosIndicador(int codigoIndicador, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"SELECT rp.CodigoProjeto, p.NomeProjeto, rp.PercentualRealizacao AS Concluido,
	                                               u.NomeUsuario AS Responsavel, RTRIM(LTRIM(rp.CorGeral)) AS Status
                                              FROM {0}.{1}.ResumoProjeto rp INNER JOIN
	                                               {0}.{1}.Projeto p ON p.CodigoProjeto = rp.CodigoProjeto LEFT JOIN
	                                               {0}.{1}.Usuario u ON u.CodigoUsuario = p.CodigoGerenteProjeto
                                             WHERE p.CodigoEntidade = {3}
                                               AND EXISTS(SELECT 1 FROM {0}.{1}.ProjetoObjetivoEstrategico poe WHERE poe.CodigoIndicador = {2} AND poe.CodigoProjeto = rp.CodigoProjeto)
                                               {4}
                                             ORDER BY p.NomeProjeto", bancodb, Ownerdb, codigoIndicador, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region CDIS.MASTER

    public DataSet getValorParametroConfiguracaoSistema(string parametroSistema, string codigoUnidade, string where)
    {
        string comandoSQL = string.Format(@"
            SELECT Valor 
              FROM {0}.{1}.ParametroConfiguracaoSistema
             WHERE Parametro='{2}'
               AND CodigoEntidade = {4}
               {3}
        ", bancodb, Ownerdb, parametroSistema, where, codigoUnidade);

        return getDataSet(comandoSQL);
    }

    #endregion

    #region _Estrategia - Relatorios - relAnaliseCausaEfeitoPorTema.aspx

    public DataSet getTemaEstrategicoFromMapa(string codigoMapa, string tipoObjeto)
    {
        string comandoSQL = string.Format(@"
            SELECT oe.[CodigoObjetoEstrategia] , oe.[DescricaoObjetoEstrategia] 
              FROM {0}.{1}.[ObjetoEstrategia] AS [oe] 
                    INNER JOIN {0}.{1}.[TipoObjetoEstrategia] AS [toe] 
                            ON (toe.[CodigoTipoObjetoEstrategia] = oe.[CodigoTipoObjetoEstrategia]) 
             WHERE 
                    oe.[CodigoMapaEstrategico] = {2} 
               AND  toe.[IniciaisTipoObjeto] = '{3}' 
        ", bancodb, Ownerdb, codigoMapa, tipoObjeto);

        return getDataSet(comandoSQL);
    }

    public DataSet getTreeRelatorioCausaEfeito(string codigoEntidadeUsuario, string codigoMapa, string codigoTema)
    {
        string comandoSQL = string.Format(@"
            EXEC {0}.{1}.p_RelatorioCausaEfeitoTema {2}, {3}, {4}
        ", bancodb, Ownerdb, codigoEntidadeUsuario, codigoMapa, codigoTema);

        return getDataSet(comandoSQL);
    }

    #endregion

    #region _Estrategia - Relatorios - Planejamento estratégico

    public DataSet getdsRelIdentidadeOrganizacional(int codigoMapa)
    {
        string comandoSQL = string.Format(@"BEGIN
       SET NOCOUNT ON
       DECLARE @CodigoUnidadeNegocioParam Int,
               @Iniciais Char(3),
               @Descricao Varchar(500),
               @TituloObjetoSup Varchar(250),
               @Cont1 SmallInt,
               @Cont2 SmallInt,
               @Cont3 SmallInt,
               @Cont4 SmallInt,
               @CodigoObjetoEstrategia Int,
               @ConteudoLinhaAtuacao Varchar(2000),
               @ConteudoAcoesSugeridas Varchar(2000),
               @codigoTipo as int,
               @textoAcoesSugeridas Varchar(3000),
               @textoLinhaAtuacao Varchar(3000)

              SET @CodigoUnidadeNegocioParam = {2} --Parâmetro

              DECLARE @Identidade 
                TABLE
                 (Missao Varchar(1000)Default ' ',
                  Visao Varchar(1000),
                  CrencasValores Varchar(1000),
                  Tema Varchar(1000),
                  NomeMapa Varchar(1000),
                  Pessoas Varchar(8000),
                  Processos Varchar(8000),
                  MercadoImagem Varchar(8000),
                  Financeira Varchar(8000))

             INSERT INTO @Identidade
             (Missao,
              Visao,
              CrencasValores,
              Nomemapa,
              Tema,
              Pessoas,
              Processos,
              MercadoImagem,
              Financeira)
              SELECT 
                (SELECT DescricaoObjetoEstrategia
                   FROM {0}.{1}.ObjetoEstrategia AS oe INNER JOIN
                        {0}.{1}.TipoObjetoEstrategia AS toe ON (toe.CodigoTipoObjetoEstrategia = oe.CodigoTipoObjetoEstrategia
                                                    AND toe.IniciaisTipoObjeto = 'MIS')
                  WHERE oe.CodigoMapaEstrategico = me.CodigoMapaEstrategico
                    AND oe.DataExclusao IS NULL),
                (SELECT DescricaoObjetoEstrategia
                   FROM {0}.{1}.ObjetoEstrategia AS oe INNER JOIN
                        {0}.{1}.TipoObjetoEstrategia AS toe ON (toe.CodigoTipoObjetoEstrategia = oe.CodigoTipoObjetoEstrategia
                                                    AND toe.IniciaisTipoObjeto = 'VIS')
                  WHERE oe.CodigoMapaEstrategico = me.CodigoMapaEstrategico
                    AND oe.DataExclusao IS NULL),
                (SELECT DescricaoObjetoEstrategia
                   FROM {0}.{1}.ObjetoEstrategia AS oe INNER JOIN
                        {0}.{1}.TipoObjetoEstrategia AS toe ON (toe.CodigoTipoObjetoEstrategia = oe.CodigoTipoObjetoEstrategia
                                                    AND toe.IniciaisTipoObjeto = 'CRE')
                  WHERE oe.CodigoMapaEstrategico = me.CodigoMapaEstrategico
                    AND oe.DataExclusao IS NULL),
                   (SELECT DescricaoObjetoEstrategia
                   FROM {0}.{1}.ObjetoEstrategia AS oe INNER JOIN
                        {0}.{1}.TipoObjetoEstrategia AS toe ON (toe.CodigoTipoObjetoEstrategia = oe.CodigoTipoObjetoEstrategia
                                                    AND toe.IniciaisTipoObjeto = 'MAP')
                  WHERE oe.CodigoMapaEstrategico = me.CodigoMapaEstrategico
                    AND oe.DataExclusao IS NULL),'','','','',''

             FROM {0}.{1}.MapaEstrategico AS me
             WHERE me.CodigoMapaEstrategico = {2}

              SELECT Missao, 
                     Visao,
                     CrencasValores,
                     NomeMapa,
                     Tema,
                     Pessoas,
                     Processos,
                     MercadoImagem,
                     Financeira
                FROM @Identidade
            END", bancodb, Ownerdb, codigoMapa);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region _Estrategias - objetivoEstrategico - ObjetivoEstrategico_AcoesSugeridas.aspx

    public DataSet getAcoesSugeridas(string objetoAssociado)
    {
        string comandoSQL = string.Format(@"
            BEGIN

            DECLARE @CodigoTipoAssociado INT

            SET @CodigoTipoAssociado = (SELECT CodigoTipoAssociacao FROM {0}.{1}.TipoAssociacao WHERE IniciaisTipoAssociacao = 'OB')

            SELECT CodigoAcaoSugerida, DescricaoAcao, TextoAcaoSugerida, CodigoTipoAssociacao, CodigoObjetoAssociado, CodigoEntidade, DataDesativacao, DataInclusao, CodigoUsuarioInclusao, CodigoUsuarioDesativacao
            FROM {0}.{1}.AcoesSugeridas
            WHERE   DataDesativacao Is NULL
                AND CodigoObjetoAssociado = {2}
                AND CodigoTipoAssociacao = @CodigoTipoAssociado

            ORDER BY DescricaoAcao
            END
        ", bancodb, Ownerdb, objetoAssociado);

        return getDataSet(comandoSQL);
    }

    public DataSet getAcoesSugeridasObjetivoProjeto(int codigoObjetivoEstrategico, int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(@"
            BEGIN
	            DECLARE @tbResumo AS TABLE(
						            CodigoAcaoSugerida INT,
						            DescricaoAcao VarChar(250),
						            Selecionado Char(1))
            						
                DECLARE @CodigoTipoAssociado INT

                SET @CodigoTipoAssociado = (SELECT CodigoTipoAssociacao 
							                  FROM {0}.{1}.TipoAssociacao 
								             WHERE IniciaisTipoAssociacao = 'OB')

	            INSERT INTO @tbResumo
		            SELECT CodigoAcaoSugerida, DescricaoAcao, 'S'
		              FROM {0}.{1}.AcoesSugeridas
		             WHERE DataDesativacao Is NULL
		               AND CodigoObjetoAssociado = {3}
		               AND CodigoTipoAssociacao = @CodigoTipoAssociado
		               AND CodigoAcaoSugerida IN (SELECT pas.CodigoAcaoSugerido
										            FROM {0}.{1}.ProjetoAcoesSugeridas pas
									               WHERE pas.CodigoProjeto = {2}
										             AND pas.CodigoObjetivoEstrategico = {3})
            									
	            INSERT INTO @tbResumo
		            SELECT CodigoAcaoSugerida, DescricaoAcao, 'N'
		              FROM {0}.{1}.AcoesSugeridas
		             WHERE DataDesativacao Is NULL
		               AND CodigoObjetoAssociado = {3}
		               AND CodigoTipoAssociacao = @CodigoTipoAssociado
		               AND CodigoAcaoSugerida NOT IN (SELECT pas.CodigoAcaoSugerido
										            FROM {0}.{1}.ProjetoAcoesSugeridas pas
									               WHERE pas.CodigoProjeto = {2}
										             AND pas.CodigoObjetivoEstrategico = {3})
            	
	            SELECT * 
	              FROM @tbResumo
	             ORDER BY DescricaoAcao
            	   
            END

        ", bancodb, Ownerdb, codigoProjeto, codigoObjetivoEstrategico, where);

        return getDataSet(comandoSQL);
    }

    public bool incluiAcoesSugeridas(string descricao, string texto, string usuarioInclusao,
                                     string tipoAssociacao, string objetoAssociado, string codigoEntidade)
    {
        bool retorno = false;
        int registrosAfetados = 0;
        try
        {
            string comandoSQL = string.Format(@"
	            INSERT INTO {0}.{1}.AcoesSugeridas(  DescricaoAcao, TextoAcaoSugerida, DataInclusao
                                           , CodigoUsuarioInclusao, CodigoTipoAssociacao
                                           , CodigoObjetoAssociado, CodigoEntidade)
	            values(
                            '{2}'
                        ,   '{3}'
                        ,   GETDATE()
                        ,   {4}
                        ,   {5}
                        ,   {6}
                        ,   {7}
                        )        
            ", bancodb, Ownerdb, descricao, texto, usuarioInclusao, tipoAssociacao, objetoAssociado, codigoEntidade);

            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }

        return retorno;
    }

    public bool excluiAcoesSugeridas(string usuarioExclusao, string codigoAcaoSugerida)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                UPDATE {0}.{1}.AcoesSugeridas  
                   SET DataDesativacao = GETDATE()
                     , CodigoUsuarioDesativacao = {2}
                 WHERE CodigoAcaoSugerida = {3}
            ", bancodb, Ownerdb, usuarioExclusao, codigoAcaoSugerida);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool atualizaAcoesSugeridas(string descricao, string texto, string tipoAssociacao
                                      , string objetoAssociado, string chave)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"UPDATE {0}.{1}.AcoesSugeridas
                                           SET DescricaoAcao        = '{2}'
                                             , TextoAcaoSugerida    = '{3}'
                                             , CodigoTipoAssociacao = {4}
                                             , CodigoObjetoAssociado= {5}
                                         WHERE CodigoAcaoSugerida = {6}
            ", bancodb, Ownerdb, descricao, texto, tipoAssociacao, objetoAssociado, chave);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region _Estrategias - objetivoEstrategico - ObjetivoEstrategico_Analises.aspx

    public DataSet getObjetivoEstrategicoAnalises(string objetoAssociado, string codigoEntidade, string tipoAssociacao)
    {
        string comandoSQL = string.Format(@"
            DECLARE @CodigoTipoAssociado INT

            SET @CodigoTipoAssociado = (SELECT CodigoTipoAssociacao FROM TipoAssociacao WHERE IniciaisTipoAssociacao = '{4}')

            SELECT   a.CodigoAnalisePerformance
                ,convert(datetime,a.DataAnalisePerformance,103) as DataAnalisePerformance
                ,a.Recomendacoes
                ,a.CodigoUsuarioInclusao
                ,a.Analise                  -- AnalisePerformance
                ,a.CodigoObjetoAssociado    -- .CodigoObjetivoEstrategico
                ,u.NomeUsuario as NomeUsuarioInclusao
                ,(select top 1 NomeUsuario from {0}.{1}.Usuario u where CodigoUsuario = a.[CodigoUsuarioUltimaAlteracao]) as NomeUsuarioUltimaAlteracao
                ,a.DataUltimaAlteracao
                ,a.DataInclusao

            FROM        {0}.{1}.AnalisePerformance  AS a 
            INNER JOIN  {0}.{1}.Usuario             AS u ON (a.CodigoUsuarioInclusao = u.CodigoUsuario)

            WHERE a.CodigoObjetoAssociado   = {2} -- .CodigoObjetivoEstrategico = 771
                AND a.CodigoTipoAssociacao  = @CodigoTipoAssociado
                AND CodigoUnidadeNegocio    = {3} 
                AND a.DataExclusao          IS NULL
            ", bancodb, Ownerdb, objetoAssociado, codigoEntidade, tipoAssociacao);

        return getDataSet(comandoSQL);
    }

    public void incluirObjetivoEstrategicoAnalises(int codigoObjetivoEstrategico, int codigoUnidadeNegocio
                                                    , string analise, string recomendacoes, int codigoUsuario
                                                    , string codigoTipoAssociacao, ref int regAfetados, ref string mensagemErro)
    {
        try
        {
            string comandoSQL = string.Format(@"
                --inserir un novo registro ao [AnalisePerformance]
                DECLARE @CodigoTipoAssociacao AS INT
                
                SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao FROM TipoAssociacao WHERE IniciaisTipoAssociacao = '{7}'

                INSERT INTO {0}.{1}.AnalisePerformance(
                            CodigoObjetoAssociado   --CodigoObjetivoEstrategico
                        ,   CodigoTipoAssociacao 
                        ,   CodigoUnidadeNegocio
                        ,   DataAnalisePerformance
                        ,   Analise                 --AnalisePerformance
                        ,   Recomendacoes
                        ,   CodigoUsuarioInclusao
                        ,   DataInclusao
                        ,   IndicaRegistroEditavel)
                VALUES(
                            {2}
                        ,   @CodigoTipoAssociacao
                        ,   {3}
                        ,   GETDATE()
                        ,   '{4}'
                        ,   '{5}'
                        ,   {6}
                        ,   GETDATE()
                        ,   'S')
                ", bancodb, Ownerdb, codigoObjetivoEstrategico
                , codigoUnidadeNegocio
                , analise.Trim().Replace("'", "''")
                , recomendacoes.Trim().Replace("'", "''")
                , codigoUsuario, codigoTipoAssociacao);

            execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            regAfetados = -1;
            mensagemErro = ex.Message;
        }
    }

    #endregion

    #region _Estrategias - objetivoEstrategico - ObjetivoEstrategico_Estrategias.aspx

    public DataSet getObjetivoEstrategicoEstrategias(string objetoAssociado)
    {
        string comandoSQL = string.Format(@"
            BEGIN

            DECLARE @CodigoTipoAssociado INT

            SET @CodigoTipoAssociado = (SELECT CodigoTipoAssociacao FROM {0}.{1}.TipoAssociacao WHERE IniciaisTipoAssociacao = 'OB')

            SELECT * 
            FROM {0}.{1}.LinhaAtuacaoEstrategica
            WHERE   DataDesativacao Is NULL
                AND CodigoObjetoAssociado = {2}
                AND CodigoTipoAssociacao = @CodigoTipoAssociado
            END
        ", bancodb, Ownerdb, objetoAssociado);

        return getDataSet(comandoSQL);
    }

    public bool incluiObjetivoEstrategicoEstrategias(string descricao, string texto, string usuarioInclusao,
                                     string tipoAssociacao, string objetoAssociado, string codigoEntidade)
    {
        bool retorno = false;
        int registrosAfetados = 0;
        try
        {
            string comandoSQL = string.Format(@"
	            INSERT INTO {0}.{1}.LinhaAtuacaoEstrategica(  DescricaoLinhaAtuacao, TextoLinhaAtuacao, DataInclusao
                                           , CodigoUsuarioInclusao, CodigoTipoAssociacao
                                           , CodigoObjetoAssociado, CodigoEntidade)
	            values(
                            '{2}'
                        ,   '{3}'
                        ,   GETDATE()
                        ,   {4}
                        ,   {5}
                        ,   {6}
                        ,   {7}
                        )        
            ", bancodb, Ownerdb, descricao, texto, usuarioInclusao, tipoAssociacao, objetoAssociado, codigoEntidade);

            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }

        return retorno;
    }

    public bool excluiObjetivoEstrategicoEstrategias(string usuarioExclusao, string codigoAcaoSugerida)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                UPDATE {0}.{1}.LinhaAtuacaoEstrategica  
                   SET DataDesativacao = GETDATE()
                     , CodigoUsuarioDesativacao = {2}
                 WHERE CodigoLinhaAtuacao = {3}
            ", bancodb, Ownerdb, usuarioExclusao, codigoAcaoSugerida);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool atualizaObjetivoEstrategicoEstrategias(string descricao, string texto, string tipoAssociacao
                                      , string objetoAssociado, string chave)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"UPDATE {0}.{1}.LinhaAtuacaoEstrategica
                                           SET DescricaoLinhaAtuacao    = '{2}'
                                             , TextoLinhaAtuacao        = '{3}'
                                             , CodigoTipoAssociacao     = {4}
                                             , CodigoObjetoAssociado    = {5}
                                         WHERE CodigoLinhaAtuacao       = {6}
            ", bancodb, Ownerdb, descricao, texto, tipoAssociacao, objetoAssociado, chave);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region _Estrategias - objetivoEstrategico - ObjetivoEstrategico_ToDoList.aspx

    public DataSet getToDoListEstrategica(string objetoAssociado, int codigoTipoAssociacao, string codigoUnidade)
    {
        string comandoSQL = string.Format(@"
                SELECT  tdl.[CodigoToDoList]
                    ,   tdl.[CodigoUsuarioResponsavelToDoList]
                    ,   tdl.[NomeToDoList]
                    ,   usu.[NomeUsuario]
                    ,   {0}.{1}.f_GetDesempenhoPA(tdl.CodigoToDoList, {3})  AS Status
                    ,   {0}.{1}.f_GetConcluidoPA(tdl.CodigoToDoList)        AS Porcentagem
                     , LO.PesoObjetoLink

                FROM        {0}.{1}.[ToDoList]          AS [tdl]
                INNER JOIN  {0}.{1}.[TipoAssociacao]    AS [ta]     ON ( ta.[CodigoTipoAssociacao]  = tdl.[CodigoTipoAssociacao] )
                INNER JOIN  {0}.{1}.[Usuario]           AS [usu]    ON ( usu.[CodigoUsuario]        = tdl.[CodigoUsuarioResponsavelToDoList] )
                 LEFT JOIN  {0}.{1}.LinkObjeto          as [LO]     on (lo.CodigoObjetoLink = tdl.CodigoToDoList)
                WHERE   tdl.[CodigoTipoAssociacao]  = {4}
                  AND   tdl.[CodigoObjetoAssociado] = {2}
                  AND   tdl.CodigoEntidade          = {5}
                  AND   tdl.DataExclusao            IS NULL
        ", bancodb, Ownerdb, objetoAssociado, codigoUnidade, codigoTipoAssociacao, codigoUnidade);

        return getDataSet(comandoSQL);
    }


    public bool incluiToDoList_Estrategica(string usuarioInclusao, string tipoAssociacao, string usuarioResponsavel,
                                                  string objetoAssociado, string codigoEntidade, string nomeTodolist)
    {
        bool retorno = false;
        int registrosAfetados = 0;

        try
        {
            string comandoSQL = string.Format(@"
                INSERT INTO {0}.{1}.todolist( DataInclusao, CodigoUsuarioInclusao, CodigoTipoAssociacao
                                            , CodigoUsuarioResponsavelToDoList, CodigoObjetoAssociado
                                            , CodigoEntidade, NomeToDoList)
                VALUES (
                          GETDATE()
                        , {2}
                        , {3}
                        , {4}
                        , {5}
                        , {6}
                        , {7}
                        )
            ", bancodb, Ownerdb, usuarioInclusao, tipoAssociacao
             , usuarioResponsavel, objetoAssociado, codigoEntidade, nomeTodolist);

            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }
        return retorno;
    }


    public bool incluiToDoList_Estrategica(string usuarioInclusao, string tipoAssociacao, string usuarioResponsavel,
                                                  string objetoAssociado, string codigoEntidade, string nomeTodolist, out int codigoToDoList)
    {
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"
                DECLARE 
                    @codigoToDoList             Int
                INSERT INTO {0}.{1}.todolist( DataInclusao, CodigoUsuarioInclusao, CodigoTipoAssociacao
                                            , CodigoUsuarioResponsavelToDoList, CodigoObjetoAssociado
                                            , CodigoEntidade, NomeToDoList)
                VALUES (
                          GETDATE()
                        , {2}
                        , {3}
                        , {4}
                        , {5}
                        , {6}
                        , {7}
                        )

       
                        SET @codigoToDoList = SCOPE_IDENTITY()

                        SELECT @codigoToDoList AS codigoToDoList

            ", bancodb, Ownerdb, usuarioInclusao, tipoAssociacao
             , usuarioResponsavel, objetoAssociado, codigoEntidade, nomeTodolist);

            DataSet ds = getDataSet(comandoSQL);
            codigoToDoList = int.Parse(ds.Tables[0].Rows[0]["codigoToDoList"].ToString());

            retorno = true;
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }

        return retorno;
    }


    public bool incluiAtualizaPeso(int CodigoObjeto, string CodigoTipoObjeto, string CodigoObjetoLink, string CodigoTipoObjetoLink, decimal peso)
    {
        bool retorno = false;
        int registrosAfetados = 0;

        try
        {
            //Caso Parametro configuração sistema esteja configurado para salvar ParamutilizaPesoDesempenhoObjetivo
            DataSet dsParamutilizaPesoDesempenhoObjetivo = getParametrosSistema("utilizaPesoDesempenhoObjetivo");
            if (dsParamutilizaPesoDesempenhoObjetivo != null && dsParamutilizaPesoDesempenhoObjetivo.Tables[0].Rows.Count > 0 && dsParamutilizaPesoDesempenhoObjetivo.Tables[0].Rows[0]["utilizaPesoDesempenhoObjetivo"] + "" == "S")
            {
                if (peso < 0)
                {
                    peso = 0;
                }
                comandoSQL = string.Format(@"BEGIN

                    DECLARE @CodigoObjeto as bigint
                    DECLARE @CodigoTipoObjeto as smallint
                    DECLARE @CodigoObjetoLink as bigint
                    DECLARE @CodigoTipoObjetoLink as smallint
                    DECLARE @CodigoTipoLink as tinyint
                    DECLARE @PesoObjetoLink as decimal(9,4)   

                    SET @CodigoObjeto         = {0}
                    SET @CodigoTipoObjeto     = dbo.f_GetCodigoTipoAssociacao('{1}')
                    SET @CodigoObjetoLink     = {2}
                    SET @CodigoTipoObjetoLink = dbo.f_GetCodigoTipoAssociacao('{3}')
                    SET @CodigoTipoLink       = (SELECT CodigoTipoLink  FROM TipoLinkObjeto WHERE IniciaisTipoLink = 'AS')
                    SET @PesoObjetoLink       = {4}

                    IF EXISTS (SELECT 1 FROM [LinkObjeto] 
                                       WHERE [CodigoObjeto]          = @CodigoObjeto
                                         AND [CodigoTipoObjeto]      = @CodigoTipoObjeto
		                                 AND [CodigoTipoObjetoLink]  = @CodigoTipoObjetoLink
                                         AND [CodigoObjetoLink]      = @CodigoObjetoLink 
		                                 AND [CodigoTipoLink]        = @CodigoTipoLink)
                       
                            BEGIN
                                    UPDATE [LinkObjeto]
                                    SET [PesoObjetoLink]        = @PesoObjetoLink
                                    WHERE [CodigoObjeto]          = @CodigoObjeto
                                    AND [CodigoTipoObjeto]      = @CodigoTipoObjeto
		                            AND [CodigoTipoObjetoLink]  = @CodigoTipoObjetoLink 
                                    AND [CodigoObjetoLink]      = @CodigoObjetoLink 
		                            AND [CodigoTipoLink]        = @CodigoTipoLink
                            END
                            ELSE
                            BEGIN
                                    INSERT INTO [dbo].[LinkObjeto]
                                       ([CodigoObjeto]
                                       ,[CodigoObjetoPai]
                                       ,[CodigoTipoObjeto]
                                       ,[CodigoObjetoLink]
                                       ,[CodigoTipoObjetoLink]
                                       ,[CodigoTipoLink]
                                       ,[PesoObjetoLink])
                                    VALUES
                                       (@CodigoObjeto
                                       ,0
                                       ,@CodigoTipoObjeto
                                       ,@CodigoObjetoLink
                                       ,@CodigoTipoObjetoLink
                                       ,@CodigoTipoLink
                                       ,@PesoObjetoLink)
                              END
          END",
          /*{0}*/CodigoObjeto,
          /*{1}*/CodigoTipoObjeto,
          /*{2}*/CodigoObjetoLink,
          /*{3}*/CodigoTipoObjetoLink,
          /*{4}*/peso.ToString().Replace(',', '.'));
                execSQL(comandoSQL, ref registrosAfetados);
                //####################
            }
            retorno = true;
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }
        return retorno;
    }

    public bool complementaDadosToDoListEstrategico(int codigoToDoList, string toDoListName, string codigoUsuarioResponsavel)
    {
        bool retorno = false;
        int registrosAfetados = 0;
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
           UPDATE {0}.{1}.[ToDoList]
	                    SET
		                      [NomeToDoList]						= {2}
			                , [CodigoUsuarioResponsavelToDoList]	= {3}
	                    WHERE
		                    [CodigoToDoList] = {4}

            ", bancodb, Ownerdb, toDoListName, codigoUsuarioResponsavel, codigoToDoList);

            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao atualizar os dados.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }

        return retorno;
    }
    #endregion

    #region _Estrategias - objetivoEstrategico - novaMensagemObjetivoEstrategico.aspx

    public bool incluiMensagemObjeto(int codigoEntidade, int idProjeto, int codigoUsuarioInclusao, string assunto, string mensagem, DateTime dataLimiteResposta, bool respostaNecessaria, string prioridade, int[] listaUsuariosSelecionados, string iniciaisObjeto, ref string mensagemErro)
    {
        bool retorno = false;
        string dateAsString;

        // se reposta necessária e a data limite contiver um valor que não seja MinValue, registra a data.
        // Caso contrário, deixa sem data.
        if ((respostaNecessaria) && (dataLimiteResposta != DateTime.MinValue))
            dateAsString = "CONVERT(DATETIME, '" + dataLimiteResposta.Day + "/" + dataLimiteResposta.Month + "/" + dataLimiteResposta.Year + "', 103)";
        else
            dateAsString = "NULL";

        string insereDestinatarios = "";
        for (int j = 0; j < listaUsuariosSelecionados.Length; j++)
        {
            insereDestinatarios += string.Format(@"
            INSERT INTO {0}.{1}.[MensagemDestinatario]
                ([CodigoMensagem]
                ,[CodigoDestinatario])
            VALUES
                (@CodigoMensagem
                ,{2});", bancodb, Ownerdb, listaUsuariosSelecionados[j]);
        }

        string comandoSQL = string.Format(@"
        BEGIN
            DECLARE 
                    @CodigoMensagem             Int
                ,   @CodigoTipoAssociacao       SmallInt
            SELECT @CodigoTipoAssociacao = [CodigoTipoAssociacao] FROM {0}.{1}.[TipoAssociacao] WHERE [IniciaisTipoAssociacao] = '{9}'

            INSERT INTO {0}.{1}.[Mensagem]
                (     [Mensagem]
                    , [DataInclusao]
                    , [IndicaRespostaNecessaria]
                    , [CodigoUsuarioInclusao]
                    , [CodigoEntidade]
                    , [CodigoObjetoAssociado]
                    , [CodigoTipoAssociacao]
                    , [DataLimiteResposta]
                    , [Prioridade]
                    , [Assunto]
                )
            VALUES
                ( '{2}', GETDATE(), '{3}' , {4}, {5} , {6} , @CodigoTipoAssociacao, {7}, {10}, '{11}' )
         
             SELECT @CodigoMensagem = SCOPE_IDENTITY()
 
           {8}
        END
        ", bancodb, Ownerdb, mensagem.Replace("'", "''"), (respostaNecessaria) ? "S" : "N", codigoUsuarioInclusao, codigoEntidade
         , idProjeto, dateAsString, insereDestinatarios, iniciaisObjeto, prioridade == "" ? "''" : prioridade, assunto);
        int regAfetados = 0;
        try
        {
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;

    }
    public DataSet getAnalisePerformance(string codigoEntidade, string codigoObjetivoEstrategico, string ano, string mes)
    {
        comandoSQL = string.Format(@"
         SELECT [CodigoAnalisePerformance]
         ,[CodigoIndicador]
         ,[CodigoObjetivoEstrategico]
          ,[CodigoUnidadeNegocio]
          ,[CodigoPlanoAcao]
          ,[DataAnalisePerformance]
          ,[Recomendacoes]
          ,[CodigoUsuarioInclusao]
          ,[DataInclusao]
          ,[CodigoUsuarioUltimaAlteracao]
          ,[DataUltimaAlteracao]
          ,[CodigoUsuarioExclusao]
          ,[DataExclusao]
          ,[AnalisePerformance]
         FROM {0}.{1}.[AnalisePerformance] WHERE 
         CodigoUnidadeNegocio = {2} AND 
         CodigoObjetivoEstrategico = {3} AND
         YEAR(DataInclusao) = {4} AND
         MONTH(DataInclusao) = {5}
 ", bancodb, Ownerdb, codigoEntidade, codigoObjetivoEstrategico, ano, mes);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region _Estrategias - pdca - descricaoSimples.aspx
    public DataSet getMapaDaEntidade(int codigoEntidade)
    {
        string comandoSQL = string.Format(@"
                DECLARE @CodigoMapa INT

                SELECT  TOP 1 @CodigoMapa =  CodigoMapaEstrategico 
                FROM    {0}.{1}.MapaEstrategico

                WHERE   IndicaMapaEstrategicoAtivo  = 'S'
                  AND   CodigoUnidadeNegocio        = {2}

                ORDER BY CodigoMapaEstrategico DESC

                SELECT @CodigoMapa AS CodigoMapaDaEntidade
                ", bancodb, Ownerdb, codigoEntidade);

        return getDataSet(comandoSQL);
    }

    #endregion

    #region _Estrategias - Wizard - Indicadores.aspx

    public DataSet getIndicadoresOE(string where, int codigoObjeticoEstrategico, int codigoUnidadeSelecionada, string selectDePermissao)
    {
        string comandoSQL = string.Format(@"
                    SELECT  i.[CodigoIndicador]
                        ,   i.[NomeIndicador]
                        ,   ioe.[CodigoObjetivoEstrategico]		AS [CodigoObjetoEstrategia]
                        ,   {0}.{1}.f_GetUltimoDesempenhoIndicador({3}, i.[CodigoIndicador], YEAR(GETDATE()), MONTH(GETDATE()), 'A') AS [StatusDesempenho] 
                        ,   i.[IndicadorResultante]
                        ,   i.Polaridade
                        ,   ISNULL(lo.PesoObjetoLink,0) as PesoObjetoLink
                        {5}

                    FROM        {0}.{1}.[Indicador]                     AS [i]
                    INNER JOIN  {0}.{1}.[IndicadorObjetivoEstrategico]  AS [ioe]    ON (ioe.[CodigoIndicador]   = i.[CodigoIndicador])
                    LEFT JOIN   {0}.{1}.[LinkObjeto] as lo on (lo.CodigoObjetoLink = ioe.CodigoIndicador 
																	  and lo.CodigoTipoObjetoLink = dbo.f_GetCodigoTipoAssociacao('IN')
					                                                  and lo.CodigoTipoLink = (select CodigoTipoLink 
																	                            from TipoLinkObjeto 
																								where IniciaisTipoLink = 'AS')
																	  and lo.CodigoObjeto = ioe.CodigoObjetivoEstrategico
																	  and lo.CodigoTipoObjeto = dbo.f_GetCodigoTipoAssociacao('OB'))
                     WHERE       ioe.[CodigoObjetivoEstrategico] = {2}
                        AND     i.[DataExclusao]                IS NULL
                        {4}

                    ORDER BY i.[NomeIndicador]
                    ", bancodb, Ownerdb, codigoObjeticoEstrategico, codigoUnidadeSelecionada, where
                     , selectDePermissao.Equals("") ? "" : ", " + selectDePermissao);

        return getDataSet(comandoSQL);
    }

    public bool verificaPermissaoOE(int codigoObjetivo, int codigoEntidade)
    {
        string comandoSQL = string.Format(@"
                SELECT oe.CodigoMapaEstrategico 
                  FROM {0}.{1}.ObjetoEstrategia oe INNER JOIN
	                   {0}.{1}.MapaEstrategico me ON me.CodigoMapaEstrategico = oe.CodigoMapaEstrategico
                 WHERE oe.CodigoObjetoEstrategia = {2}
                   AND me.CodigoUnidadeNegocio = {3}
                ", bancodb, Ownerdb, codigoObjetivo, codigoEntidade);

        return getDataSet(comandoSQL).Tables[0].Rows.Count > 0;
    }

    public bool excluiIndicadoresOE(int codigoIndicador, int codigoObjetivo, ref int registrosAfetados, ref string codigoErro, ref string mensagemErro)
    {

        try
        {
            comandoSQL = string.Format(@"
BEGIN
	DECLARE @TranCounter INT;

	SET @TranCounter = @@TRANCOUNT;

	IF @TranCounter > 0
		SAVE TRANSACTION savePoint_nomedosavepoint;
	ELSE
		BEGIN TRANSACTION;

	BEGIN TRY
		DECLARE @ou_codigoRetorno INT
			,@ou_msgRetorno VARCHAR(4000)

		-- INCLUIR AQUI O BLOCO TSQL
		UPDATE {0}.{1}.ProjetoObjetivoEstrategico
		   SET CodigoIndicador = NULL
		WHERE CodigoIndicador = {2}
			AND CodigoObjetivoEstrategico = {3}
			
		DELETE
		FROM {0}.{1}.IndicadorObjetivoEstrategico
		WHERE CodigoIndicador = {2}
			AND CodigoObjetivoEstrategico = {3}

        DELETE
		FROM LinkObjeto
		WHERE CodigoObjeto = {3} and CodigoObjetoLink = {2} and
		CodigoTipoObjeto = (Select CodigoTipoAssociacao from TipoAssociacao where IniciaisTipoAssociacao = 'OB') and 
		CodigoTipoObjetoLink = (select CodigoTipoAssociacao from TipoAssociacao where IniciaisTipoAssociacao = 'IN') and 
		CodigoTipoLink = (select CodigoTipoLink from TipoLinkObjeto where IniciaisTipoLink = 'AS')

     -- Se for o caso, faz o COMMIT	
		IF (@@TRANCOUNT > 0)
        BEGIN 
			SET @ou_codigoRetorno = 0;
		    SET @ou_msgRetorno = '';
        END
  		COMMIT TRANSACTION;
		SELECT @ou_codigoRetorno as codigoRetorno, @ou_msgRetorno as msgRetorno
	END TRY

	BEGIN CATCH
		SET @ou_codigoRetorno = ERROR_NUMBER();
		SET @ou_msgRetorno = ERROR_MESSAGE();

		-- rollback transação caso a transação tenha sido criada aqui
		IF @TranCounter = 0
			ROLLBACK TRANSACTION;
		ELSE
		BEGIN
			-- se a transação foi criada antes e ainda continua válida, dá rollback até 
			-- o ponto criado nesta proc
			IF XACT_STATE() <> - 1
				ROLLBACK TRANSACTION savePoint_nomedosavepoint;
		END -- ELSE 

		SELECT @ou_codigoRetorno AS codigoRetorno
			,@ou_msgRetorno AS msgRetorno
	END CATCH;
END
               ", bancodb, Ownerdb, codigoIndicador, codigoObjetivo);

            DataSet ds = getDataSet(comandoSQL);
            codigoErro = ds.Tables[0].Rows[0]["codigoRetorno"].ToString();
            mensagemErro = ds.Tables[0].Rows[0]["msgRetorno"].ToString();

            return (int.Parse(codigoErro) == 0);
        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            return false;
        }
    }

    public bool incluiIndicadorOE(int codigoIndicador, int codigoObjetivo, ref int registrosAfetados)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(
            @"INSERT INTO {0}.{1}.IndicadorObjetivoEstrategico(CodigoIndicador, CodigoObjetivoEstrategico) VALUES
                                                               ({2}, {3})", bancodb, Ownerdb, codigoIndicador, codigoObjetivo);
            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);

        }
        return retorno;
    }

    public bool incluiIndicadorOEPeso(int codigoIndicador, int codigoObjetivo, string Peso, ref int registrosAfetados)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(
            @"INSERT INTO {0}.{1}.IndicadorObjetivoEstrategico(CodigoIndicador, CodigoObjetivoEstrategico) VALUES
                                                               ({2}, {3})", bancodb, Ownerdb, codigoIndicador, codigoObjetivo);
            execSQL(comandoSQL, ref registrosAfetados);

            comandoSQL = string.Format(
                                            @"INSERT INTO {0}.{1}.LinkObjeto(   CodigoObjeto,  --1
                                                                                CodigoTipoObjeto, --2
                                                                                CodigoObjetoLink, --3
                                                                                CodigoTipoObjetoLink, --4                                                                      
                                                                                CodigoTipoLink, --5                                           
                                                                                CodigoObjetoPai, --6
                                                                                CodigoObjetoPaiLink, --7
                                                                                PesoObjetoLink --8
                                                                            ) VALUES
                                                                                ({3}, --1
                                                                                (Select CodigoTipoAssociacao from TipoAssociacao where IniciaisTipoAssociacao = 'OB'), --2
                                                                                {2}, --3
                                                                                (select CodigoTipoAssociacao from TipoAssociacao where IniciaisTipoAssociacao = 'IN'), --4
                                                                                (select CodigoTipoLink from TipoLinkObjeto where IniciaisTipoLink = 'AS'), --5
                                                                                0, --6
                                                                                0, --7
                                                                                {4} --8
                                                                            )", bancodb, Ownerdb, codigoIndicador, codigoObjetivo, Peso);
            execSQL(comandoSQL, ref registrosAfetados);







            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);

        }
        return retorno;
    }


    public DataSet getObjetivoEstrategico(int? codigoUnidadeNegocio, int? codigoObjetivoEstrategico, string where)
    {
        if (codigoUnidadeNegocio.HasValue)
            where += " AND me.CodigoUnidadeNegocio = " + codigoUnidadeNegocio;
        if (codigoObjetivoEstrategico.HasValue)
            where += " AND obj.CodigoObjetoEstrategia = " + codigoObjetivoEstrategico;

        string comandoSQL = string.Format(@"
                SELECT      psp.CodigoMapaEstrategico
                        ,   psp.CodigoObjetoEstrategia AS CodigoPerspectiva
                        ,   psp.TituloObjetoEstrategia AS Perspectiva
                        ,   obj.CodigoObjetoEstrategia AS CodigoObjetivoEstrategico
                        ,   obj.DescricaoObjetoEstrategia
                        ,   obj.TituloObjetoEstrategia AS ObjetivoEstrategico
                        ,   psp.DescricaoObjetoEstrategia + ' - ' + obj.DescricaoObjetoEstrategia as PerspectivaObjetivo
                        ,   me.TituloMapaEstrategico
                        ,   tem.TituloObjetoEstrategia AS Tema
                        ,   u.NomeUsuario
                FROM        {0}.{1}.objetoEstrategia        AS psp
                INNER JOIN  {0}.{1}.objetoEstrategia        AS tem              ON (tem.CodigoObjetoEstrategiaSuperior  = psp.CodigoObjetoEstrategia)
                INNER JOIN  {0}.{1}.objetoEstrategia        AS obj      ON (obj.CodigoObjetoEstrategiaSuperior  = tem.CodigoObjetoEstrategia)
                INNER JOIN  {0}.{1}.TipoObjetoEstrategia    AS toePsp   ON (toepsp.CodigoTipoObjetoEstrategia   = psp.CodigoTipoObjetoEstrategia AND toepsp.IniciaisTipoObjeto = 'PSP')
                INNER JOIN  {0}.{1}.TipoObjetoEstrategia    AS toeObj   ON (toeObj.CodigoTipoObjetoEstrategia   = obj.CodigoTipoObjetoEstrategia AND toeObj.IniciaisTipoObjeto = 'OBJ')
                INNER JOIN  {0}.{1}.MapaEstrategico         AS me       ON (me.CodigoMapaEstrategico            = psp.CodigoMapaEstrategico)
                LEFT JOIN   {0}.{1}.Usuario                                           AS u                   ON (u.CodigoUsuario = obj.CodigoResponsavelObjeto)
                WHERE 1=1 
                  {2} 
                 UNION   
                 SELECT      psp.CodigoMapaEstrategico
                        ,   psp.CodigoObjetoEstrategia AS CodigoPerspectiva
                        ,   psp.TituloObjetoEstrategia AS Perspectiva
                        ,   obj.CodigoObjetoEstrategia AS CodigoObjetivoEstrategico
                        ,   obj.DescricaoObjetoEstrategia
                        ,   obj.TituloObjetoEstrategia AS ObjetivoEstrategico
                        ,   psp.DescricaoObjetoEstrategia + ' - ' + obj.DescricaoObjetoEstrategia as PerspectivaObjetivo
                        ,   me.TituloMapaEstrategico
                        ,   null
                        ,   u.NomeUsuario
                FROM        {0}.{1}.objetoEstrategia        AS psp 
                INNER JOIN  {0}.{1}.objetoEstrategia        AS obj      ON (obj.CodigoObjetoEstrategiaSuperior  = psp.CodigoObjetoEstrategia)
                INNER JOIN  {0}.{1}.TipoObjetoEstrategia    AS toePsp   ON (toepsp.CodigoTipoObjetoEstrategia   = psp.CodigoTipoObjetoEstrategia AND toepsp.IniciaisTipoObjeto = 'PSP')
                INNER JOIN  {0}.{1}.TipoObjetoEstrategia    AS toeObj   ON (toeObj.CodigoTipoObjetoEstrategia   = obj.CodigoTipoObjetoEstrategia AND toeObj.IniciaisTipoObjeto = 'OBJ')
                INNER JOIN  {0}.{1}.MapaEstrategico         AS me       ON (me.CodigoMapaEstrategico            = psp.CodigoMapaEstrategico)
                  LEFT JOIN   {0}.{1}.Usuario                                         AS u                   ON (u.CodigoUsuario = obj.CodigoResponsavelObjeto)
                WHERE 1=1 
                  {2}    
               ORDER BY obj.DescricaoObjetoEstrategia ASC
                ", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getTemaEstrategico(int codigoTema, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT      psp.CodigoMapaEstrategico
                        ,   psp.CodigoObjetoEstrategia AS CodigoPerspectiva
                        ,   psp.TituloObjetoEstrategia AS Perspectiva               
                        ,   me.TituloMapaEstrategico
                        ,   tem.TituloObjetoEstrategia AS Tema
                FROM        {0}.{1}.objetoEstrategia        AS psp
                INNER JOIN  {0}.{1}.objetoEstrategia        AS tem              ON (tem.CodigoObjetoEstrategiaSuperior  = psp.CodigoObjetoEstrategia)
                INNER JOIN  {0}.{1}.TipoObjetoEstrategia    AS toePsp   ON (toepsp.CodigoTipoObjetoEstrategia   = psp.CodigoTipoObjetoEstrategia AND toepsp.IniciaisTipoObjeto = 'PSP')
                INNER JOIN  {0}.{1}.MapaEstrategico         AS me       ON (me.CodigoMapaEstrategico            = psp.CodigoMapaEstrategico)
                WHERE tem.CodigoObjetoEstrategia = {2}
                  {3} 
                ", bancodb, Ownerdb, codigoTema, where);

        return getDataSet(comandoSQL);
    }
    public DataSet getProjetosDisponiveisObjetivo(int codigoEntidade, int codigoObjetivoEstrategico, string where)
    {

        string comandoSQL = string.Format(@"
                BEGIN
                    DECLARE     @CodigoObjetivo INT
                            ,   @CodigoEntidade INT
                            ,   @CodigoUnidade INT

                    SET @CodigoObjetivo = {3}
                    SET @CodigoEntidade = {2}

                    SELECT      p.CodigoProjeto
                            ,   p.NomeProjeto

                    FROM        {0}.{1}.Projeto AS p 
                    INNER JOIN  {0}.{1}.Status  AS st ON (st.CodigoStatus = p.CodigoStatusProjeto AND st.IndicaSelecaoPortfolio = 'S')

                    WHERE   p.DataExclusao          IS NULL
                        AND P.CodigoTipoProjeto     NOT IN(4,5) -- não e (demanda complexa, demanda simples).
                        AND p.CodigoEntidade        = @CodigoEntidade
                        AND p.CodigoProjeto         NOT IN(SELECT poe.CodigoProjeto 
                                                             FROM {0}.{1}.ProjetoObjetivoEstrategico AS poe
                                                             WHERE poe.CodigoObjetivoEstrategico = @CodigoObjetivo
                                                           )
                        AND  p.NomeProjeto != ''
                    ORDER BY p.NomeProjeto       
                END
                ", bancodb, Ownerdb, codigoEntidade, codigoObjetivoEstrategico, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getPeriodicidadeIndicadorPorMeta(int codigoUnidade, string descricaoMeta, string where)
    {
        string comandoSQL = string.Format(@"
            

-- consulta antigua : EXEC {0}.{1}.p_GetIndicadorPeriodicidade {2}, (SELECT CodigoIndicador FROM {0}.{1}IndicadorUnidade WHERE Meta LIKE '{3}')
            -- nova consulta :
            BEGIN
	        declare @codigoIndicador INT
            SELECT  @codigoIndicador =  CodigoIndicador FROM {0}.{1}.IndicadorUnidade WHERE Meta LIKE '{3}'  
            DECLARE @CodigoUsuarioInclusao INT
	            DECLARE @tbResumo TABLE(Periodo VarChar(50),
									            ValorRealizado decimal(18,4),
									            ValorPrevisto decimal(18,4),

									            CorIndicador VarChar(10),
									            Ano int,
									            mes int
                                        )
            													
	            INSERT INTO @tbResumo
		            EXEC {0}.{1}.p_GetIndicadorPeriodicidade {2}, @codigoIndicador, 'A'

	            SELECT      Periodo
                        ,   ValorRealizado
                        ,   ValorPrevisto

                        ,   CorIndicador
                        ,   Ano
                        ,   mes
                        ,   (SELECT     CodigoAnalisePerformance 
                                FROM    {0}.{1}.AnalisePerformance AS ap 
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = @codigoIndicador
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes) AS CodigoAnalisePerformance
                        ,   (SELECT     DataAnalisePerformance 
                                FROM    {0}.{1}.AnalisePerformance AS ap 
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = @codigoIndicador
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes) AS DataAnalisePerformance
                        ,   (SELECT     Recomendacoes 
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = @codigoIndicador
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes) AS Recomendacoes
                        ,   (SELECT     Analise 
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = @codigoIndicador
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes) AS Analise
                        ,   (SELECT     DataInclusao
                                FROM    {0}.{1}.AnalisePerformance ap 
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = @codigoIndicador
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes) AS DataInclusao
                        ,   (SELECT     CodigoUsuarioInclusao
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = @codigoIndicador
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes) AS CodigoUsuarioInclusao
                        ,   (SELECT     u.NomeUsuario
                                FROM        {0}.{1}.AnalisePerformance  AS ap 
                                INNER JOIN  {0}.{1}.Usuario             AS u ON (ap.CodigoUsuarioInclusao = u.CodigoUsuario)
                                WHERE ap.DataExclusao       IS NULL
                                    AND ap.CodigoIndicador  = @codigoIndicador
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes) AS NomeUsuario
                        ,   (SELECT     u.NomeUsuario
                                FROM        {0}.{1}.AnalisePerformance  AS ap 
                                INNER JOIN  {0}.{1}.Usuario             AS u ON (ISNULL(ap.CodigoUsuarioUltimaAlteracao, ap.CodigoUsuarioInclusao) = u.CodigoUsuario)
                                WHERE   ap.DataExclusao IS NULL
                                    AND ap.CodigoIndicador = @codigoIndicador
                                    AND ap.Ano = tb.Ano
                                    AND ap.Mes = tb.Mes) AS NomeUsuarioUltimaAlteracao
                        ,   (SELECT     CONVERT(Char(10), ISNULL(DataUltimaAlteracao, DataInclusao), 103)
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = @codigoIndicador
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes) AS DataUltimaAlteracaoFormatada
                        ,   (SELECT     ISNULL(DataUltimaAlteracao, DataInclusao)
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = @codigoIndicador
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes) AS DataUltimaAlteracao
                        ,   UPPER( {0}.{1}.f_GetNomePeriodoIndicador ( @codigoIndicador , tb.Ano, tb.Mes,1,4)) AS DetalhamentoPeriodo
	              FROM @tbResumo tb 
                 WHERE 1 = 1
                   {4}
            END
        ", bancodb, Ownerdb, codigoUnidade, descricaoMeta, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getPeriodicidadeIndicador(int codigoUnidade, int codigoIndicador, string where)
    {
        string comandoSQL = string.Format(@"
            -- consulta antigua : EXEC {0}.{1}.p_GetIndicadorPeriodicidade {2}, {3}
            -- nova consulta :
            BEGIN
	            DECLARE @CodigoUsuarioInclusao INT
	            DECLARE @tbResumo TABLE(Periodo VarChar(50),
									            ValorRealizado decimal(18,4),
									            ValorPrevisto decimal(18,4),

									            CorIndicador VarChar(10),
									            Ano int,
									            mes int
                                        )
            													
	            INSERT INTO @tbResumo
		            EXEC {0}.{1}.p_GetIndicadorPeriodicidade {2}, {3}, 'A'

	            SELECT      Periodo
                        ,   ValorRealizado
                        ,   ValorPrevisto

                        ,   CorIndicador
                        ,   Ano
                        ,   mes
                        ,   (SELECT     CodigoAnalisePerformance 
                                FROM    {0}.{1}.AnalisePerformance AS ap 
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = {3}
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoUnidadeNegocio = {2}) AS CodigoAnalisePerformance
                        ,   (SELECT     DataAnalisePerformance 
                                FROM    {0}.{1}.AnalisePerformance AS ap 
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = {3}
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoUnidadeNegocio = {2}) AS DataAnalisePerformance
                        ,   (SELECT     Recomendacoes 
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = {3}
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoUnidadeNegocio = {2}) AS Recomendacoes
                        ,   (SELECT     Analise 
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = {3}
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoUnidadeNegocio = {2}) AS Analise
                        ,   (SELECT     DataInclusao
                                FROM    {0}.{1}.AnalisePerformance ap 
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = {3}
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoUnidadeNegocio = {2}) AS DataInclusao
                        ,   (SELECT     CodigoUsuarioInclusao
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = {3}
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoUnidadeNegocio = {2}) AS CodigoUsuarioInclusao
                        ,   (SELECT     u.NomeUsuario
                                FROM        {0}.{1}.AnalisePerformance  AS ap 
                                INNER JOIN  {0}.{1}.Usuario             AS u ON (ap.CodigoUsuarioInclusao = u.CodigoUsuario)
                                WHERE ap.DataExclusao       IS NULL
                                    AND ap.CodigoIndicador  = {3}
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoUnidadeNegocio = {2}) AS NomeUsuario
                        ,   (SELECT     u.NomeUsuario
                                FROM        {0}.{1}.AnalisePerformance  AS ap 
                                INNER JOIN  {0}.{1}.Usuario             AS u ON (ISNULL(ap.CodigoUsuarioUltimaAlteracao, ap.CodigoUsuarioInclusao) = u.CodigoUsuario)
                                WHERE   ap.DataExclusao IS NULL
                                    AND ap.CodigoIndicador = {3}
                                    AND ap.Ano = tb.Ano
                                    AND ap.Mes = tb.Mes
                                    AND ap.CodigoUnidadeNegocio = {2}) AS NomeUsuarioUltimaAlteracao
                       ,      (SELECT      ISNULL(DataUltimaAlteracao, DataInclusao)
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = {3}
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoUnidadeNegocio = {2}) AS DataUltimaAlteracao
                        ,   (SELECT     CONVERT(Char(10), ISNULL(DataUltimaAlteracao, DataInclusao), 103)
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.CodigoIndicador  = {3}
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoUnidadeNegocio = {2}) AS DataUltimaAlteracaoFormatada
                        ,   CASE WHEN Exists(SELECT 1 
                                               FROM PeriodoEstrategia AS pe INNER JOIN
                                                    IndicadorUnidade AS iu ON (iu.CodigoIndicador = {3}) INNER JOIN
                                                    UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio
                                                                         AND un.CodigoEntidade = pe.CodigoUnidadeNegocio)
                                              WHERE pe.Ano = tb.Ano 
                                                AND pe.IndicaTipoDetalheVisualizacao = 'A') THEN CONVERT(Varchar,tb.Ano)
                                                                                           ELSE UPPER( dbo.f_GetNomePeriodoIndicador ( {3} , tb.Ano, tb.Mes,2,2)) END
 DetalhamentoPeriodo
                        ,   CAST(CASE WHEN dbo.f_GetDataInicioPeriodoIndicador({3}, tb.Ano, tb.mes) < GETDATE() THEN 'S' ELSE 'N' END AS char(1)) AS PeriodoPermitidoAnalise
	              FROM @tbResumo tb 
                 WHERE 1 = 1
                   {4}
                 order by tb.ano, tb.mes asc
                  
            END
        ", bancodb, Ownerdb, codigoUnidade, codigoIndicador, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getUltimaAnaliseIndicador(int codigoIndicador, char indicaTipoIndicador, string where)
    {
        string comandoSQL = string.Format(@"
            BEGIN 
	            DECLARE @DataUltimaAnalise DateTime,
		                @CodigoIndicador int,
                        @TipoIndicador Char(1)
            	
	            SET @CodigoIndicador = {2}
                SET @TipoIndicador = '{3}'

	            SELECT @DataUltimaAnalise = MAX(CONVERT(DateTime, '01/' + Convert(VarChar, Mes) + '/' + Convert(VarChar, Ano), 103))
	              FROM {0}.{1}.AnalisePerformance AS ap
	             WHERE ap.DataExclusao IS NULL
	               AND ap.CodigoIndicador = @CodigoIndicador
                   AND ap.IndicaTipoIndicador = @TipoIndicador
                   {4}            	    
            	                                    
	            SELECT Analise, CONVERT(VarChar(10), DataAnalisePerformance, 103) AS DataAnalise,
		               u.NomeUsuario AS Responsavel
	              FROM {0}.{1}.AnalisePerformance ap INNER JOIN
		               {0}.{1}.Usuario u ON u.CodigoUsuario = ap.CodigoUsuarioInclusao
	             WHERE Ano = YEAR(@DataUltimaAnalise)
	               AND Mes = MONTH(@DataUltimaAnalise)
	               AND ap.CodigoIndicador = @CodigoIndicador
                   AND ap.IndicaTipoIndicador = @TipoIndicador
                   {4}
            	   
            END
        ", bancodb, Ownerdb, codigoIndicador, indicaTipoIndicador, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getPeriodicidadeIndicadorGrafico(int codigoUnidade, int codigoIndicador, string tipo)
    {
        string comandoSQL = string.Format(@"
            EXEC {0}.{1}.p_GetIndicadorPeriodicidade {2}, {3}, '{4}'           
        ", bancodb, Ownerdb, codigoUnidade, codigoIndicador, tipo);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getDesempenhoIndicadorUF(int codigoIndicador, int codigoUsuario, int mes, int ano, char tipoDesempenho, string siglaUF, string where)
    {
        string comandoSQL = string.Format(@"SELECT f.Codigo AS CodigoUnidadeNegocio, f.CodigoUF, f.CorUF, f.Realizado, f.Meta, UF.SiglaUF, QtdUnidades, f.SiglaUnidade AS SiglaUnidadeNegocio, f.NomeUnidade AS NomeUnidadeNegocio
                                              FROM {0}.{1}.f_GetDesempenhoIndicadorUF({2}, '{5}', {4}, {3}, {7}, {6}) f INNER JOIN
	                                               {0}.{1}.UF ON UF.IdentificacaoUFMapa = f.CodigoUF 
                                             WHERE 1 = 1 
                                               {8}
                                             ", bancodb, Ownerdb, codigoIndicador, mes, ano, tipoDesempenho, codigoUsuario, siglaUF.ToUpper() == "NULL" ? "NULL" : "'" + siglaUF + "'", where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getDesempenhoIndicadorUnidade(int codigoIndicador, int mes, int ano, char tipoDesempenho, int codigoUnidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(@"BEGIN 
  DECLARE @CodigoIndicador Int,
          @CodigoUnidade   Int,
          @Ano      Int,
          @Mes      Smallint
          
  /* -- Parâmetros --*/
  SET @CodigoIndicador = {2}
  SET @CodigoUnidade   = {6}
  SET @Ano = {4}
  SET @Mes = {3}        
  
  DECLARE @Meta Decimal(25,4),
          @Resultado Decimal(25,4),
          @CorIndicador Varchar(15),          
          @Polaridade  Varchar(3)
         
  SELECT @Polaridade = Polaridade 
    FROM {0}.{1}.Indicador
   WHERE CodigoIndicador = @CodigoIndicador        
          
  SET @Meta = {0}.{1}.f_GetMetaAcumuladaIndicador(@CodigoUnidade, @CodigoIndicador, @Ano, @Mes)
  SET @Resultado = {0}.{1}.f_GetResultadoIndicador(@CodigoUnidade, @CodigoIndicador, 'A', @Ano, @Mes)
  SET @CorIndicador = {0}.{1}.f_GetCorDesempenhoResultado(@meta, @Resultado, @Polaridade, 'IN', @CodigoIndicador)
  
  SELECT @Meta as Meta, @Resultado AS Realizado, @CorIndicador
END
                                             ", bancodb, Ownerdb, codigoIndicador, mes, ano, tipoDesempenho, codigoUnidade, codigoUsuario, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public string getGraficoDesempenhoIndicador(DataTable dt, int fonte, string titulo, int casaDecimais, string polaridade, int codigoIndicador)
    {
        StringBuilder xml = new StringBuilder();

        float resultado = 0;
        float meta = 0;

        string strResultado = "";
        string strMeta = "";

        try
        {

            strResultado = dt.Rows[0]["Realizado"].ToString();
            strMeta = dt.Rows[0]["Meta"].ToString();
            if (!string.IsNullOrWhiteSpace(strResultado))
                resultado = float.Parse(strResultado);
            if (!string.IsNullOrWhiteSpace(strMeta))
                meta = float.Parse(strMeta);
        }
        catch
        {

        }

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1"" ";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        double limiteCritico, limiteAtencao, limiteSatisfatorio, limiteExcelente, limiteInferiorGrafico, limiteSuperiorGrafico;
        int retProc;

        retProc = getValoresGraficoGaugeIndicador(codigoIndicador, ref polaridade, meta, resultado, casaDecimais, out limiteInferiorGrafico, out limiteSuperiorGrafico, out limiteCritico, out limiteAtencao, out limiteSatisfatorio, out limiteExcelente);

        string strLimiteSuperior = limiteSuperiorGrafico.ToString().Replace(",", ".");
        string strLimiteInferior = limiteInferiorGrafico.ToString().Replace(",", ".");

        //configuração geral do gráfico
        xml.Append("<chart upperLimit=\"" + strLimiteSuperior + "\" showValue=\"1\" decimals=\"" + casaDecimais + "\" decimalSeparator=\",\" " +
                            "inDecimalSeparator=\",\" baseFontSize=\"" + fonte + "\"  " + exportar +
                            "bgColor=\"FFFFFF\" showBorder=\"0\" chartTopMargin=\"5\" " +
                            "chartBottomMargin=\"20\" lowerLimit=\"" + strLimiteInferior + "\" " +
                            "gaugeFillRatio=\"\" inThousandSeparator=\".\" thousandSeparator=\".\">");
        xml.Append(" <colorRange>");

        if (polaridade == "NEG")
        {
            xml.Append(" <color maxValue=\"" + strLimiteSuperior + "\" minValue=\"" + limiteCritico + "\" name=\"\" code=\"" + corCritico + "\" />");
            xml.Append(" <color maxValue=\"" + limiteCritico + "\" minValue=\"" + limiteAtencao + "\" name=\"\" code=\"" + corAtencao + "\" />");
            xml.Append(" <color maxValue=\"" + limiteAtencao + "\" minValue=\"" + limiteSatisfatorio + "\" name=\"\" code=\"" + corSatisfatorio + "\" />");
            xml.Append(" <color maxValue=\"" + limiteSatisfatorio + "\" minValue=\"" + limiteExcelente + "\" name=\"\" code=\"" + corExcelente + "\" />");
        }
        else
        {
            xml.Append(" <color minValue=\"" + strLimiteInferior + "\" maxValue=\"" + limiteCritico + "\" name=\"Ruim\" code=\"" + corCritico + "\" />");
            xml.Append(" <color minValue=\"" + limiteCritico + "\" maxValue=\"" + limiteAtencao + "\" name=\"Regular\" code=\"" + corAtencao + "\" />");
            xml.Append(" <color minValue=\"" + limiteAtencao + "\" maxValue=\"" + limiteSatisfatorio + "\" name=\"Bom\" code=\"" + corSatisfatorio + "\" />");
            xml.Append(" <color minValue=\"" + limiteSatisfatorio + "\" maxValue=\"" + limiteExcelente + "\" name=\"Excelente\" code=\"" + corExcelente + "\" />");
        }

        xml.Append(" </colorRange>");
        xml.Append(" <dials>");
        xml.Append(" <dial value=\"" + (resultado) + "\"/>");
        xml.Append(" </dials>");
        xml.Append(" <trendpoints>");
        xml.Append(" <point startValue=\"" + meta.ToString().Replace(',', '.') + "\" markerTooltext=\"Meta = " + meta + "\" useMarker=\"1\" dashed=\"1\" dashLen=\"2\" dashGap=\"2\" valueInside=\"1\"/>");
        xml.Append(" </trendpoints>");
        if (titulo != "")
        {
            xml.Append(string.Format(@"<annotations origW=""890"" origH=""120"" constrainedScale='0'>
                                              <annotationGroup id=""Grp1"" autoScale=""1"">
                                                 <annotation type=""text"" font=""Verdana"" bold=""1"" fontSize=""15"" fontColor=""000000"" align=""center"" x=""435"" y=""20"" label=""{0}"" />
                                              </annotationGroup>
                                           </annotations>
                                        ", titulo));
        }
        xml.Append("</chart>");

        return xml.ToString();
    }

    public string getGraficoDesempenhoGeralIndicador(DataTable dt, int fonte, string titulo, int casaDecimais, string polaridade, int codigoIndicador)
    {
        StringBuilder xml = new StringBuilder();

        float resultado = 0;
        float meta = 0;

        string resutadoStr = "";
        string metaStr = "";

        try
        {

            resutadoStr = dt.Rows[0]["Realizado"].ToString();
            metaStr = dt.Rows[0]["Meta"].ToString();
            if ((resutadoStr != "") && (metaStr != ""))
            {
                resultado = float.Parse(resutadoStr);
                meta = float.Parse(metaStr);
            }
            if (!string.IsNullOrWhiteSpace(resutadoStr))
                resultado = float.Parse(resutadoStr);
            if (!string.IsNullOrWhiteSpace(metaStr))
                meta = float.Parse(metaStr);
        }
        catch
        {

        }

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1"" ";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        double limiteCritico, limiteAtencao, limiteSatisfatorio, limiteExcelente, limiteInferiorGrafico, limiteSuperiorGrafico;
        int retProc;

        retProc = getValoresGraficoGaugeIndicador(codigoIndicador, ref polaridade, meta, resultado, casaDecimais, out limiteInferiorGrafico, out limiteSuperiorGrafico, out limiteCritico, out limiteAtencao, out limiteSatisfatorio, out limiteExcelente);

        string strLimiteSuperior = limiteSuperiorGrafico.ToString().Replace(",", ".");
        string strLimiteInferior = limiteInferiorGrafico.ToString().Replace(",", ".");

        xml.Append("<chart upperLimit=\"" + strLimiteSuperior + "\" showValue=\"1\" decimals=\"" + casaDecimais + "\" decimalSeparator=\".\" " +
                            "inDecimalSeparator=\",\" baseFontSize=\"" + fonte + "\"  " + exportar +
                            "bgColor=\"FFFFFF\" showBorder=\"0\" chartTopMargin=\"1\" minorTMNumber=\"4\" chartRightMargin=\"40\" placeTicksInside=\"1\" " +
                            "chartBottomMargin=\"1\" lowerLimit=\"" + strLimiteInferior + "\" pointerRadius=\"6\" majorTMNumber=\"3\" tickValueDistance=\"3\" tickMarkDistance=\"0\" " +
                            "gaugeFillRatio=\"\" inThousandSeparator=\".\" thousandSeparator=\".\" showGaugeLabels=\"0\" >");
        xml.Append(" <colorRange>");

        if (polaridade == "NEG")
        {
            xml.Append(" <color maxValue=\"" + strLimiteSuperior + "\" minValue=\"" + limiteCritico + "\" name=\"\" code=\"" + corCritico + "\" />");
            xml.Append(" <color maxValue=\"" + limiteCritico + "\" minValue=\"" + limiteAtencao + "\" name=\"\" code=\"" + corAtencao + "\" />");
            xml.Append(" <color maxValue=\"" + limiteAtencao + "\" minValue=\"" + limiteSatisfatorio + "\" name=\"\" code=\"" + corSatisfatorio + "\" />");
            xml.Append(" <color maxValue=\"" + limiteSatisfatorio + "\" minValue=\"" + limiteExcelente + "\" name=\"\" code=\"" + corExcelente + "\" />");
        }
        else
        {
            xml.Append(" <color minValue=\"" + strLimiteInferior + "\" maxValue=\"" + limiteCritico + "\" name=\"Ruim\" code=\"" + corCritico + "\" />");
            xml.Append(" <color minValue=\"" + limiteCritico + "\" maxValue=\"" + limiteAtencao + "\" name=\"Regular\" code=\"" + corAtencao + "\" />");
            xml.Append(" <color minValue=\"" + limiteAtencao + "\" maxValue=\"" + limiteSatisfatorio + "\" name=\"Bom\" code=\"" + corSatisfatorio + "\" />");
            xml.Append(" <color minValue=\"" + limiteSatisfatorio + "\" maxValue=\"" + limiteExcelente + "\" name=\"Excelente\" code=\"" + corExcelente + "\" />");
        }


        xml.Append(" </colorRange>");
        xml.Append("<value>" + resultado + "</value>");
        xml.Append(" <trendpoints showOnTop=\"0\">");
        xml.Append(" <point displayValue=\" \" startValue=\"" + meta.ToString().Replace(',', '.') + "\" markerTooltext=\"Meta = " + meta + "\" showOnTop=\"0\" useMarker=\"1\" dashed=\"1\" thickness=\"3\" dashLen=\"3\" dashGap=\"3\"/>");
        xml.Append(" </trendpoints>");
        xml.Append("</chart>");

        return xml.ToString();
    }

    public DataSet getComparacaoIndicador(int codigoUnidade, int codigoIndicador, int mes, int ano, string where)
    {
        string comandoSQL = string.Format(@"SELECT Unidade, ISNULL(ValorMinimo, 0) AS ValorMinimo, ISNULL(ValorMedio, 0) AS ValorMedio, ISNULL(ValorMaximo, 0) AS ValorMaximo, TipoRegistro  
                                            FROM {0}.{1}.f_GetComparacaoIndicador({2}, {3}, {5}, {4})
                                             ", bancodb, Ownerdb, codigoUnidade, codigoIndicador, mes, ano, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public string geraXMLMapasCores(DataTable dt, int fonte, bool habilitaLink, int casasDecimais)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        //Configuração GERAL do gráfico
        xml.Append(string.Format(@"<map  showLabels=""0"" showLegend=""1"" showMarkerLabels=""1"" borderColor=""000000"" mapBottomMargin=""{0}"" mapTopMargin=""{1}""  
                        mapRightMargin=""{2}"" mapLeftMargin=""{3}"" showCanvasBorder=""0"" useHoverColor=""0"" useSNameInToolTip=""1""
                        baseFontSize=""{4}"" legendPosition=""{5}"" connectorColor=""000000"" fillColor=""FFFFFF""
                        fillAlpha=""100"" showBevel=""0"" showShadow=""0"" legendPadding=""5"" legendAllowDrag=""1"">",
                                                                                                   5,        // {0} Margem Inferior
                                                                                                   5,        // {1} Margem Superior
                                                                                                   5,        // {2} Margem Direita
                                                                                                   5,        // {3} Margem Esquerda
                                                                                                   fonte,        // {4} Tamanho da Fonte
                                                                                                   "BOTTOM"));
        xml.Append(string.Format(@"<colorRange>
                            <color minValue=""31"" maxValue=""40"" displayValue=""Meta Superada   "" color=""{0}"" />
		                    <color minValue=""0"" maxValue=""10"" displayValue=""Meta Atingida   "" color=""{1}"" />
	                        <color minValue=""11"" maxValue=""20"" displayValue=""Abaixo da Meta(Alerta)   "" color=""{2}"" />
        	                <color minValue=""21"" maxValue=""30"" displayValue=""Muito Abaixo da Meta   "" color=""{3}"" />
                            <color minValue=""41"" maxValue=""50"" displayValue=""Atualização Pendente"" color=""8A8A8A"" />
	                </colorRange>", corExcelente
                                  , corSatisfatorio
                                  , corAtencao
                                  , corCritico
                                  ));

        //Definição dos valores para cada estado brasileiro
        xml.Append("<data>");

        string valor;

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            switch (dt.Rows[i]["CorUF"].ToString().ToUpper())
            {
                case "VERDE":
                    valor = "5";
                    break;
                case "AMARELO":
                    valor = "15";
                    break;
                case "VERMELHO":
                    valor = "25";
                    break;
                case "AZUL":
                    valor = "35";
                    break;
                default:
                    valor = "45";
                    break;

            }

            string toolTip = string.Format(@"{0}", dt.Rows[i]["SiglaUF"].ToString()) +
                             string.Format("{1}Meta: {0:n" + casasDecimais + "}", float.Parse(dt.Rows[i]["Meta"].ToString()), "{br}") +
                             string.Format("{1}Realizado: {0:n" + casasDecimais + "}", float.Parse(dt.Rows[i]["Realizado"].ToString()), "{br}");

            string link = "";

            if (habilitaLink)
            {
                int qtdUnidades = int.Parse(dt.Rows[i]["QtdUnidades"].ToString());

                string funcaoClick = qtdUnidades > 1 ? "abreGridUnidades('" + dt.Rows[i]["SiglaUF"].ToString() + "')" : "atualizaUnidade(" + dt.Rows[i]["CodigoUnidadeNegocio"].ToString() + ")";

                link = string.Format(@"link=""JavaScript:{0}""", funcaoClick);
            }

            xml.Append(string.Format(@"<entity id=""{0}"" toolText=""{1}""  alpha=""100"" value=""{2}"" {3}/>", dt.Rows[i]["CodigoUf"].ToString(),
                toolTip, valor, link));
        }
        xml.Append("</data>");

        //Estilos do gráfico
        xml.Append("<styles>");
        xml.Append("<definition>");

        //Configuração da animação do gráfico
        xml.Append(@"<style type=""animation"" name=""animX"" param=""_xscale"" start=""0"" duration=""0.6"" />");
        xml.Append(@"<style type=""animation"" name=""animY"" param=""_yscale"" start=""0"" duration=""0.6"" />");
        xml.Append(@"<style name=""myHTMLFon"" type=""font"" isHTML=""1""/>");
        xml.Append("</definition>");
        xml.Append("<application>");
        xml.Append(@"<apply toObject=""PLOT"" styles=""animX,animY""/>");
        xml.Append(@"<apply toObject=""TOOLTIP"" styles=""myHTMLFon""/>");
        xml.Append("</application>");
        xml.Append("</styles>");
        xml.Append("</map>");

        return xml.ToString();
    }


    public string getGraficoPeriodosIndicador(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot)
    {

        DataSet ds = getParametrosSistema("ordemApresentacaoGraficoIndicador");
        bool indicaGraficoOrdemAscendente = true;
        indicaGraficoOrdemAscendente = (DataSetOk(ds) && DataTableOk(ds.Tables[0])) &&
            !(ds.Tables[0].Rows[0]["ordemApresentacaoGraficoIndicador"].ToString() == "Descendente");

        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        int i = 0;

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}""  baseFontSize=""{2}"" chartBottomMargin=""0"" chartLeftMargin=""1"" chartRightMargin=""1"" chartTopMargin=""3"" canvasBgColor=""F7F7F7"" canvasLeftMargin=""0"" canvasRightMargin=""20"" canvasBottomMargin=""0""
                                    numDivLines=""{4}"" showvalues=""1""  numVisiblePlot=""{5}"" scrollToEnd=""0"" showBorder=""0"" BgColor=""F7F7F7"" slantLabels=""1"" labelDisplay=""NONE"" rotateLabels=""1"" labelPadding=""0""
                                    inDecimalSeparator="","" scrollHeight=""{3}"" showLegend=""0"" decimals=""{6}"" inThousandSeparator=""."" decimalSeparator="",""  thousandSeparator=""."" >", usarGradiente + usarBordasArredondadas + exportar
                                                                                                                                                          , titulo
                                                                                                                                                          , fonte
                                                                                                                                                          , scrollHeight
                                                                                                                                                          , numDivLines
                                                                                                                                                          , numVisiblePlot
                                                                                                                                                          , casasDecimais));
        xml.Append(string.Format(@"<categories>"));

        try
        {
            for (i = (indicaGraficoOrdemAscendente == true) ? 0 : dt.Rows.Count - 1; (indicaGraficoOrdemAscendente == true) ? i < dt.Rows.Count : i >= 0; i = (indicaGraficoOrdemAscendente == true) ? i + 1 : i - 1)
            {
                xml.Append(string.Format(@"<category label=""{0}"" />", dt.Rows[i]["Periodo"].ToString()));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));
        xml.Append(string.Format(@"<dataset seriesName=""Meta"" valuePosition=""ABOVE""  anchorRadius=""4"" showValues=""1"" renderAs=""Line"">"));

        try
        {
            for (i = (indicaGraficoOrdemAscendente == true) ? 0 : dt.Rows.Count - 1; (indicaGraficoOrdemAscendente == true) ? i < dt.Rows.Count : i >= 0; i = (indicaGraficoOrdemAscendente == true) ? i + 1 : i - 1)
            {
                string valorMeta = dt.Rows[i]["ValorMeta"].ToString();
                string valorResultado = dt.Rows[i]["ValorRealizado"].ToString();

                string posicao = "";

                if (valorMeta != "" && valorResultado != "")
                {
                    if (double.Parse(valorMeta) <= double.Parse(valorResultado))
                        posicao = @"valuePosition=""BELOW""";
                    else
                        posicao = @"valuePosition=""ABOVE""";
                }

                string displayMeta = valorMeta == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorMeta));
                string traduzMeta = Resources.traducao.meta;
                xml.Append(string.Format(@"<set value=""{0}"" toolText=""{3}: {1}"" {2}/> ", valorMeta == "" ? "0" : valorMeta, displayMeta, posicao, traduzMeta));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Resultado"" showValues=""1"">"));

        try
        {
            for (i = (indicaGraficoOrdemAscendente == true) ? 0 : dt.Rows.Count - 1; (indicaGraficoOrdemAscendente == true) ? i < dt.Rows.Count : i >= 0; i = (indicaGraficoOrdemAscendente == true) ? i + 1 : i - 1)
            {
                string corBarra;

                switch (dt.Rows[i]["CorIndicador"].ToString().ToLower())
                {
                    case "verde":
                        corBarra = corSatisfatorio;
                        break;
                    case "amarelo":
                        corBarra = corAtencao;
                        break;
                    case "vermelho":
                        corBarra = corCritico;
                        break;
                    case "azul":
                        corBarra = corExcelente;
                        break;
                    default:
                        corBarra = "FFFFFF";
                        break;
                }

                string valorMeta = dt.Rows[i]["ValorMeta"].ToString();
                string valorResultado = dt.Rows[i]["ValorRealizado"].ToString();

                string posicao = "";

                if (valorMeta != "" && valorResultado != "")
                {
                    if (double.Parse(valorMeta) > double.Parse(valorResultado))
                        posicao = @"valuePosition=""BELOW""";
                    else
                        posicao = @"valuePosition=""ABOVE""";
                }

                string displayResultado = valorResultado == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorResultado));
                string traduzResultado = Resources.traducao.resultado;
                xml.Append(string.Format(@"<set value=""{0}"" toolText=""{4}: {1}"" color=""{2}"" {3}/> ", valorResultado == "" ? "0" : valorResultado, displayResultado, corBarra, posicao, traduzResultado));

            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Verdana\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("</definition>");
        xml.Append("<application>");
        xml.Append("<apply toObject=\"DATALABELS\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"DATAVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"XAXISNAME \" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISNAME\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"TOOLTIP\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"Caption\" styles=\"fontTitulo\" />");
        xml.Append("</application>");
        xml.Append("</styles>");
        xml.Append("</chart>");


        return xml.ToString();
    }

    public string getGraficoPeriodosIndicadorMetas(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot, string codigoMeta)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        int i = 0;

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""2"" chartTopMargin=""3"" canvasBgColor=""F7F7F7"" canvasBottomMargin=""2""
                                    numDivLines=""{4}"" showvalues=""0""  numVisiblePlot=""{5}"" scrollToEnd=""1"" showBorder=""0"" BgColor=""F7F7F7"" labelPadding=""1"" slantLabels=""1""
                                    inDecimalSeparator="","" scrollHeight=""{3}"" showLegend=""1"" inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" >", usarGradiente + usarBordasArredondadas + exportar
                                                                                                                                                          , titulo
                                                                                                                                                          , fonte
                                                                                                                                                          , scrollHeight
                                                                                                                                                          , numDivLines
                                                                                                                                                          , numVisiblePlot));
        xml.Append(string.Format(@"<categories>"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<category label=""{0}"" />", dt.Rows[i]["DetalhamentoPeriodo"].ToString()));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));
        xml.Append(string.Format(@"<dataset seriesName=""Meta"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valorMeta = dt.Rows[i]["ValorPrevisto"].ToString();
                string ano = dt.Rows[i]["Ano"].ToString();
                string mes = dt.Rows[i]["Mes"].ToString();

                string link = "";

                if (dt.Rows[i]["Analise"].ToString() != "" || dt.Rows[i]["Recomendacoes"].ToString() != "")
                    link = string.Format(@"link=""JavaScript:abreAnalise({0}, {1}, {2});""", ano, mes, codigoMeta);


                string displayMeta = valorMeta == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorMeta));
                xml.Append(string.Format(@"<set value=""{0}"" {2} toolText=""Meta: {1}""/> ", valorMeta == "" ? "0" : valorMeta, displayMeta, link));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Resultado"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {

                string valorResultado = dt.Rows[i]["ValorRealizado"].ToString();
                int ano = int.Parse(dt.Rows[i]["Ano"].ToString());
                int mes = int.Parse(dt.Rows[i]["Mes"].ToString());

                string link = "";

                if (dt.Rows[i]["Analise"].ToString() != "" || dt.Rows[i]["Recomendacoes"].ToString() != "")
                    link = string.Format(@"link=""JavaScript:abreAnalise({0}, {1}, {2});""", ano, mes, codigoMeta);

                string displayResultado = valorResultado == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorResultado));

                //if (ano > DateTime.Now.Year || (ano == DateTime.Now.Year && mes > DateTime.Now.Month))
                //    valorResultado = "";
                //else
                //    valorResultado = valorResultado == "" ? "0" : valorResultado;

                xml.Append(string.Format(@"<set value=""{0}"" {2} toolText=""Resultado: {1}"" /> ", valorResultado, displayResultado, link));

            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Verdana\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("</definition>");
        xml.Append("<application>");
        xml.Append("<apply toObject=\"DATALABELS\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"DATAVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"XAXISNAME \" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISNAME\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"TOOLTIP\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"Caption\" styles=\"fontTitulo\" />");
        xml.Append("</application>");
        xml.Append("</styles>");
        xml.Append("</chart>");


        return xml.ToString();
    }

    public string getGraficoPeriodosIndicadorMetasComLabel(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot, string codigoMeta)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        int i = 0;

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""2"" chartTopMargin=""3"" canvasBgColor=""F7F7F7"" canvasBottomMargin=""2""
                                    numDivLines=""{4}"" showvalues=""1""  numVisiblePlot=""{5}"" scrollToEnd=""1"" showBorder=""0"" BgColor=""F7F7F7"" labelPadding=""1"" slantLabels=""1""
                                    inDecimalSeparator="","" scrollHeight=""{3}"" showLegend=""1"" inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" >", usarGradiente + usarBordasArredondadas + exportar
                                                                                                                                                          , titulo
                                                                                                                                                          , fonte
                                                                                                                                                          , scrollHeight
                                                                                                                                                          , numDivLines
                                                                                                                                                          , numVisiblePlot));
        xml.Append(string.Format(@"<categories>"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<category label=""{0}"" />", dt.Rows[i]["DetalhamentoPeriodo"].ToString()));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));
        xml.Append(string.Format(@"<dataset seriesName=""Meta"" showValues=""1"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valorMeta = dt.Rows[i]["ValorPrevisto"].ToString();
                string ano = dt.Rows[i]["Ano"].ToString();
                string mes = dt.Rows[i]["Mes"].ToString();

                string link = "";

                if (dt.Rows[i]["Analise"].ToString() != "" || dt.Rows[i]["Recomendacoes"].ToString() != "")
                    link = string.Format(@"link=""JavaScript:abreAnalise({0}, {1}, {2});""", ano, mes, codigoMeta);


                string displayMeta = valorMeta == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorMeta));
                xml.Append(string.Format(@"<set value=""{0}"" {2} toolText=""Meta: {1}""/> ", valorMeta == "" ? "0" : valorMeta, displayMeta, link));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Resultado"" showValues=""1"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {

                string valorResultado = dt.Rows[i]["ValorRealizado"].ToString();
                int ano = int.Parse(dt.Rows[i]["Ano"].ToString());
                int mes = int.Parse(dt.Rows[i]["Mes"].ToString());

                string link = "";

                if (dt.Rows[i]["Analise"].ToString() != "" || dt.Rows[i]["Recomendacoes"].ToString() != "")
                    link = string.Format(@"link=""JavaScript:abreAnalise({0}, {1}, {2});""", ano, mes, codigoMeta);

                string displayResultado = valorResultado == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorResultado));

                //if (ano > DateTime.Now.Year || (ano == DateTime.Now.Year && mes > DateTime.Now.Month))
                //    valorResultado = "";
                //else
                //    valorResultado = valorResultado == "" ? "0" : valorResultado;

                xml.Append(string.Format(@"<set value=""{0}"" {2} toolText=""Resultado: {1}"" /> ", valorResultado, displayResultado, link));

            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Verdana\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("</definition>");
        xml.Append("<application>");
        xml.Append("<apply toObject=\"DATALABELS\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"DATAVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"XAXISNAME \" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISNAME\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"TOOLTIP\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"Caption\" styles=\"fontTitulo\" />");
        xml.Append("</application>");
        xml.Append("</styles>");
        xml.Append("</chart>");


        return xml.ToString();
    }


    public string getGraficoPeriodosIndicadorMetas2(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot, string codigoMeta, string codigoUnidadeNegocio)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        int i = 0;

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""2"" chartTopMargin=""3"" canvasBgColor=""F7F7F7"" canvasBottomMargin=""2""
                                    numDivLines=""{4}"" showvalues=""1""  numVisiblePlot=""{5}"" scrollToEnd=""1"" showBorder=""0"" BgColor=""F7F7F7"" labelPadding=""1"" slantLabels=""1""
                                    inDecimalSeparator="","" scrollHeight=""{3}"" showLegend=""1"" inThousandSeparator=""."" decimals=""{6}"" decimalSeparator="","" thousandSeparator=""."" valuePadding=""5"" >", usarGradiente + usarBordasArredondadas + exportar
                                                                                                                                                          , titulo
                                                                                                                                                          , fonte
                                                                                                                                                          , scrollHeight
                                                                                                                                                          , numDivLines
                                                                                                                                                          , numVisiblePlot
                                                                                                                                                          , casasDecimais));
        xml.Append(string.Format(@"<categories>"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<category label=""{0}"" />", dt.Rows[i]["DetalhamentoPeriodo"].ToString()));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));
        xml.Append(string.Format(@"<dataset seriesName=""Meta"" showValues=""1"" renderAs=""Line"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valorMeta = dt.Rows[i]["ValorPrevisto"].ToString();
                string valorResultado = dt.Rows[i]["ValorRealizado"].ToString();
                string ano = dt.Rows[i]["Ano"].ToString();
                string mes = dt.Rows[i]["Mes"].ToString();

                string link = "";

                if (dt.Rows[i]["Analise"].ToString() != "" || dt.Rows[i]["Recomendacoes"].ToString() != "")
                    link = string.Format(@"link=""JavaScript:abreAnalise({0}, {1}, {2}, {3});""", ano, mes, codigoMeta, codigoUnidadeNegocio);


                string displayMeta = valorMeta == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorMeta));

                string posicao = "";

                if (valorMeta != "" && valorResultado != "")
                {
                    if (double.Parse(valorMeta) <= double.Parse(valorResultado))
                        posicao = @"valuePosition=""BELOW""";
                    else
                        posicao = @"valuePosition=""ABOVE""";
                }

                xml.Append(string.Format(@"<set anchorRadius='8' anchorSides='20' anchorBorderColor='CCCCCC' value=""{0}"" {2} toolText=""Meta: {1}"" {3}/> ", valorMeta == "" ? "0" : valorMeta, displayMeta, link, posicao));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Resultado"" showValues=""1"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {

                string valorMeta = dt.Rows[i]["ValorPrevisto"].ToString();
                string valorResultado = dt.Rows[i]["ValorRealizado"].ToString();
                int ano = int.Parse(dt.Rows[i]["Ano"].ToString());
                int mes = int.Parse(dt.Rows[i]["Mes"].ToString());

                string link = "";

                if (dt.Rows[i]["Analise"].ToString() != "" || dt.Rows[i]["Recomendacoes"].ToString() != "")
                    link = string.Format(@"link=""JavaScript:abreAnalise({0}, {1}, {2}, {3});""", ano, mes, codigoMeta, codigoUnidadeNegocio);

                string displayResultado = valorResultado == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorResultado));
                string posicao = "";

                if (valorMeta != "" && valorResultado != "")
                {
                    if (double.Parse(valorMeta) > double.Parse(valorResultado))
                        posicao = @"valuePosition=""BELOW""";
                    else
                        posicao = @"valuePosition=""ABOVE""";
                }

                //if (ano > DateTime.Now.Year || (ano == DateTime.Now.Year && mes > DateTime.Now.Month))
                //    valorResultado = "";
                //else
                //    valorResultado = valorResultado == "" ? "0" : valorResultado;

                xml.Append(string.Format(@"<set value=""{0}"" {2} toolText=""Resultado: {1}"" {3} /> ", valorResultado, displayResultado, link, posicao));

            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Verdana\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("</definition>");
        xml.Append("<application>");
        xml.Append("<apply toObject=\"DATALABELS\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"DATAVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"XAXISNAME \" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISNAME\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"TOOLTIP\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"Caption\" styles=\"fontTitulo\" />");
        xml.Append("</application>");
        xml.Append("</styles>");
        xml.Append("</chart>");


        return xml.ToString();
    }

    public string getGraficoComparacaoIndicador(DataTable dt, string titulo, int fonte, string tipoRegistro, int casasDecimais)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();


        try
        {
            string exportar = "";

            if (fonte > 12)
            {
                exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
            }
            else
            {
                exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
            }

            //configuração geral do gráfico
            xml.Append(string.Format(@"<chart caption=""{0}"" canvasBgColor=""F7F7F7"" showBorder=""0"" BgColor=""F7F7F7"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" 
                 chartTopMargin=""5"" chartRightMargin=""8"" inThousandSeparator=""."" thousandSeparator=""."" chartBottomMargin=""2"" chartLeftMargin=""4"" 
                 adjustDiv=""1""  slantLabels=""1"" labelDisplay=""ROTATE"" inDecimalSeparator="","" decimalSeparator="",""
                 showValues=""1"" {1} {2} showShadow=""0"" showLabels=""0"" decimals=""{4}"" baseFontSize=""{3}"">", titulo, usarGradiente + usarBordasArredondadas, exportar, fonte, casasDecimais));

            xml.Append(@"<categories>
                          <category label=""Mínimo"" /> 
                          <category label=""Médio"" /> 
                          <category label=""Máximo"" /> 
                        </categories>");

            string descricao = "";
            double valorMin = 0, valorMed = 0, valorMax = 0;

            DataRow[] dr = dt.Select("TipoRegistro = '" + tipoRegistro + "'");

            if (dr.Length > 0)
            {
                descricao = dr[0]["Unidade"].ToString();
                valorMin = double.Parse(dr[0]["ValorMinimo"].ToString());
                valorMed = double.Parse(dr[0]["ValorMedio"].ToString());
                valorMax = double.Parse(dr[0]["ValorMaximo"].ToString());

                xml.Append(string.Format(@"<dataset seriesName=""{0}"" >", descricao));

                xml.Append(string.Format(@"<set label=""Mínimo - {0}"" value=""{1}""/>", descricao, valorMin));
                xml.Append(string.Format(@"<set label=""Médio - {0}"" value=""{1}""/>", descricao, valorMed));
                xml.Append(string.Format(@"<set label=""Máximo - {0}"" value=""{1}""/>", descricao, valorMax));

                xml.Append("</dataset>");
            }

            dr = dt.Select("TipoRegistro = 'UF'");

            if (dr.Length > 0)
            {
                descricao = dr[0]["Unidade"].ToString();
                valorMin = double.Parse(dr[0]["ValorMinimo"].ToString());
                valorMed = double.Parse(dr[0]["ValorMedio"].ToString());
                valorMax = double.Parse(dr[0]["ValorMaximo"].ToString());

                xml.Append(string.Format(@"<dataset seriesName=""{0}"" renderAs=""Line"" >", descricao));

                xml.Append(string.Format(@"<set label=""Mínimo - {0}"" value=""{1}""/>", descricao, valorMin));
                xml.Append(string.Format(@"<set label=""Médio - {0}"" value=""{1}""/>", descricao, valorMed));
                xml.Append(string.Format(@"<set label=""Máximo - {0}"" value=""{1}""/>", descricao, valorMax));

                xml.Append("</dataset>");
            }

        }
        catch
        {
            return "";
        }

        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Verdana\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("</definition>");
        xml.Append("<application>");
        xml.Append("<apply toObject=\"DATALABELS\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"DATAVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"XAXISNAME \" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISNAME\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"TOOLTIP\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"Caption\" styles=\"fontTitulo\" />");
        xml.Append("</application>");
        xml.Append("</styles>");
        xml.Append("</chart>");


        return xml.ToString();
    }

    public DataSet getAnosAtivosUnidade(int codigoUnidade, string where)
    {
        string comandoSQL = string.Format(@"SELECT pe.Ano 
                                              FROM PeriodoEstrategia pe
                                             WHERE pe.IndicaAnoAtivo = 'S'
                                               AND pe.CodigoUnidadeNegocio = {2}
                                               {3}
                                             ORDER BY pe.Ano", bancodb, Ownerdb, codigoUnidade, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getAnosAtivosIndicador(int codigoIndicador, string where)
    {
        string comandoSQL = string.Format(@"SELECT pe.Ano 
                                              FROM PeriodoEstrategia pe
                                             WHERE pe.IndicaAnoAtivo = 'S'
                                               AND pe.CodigoUnidadeNegocio IN (SELECT un.CodigoEntidade
                                                     FROM {0}.{1}.IndicadorUnidade iu INNER JOIN
                                                          {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio
                                                    WHERE CodigoIndicador = {2}
                                                      AND IndicaUnidadeCriadoraIndicador = 'S')
                                               {3}
                                             ORDER BY pe.Ano", bancodb, Ownerdb, codigoIndicador, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getPeriodosAtivosIndicador(int codigoIndicador, string where)
    {
        string comandoSQL = string.Format(@"SELECT Periodo, AnoMes, PeriodoAtual 
                                              FROM {0}.{1}.f_GetOpcoesPeriodicidadeIndicador({2})
                                            WHERE 1 = 1 
                                              {3}", bancodb, Ownerdb, codigoIndicador, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getUnidadesUsuarioIndicador(int codigoIndicador, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(@"SELECT CodigoUnidadeNegocio, SiglaUnidadeNegocio,un.NomeUnidadeNegocio 
                                              FROM {0}.{1}.f_GetUnidadesUsuarioIndicador({3}, {2}) f INNER JOIN
			                                       {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = f.CodigoUnidade
                                             WHERE 1 = 1 
                                              {4}", bancodb, Ownerdb, codigoIndicador, codigoUsuario, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getUnidadesUsuarioIndicadorPorPermissao(int codigoIndicador, int codigoUsuario, int codigoEntidade, string IniciaisPermissao, string where)
    {
        string comandoSQL = string.Format(@"SELECT CodigoUnidadeNegocio, SiglaUnidadeNegocio,un.NomeUnidadeNegocio 
                                              FROM {0}.{1}.f_GetUnidadesUsuarioIndicadorPorPermissao({3}, {5}, {2}, NULL, '{6}') f INNER JOIN
			                                       {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = f.CodigoUnidade
                                             WHERE 1 = 1 
                                              {4}", bancodb, Ownerdb, codigoIndicador, codigoUsuario, where, codigoEntidade, IniciaisPermissao);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getResponsaveisMetasUsuario(int codigoEntidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(@"SELECT DISTINCT u.CodigoUsuario, u.NomeUsuario,u.EMail
	                                          FROM {0}.{1}.Indicador i INNER JOIN
                                                   {0}.{1}.IndicadorUnidade iu ON (iu.CodigoIndicador = i.CodigoIndicador) INNER JOIN
                                                   {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio AND un.CodigoEntidade = {3} INNER JOIN
		                                           {0}.{1}.Usuario u ON u.CodigoUsuario = iu.CodigoResponsavelIndicadorUnidade
	                                         WHERE iu.Meta IS NOT NULL
                                               {4}
                                               AND EXISTS(SELECT 1
					                                        FROM {0}.{1}.f_GetIndicadoresUsuario({2}, {3}, 'N') f
				                                           WHERE f.CodigoIndicador = i.CodigoIndicador)
                                             ORDER BY u.NomeUsuario
                                               ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getDadosIndicador(int codigoIndicador, string where)
    {
        comandoSQL = string.Format(@"SELECT i.NomeIndicador
                                          , i.Polaridade
                                          , {0}.{1}.f_GetFormulaPorExtenso(i.CodigoIndicador) AS FormulaIndicador
                                          , tp.DescricaoPeriodicidade_PT
                                          , u.NomeUsuario
                                          , tum.DescricaoUnidadeMedida_PT + '(' + tum.SiglaUnidadeMedida + ')' AS DescricaoUnidadeMedida_PT
                                          , oe.DescricaoObjetoEstrategia
                                          , tum.SiglaUnidadeMedida
                                          , i.CasasDecimais
                                          , me.TituloMapaEstrategico
                                          ,i.GlossarioIndicador
                                      FROM {0}.{1}.Indicador i INNER JOIN
	                                       {0}.{1}.IndicadorObjetivoEstrategico ioe ON ioe.CodigoIndicador = i.CodigoIndicador INNER JOIN
	                                       {0}.{1}.ObjetoEstrategia oe ON oe.CodigoObjetoEstrategia = ioe.CodigoObjetivoEstrategico INNER JOIN
	                                       {0}.{1}.TipoUnidadeMedida tum ON tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida INNER JOIN
	                                       {0}.{1}.TipoPeriodicidade tp ON tp.CodigoPeriodicidade = i.CodigoPeriodicidadeCalculo LEFT JOIN
	                                       {0}.{1}.Usuario u ON u.CodigoUsuario = i.CodigoUsuarioResponsavel INNER JOIN
                                           {0}.{1}.MapaEstrategico AS me ON (me.CodigoMapaEstrategico = oe.CodigoMapaEstrategico)
                                     WHERE i.CodigoIndicador = {2}
                                       {3}", bancodb, Ownerdb, codigoIndicador, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getInformacoesIndicador(int codigoIndicador, int codigoObjetivo)
    {
        string where = "";
        string innerJoin = "";
        string colunas = "";

        if (codigoObjetivo > 0)
        {
            where = " AND oe.CodigoObjetoEstrategia = " + codigoObjetivo;
            innerJoin = string.Format(@"{0}.{1}.IndicadorObjetivoEstrategico ioe ON ioe.CodigoIndicador = i.CodigoIndicador INNER JOIN
	                                       {0}.{1}.ObjetoEstrategia oe ON oe.CodigoObjetoEstrategia = ioe.CodigoObjetivoEstrategico INNER JOIN
                                           {0}.{1}.MapaEstrategico AS me ON (me.CodigoMapaEstrategico = oe.CodigoMapaEstrategico) INNER JOIN", bancodb, Ownerdb);
            colunas = @", me.TituloMapaEstrategico
                                          , oe.DescricaoObjetoEstrategia";
        }
        else
        {
            colunas = @", '' as TituloMapaEstrategico
                                          , '' as DescricaoObjetoEstrategia";
        }

        comandoSQL = string.Format(@"SELECT i.NomeIndicador
                                          , i.Polaridade
                                          , {0}.{1}.f_GetFormulaPorExtenso(i.CodigoIndicador) AS FormulaIndicador
                                          , tp.DescricaoPeriodicidade_PT
                                          , u.NomeUsuario
                                          , tum.DescricaoUnidadeMedida_PT + '(' + tum.SiglaUnidadeMedida + ')' AS DescricaoUnidadeMedida_PT
                                          , tum.SiglaUnidadeMedida
                                          , i.CasasDecimais
                                          {5}
                                          ,i.GlossarioIndicador
                                      FROM {0}.{1}.Indicador i INNER JOIN
	                                       {4}
	                                       {0}.{1}.TipoUnidadeMedida tum ON tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida INNER JOIN
	                                       {0}.{1}.TipoPeriodicidade tp ON tp.CodigoPeriodicidade = i.CodigoPeriodicidadeCalculo LEFT JOIN
	                                       {0}.{1}.Usuario u ON u.CodigoUsuario = i.CodigoUsuarioResponsavel                                           
                                     WHERE i.CodigoIndicador = {2}
                                       {3}", bancodb, Ownerdb, codigoIndicador, where, innerJoin, colunas);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getPeriodoVigenciaIndicador(int codigoIndicador)
    {
        comandoSQL = string.Format(@"SELECT i.DataInicioValidadeMeta, i.DataTerminoValidadeMeta, i.IndicaAcompanhamentoMetaVigencia
                                      FROM {0}.{1}.Indicador i
                                     WHERE i.CodigoIndicador = {2}", bancodb, Ownerdb, codigoIndicador);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public string getNomeIndicador(int codigoIndicador)
    {
        string nomeIndicador = "";

        comandoSQL = string.Format(@"SELECT i.NomeIndicador                                          
                                      FROM {0}.{1}.Indicador i
                                     WHERE i.CodigoIndicador = {2}
                                       ", bancodb, Ownerdb, codigoIndicador);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            nomeIndicador = ds.Tables[0].Rows[0]["NomeIndicador"].ToString();

        return nomeIndicador;
    }

    public string getNomeObjetivo(int codigoObjetivo)
    {
        string nomeObjetivo = "";

        comandoSQL = string.Format(@"SELECT DescricaoObjetoEstrategia 
                                       FROM {0}.{1}.ObjetoEstrategia
                                      WHERE CodigoObjetoEstrategia = {2}
                                       ", bancodb, Ownerdb, codigoObjetivo);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            nomeObjetivo = ds.Tables[0].Rows[0]["DescricaoObjetoEstrategia"].ToString();

        return nomeObjetivo;
    }

    public void getAgrupamentoIndicador(int codigoIndicador, ref string sigla, ref string nome)
    {
        comandoSQL = string.Format(@"SELECT CASE WHEN IndicaCriterio= 'S' THEN 'STT' ELSE tfd.NomeFuncaoBD END AS NomeFuncaoBD
                                          , CASE WHEN IndicaCriterio= 'S' THEN '{3}' ELSE tfd.NomeFuncao END AS NomeFuncao                     
                                      FROM {0}.{1}.Indicador i INNER JOIN
                                           {0}.{1}.TipoFuncaoDado tfd ON tfd.CodigoFuncao = i.CodigoFuncaoAgrupamentoMeta
                                     WHERE i.CodigoIndicador = {2}
                                       ", bancodb, Ownerdb, codigoIndicador, Resources.traducao._ltima);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            sigla = ds.Tables[0].Rows[0]["NomeFuncaoBD"].ToString();
            nome = ds.Tables[0].Rows[0]["NomeFuncao"].ToString();
        }
    }

    public string getNomeIndicadorOperacional(int codigoIndicador)
    {
        string nomeIndicador = "";

        comandoSQL = string.Format(@"SELECT i.NomeIndicador                                          
                                      FROM {0}.{1}.IndicadorOperacional i
                                     WHERE i.CodigoIndicador = {2}
                                       ", bancodb, Ownerdb, codigoIndicador);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            nomeIndicador = ds.Tables[0].Rows[0]["NomeIndicador"].ToString();

        return nomeIndicador;
    }
    public DataSet getMensagensIndicador(int codigoEntidade, int codigoIndicador, string where)
    {
        string comandoSQL = string.Format(@"
            BEGIN	
	            DECLARE @CodigoTipoAssociacao int,
		                @CodigoIndicador int,
		                @CodigoEntidade int
            		
	            SET @CodigoIndicador = {2}
	            SET @CodigoEntidade = {3}
            		
	            SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao
			      FROM {0}.{1}.TipoAssociacao 
			     WHERE IniciaisTipoAssociacao = 'IN'

	            SELECT m.CodigoMensagem, m.Mensagem, m.DataInclusao, m.DataLimiteResposta, m.CodigoUsuarioInclusao, u.NomeUsuario
	              FROM {0}.{1}.Mensagem m INNER JOIN
                       {0}.{1}.Usuario u ON u.CodigoUsuario = m.CodigoUsuarioInclusao
	             WHERE CodigoTipoAssociacao = @CodigoTipoAssociacao
	               AND CodigoObjetoAssociado = @CodigoIndicador
	               AND DataResposta IS NULL
	               AND CodigoEntidade = @CodigoEntidade
            END
            ", bancodb, Ownerdb, codigoIndicador, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    //Indicadores

    public DataSet getPendenciaAtualizacaoIndicadores(int codigoEntidade)
    {
        string comandoSQL = string.Format(@"
        --O select deverá ser substituído pelo apresentado abaixo. Agora há um parâmetro para informar qual é a entidade.
        SELECT un.NomeUnidadeNegocio,
               i.NomeIndicador,
               u.NomeUsuario,
               ri.Ano,
               {0}.{1}.f_GetNomePeriodoIndicador(i.CodigoIndicador,ri.Ano,ri.Mes,0,0) as Mes,
               DateAdd(D,i.LimiteAlertaEdicaoIndicador,DateAdd(MI,1,{0}.{1}.[f_GetDataRefUltimoPeriodoIndicador](i.CodigoIndicador,ri.Ano,ri.Mes,0))) AS DataVencimento
          FROM {0}.{1}.ResumoIndicador AS ri INNER JOIN
               {0}.{1}.Indicador AS i ON (i.CodigoIndicador = ri.CodigoIndicador) INNER JOIN
               {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = ri.CodigoUnidadeNegocio) INNER JOIN
               {0}.{1}.IndicadorUnidade AS iu ON (iu.CodigoIndicador = i.CodigoIndicador
                      AND iu.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio) INNER JOIN
               {0}.{1}.Usuario AS u ON (u.CodigoUsuario = iu.CodigoResponsavelIndicadorUnidade)                       
         WHERE ri.ResultadoMes IS NULL
           AND iu.DataExclusao IS NULL
           AND i.DataExclusao IS NULL
           AND un.CodigoEntidade = {2}
           AND DateAdd(D,i.LimiteAlertaEdicaoIndicador,DateAdd(MI,1,{0}.{1}.[f_GetDataRefUltimoPeriodoIndicador](i.CodigoIndicador,ri.Ano,ri.Mes,0))) < GETDATE()
           AND (ri.Ano < YEAR(GetDate()) OR (ri.Ano = YEAR(GetDate()) AND ri.Mes < MONTH(GetDate())))
         UNION
        SELECT un.NomeUnidadeNegocio,
               i.NomeIndicador,
               u.NomeUsuario,
               ri.Ano,
               {0}.{1}.f_GetNomePeriodoIndicador(i.CodigoIndicador,ri.Ano,ri.Mes,0,0) as Mes,
               DateAdd(D,i.LimiteAlertaEdicaoIndicador,DateAdd(MI,1,{0}.{1}.[f_GetDataRefUltimoPeriodoIndicador](i.CodigoIndicador,ri.Ano,ri.Mes,0))) AS DataVencimento
          FROM {0}.{1}.ResumoIndicador AS ri INNER JOIN
               {0}.{1}.Indicador AS i ON (i.CodigoIndicador = ri.CodigoIndicador) INNER JOIN
               {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = ri.CodigoUnidadeNegocio) INNER JOIN
               {0}.{1}.IndicadorUnidade AS iu ON (iu.CodigoIndicador = i.CodigoIndicador
                              AND iu.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio) INNER JOIN
               {0}.{1}.Usuario AS u ON (u.CodigoUsuario = iu.CodigoResponsavelIndicadorUnidade)                       
         WHERE ri.ResultadoMes IS NULL
           AND iu.DataExclusao IS NULL
           AND i.DataExclusao IS NULL
           AND un.CodigoEntidade <> {2}
           AND un.CodigoEntidade IN (SELECT CodigoEntidade FROM {0}.{1}.UnidadeNegocio WHERE CodigoUnidadeNegocioSuperior = {2})
           AND DateAdd(D,i.LimiteAlertaEdicaoIndicador,DateAdd(MI,1,{0}.{1}.[f_GetDataRefUltimoPeriodoIndicador](i.CodigoIndicador,ri.Ano,ri.Mes,0))) < GETDATE()
           AND (ri.Ano < YEAR(GetDate()) OR (ri.Ano = YEAR(GetDate()) AND ri.Mes < MONTH(GetDate())))   
       ORDER BY DataVencimento

", bancodb, Ownerdb, codigoEntidade);
        return getDataSet(comandoSQL);

    }

    public DataSet getIndicadores(int idEntidadLogada, int idUsuarioLogado, string podeAgruparPorMapa, string where)
    {
        string colunaDescricaoMapaPrimeiroSelect = @",   CAST('' AS Varchar(100)) AS MapaEstrategico";
        string colunaDescricaoMapaSegundoSelect = "";
        string wherePrimeiroSelect = "1 = 1";
        string whereSegundoSelect = "1 = 1";
        string leftJoinSegundoSelect = "";

        if (podeAgruparPorMapa == "S")
        {
            colunaDescricaoMapaPrimeiroSelect = @",	CAST( '(Sem Mapa Associado)' AS Varchar(100)) AS MapaEstrategico";
            colunaDescricaoMapaSegundoSelect = @",		me.TituloMapaEstrategico AS MapaEstrategico";
            wherePrimeiroSelect = string.Format(@" NOT EXISTS
    ( SELECT TOP 1 1 
		    FROM 
			    {0}.{1}.IndicadorObjetivoEstrategico ioe 
			    INNER JOIN {0}.{1}.ObjetoEstrategia oe ON oe.CodigoObjetoEstrategia = ioe.CodigoObjetivoEstrategico and oe.DataExclusao IS NULL 
			    INNER JOIN {0}.{1}.MapaEstrategico me ON me.CodigoMapaEstrategico = oe.CodigoMapaEstrategico and me.IndicaMapaEstrategicoAtivo = 'S' 
			    INNER JOIN {0}.{1}.UnidadeNegocio un2 ON un2.CodigoUnidadeNegocio = me.CodigoUnidadeNegocio and un2.DataExclusao IS NULL and un2.IndicaUnidadeNegocioAtiva = 'S' 
		    WHERE ioe.CodigoIndicador = tb.CodigoIndicador
			    AND	( un2.CodigoEntidade = {2}
						    OR EXISTS
						    (	SELECT TOP 1 1 
								    FROM {0}.{1}.PermissaoMapaEstrategicoUnidade pmeu
								    WHERE pmeu.CodigoMapaEstrategico = me.CodigoMapaEstrategico
									    and pmeu.CodigoUnidadeNegocio = {2} 
						    )
					    )
    )", bancodb, Ownerdb, idEntidadLogada);
            leftJoinSegundoSelect = string.Format(@"
    INNER JOIN {0}.{1}.IndicadorObjetivoEstrategico ioe ON tb.CodigoIndicador = ioe.CodigoIndicador 
    INNER JOIN {0}.{1}.ObjetoEstrategia oe ON oe.CodigoObjetoEstrategia = ioe.CodigoObjetivoEstrategico and oe.DataExclusao IS NULL 
    INNER JOIN {0}.{1}.MapaEstrategico me ON me.CodigoMapaEstrategico = oe.CodigoMapaEstrategico and me.IndicaMapaEstrategicoAtivo = 'S' 
    INNER JOIN {0}.{1}.UnidadeNegocio un2 ON un2.CodigoUnidadeNegocio = me.CodigoUnidadeNegocio and un2.DataExclusao IS NULL and un2.IndicaUnidadeNegocioAtiva = 'S'

", bancodb, Ownerdb);

            whereSegundoSelect = string.Format(@"
    un2.CodigoEntidade = {2}
    OR EXISTS
	    (	SELECT TOP 1 1 
			    FROM {0}.{1}.PermissaoMapaEstrategicoUnidade pmeu
			    WHERE pmeu.CodigoMapaEstrategico = me.CodigoMapaEstrategico
				    and pmeu.CodigoUnidadeNegocio = {2} 
	    )", bancodb, Ownerdb, idEntidadLogada);
        }

        StringBuilder comandoSQL = new StringBuilder();
        comandoSQL.AppendLine(string.Format(@"
BEGIN
	DECLARE @tbResumo TABLE 
		(
			  [CodigoIndicador]					        INT
			, [IndicaUnidadeCriadoraIndicador]	        CHAR(1)
			, [CodigoResponsavelIndicadorUnidade]	    INT
			, [CodigoResponsavelAtualizacaoIndicador]   INT
            , [CodigoUnidadeNegocio]                    INT
            , [EfeitoPermissaoCompartilhar]			    INT NOT NULL
            , [EfeitoPermissaoEditarRsp]			    INT NOT NULL
		)

	INSERT INTO @tbResumo			
		SELECT -- dos indicadores criados na ENTIDADE atual
					i.[CodigoIndicador]
        , 'S'
        , iu.[CodigoResponsavelIndicadorUnidade]
        , iu.[CodigoResponsavelAtualizacaoIndicador]
        , iu.[CodigoUnidadeNegocio]
        , {0}.{1}.f_VerificaAcessoConcedido({3}, {2}, i.[CodigoIndicador], NULL, 'IN', iu.[CodigoUnidadeNegocio], NULL, 'IN_Compart')
        , 0
			FROM
				{0}.{1}.[Indicador]					AS [i]
        INNER JOIN {0}.{1}.[IndicadorUnidade]		AS [iu]		ON 
					(			iu.[CodigoIndicador]		= i.[CodigoIndicador]
						AND iu.[CodigoUnidadeNegocio]		= {2}
						AND iu.[DataExclusao]			    IS NULL
						AND iu.[IndicaUnidadeCriadoraIndicador] = 'S'
					)
      WHERE   i.[DataExclusao]          IS NULL
		UNION ALL
		SELECT -- dos indicadores criados em UNIDADE da ENTIDADE atual
					i.[CodigoIndicador]
        , 'S'
        , iu.[CodigoResponsavelIndicadorUnidade]
        , iu.[CodigoResponsavelAtualizacaoIndicador]
        , iu.[CodigoUnidadeNegocio]
        , 0
        , 0
			FROM
				{0}.{1}.[Indicador]					AS [i]
        INNER JOIN {0}.{1}.[IndicadorUnidade]		AS [iu]		ON 
					(			iu.[CodigoIndicador]	    = i.[CodigoIndicador]
						AND iu.[DataExclusao]			IS NULL
						AND iu.[IndicaUnidadeCriadoraIndicador] = 'S'
					)
				INNER JOIN {0}.{1}.[UnidadeNegocio]			AS [un]		ON 
					(			un.[CodigoUnidadeNegocio]	= iu.[CodigoUnidadeNegocio]
						AND un.DataExclusao			        IS NULL
						AND un.[IndicaUnidadeNegocioAtiva]	= 'S'
						AND un.[CodigoEntidade]			    = {2}
					)
      WHERE   i.[DataExclusao]          IS NULL
				AND NOT EXISTS
					( SELECT TOP 1 1 
							FROM {0}.{1}.[IndicadorUnidade]				AS [iu2]
							WHERE iu2.CodigoIndicador	                    = i.CodigoIndicador
								AND iu2.DataExclusao		                IS NULL
								AND iu2.[CodigoUnidadeNegocio]		        = {2}
								AND iu2.[IndicaUnidadeCriadoraIndicador]	= 'S'
					);

	INSERT INTO @tbResumo			
		SELECT -- dos indicadores criados em outra entidade, mas compartilhado com a entidade atual
		  i.[CodigoIndicador]
        , 'N'
        , iu.[CodigoResponsavelIndicadorUnidade]
        , iu.[CodigoResponsavelAtualizacaoIndicador]
        , iu.[CodigoUnidadeNegocio]
        , 0
        , {0}.{1}.f_VerificaAcessoConcedido({3}, {2}, i.[CodigoIndicador], NULL, 'IN', iu.[CodigoUnidadeNegocio], NULL, 'IN_AltRsp')
			FROM
				{0}.{1}.[Indicador]					AS [i]
        INNER JOIN {0}.{1}.[IndicadorUnidade]		AS [iu]		ON 
					(			iu.[CodigoIndicador]	= i.[CodigoIndicador]
						AND iu.[CodigoUnidadeNegocio]	= {2}
						AND iu.[DataExclusao]			IS NULL
						AND iu.[IndicaUnidadeCriadoraIndicador] != 'S'
					)
      WHERE   i.[DataExclusao]          IS NULL
				AND i.[CodigoIndicador] NOT IN ( SELECT [CodigoIndicador] FROM @tbResumo )"
            , bancodb, Ownerdb, idEntidadLogada, idUsuarioLogado));

        string selectBasico = @"
    SELECT  iu.CodigoReservado      , tb.CodigoIndicador
        ,   i.NomeIndicador         , i.CodigoUnidadeMedida
        ,   i.GlossarioIndicador    , i.CasasDecimais
        ,   tb.CodigoResponsavelIndicadorUnidade AS CodigoResponsavel   , i.Polaridade
        ,   i.FormulaIndicador      , i.IndicaIndicadorCompartilhado
        ,   i.IndicadorResultante   , i.CodigoPeriodicidadeCalculo
        ,   i.FonteIndicador        , tb.IndicaUnidadeCriadoraIndicador
        ,   i.IndicaCriterio        , i.LimiteAlertaEdicaoIndicador
        ,   i.CodigoFuncaoAgrupamentoMeta   , u.NomeUsuario AS NomeUsuarioResponsavel
        ,   tb.CodigoResponsavelAtualizacaoIndicador, uRes.NomeUsuario AS NomeUsuarioResponsavelResultado
        ,   iu.CodigoUnidadeNegocio, un.NomeUnidadeNegocio, i.DataInicioValidadeMeta, i.DataTerminoValidadeMeta, i.IndicaAcompanhamentoMetaVigencia
        ,   CASE WHEN un.CodigoUnidadeNegocio = un.CodigoEntidade THEN 'S' ELSE 'N' END AS PodeCompartilhar
        ,   {0}.{1}.f_GetFormulaFormatoCliente(i.codigoIndicador) AS FormulaFormatoCliente
        ,   {0}.{1}.f_VerificaAcessoConcedido({3}, {2}, i.codigoIndicador, NULL, 'IN', iu.CodigoUnidadeNegocio, NULL, 'IN_Alt')     * 2  +
            {0}.{1}.f_VerificaAcessoConcedido({3}, {2}, i.codigoIndicador, NULL, 'IN', iu.CodigoUnidadeNegocio, NULL, 'IN_AdmPrs')  * 4  +
            tb.[EfeitoPermissaoCompartilhar] * 8  + tb.[EfeitoPermissaoEditarRsp]  * 16 AS [Permissoes]
        ,   ISNULL((SELECT top 1 'S' FROM {0}.{1}.ResumoIndicador ri WHERE ri.CodigoIndicador = tb.CodigoIndicador AND ri.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio), 'N') AS PossuiMetaResultado
		,	CASE 
				WHEN(
					EXISTS(	 SELECT 1 
							   FROM {0}.{1}.MetaIndicadorUnidade AS miu 
							  WHERE miu.CodigoIndicador = tb.CodigoIndicador) OR
					EXISTS(	 SELECT 1 
							   FROM {0}.{1}.DadoIndicador AS di INNER JOIN
									{0}.{1}.DadoUnidade AS du ON du.CodigoDado = di.CodigoDado INNER JOIN
									{0}.{1}.ResultadoDadoUnidade AS rdu ON rdu.CodigoDado = du.CodigoDado AND
																   rdu.CodigoUnidadeNegocio = du.CodigoUnidadeNegocio
							  WHERE di.CodigoIndicador = tb.CodigoIndicador
								AND du.DataExclusao IS NULL)) 
				THEN 'S' 
				ELSE 'N' 
			END AS ExisteDependencia
        {4}
    FROM          @tbResumo tb 
    LEFT JOIN    {0}.{1}.Usuario u             ON u.CodigoUsuario      = tb.CodigoResponsavelIndicadorUnidade 
    INNER JOIN    {0}.{1}.Indicador i           ON i.CodigoIndicador   = tb.CodigoIndicador
    INNER JOIN    {0}.{1}.IndicadorUnidade iu   ON iu.CodigoIndicador  = tb.CodigoIndicador  and iu.CodigoUnidadeNegocio = tb.CodigoUnidadeNegocio
    INNER JOIN    {0}.{1}.UnidadeNegocio un     ON un.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio 
    LEFT  JOIN    {0}.{1}.Usuario uRes          ON uRes.CodigoUsuario  = tb.CodigoResponsavelAtualizacaoIndicador 
    {5}
    WHERE {6} 
        {7}";

        comandoSQL.AppendLine(string.Format(selectBasico, bancodb, Ownerdb, idEntidadLogada, idUsuarioLogado, colunaDescricaoMapaPrimeiroSelect, "", wherePrimeiroSelect, where));

        if (podeAgruparPorMapa == "S")
        {
            comandoSQL.AppendLine(" UNION ALL ");
            comandoSQL.AppendLine(string.Format(selectBasico, bancodb, Ownerdb, idEntidadLogada, idUsuarioLogado, colunaDescricaoMapaSegundoSelect, leftJoinSegundoSelect, whereSegundoSelect, where));
        }
        comandoSQL.AppendLine("ORDER BY i.NomeIndicador");
        comandoSQL.AppendLine("END");
        return getDataSet(comandoSQL.ToString());
    }

    public DataSet getIndicadoresNaoAssociados(int codigoEntidade, int codigoMapa, string where)
    {
        int codigoUnidadeLogada = int.Parse(getInfoSistema("CodigoEntidade").ToString());

        string comandoSQL = string.Format(@"
        DECLARE @RC int
        DECLARE @in_CodigoUnidadeNegocio int
        DECLARE @in_CodigoMapaestrategico int
        DECLARE @in_CodigoEntidade int

        SET @in_CodigoUnidadeNegocio = {0}
        SET @in_CodigoMapaestrategico = {1}
        SET @in_CodigoEntidade = {2}

        EXECUTE @RC = [dbo].[p_GetIndicadoresNaoAssociadosAoObjetivo] 
           @in_CodigoUnidadeNegocio
          ,@in_CodigoMapaestrategico
          ,@in_CodigoEntidade ", codigoEntidade, codigoMapa, codigoUnidadeLogada);
        return getDataSet(comandoSQL);
    }

    /// <summary>
    ///  Devolve, para um indicador, seus valores de gráficoss de gauge (speedômetro).
    ///  Por valores de gráfico de gauge, entende-se o valor da meta, o valor obtido (resultado), o valor do limite superior do gráfico e os valores dos 'limites' de cada faixa (vermelho, amarelo, verde e azul)
    ///  A quantidade de casas decimais consideradas na precisão dos cálculos serão a indicada no parâmetro casasDecimais, que deve ser o registrado para o indicador. 
    /// </summary>
    /// <param name="codigoIndicador">Código que identifica o indicador na base de dados</param>
    /// <param name="polaridade">Os valores 'POS' ou 'NEG' indicando uma polaridade negativa ou positiva para o indicador</param>
    /// <param name="meta">O valor da meta que será apresentada no gráfico</param>
    /// <param name="resultado">O valor do resultado que será apresentado no gráfico</param>
    /// <param name="casasDecimais">A quantidade de casas decimais a ser usada na apresentação dos valores (deve coincidir com a qtde de casas decimais gravada no registro do indicador)</param>
    /// <param name="lowerLimit">Parâmetro de retorno: conterá o limite inferior a ser usado no gráfico.</param>
    /// <param name="upperLimit">Parâmetro de retorno: conterá o limite superior a ser usado no gráfico.</param>
    /// <param name="limiteCritico">Parâmetro de retorno: conterá o limite até o qual o resultado será considerado em situação crítica.</param>
    /// <param name="limiteAtencao">Parâmetro de retorno: conterá o limite até o qual o resultado será considerado em situação de atenção</param>
    /// <param name="limiteSatisfatorio">Parâmetro de retorno: conterá o limite até o qual o resultado será considerado em situação satisfatória</param>
    /// <param name="limiteExcelente">Parâmetro de retorno: conterá o limite até o qual o resultado será considerado em situação de excelência</param>
    /// <returns></returns>
    public int getValoresGraficoGaugeIndicador(int codigoIndicador, ref string polaridade, double meta, double resultado, int casasDecimais, out double lowerLimit, out double upperLimit, out double limiteCritico, out double limiteAtencao, out double limiteSatisfatorio, out double limiteExcelente)
    {

        double ruim = 0, atencao = 0, satisfatorio = 0;
        double valRefMeta; // valor de referência da meta a partir do qual serão definidas as faixas; 
        double valDesloc; // valor de deslocamento das faixas na régua, caso a meta seja negativa;

        limiteCritico = limiteAtencao = limiteSatisfatorio = limiteExcelente = 0;
        valRefMeta = Math.Abs(meta);
        valDesloc = valRefMeta - meta;

        try
        {
            DataSet dsParametros = getFaixasToleranciaIndicador(codigoIndicador);

            ruim = dsParametros.Tables[0].Rows[0]["stCritico"] + "" != "" ? double.Parse(dsParametros.Tables[0].Rows[0]["stCritico"] + "") : 0;
            atencao = dsParametros.Tables[0].Rows[0]["stAtencao"] + "" != "" ? double.Parse(dsParametros.Tables[0].Rows[0]["stAtencao"] + "") : 0;
            satisfatorio = dsParametros.Tables[0].Rows[0]["stSatisfatorio"] + "" != "" ? double.Parse(dsParametros.Tables[0].Rows[0]["stSatisfatorio"] + "") : 0;
        }
        catch { }

        if (polaridade == "NEG")
        {
            if (ruim != 0)
            {
                limiteCritico = Math.Round((valRefMeta / (ruim / 100.0)) - valDesloc, casasDecimais);
            }

            if (atencao != 0)
            {
                limiteAtencao = Math.Round((valRefMeta / (atencao / 100.0)) - valDesloc, casasDecimais);
            }

            if (satisfatorio != 0)
            {
                limiteSatisfatorio = Math.Round((valRefMeta / (satisfatorio / 100.0)) - valDesloc, casasDecimais);
            }

            limiteExcelente = Math.Round(Math.Abs(limiteSatisfatorio) / 1.2 - valDesloc, casasDecimais);
            lowerLimit = Math.Round(Math.Min(0.00, limiteExcelente - limiteExcelente * 0.2 * Math.Sign(meta)), casasDecimais);
            lowerLimit = Math.Min(lowerLimit, resultado);
            upperLimit = Math.Round((limiteCritico + valDesloc) / 0.8 - valDesloc, casasDecimais);
            upperLimit = Math.Max(upperLimit, resultado);
        }
        else
        {
            limiteCritico = Math.Round(((ruim * valRefMeta / 100)) - valDesloc, casasDecimais);
            limiteAtencao = Math.Round(((atencao * valRefMeta / 100)) - valDesloc, casasDecimais);
            limiteSatisfatorio = Math.Round(((satisfatorio * valRefMeta / 100)) - valDesloc, casasDecimais);
            limiteExcelente = Math.Round(Math.Abs(limiteSatisfatorio) * 1.2 - valDesloc, casasDecimais);
            lowerLimit = Math.Round(Math.Min(0.00, limiteCritico - limiteCritico * 0.2 * Math.Sign(meta)), casasDecimais);
            lowerLimit = Math.Min(lowerLimit, resultado);
            upperLimit = Math.Max(limiteExcelente, resultado);
        }

        return 1; // retorna 1 indicando que deu tudo certo na execução da função
    }

    public DataSet getFaixasToleranciaIndicador(int codigoIndicador)
    {
        string comandoSQL = string.Format(@"DECLARE @CorDesempenho VarChar(8),
                                                    @stSatisfatorio Decimal(25,2),
                                                    @stAtencao Decimal(25,2),
                                                    @stCritico Decimal(25,2),
                                                    @Desempenho Decimal(25,2)
                                              
                                                      
	                                            SET @stCritico			= {0}.{1}.f_GetValorLimiteSuperiorNivelDesempenho({2}, 'IN', 'C');
	                                            SET @stAtencao			= {0}.{1}.f_GetValorLimiteSuperiorNivelDesempenho({2}, 'IN', 'A');
	                                            SET @stSatisfatorio     = {0}.{1}.f_GetValorLimiteSuperiorNivelDesempenho({2}, 'IN', 'S');
                                            	
	                                            SELECT @stCritico AS stCritico, @stAtencao AS stAtencao, @stSatisfatorio AS stSatisfatorio",
                bancodb, Ownerdb, codigoIndicador);
        return getDataSet(comandoSQL);
    }

    public DataSet getFaixaToleranciaObjeto(int codigoObjetoAssociado, string tipoAssociacao, string where)
    {
        int codigoTipoAssociacao = getCodigoTipoAssociacao(tipoAssociacao);

        string comandoSQL = string.Format(@"SELECT CodigoFaixa, CodigoTipoAssociacao, CodigoObjetoAssociado, 
	                                               ValorLimiteSuperior, ValorLimiteInferior, CorDesempenho, AssuntoDesempenho
                                              FROM {0}.{1}.FaixaDesempenhoObjeto
                                             WHERE CodigoObjetoAssociado = {2}
                                               AND CodigoTipoAssociacao = {3}
                                               {4}
                                             ORDER BY ValorLimiteInferior
    ",
                bancodb, Ownerdb, codigoObjetoAssociado, codigoTipoAssociacao, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiFaixaTolerancia(string tipoAssociacao, int codigoObjetoAssociado, double valorLimiteInferior,
        double valorLimiteSuperior, string corDesempenho, string assuntoDesempenho)
    {
        bool retorno = false;
        int regAfetados = 0;
        string comandoSQL = "";

        int codigoTipoAssociacao = getCodigoTipoAssociacao(tipoAssociacao);

        try
        {
            comandoSQL = string.Format(
                @"  INSERT INTO {0}.{1}.FaixaDesempenhoObjeto(CodigoTipoAssociacao, CodigoObjetoAssociado, ValorLimiteInferior, ValorLimiteSuperior, CorDesempenho, AssuntoDesempenho)
                       VALUES ({2}, {3}, {4}, {5}, '{6}', '{7}')
                 ", bancodb, Ownerdb, codigoTipoAssociacao, codigoObjetoAssociado,
                  valorLimiteInferior.ToString().Replace(",", "."),
                  valorLimiteSuperior.ToString().Replace(",", "."), corDesempenho, assuntoDesempenho);

            execSQL(comandoSQL, ref regAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }
        return retorno;
    }

    public bool atualizaFaixaTolerancia(int codigoFaixa, double valorLimiteInferior,
        double valorLimiteSuperior, string corDesempenho, string assuntoDesempenho)
    {
        bool retorno = false;
        int regAfetados = 0;
        string comandoSQL = "";

        try
        {
            comandoSQL = string.Format(
                @"  UPDATE {0}.{1}.FaixaDesempenhoObjeto SET ValorLimiteInferior = {3}
                                                            ,ValorLimiteSuperior = {4}
                                                            ,CorDesempenho = '{5}'
                                                            ,AssuntoDesempenho = '{6}'
                    WHERE CodigoFaixa = {2}
                 ", bancodb, Ownerdb, codigoFaixa,
                  valorLimiteInferior.ToString().Replace(",", "."),
                  valorLimiteSuperior.ToString().Replace(",", "."), corDesempenho, assuntoDesempenho);

            execSQL(comandoSQL, ref regAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao editar o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }
        return retorno;
    }

    public bool excluiFaixaTolerancia(int codigoFaixa)
    {
        bool retorno = false;
        int regAfetados = 0;
        string comandoSQL = "";

        try
        {
            comandoSQL = string.Format(
                @"  DELETE FROM {0}.{1}.FaixaDesempenhoObjeto WHERE CodigoFaixa = {2}                 
                 ", bancodb, Ownerdb, codigoFaixa);

            execSQL(comandoSQL, ref regAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao excluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }
        return retorno;
    }

    public bool excluiFaixasToleranciaObjeto(string tipoAssociacao, int codigoObjetoAssociado)
    {
        bool retorno = false;
        int regAfetados = 0;
        string comandoSQL = "";

        int codigoTipoAssociacao = getCodigoTipoAssociacao(tipoAssociacao);

        try
        {
            comandoSQL = string.Format(
                @"  DELETE FROM {0}.{1}.FaixaDesempenhoObjeto WHERE CodigoTipoAssociacao = {2} AND CodigoObjetoAssociado = {3}                    
                 ", bancodb, Ownerdb, codigoTipoAssociacao, codigoObjetoAssociado);

            execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao excluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }
        return retorno;
    }

    public DataSet getUnidadesEntidadesSelecionadasCompartilhamentoIndicador(int codigoIndicador, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT un.CodigoUnidadeNegocio
                      ,un.NomeUnidadeNegocio
                      ,CASE WHEN un.CodigoEntidade = un.CodigoUnidadeNegocio THEN 'Entidade' ELSE 'Unidade' END AS Tipo
                      ,iu.CodigoResponsavelIndicadorUnidade AS CodigoResponsavel
                      ,u.NomeUsuario AS Responsavel, iu.CodigoResponsavelAtualizacaoIndicador, uRes.NomeUsuario AS NomeUsuarioResponsavelResultado
                  FROM {0}.{1}.UnidadeNegocio AS un INNER JOIN
			           {0}.{1}.IndicadorUnidade	AS iu ON iu.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio LEFT JOIN
			           {0}.{1}.Usuario u ON u.CodigoUsuario = iu.CodigoResponsavelIndicadorUnidade LEFT JOIN
                       {0}.{1}.Usuario uRes ON uRes.CodigoUsuario = iu.CodigoResponsavelAtualizacaoIndicador 
                 WHERE iu.CodigoIndicador = {2}
                   AND iu.IndicaUnidadeCriadoraIndicador != 'S'
                   AND iu.DataExclusao IS NULL
                   {3}
                 ORDER BY Tipo, un.NomeUnidadeNegocio
    ",
                bancodb, Ownerdb, codigoIndicador, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getUnidadesEntidadesDisponiveisCompartilhamentoIndicador(int codigoEntidade, int codigoIndicador, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT un.CodigoUnidadeNegocio
                      ,un.NomeUnidadeNegocio
                      ,CASE WHEN un.CodigoEntidade = un.CodigoUnidadeNegocio THEN '../../imagens/Entidade.png' ELSE '../../imagens/Unidade.png' END AS Tipo
                  FROM dbo.UnidadeNegocio   AS un
                 WHERE (un.CodigoUnidadeNegocioSuperior = {2} OR CodigoEntidade = {2})
                   AND un.CodigoUnidadeNegocio != un.CodigoUnidadeNegocioSuperior
                   AND un.DataExclusao IS NULL
                   AND un.IndicaUnidadeNegocioAtiva = 'S'
                   {4}
                   AND NOT EXISTS( SELECT 1 
                                     FROM dbo.IndicadorUnidade AS iu 
                                    WHERE iu.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio 
                                      AND iu.CodigoIndicador = {3}
                                      AND iu.DataExclusao IS NULL )
                 ORDER BY Tipo, un.NomeUnidadeNegocio
    ",
                bancodb, Ownerdb, codigoEntidade, codigoIndicador, where);
        return getDataSet(comandoSQL);
    }

    //by Alejandro 17/07/2010 ... 
    public DataSet getDadosIndicadorEstrategiaGrid(int codigoUnidade, int codigoIndicador, string where, string whereIndicador)
    {
        //Consultados para o carregamento de ComboBox que contém os dados disponíveis para o Dados.
        //(Ao adicionar um novo dado visível, Invisivel si se está editando os dados).
        string comandoSQL = string.Format(@"
                SELECT  d.[CodigoDado], d.[DescricaoDado], '~/imagens/dado.png' AS UrlTipoComponente, 'D' AS Tipo
                FROM        {0}.{1}.[Dado] d 
                INNER JOIN  {0}.{1}.DadoUnidade du ON  du.CodigoDado = d.CodigoDado 
                INNER JOIN  {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = du.CodigoUnidadeNegocio

                WHERE   un.CodigoEntidade = {2}
                  AND   d.DataExclusao IS NULL
                  {4}

                UNION
	            
                SELECT i.CodigoIndicador * -1,i.NomeIndicador, '~/imagens/indicadoresMenor.png', 'I'

                FROM        {0}.{1}.Indicador i 
                INNER JOIN  {0}.{1}.IndicadorUnidade iu ON iu.CodigoIndicador = i.CodigoIndicador
                INNER JOIN  {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio

                WHERE un.CodigoEntidade = {2}
                  AND iu.DataExclusao IS NULL
                  AND i.DataExclusao IS NULL
                  AND i.CodigoIndicador <> {3}
                  AND (i.CodigoIndicador NOT IN (SELECT ri.CodigoIndicadorRelacionado 
                                                 FROM {0}.{1}.RelacaoIndicador ri
                                                 WHERE ri.CodigoIndicador = {3})
                      {5})

                ORDER BY Tipo, DescricaoDado
                ", bancodb, Ownerdb, codigoUnidade, codigoIndicador, where, whereIndicador);
        return getDataSet(comandoSQL);
    }

    public bool incluiComponenteIndicador(int codigoIndicador, int codigoComponente, int codigoFuncao)
    {
        bool retorno = false;
        int regAfetados = 0;
        string comandoSQL = "";

        string funcaoComponente = codigoFuncao.ToString();

        if (codigoFuncao == 0)
            funcaoComponente = "null";

        try
        {
            if (codigoComponente > 0)
                comandoSQL = string.Format(
                    @"Insert Into {0}.{1}.DadoIndicador (codigoIndicador, codigoDado, CodigoFuncaoDadoIndicador) 
                                         values ({2}, {3}, {4})", bancodb, Ownerdb, codigoIndicador, codigoComponente, funcaoComponente);
            else
                comandoSQL = string.Format(
                    @"Insert Into {0}.{1}.RelacaoIndicador(codigoIndicador, CodigoIndicadorRelacionado, CodigoFuncaoIndicador) 
                                         values ({2}, {3}, {4})", bancodb, Ownerdb, codigoIndicador, (codigoComponente * -1), funcaoComponente);

            execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }
        return retorno;
    }

    public bool atualizaComponenteIndicador(int codigoIndicador, int codigoComponente, int codigoFuncao)
    {
        bool retorno = false;
        int regAfetados = 0;
        string comandoSQL = "";

        string funcaoComponente = codigoFuncao.ToString();

        if (codigoFuncao == 0)
            funcaoComponente = "null";

        try
        {
            if (codigoComponente > 0)
                comandoSQL = string.Format(
                    @"Update {0}.{1}.DadoIndicador set CodigoFuncaoDadoIndicador = {4} 
                       where codigoIndicador = {2}
                         and codigoDado = {3}", bancodb, Ownerdb, codigoIndicador, codigoComponente, funcaoComponente);
            else
                comandoSQL = string.Format(
                    @"Update {0}.{1}.RelacaoIndicador SET CodigoFuncaoIndicador = {4} 
                       where codigoIndicador = {2}
                         and CodigoIndicadorRelacionado = {3}", bancodb, Ownerdb, codigoIndicador, (codigoComponente * -1), funcaoComponente);

            execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }
        return retorno;
    }

    public bool excluiComponenteIndicador(int codigoIndicador, int codigoComponente)
    {
        bool retorno = false;
        int regAfetados = 0;
        string comandoSQL = "";

        try
        {
            if (codigoComponente > 0)
                comandoSQL = string.Format(
                    @"DELETE {0}.{1}.DadoIndicador 
                       where codigoIndicador = {2}
                         and codigoDado = {3}", bancodb, Ownerdb, codigoIndicador, codigoComponente);
            else
                comandoSQL = string.Format(
                    @"DELETE {0}.{1}.RelacaoIndicador
                       where codigoIndicador = {2}
                         and CodigoIndicadorRelacionado = {3}", bancodb, Ownerdb, codigoIndicador, (codigoComponente * -1));

            execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }
        return retorno;
    }

    public DataSet getGrupoIndicador(string where)
    {
        string comandoSQL = String.Format(@"
        SELECT CodigoGrupoIndicador,
               DescricaoGrupoIndicador,
               IndicaGrupoOperacional,
               IndicaGrupoEstrategico,
               IniciaisGrupoControladoSistema 
          FROM {0}.{1}.GrupoIndicador
          WHERE (1 = 1)
           {2}", getDbName(), getDbOwner(), where);
        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public bool incluiGrupoIndicador(string descricaoGrupoIndicador, string indicaGrupoOperacional, string indicaGrupoEstrategico, string IniciaisGrupoControladoSistema, int codigoEntidade, ref string mensagemErro)
    {

        //por enquanto as iniciais esta vindo como parametro, mas nao esta sendo usado pois precisa esclarecimentos
        bool retorno = false;
        comandoSQL = string.Format(
        @"INSERT INTO {0}.{1}.GrupoIndicador
                             (DescricaoGrupoIndicador ,IndicaGrupoOperacional ,IndicaGrupoEstrategico ,IniciaisGrupoControladoSistema,CodigoEntidade)
                       VALUES(                   '{2}',                  '{3}','{4}'                  ,null                          , {5})",
                             getDbName(), getDbOwner(), descricaoGrupoIndicador, indicaGrupoOperacional, indicaGrupoEstrategico, codigoEntidade);
        int regAfetados = 0;
        try
        {
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }

        return retorno;
    }

    public bool atualizaGrupoIndicador(string descricaoGrupoIndicador, string indicaGrupoOperacional, string indicaGrupoEstrategico, string iniciaisGrupoControladoSistema, int codigoGrupoIndicador, ref string mensagemErro)
    {
        bool retorno = false;
        int registrosAfetados = 0;
        comandoSQL = string.Format(
        @"UPDATE {0}.{1}.GrupoIndicador
        SET DescricaoGrupoIndicador = '{2}'
           ,IndicaGrupoOperacional = '{3}'
           ,IndicaGrupoEstrategico = '{4}'
 WHERE CodigoGrupoIndicador = {5}", getDbName(), getDbOwner(), descricaoGrupoIndicador, indicaGrupoOperacional, indicaGrupoEstrategico, codigoGrupoIndicador);
        try
        {
            retorno = execSQL(comandoSQL, ref registrosAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }

        return retorno;
    }

    public bool excluiGrupoIndicador(int codigoGrupoIndicador, ref string mensagemErro)
    {
        bool retorno = false;
        int registrosAfetados = 0;
        comandoSQL = string.Format(
        @"DELETE FROM {0}.{1}.GrupoIndicador
                WHERE CodigoGrupoIndicador = {2} 
                  AND IniciaisGrupoControladoSistema 
                  IS NULL", getDbName(), getDbOwner(), codigoGrupoIndicador);

        try
        {
            retorno = execSQL(comandoSQL, ref registrosAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;
    }

    /// <summary>
    /// </summary>
    /// <param name="codigoIndicador"></param>
    /// <returns></returns>
    public DataSet getDadosGrid(string CodigoIndicador)
    {
        string comandoSQL = string.Format(@"
        SELECT f.Sequencia, f.CodigoDado, f.CodigoIndicador, f.DescricaoDado
		      ,ISNULL(f.CodigoFuncaoDadoIndicador, 0) as CodigoFuncaoDadoIndicador, ISNULL(f.NomeFuncao, 'STATUS') as NomeFuncao, f.valorDado 
         FROM {0}.{1}.f_GetDadosIndicador({2}) f", bancodb, Ownerdb, CodigoIndicador);
        return getDataSet(comandoSQL);
    }

    public bool incluiDadoGrid(string nomeDado, string codigoUsuarioLogado, string codigoEntidadLogada)
    {
        bool retorno = false;
        int regAfetados = 0;
        string comandoSQL = "";

        try
        {
            comandoSQL = string.Format(
                @"BEGIN 
                 
                 INSERT INTO {0}.{1}.dado (DescricaoDado, CodigoUnidadeMedida, DataInclusao, CodigoUsuarioInclusao, CodigoFuncaoAgrupamentoDado)
                 VALUES ('{2}', 1, GETDATE(), {3}, 1)

                 INSERT INTO {0}.{1}.dadoUnidade (CodigoDado, CodigoUnidadeNegocio, DataInclusao, CodigoUsuarioInclusao, IndicaUnidadeCriadoraDado)
                 VALUES (scope_identity(), {4}, GETDATE(), {3}, 'S')
                
                 END", bancodb, Ownerdb, nomeDado, codigoUsuarioLogado, codigoEntidadLogada);

            execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }
        return retorno;
    }

    public DataSet getUsuariosRecursoProjetoAgil(int codigoProjeto)
    {
        string comandoSQL = string.Format(
            @"  SELECT * FROM {0}.{1}.f_Agil_GetEquipe({2});", bancodb, Ownerdb, codigoProjeto);
        return getDataSet(comandoSQL);
    }
    public DataSet getUnidadeMedida()
    {
        string comandoSQL = string.Format(@"
            SELECT CodigoUnidadeMedida, SiglaUnidadeMedida
            FROM {0}.{1}.TipoUnidadeMedida", bancodb, Ownerdb);
        return getDataSet(comandoSQL);
    }

    public DataSet getPeriodicidade(string where)
    {
        string comandoSQL = string.Format(@"
            SELECT CodigoPeriodicidade, DescricaoPeriodicidade_PT, IntervaloDias
            FROM {0}.{1}.TipoPeriodicidade
            WHERE 1 = 1
              {2}
            ORDER BY IntervaloDias", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiIndicador(string nomeIndicador, string responsavelIndicador, string fonteIndicador,
                         string unidadeMedida, string periodicidade, string polaridade, string formulaIndicador,
                         string codigoUnidade, string usuarioLogado, string glossarioIndicador,
                         string cassasDecimais, string indicadorResultante, string codigoAgrupamentoMeta,
                         string indicaCriterio, int limiteDias, string codigoReservado, string responsavelResultados,
                         string dataInicioValidadeMeta, string dataTerminoValidadeMeta, string indicaAcompanhamentoMetaVigencia,
                         ref int novoCodigoIndicador)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
              BEGIN

                    DECLARE @CodigoIndicador int

                INSERT INTO {0}.{1}.Indicador 
                    ( NomeIndicador, CodigoUnidadeMedida, GlossarioIndicador, CasasDecimais, Polaridade,    
                      FormulaIndicador, IndicaIndicadorCompartilhado, CodigoPeriodicidadeCalculo,                                       
                      FonteIndicador, CodigoUsuarioResponsavel, DataInclusao, CodigoUsuarioInclusao, IndicadorResultante,
                      CodigoFuncaoAgrupamentoMeta, IndicaCriterio, LimiteAlertaEdicaoIndicador, DataInicioValidadeMeta, DataTerminoValidadeMeta, IndicaAcompanhamentoMetaVigencia)
                VALUES ( '{2}', {3}, '{10}', {11}, '{4}', '{13}', 'N', {5}, '{6}', {7}, GETDATE(), {9}, '{12}', {14}, '{15}', {16}, {19}, {20}, '{21}')

                SET @CodigoIndicador = scope_identity()

                --Cadastrando ao campo 'IndicaUnidadeCriadoraIndicador' como 'S'
                INSERT INTO {0}.{1}.IndicadorUnidade 
                    (CodigoIndicador, CodigoUnidadeNegocio, CodigoResponsavelIndicadorUnidade, IndicaUnidadeCriadoraIndicador, DataInclusao, CodigoUsuarioInclusao, CodigoReservado, CodigoResponsavelAtualizacaoIndicador)
                VALUES 
                    (@CodigoIndicador, {8}, {7}, 'S', GETDATE(), {9},'{17}', {18})
                
                SELECT @CodigoIndicador AS CodigoIndicador

              END", bancodb, Ownerdb, nomeIndicador, unidadeMedida, polaridade, periodicidade, fonteIndicador,
                  responsavelIndicador, codigoUnidade, usuarioLogado, glossarioIndicador, cassasDecimais,
                  indicadorResultante, formulaIndicador, codigoAgrupamentoMeta, indicaCriterio, limiteDias, codigoReservado, responsavelResultados,
                  dataInicioValidadeMeta, dataTerminoValidadeMeta, indicaAcompanhamentoMetaVigencia);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigoIndicador = int.Parse(ds.Tables[0].Rows[0]["CodigoIndicador"].ToString());

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro" + Environment.NewLine + Delimitador_Erro + Environment.NewLine + ex.Message + Environment.NewLine + comandoSQL);

        }
        return retorno;
    }

    public bool atualizaIndicador(string codigoIndicador, string nomeIndicador,
                         string responsavelIndicador, string fonteIndicador,
                         string unidadeMedida, string periodicidade,
                         string polaridade, string formulaIndicador,
                         string usuarioLogado, string CasasDecimais,
                         string IndicadorResultante, string glossarioIndicador,
                         string codigoAgrupamentoMeta, string indicaCriterio, int limiteDias, string codigoReservado, int codigoUnidadeNegocio, string responsavelResultados,
                         string dataInicioValidadeMeta, string dataTerminoValidadeMeta, string indicaAcompanhamentoMetaVigencia, ref string msgErro)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(
            @"BEGIN
                UPDATE {0}.{1}.Indicador 
                   SET NomeIndicador = '{2}',
                      CodigoUnidadeMedida = {3},
                      GlossarioIndicador = '{10}',
                      CasasDecimais = {11},
                      Polaridade = '{4}',
                      IndicaIndicadorCompartilhado = 'N',
                      CodigoPeriodicidadeCalculo = {5},
                      FonteIndicador = '{6}',
                      CodigoUsuarioResponsavel = {7},
                      FormulaIndicador = '{8}',
                      IndicadorResultante = '{12}',
                      CodigoFuncaoAgrupamentoMeta = {13},
                      IndicaCriterio = '{14}',
                      LimiteAlertaEdicaoIndicador = {15},
                      DataInicioValidadeMeta = {19}, 
                      DataTerminoValidadeMeta = {20}, 
                      IndicaAcompanhamentoMetaVigencia = '{21}'
                 WHERE CodigoIndicador = {9}
              END
              UPDATE {0}.{1}.[IndicadorUnidade]
                 SET [CodigoReservado] = '{16}'
                    ,CodigoResponsavelIndicadorUnidade = {7}
                    ,CodigoResponsavelAtualizacaoIndicador = {18} 
                    ,CodigoUnidadeNegocio = {17}
               WHERE (CodigoIndicador = {9} and 
                      IndicaUnidadeCriadoraIndicador = 'S')

                    ", bancodb, Ownerdb, nomeIndicador, unidadeMedida, polaridade,
                    periodicidade, fonteIndicador, responsavelIndicador, formulaIndicador,
                    codigoIndicador, glossarioIndicador, CasasDecimais, IndicadorResultante,
                    codigoAgrupamentoMeta, indicaCriterio, limiteDias, codigoReservado, codigoUnidadeNegocio, responsavelResultados,
                  dataInicioValidadeMeta, dataTerminoValidadeMeta, indicaAcompanhamentoMetaVigencia);

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch (Exception ex)
        {
            msgErro = ex.Message;
            return false;
        }
    }

    public bool excluiIndicador(string CodigoIndicador, string CodigoUsuario)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
            UPDATE {0}.{1}.Indicador
            SET
            DataExclusao = GETDATE(),
            CodigoUsuarioExclusao = {2}
            WHERE CodigoIndicador = {3}

            UPDATE {0}.{1}.IndicadorUnidade
            SET
            DataExclusao = GETDATE(),
            CodigoUsuarioExclusao = {2}
            WHERE CodigoIndicador = {3}", bancodb, Ownerdb, CodigoUsuario, CodigoIndicador);
            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    //Análise do Indicador

    public bool excluiAnalise(int codigoUsuario, int codigoAnalise, ref int registrosAfetados)
    {
        bool retorno = true;
        string comandoSQL = "";

        try
        {
            comandoSQL += string.Format(@"
                    UPDATE {0}.{1}.AnalisePerformance
                    SET     CodigoUsuarioExclusao   = {2}
                        ,   DataExclusao            = GETDATE()

                    WHERE CodigoAnalisePerformance = {3}
                ", bancodb, Ownerdb, codigoUsuario, codigoAnalise);

            execSQL(comandoSQL, ref registrosAfetados);
        }
        catch (Exception e)
        {
            retorno = false;
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao inserir o registro.\n\n" + Delimitador_Erro + " \n " + e.Message + "\n" + comandoSQL);
        }

        return retorno;
    }

    public void incluirAnalise(int codigoIndicador, int codigoUnidadeNegocio, string analise
                                , string recomendacoes, int codigoUsuario, string tipoIndicador
                                , int ano, int mes, ref int regAfetados)
    {
        try
        {
            string comandoSQL = string.Format(@"
                    INSERT INTO {0}.{1}.AnalisePerformance(
                            CodigoIndicador
                        ,   IndicaTipoIndicador
                        ,   CodigoUnidadeNegocio
                        ,   DataAnalisePerformance
                        ,   Analise                 --AnalisePerformance
                        ,   Recomendacoes
                        ,   CodigoUsuarioInclusao
                        ,   DataInclusao
                        ,   Ano
                        ,	Mes
                        ,   IndicaRegistroEditavel)
                    VALUES(
                            {2}
                        ,   '{7}'
                        ,   {3}
                        ,   GETDATE()
                        ,   '{4}'
                        ,   {5}
                        ,   {6}
                        ,   GETDATE()
                        ,   {8}
                        ,   {9}
                        ,   'S')
                    ", bancodb, Ownerdb, codigoIndicador, codigoUnidadeNegocio
                     , analise.Trim().Replace("'", "''")
                     , recomendacoes.Trim().Equals("NULL") ? "NULL" : "'" + recomendacoes.Trim().Replace("'", "''") + "'"
                     , codigoUsuario, tipoIndicador, ano, mes);

            execSQL(comandoSQL, ref regAfetados);
        }
        catch
        {
        }
    }

    public bool atualizaAnalise(string analise, string recomendacoes, int codigoUsuario, int codigoAnalise, ref int registrosAfetados, ref string mensagemErro)
    {
        bool retorno = true;
        string comandoSQL = "";

        try
        {
            comandoSQL = string.Format(@"
                    UPDATE {0}.{1}.AnalisePerformance
                    SET 
                        DataAnalisePerformance          = GETDATE()
                    ,   Analise                         = '{2}'
                    ,   Recomendacoes                   = {3}
                    ,   CodigoUsuarioUltimaAlteracao    = {4}
                    ,   DataUltimaAlteracao             = GETDATE()

                    WHERE CodigoAnalisePerformance = {5}
                  ", bancodb, Ownerdb
                   , analise.Trim().Replace("'", "''")
                   , recomendacoes.Trim().Equals("NULL") ? "NULL" : "'" + recomendacoes.Trim().Replace("'", "''") + "'"
                   , codigoUsuario, codigoAnalise);

            execSQL(comandoSQL, ref registrosAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = getErroIncluiRegistro(ex.Message) + comandoSQL;
        }
        return retorno;
    }

    public DataSet getMapaDoObjetivo(int codigoEntidadeLogada, int codigoObjetivo)
    {
        string comandoSQL = string.Format(@"
                DECLARE @CodigoEntidadeLogada   INT
                    ,   @CodigoObjetivo         INT

                SET @CodigoEntidadeLogada   = {2}
                SET @CodigoObjetivo         = {3}

                SELECT  i.[CodigoIndicador]
                    ,   iu.[Meta] 

                FROM        {0}.{1}.[IndicadorObjetivoEstrategico]  AS [ioe]
                INNER JOIN  {0}.{1}.[Indicador]                     AS [i]  ON (i.[CodigoIndicador]		= ioe.[CodigoIndicador])
                INNER JOIN  {0}.{1}.[IndicadorUnidade]              AS [iu] ON ( iu.[CodigoIndicador] = i.[CodigoIndicador])
                INNER JOIN  {0}.{1}.[UnidadeNegocio]                AS [un] ON ( un.[CodigoUnidadeNegocio] = iu.[CodigoUnidadeNegocio])

                WHERE   ioe.[CodigoObjetivoEstrategico] = @CodigoObjetivo 
                  AND   i.[DataExclusao]                IS NULL
                  AND   iu.[DataExclusao]               IS NULL
                  AND   un.[CodigoEntidade]             IN (SELECT un2.[CodigoEntidade] FROM {0}.{1}.[UnidadeNegocio] AS [un2] WHERE un2.[CodigoUnidadeNegocio] = @CodigoEntidadeLogada)
                  AND   LEN(iu.[Meta])                  >0

                ORDER BY iu.[Meta]
                ", bancodb, Ownerdb, codigoEntidadeLogada, codigoObjetivo);
        return getDataSet(comandoSQL);
    }
    #endregion

    #region _Estrategias - Wizard - dadosIndicadores.aspx

    public DataSet getAgrupamentoFuncao()
    {
        string comandoSQL = string.Format(@"
                                SELECT CodigoFuncao
                                    ,NomeFuncao
                                FROM {0}.{1}.TipoFuncaoDado", bancodb, Ownerdb);
        return getDataSet(comandoSQL);
    }

    public DataSet getDados(string where)
    {
        comandoSQL = string.Format(@"SELECT d.CodigoDado
                                          ,d.DescricaoDado
                                          ,d.GlossarioDado
                                          ,d.CodigoUnidadeMedida
                                          ,d.CasasDecimais
                                          ,d.ValorMinimo
                                          ,d.ValorMaximo
                                          ,d.CodigoFuncaoAgrupamentoDado
                                          ,d.CodigoUsuarioInclusao
                                          ,tum.SiglaUnidadeMedida
                                          ,dun.CodigoReservado
                                          ,dun.IndicaUnidadeCriadoraDado
                                        FROM {0}.{1}.Dado d INNER JOIN
                                             {0}.{1}.TipoUnidadeMedida tum ON tum.CodigoUnidadeMedida = d.CodigoUnidadeMedida inner join
                                             {0}.{1}.DadoUnidade dun on (dun.CodigoDado = d.CodigoDado)                                             
                                       WHERE d.DataExclusao is NULL
                                           {2}
                                       ORDER BY DescricaoDado", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    public DataSet incluiDadoIndicadores(string descricaoDado, string glossarioDado, int codigoUnidadeMedida, int casasDecimais, string valorMinimo, string valorMaximo, int CodigoAgrupamentoDado, int CodigoUsuarioInclusao, int codigoEntidad, string codigoReservado)
    {
        comandoSQL = string.Format(@"
                DECLARE @codigoDado int

                 INSERT INTO {0}.{1}.Dado 
                       (DescricaoDado, GlossarioDado, CodigoUnidadeMedida, CasasDecimais, ValorMinimo, ValorMaximo, CodigoFuncaoAgrupamentoDado, DataInclusao, CodigoUsuarioInclusao)
                 VALUES('{2}'        , '{3}'        , {4}                , {5}          , {6}        , {7}        , {8}                        , GETDATE()   ,                   {9})

                SET @codigoDado = scope_identity()

                 INSERT INTO {0}.{1}.DadoUnidade
                       (CodigoDado, CodigoUnidadeNegocio, IndicaUnidadeCriadoraDado, DataInclusao, CodigoUsuarioInclusao,CodigoReservado)
                 VALUES(@codigoDado, {10}               , 'S'                      , GETDATE()   , {9},           '{11}' )


                SELECT @codigoDado AS codigoDado", bancodb, Ownerdb, descricaoDado.Replace("'", "''"), glossarioDado.Replace("'", "''"), codigoUnidadeMedida, casasDecimais, valorMinimo.Replace(",", "."), valorMaximo.Replace(",", "."), CodigoAgrupamentoDado, CodigoUsuarioInclusao, codigoEntidad, codigoReservado);

        return getDataSet(comandoSQL);
    }

    public bool atualizaDadoIndicadores(string descricaoDado, string glossarioDado, int codigoUnidadeMedida, int casasDecimais, string valorMinimo, string valorMaximo, int CodigoAgrupamentoDado, int CodigoDado, string codigoReservado, int codigoUnidadeNegocio)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
            BEGIN
                UPDATE {0}.{1}.Dado
                   SET DescricaoDado = '{2}'
                      ,GlossarioDado = '{3}'
                      ,CodigoUnidadeMedida = {4}
                      ,CasasDecimais = {5}
                      ,ValorMinimo = {6}
                      ,ValorMaximo = {7}
                      ,CodigoFuncaoAgrupamentoDado = {8}
                 WHERE CodigoDado = {9}

                UPDATE {0}.{1}.[DadoUnidade]
                   SET [CodigoReservado] = '{10}'
                 WHERE (CodigoDado = {9} AND CodigoUnidadeNegocio = {11})
            END", bancodb, Ownerdb, descricaoDado, glossarioDado.Replace("'", "''"), codigoUnidadeMedida, casasDecimais, valorMinimo.Replace(",", "."), valorMaximo.Replace(",", "."), CodigoAgrupamentoDado, CodigoDado, codigoReservado, codigoUnidadeNegocio);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool excluiDadoIndicadores(int CodigoDado, int codigoUsuario, ref string msg)
    {
        try
        {
            comandoSQL = string.Format(@"
                BEGIN
                    UPDATE {0}.{1}.Dado 
                    SET  DataExclusao=GETDATE()
                        ,CodigoUsuarioExclusao={3}
                    WHERE CodigoDado = {2}

                    UPDATE {0}.{1}.DadoUnidade
                    SET DataExclusao=GETDATE()
                        ,CodigoUsuarioExclusao={3}
                    WHERE CodigoDado = {2}
                END", bancodb, Ownerdb, CodigoDado, codigoUsuario);
            int registrosAfetados = 0;
            execSQL(comandoSQL, ref registrosAfetados);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public DataSet getRelAnaliseDados(int codigoUsuarioResponsavel, int codigoEntidadeLogada, int? codigoMapa, int? codigoIndicador)
    {
        string strCodigoIndicador = codigoIndicador.HasValue ? codigoIndicador.Value.ToString() : "NULL";
        string strCodigoMapa = codigoMapa.HasValue ? codigoMapa.Value.ToString() : "NULL";
        if ((codigoIndicador.HasValue == false) && (codigoMapa.HasValue == false))
            return null;
        else
        {
            string comandoSQL = string.Format(@"
                SELECT * FROM {0}.{1}.[f_GetDadosOLAP_AnaliseDadosEstrategia] ({2}, {3}, {4}, {5})

        ", bancodb, Ownerdb, codigoUsuarioResponsavel, codigoEntidadeLogada, strCodigoMapa, strCodigoIndicador);
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }
    }

    public DataSet getRelAnaliseDadosIndicador(int codigoUsuarioResponsavel, int codigoEntidadeLogada, int codigoIndicador, int codigoMapa)
    {
        string comandoSQL = string.Format(@"
                BEGIN
                      DECLARE @CodigoIndicadorParam Int,
                              @CodigoUsuarioParam  Int,
                              @CodigoEntidadeParam Int,
                              @CodigoDado      Int,
                              @CodigoMapaParam   Int,
                              @TipoInformacao    Char(1),
                              @FuncaoIndicador   Varchar(50)
                      
                          SET @CodigoUsuarioParam = {2}
                          SET @CodigoIndicadorParam = {4}
                          SET @CodigoEntidadeParam = {3}
                          SET @CodigoMapaParam = {5}
                      
                          DECLARE @Dados TABLE  
                          (
                              [Unidade]   Varchar(100)
                              , [CodigoDado] Int
                              , [Dado] Varchar(100)
                              , [Ano]  Smallint
                              , [Mes]  Varchar(3)
                              , [Valor]   Decimal(18,4)
                              , [Medida]  Varchar(7)
                              , [Funcao]  Varchar(50)
                              , [Decimais] Tinyint
                          )

                          DECLARE cCursor CURSOR LOCAL FOR
                           
                           SELECT CodigoDado, 'D', NULL
                             FROM {0}.{1}.DadoIndicador
                            WHERE CodigoIndicador = @CodigoIndicadorParam
                           
                            UNION
                       
                            --SELECT CodigoDado, 'D', NULL
                            -- FROM dbo.RelacaoIndicador AS ri INNER JOIN
                            --      dbo.DadoIndicador AS di ON (di.CodigoIndicador = ri.CodigoIndicadorRelacionado)
                            --WHERE ri.CodigoIndicador = @CodigoIndicadorParam
                       
                            SELECT CodigoIndicadorRelacionado, 'I', tfd.NomeFuncaoBD
                              FROM {0}.{1}.RelacaoIndicador AS ri 
                         LEFT JOIN {0}.{1}.TipoFuncaoDado AS tfd ON (tfd.CodigoFuncao = ri.CodigoFuncaoIndicador)
                             WHERE CodigoIndicador = @CodigoIndicadorParam

                       OPEN cCursor
                       
                       FETCH NEXT FROM cCursor INTO  @CodigoDado, @TipoInformacao, @FuncaoIndicador
                       
                       WHILE @@FETCH_STATUS = 0 
                         BEGIN
                           IF @TipoInformacao = 'D'
                              INSERT INTO @Dados
                                   SELECT [Unidade]        
                                        , [CodigoDado]      
                                        , [Dado]         
                                        , [Ano]          
                                        , [Mes]          
                                        , [Valor]         
                                        , [Medida]        
                                        , [Funcao]        
                                        , [Decimais]       
                                   FROM {0}.{1}.f_GetDadosOLAP_AnaliseDadosEstrategia (@CodigoUsuarioParam, @CodigoEntidadeParam, @CodigoMapaParam, @CodigoDado)
                           ELSE
                               INSERT INTO @Dados
                                    SELECT [Unidade]        
                                        , [CodigoIndicador]      
                                        , [Indicador]         
                                        , [Ano]          
                                        , [Mes]          
                                        , [Resultado]
                                        , [Medida]        
                                        , @FuncaoIndicador        
                                        , [Decimais]       
                                    FROM {0}.{1}.[f_GetDadosOLAP_IndicadorUndidades] (@CodigoUsuarioParam, @CodigoEntidadeParam, @CodigoDado, @CodigoEntidadeParam)  
                
                           FETCH NEXT FROM cCursor INTO @CodigoDado, @TipoInformacao, @FuncaoIndicador

                         END

                      CLOSE cCursor
                      
                      DEALLOCATE cCursor
                      
                      SELECT * FROM @Dados
                    END", bancodb, Ownerdb, codigoUsuarioResponsavel, codigoEntidadeLogada, codigoIndicador, codigoMapa <= 0 ? "null" : codigoMapa.ToString());
        DataSet ds = getDataSet(comandoSQL);
        return ds;

    }

    public DataSet getDadosOLAP_AnaliseIndicador(int codigoUsuario, int? codigoIndicador, int? codigoUnidade, int codigoEntidade)
    {
        string strCodigoIndicador = codigoIndicador.HasValue ? codigoIndicador.Value.ToString() : "NULL";
        string strCodigoUnidade = codigoUnidade.HasValue ? codigoUnidade.Value.ToString() : "NULL";

        string comandoSQL = string.Format(@"
			 SELECT
		          [Indicador]		
		        , [Unidade]	
		        , [Ano]					
		        , [MesPorExtenso]				
		        , [Meta]				
		        , [Resultado]			
		        , [Desempenho]
		        , [MetaAcumulada]		
		        , [ResultadoAcumulado]	
		        , [DesempenhoAcumulado]				
		        , [Medida]				
		        , [Decimais]		
		        , [MetaRefAno]
		        , [ResultadoRefAno]		
		        , [DesempenhoRefAno]
		        , [MetaRefIndicador]
		        , [ResultadoRefIndicador]		
		        , [DesempenhoRefIndicador]		
            FROM {0}.{1}.f_GetDadosOLAP_IndicadorUndidades({2}, {5}, {3}, {4})", bancodb, Ownerdb, codigoUsuario, strCodigoIndicador, strCodigoUnidade, codigoEntidade);
        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getDadosOLAP_AnaliseIndicador2(int codigoUsuario, int? codigoIndicador, int? codigoUnidade, int? codigoMapa, int codigoEntidade)
    {
        string strCodigoIndicador = codigoIndicador.HasValue ? codigoIndicador.Value.ToString() : "NULL";
        string strCodigoUnidade = codigoUnidade.HasValue ? codigoUnidade.Value.ToString() : "NULL";
        string strCodigoMapa = codigoMapa.HasValue ? codigoMapa.Value.ToString() : "NULL";

        string comandoSQL = string.Format(@"
			 SELECT
		          [Indicador]		
		        , [Unidade]	
                , [MapaEstrategico]
                , [Perspectiva]
                , [Objetivo]
		        , [Ano]					
		        , [MesPorExtenso]				
		        , [Meta]				
		        , [Resultado]			
		        , [Desempenho]
		        , [DesempNum]
		        , [MetaAcumulada]		
		        , [ResultadoAcumulado]	
		        , [DesempenhoAcumulado]				
		        , [DesempNumAcumulado]
		        , [Medida]				
		        , [Decimais]		
		        , [MetaRefAno]
		        , [ResultadoRefAno]		
		        , [DesempenhoRefAno]
		        , [DesempNumRefAno]
		        , [MetaRefIndicador]
		        , [ResultadoRefIndicador]		
		        , [DesempenhoRefIndicador]		
		        , [DesempNumRefIndicador]
            FROM {0}.{1}.f_GetDadosOLAP_IndicadorUndidadesMapas({2}, {5}, {3}, {4},{6})", bancodb, Ownerdb, codigoUsuario, strCodigoIndicador, strCodigoUnidade, codigoEntidade, strCodigoMapa);
        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getUnidadesIndicadoresAcessoUsuario(int codigoUsuario, int codigoEntidadeLogada, string where)
    {
        string comandoSQL = string.Format(@"
			   SELECT un.CodigoUnidadeNegocio, un.NomeUnidadeNegocio, un.SiglaUnidadeNegocio
		         FROM {0}.{1}.UnidadeNegocio AS un 
		        WHERE un.[CodigoUnidadeNegocio] IN (SELECT DISTINCT f.[CodigoUnidadeNegocio] FROM {0}.{1}.f_GetUnidadesIndicadoresUsuario({2}, {3}, NULL, NULL, NULL) AS [f] )
                     {4} 

                ORDER BY un.NomeUnidadeNegocio"
            , bancodb, Ownerdb, codigoUsuario, codigoEntidadeLogada, where);
        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getIndicadoresAcessoUsuario(int codigoUsuario, int codigoEntidadeLogada, string where)
    {
        string comandoSQL = string.Format(@" 
                SELECT i.CodigoIndicador, i.NomeIndicador 
                  FROM {0}.{1}.Indicador i 
		         WHERE i.[CodigoIndicador] IN (SELECT DISTINCT f.[CodigoIndicador] FROM {0}.{1}.f_GetUnidadesIndicadoresUsuario({2}, {3}, NULL, NULL, NULL) AS [f] )
                     {4} 

                 ORDER BY i.NomeIndicador"
            , bancodb, Ownerdb, codigoUsuario, codigoEntidadeLogada, where);
        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    #endregion

    #region _Estrategias - Wizard - indicadorXobjetivoEstrategico.aspx

    public DataSet getComboObjetivo(string codigoMapaEstrategico)
    {
        string comandoSQL = string.Format(@"
                SELECT  CodigoObjetoEstrategia, 
                        TituloObjetoEstrategia,
                        DescricaoObjetoEstrategia
                FROM {0}.{1}.ObjetoEstrategia AS oe INNER JOIN
		             {0}.{1}.TipoObjetoEstrategia AS toe ON oe.CodigoTipoObjetoEstrategia = toe.CodigoTipoObjetoEstrategia
                WHERE oe.CodigoMapaEstrategico= {2}
                  AND toe.IniciaisTipoObjeto = 'OBJ'
                ", bancodb, Ownerdb, codigoMapaEstrategico);
        return getDataSet(comandoSQL);
    }

    public DataSet incluiDadoIndicadores(string codigoIndicador, string codigoObjetivoEstrategico)
    {
        comandoSQL = string.Format(@"
                    INSERT INTO {0}.{1}.IndicadorObjetivoEstrategico
                    (CodigoIndicador, CodigoObjetivoEstrategico)
                    VALUES({2}, {3})
                    ", bancodb, Ownerdb, codigoIndicador, codigoObjetivoEstrategico);

        return getDataSet(comandoSQL);
    }

    #endregion

    #region _Estrategias - Wizard - PeriodoEstrategico.aspx

    public DataSet getGridPeriodoEstrategico(string codigoUnidade)
    {
        string comandoSQL = string.Format(@"
                SELECT 
                    CodigoUnidadeNegocio, 
                    Ano, 
                    CASE IndicaAnoAtivo WHEN 'S' THEN '{3}'
                                        WHEN 'N' THEN '{4}' END AS AnoAtivo, 
                    CASE IndicaMetaEditavel WHEN 'S' THEN '{3}'
                                        WHEN 'N' THEN '{4}' END AS MetaEditavel,
                    CASE IndicaResultadoEditavel WHEN 'S' THEN '{3}'
                                        WHEN 'N' THEN '{4}' END AS ResultadoEditavel,
                    IndicaAnoAtivo, 
                    IndicaMetaEditavel,
                    IndicaResultadoEditavel,
                    IndicaTipoDetalheVisualizacao,
                    CASE WHEN IndicaTipoDetalheVisualizacao = 'A' THEN '{5}' ELSE '{6}' END AS TipoVisualizacao
                FROM {0}.{1}.PeriodoEstrategia
                WHERE CodigoUnidadeNegocio = {2}
                ", bancodb, Ownerdb, codigoUnidade, Resources.traducao.sim, Resources.traducao.nao, Resources.traducao.total, Resources.traducao.no_per_odo);

        return getDataSet(comandoSQL);
    }

    public bool incluiPeriodoEstrategico(string codigoUnidade, string ano, string indicaAnoAtivo, string indicaMetaEditavel,
                                            string indicaResultadoEditavel, string indicaTipoDetalheVisualizacao)
    {
        int regAf = 0;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"
                BEGIN
                    IF NOT EXISTS( SELECT 1 FROM {0}.{1}.PeriodoEstrategia WHERE CodigoUnidadeNegocio = {2} AND Ano = {3}) 
                    BEGIN
                        INSERT  INTO {0}.{1}.PeriodoEstrategia
                                (CodigoUnidadeNegocio, Ano, IndicaAnoAtivo, IndicaMetaEditavel, IndicaResultadoEditavel, IndicaTipoDetalheVisualizacao)
                        VALUES({2}, {3}, '{4}', '{5}', '{6}', '{7}');
                        SELECT 1
                    END
                END
                ", bancodb, Ownerdb, codigoUnidade, ano, indicaAnoAtivo, indicaMetaEditavel, indicaResultadoEditavel, indicaTipoDetalheVisualizacao);
            execSQL(comandoSQL, ref regAf);
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaPeriodoEstrategico(string codigoUnidade, string ano, string indicaAnoAtivo, string indicaMetaEditavel,
                                            string indicaResultadoEditavel, string indicaTipoDetalheVisualizacao, ref string msgErro)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                  UPDATE {0}.{1}.PeriodoEstrategia 
                     SET IndicaAnoAtivo = '{4}',
                         IndicaMetaEditavel = '{5}',
                         IndicaResultadoEditavel = '{6}',
                         IndicaTipoDetalheVisualizacao = '{7}'
                   WHERE CodigoUnidadeNegocio = {2}
                     AND Ano = {3}
                ", bancodb, Ownerdb, codigoUnidade, ano, indicaAnoAtivo, indicaMetaEditavel, indicaResultadoEditavel, indicaTipoDetalheVisualizacao);

            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgErro = ex.Message + " \n" + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    #endregion

    #region _Estrategias - Wizard - projetoObjetivos.aspx

    public bool incluiAssociacaoProjetoObjetivo(int codigoProjeto, int codigoObjetivoEstrategico,
                                      int[] acoesSugeridas, string prioridade, int codigoIndicador)
    {
        int regAf = 0;
        bool retorno = true;
        try
        {
            string comandoAcoes = "";

            foreach (int codigoAcao in acoesSugeridas)
            {
                comandoAcoes += string.Format(@"
                    INSERT INTO {0}.{1}.ProjetoAcoesSugeridas(CodigoProjeto, CodigoObjetivoEstrategico, CodigoAcaoSugerido)
                    VALUES({2}, {3}, {4})
                ", bancodb, Ownerdb, codigoProjeto, codigoObjetivoEstrategico, codigoAcao);
            }

            comandoSQL = string.Format(@"
                BEGIN
                    IF NOT EXISTS( SELECT 1 FROM {0}.{1}.ProjetoObjetivoEstrategico WHERE codigoProjeto = {2} AND codigoObjetivoEstrategico = {3}) 
                    BEGIN
                        INSERT  INTO {0}.{1}.ProjetoObjetivoEstrategico
                                (codigoProjeto, codigoObjetivoEstrategico, indicaObjetivoEstrategicoPrincipal, Prioridade, CodigoIndicador)
                        VALUES({2}, {3}, 'N', '{5}', {6});
                    END
                    {4}
                END
                ", bancodb, Ownerdb, codigoProjeto, codigoObjetivoEstrategico, comandoAcoes, prioridade
                 , codigoIndicador != -1 ? codigoIndicador.ToString() : "NULL");

            execSQL(comandoSQL, ref regAf);
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }


    public bool incluiAssociacaoProjetoObjetivoPeso(int codigoProjeto, int codigoObjetivoEstrategico, int[] acoesSugeridas, string prioridade, int codigoIndicador, string peso, ref string mensagemErro)
    {
        int regAf = 0;
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            string comandoAcoes = "";

            foreach (int codigoAcao in acoesSugeridas)
            {
                comandoAcoes += string.Format(@"
                    INSERT INTO {0}.{1}.ProjetoAcoesSugeridas(CodigoProjeto, CodigoObjetivoEstrategico, CodigoAcaoSugerido)
                    VALUES({2}, {3}, {4})
                ", bancodb, Ownerdb, codigoProjeto, codigoObjetivoEstrategico, codigoAcao);
            }

            comandoSQL = string.Format(@"
                BEGIN
                    IF NOT EXISTS( SELECT 1 FROM {0}.{1}.ProjetoObjetivoEstrategico WHERE codigoProjeto = {2} AND codigoObjetivoEstrategico = {3}) 
                    BEGIN
                        INSERT  INTO {0}.{1}.ProjetoObjetivoEstrategico
                                (codigoProjeto, codigoObjetivoEstrategico, indicaObjetivoEstrategicoPrincipal, Prioridade, CodigoIndicador)
                        VALUES({2}, {3}, 'N', '{5}', {6});
                    END
                    {4}
                END
                ", bancodb, Ownerdb, codigoProjeto, codigoObjetivoEstrategico, comandoAcoes, prioridade
                 , codigoIndicador != -1 ? codigoIndicador.ToString() : "NULL");
            try
            {
                retorno = execSQL(comandoSQL, ref regAf);
            }
            catch (Exception ex)
            {
                mensagemErro = ex.Message;
            }



            //Caso Parametro configuração sistema esteja configurado para salvar ParamutilizaPesoDesempenhoObjetivo
            DataSet dsParamutilizaPesoDesempenhoObjetivo = getParametrosSistema("utilizaPesoDesempenhoObjetivo");
            if (dsParamutilizaPesoDesempenhoObjetivo != null && dsParamutilizaPesoDesempenhoObjetivo.Tables[0].Rows.Count > 0 && dsParamutilizaPesoDesempenhoObjetivo.Tables[0].Rows[0]["utilizaPesoDesempenhoObjetivo"] + "" == "S")
            {
                comandoSQL = string.Format(
                                               @"INSERT INTO {0}.{1}.LinkObjeto(   CodigoObjeto,  --1
                                                                                CodigoTipoObjeto, --2
                                                                                CodigoObjetoLink, --3
                                                                                CodigoTipoObjetoLink, --4                                                                      
                                                                                CodigoTipoLink, --5                                           
                                                                                CodigoObjetoPai, --6
                                                                                CodigoObjetoPaiLink, --7
                                                                                PesoObjetoLink --8
                                                                            ) VALUES
                                                                                ({3}, --1
                                                                                (Select CodigoTipoAssociacao from TipoAssociacao where IniciaisTipoAssociacao = 'OB'), --2
                                                                                {5}, --3
                                                                                (select CodigoTipoAssociacao from TipoAssociacao where IniciaisTipoAssociacao = 'PR'), --4
                                                                                (select CodigoTipoLink from TipoLinkObjeto where IniciaisTipoLink = 'AS'), --5
                                                                                0, --6
                                                                                0, --7
                                                                                {4} --8
                                                                            )", bancodb, Ownerdb, codigoIndicador, codigoObjetivoEstrategico, peso, codigoProjeto);
                try
                {
                    retorno = execSQL(comandoSQL, ref registrosAfetados);
                }
                catch (Exception ex)
                {
                    mensagemErro = ex.Message;
                }

            }
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaAssociacaoProjetoObjetivo(int codigoProjeto, int codigoObjetivoEstrategico,
                                                   string prioridade, int codigoMeta, string peso, ref string mensagemErro)
    {

        DataSet dsParamutilizaPesoDesempenhoObjetivo = getParametrosSistema("utilizaPesoDesempenhoObjetivo");
        if (dsParamutilizaPesoDesempenhoObjetivo != null && dsParamutilizaPesoDesempenhoObjetivo.Tables[0].Rows.Count > 0 && dsParamutilizaPesoDesempenhoObjetivo.Tables[0].Rows[0]["utilizaPesoDesempenhoObjetivo"] + "" == "S")
        {
            comandoSQL = string.Format(@"
               UPDATE [LinkObjeto]
           SET [PesoObjetoLink] = {2}
        WHERE [CodigoObjeto] = {0}
          AND [CodigoTipoObjeto] = dbo.f_GetCodigoTipoAssociacao('OB')
		  AND [CodigoTipoObjetoLink]  = dbo.f_GetCodigoTipoAssociacao('PR')
          AND [CodigoObjetoLink] = {1} 
		  AND [CodigoTipoLink] = (select CodigoTipoLink  from TipoLinkObjeto where IniciaisTipoLink = 'AS')",
          codigoObjetivoEstrategico,
          codigoProjeto,
          peso);
            int registrosAfetados = 0;
            execSQL(comandoSQL, ref registrosAfetados);
        }

        int regAf = 0;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"
                UPDATE {0}.{1}.ProjetoObjetivoEstrategico
                SET     Prioridade      = '{4}'
                    {5}
                WHERE codigoProjeto = {2} AND codigoObjetivoEstrategico = {3}
                ", bancodb, Ownerdb, codigoProjeto, codigoObjetivoEstrategico, prioridade
                 , codigoMeta != -1 ? ",   CodigoIndicador = " + codigoMeta : ",   CodigoIndicador = NULL");

            execSQL(comandoSQL, ref regAf);
        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            retorno = false;
        }
        return retorno;
    }

    public bool excluiAssociacaoProjetoObjetivo(string codigoProjeto, string codigoObjetivoEstrategico, ref string mensagemErro)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"
                    DELETE {0}.{1}.ProjetoAcoesSugeridas 
                     WHERE codigoProjeto = {2} AND codigoObjetivoEstrategico = {3}
                    DELETE {0}.{1}.[ProjetoObjetivoEstrategico] 
                     WHERE codigoProjeto = {2} AND codigoObjetivoEstrategico = {3}
                    
        DELETE from LinkObjeto
        WHERE CodigoObjeto = {3} AND CodigoObjetoLink = {2} and
		CodigoTipoObjeto = (Select CodigoTipoAssociacao from TipoAssociacao where IniciaisTipoAssociacao = 'OB') and 
		CodigoTipoObjetoLink = (select CodigoTipoAssociacao from TipoAssociacao where IniciaisTipoAssociacao = 'PR') and 
		CodigoTipoLink = (select CodigoTipoLink from TipoLinkObjeto where IniciaisTipoLink = 'AS')
                    ", bancodb, Ownerdb, codigoProjeto, codigoObjetivoEstrategico);

            retorno = execSQL(comandoSQL, ref registrosAfetados);

        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;
    }

    public bool incluiAcoesSugeridasProjeto(int codigoProjeto, int codigoObjetivoEstrategico, int[] acoesSugeridas, ref string mensagemErro)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            string comandoAcoes = "";

            foreach (int codigoAcao in acoesSugeridas)
            {
                comandoAcoes += string.Format(@"INSERT INTO {0}.{1}.ProjetoAcoesSugeridas(CodigoProjeto, CodigoObjetivoEstrategico, CodigoAcaoSugerido)
                                                        VALUES({2}, {3}, {4})", bancodb, Ownerdb, codigoProjeto, codigoObjetivoEstrategico, codigoAcao);
            }

            comandoSQL = string.Format(@"
                    DELETE {0}.{1}.ProjetoAcoesSugeridas 
                     WHERE codigoProjeto = {2} AND codigoObjetivoEstrategico = {3}
                    {4}
                    
                    ", bancodb, Ownerdb, codigoProjeto, codigoObjetivoEstrategico, comandoAcoes);

            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            retorno = false;
        }
        return retorno;
    }

    #endregion


    #region _Estrategia - Wizard - PermissaoObjetoEstrategia.aspx

    public DataSet getMapaEstrategico(string codigoUnidade, string where)
    {
        try
        {
            string comandoSQL = string.Format(@"
                    SELECT me.CodigoMapaEstrategico, me.TituloMapaEstrategico, me.IndicaMapaEstrategicoAtivo, me.CodigoUnidadeNegocio
                    FROM {0}.{1}.MapaEstrategico AS me INNER JOIN 
                         {0}.{1}.UnidadeNegocio AS un 
                                 ON me.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio
                    WHERE IndicaMapaEstrategicoAtivo = 'S'
	                  AND un.CodigoEntidade = {2}
                      {3}
                     ", bancodb, Ownerdb, codigoUnidade, where);

            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public DataSet getObjetosParaDarPermissao(string iniciaisTO, string codigoEntidade, string codigoME, string selectSql)
    {
        try
        {
            string comandoSQL = string.Format(@"
                DECLARE
		              @IniciaisObjetoFilho      Char(3)
	                , @CodigoMapaEstrategico    Int
	                , @CodigoEntidade           Int
                	
                SET @IniciaisObjetoFilho        = '{2}'
                SET @CodigoMapaEstrategico      = {4}
                SET @CodigoEntidade             = {3}
                	
                SELECT 
			              oFil.[CodigoObjetoEstrategia]			    AS [CodigoObjeto]
		                , CASE tFil.[IniciaisTipoObjeto] 
				                WHEN 'MAP' THEN me.[TituloMapaEstrategico] 
				                WHEN 'PSP' THEN oFil.[TituloObjetoEstrategia] 
				                ELSE oFil.[DescricaoObjetoEstrategia] 
			              END										AS [NomeObjeto]
		                , oPai.[CodigoObjetoEstrategia]			    AS [CodigoObjetoPai]
		                , CASE tPai.[IniciaisTipoObjeto] 
				                WHEN 'MAP' THEN CAST( 'Mapa : ' + me.[TituloMapaEstrategico] AS Varchar(265) )
				                WHEN 'PSP' THEN CAST( 'Perspectiva : ' + oPai.[TituloObjetoEstrategia] AS Varchar(265) ) 
				                WHEN 'TEM' THEN CAST( 'Tema : ' + oPai.[DescricaoObjetoEstrategia] AS Varchar(265) ) 
			              END										AS [NomeObjetoPai]
                        , me.TituloMapaEstrategico DoMapaEstrategico
	                FROM 
		                {0}.{1}.[objetoEstrategia]				    AS [oFil]
                		
			                INNER JOIN {0}.{1}.[TipoObjetoEstrategia]	AS [tFil]	ON 
				                (tFil.[CodigoTipoObjetoEstrategia]	= oFil.[CodigoTipoObjetoEstrategia] )
                				
			                INNER JOIN {0}.{1}.[MapaEstrategico]	AS [me]		ON 
				                (me.[CodigoMapaEstrategico]			= oFil.[CodigoMapaEstrategico] )
                				
			                INNER JOIN {0}.{1}.[UnidadeNegocio]	    AS [un]		ON 
				                (un.CodigoUnidadeNegocio			= me.[CodigoUnidadeNegocio])
                		
			                LEFT JOIN {0}.{1}.[objetoEstrategia]	AS [oPai]	ON 
				                (oPai.[CodigoObjetoEstrategia]      = oFil.[CodigoObjetoEstrategiaSuperior]) 
                				
			                LEFT JOIN {0}.{1}.[TipoObjetoEstrategia] AS [tPai]	ON 
				                ( tPai.[CodigoTipoObjetoEstrategia]	= oPai.[CodigoTipoObjetoEstrategia])
	                WHERE
				            tFil.[IniciaisTipoObjeto]       = @IniciaisObjetoFilho
		                AND oFil.DataExclusao               IS NULL
		                AND (   @CodigoMapaEstrategico      = -1 
					         OR oFil.CodigoMapaEstrategico	= @CodigoMapaEstrategico )
		                AND un.[CodigoEntidade]				= @CodigoEntidade


                -- SELECT 
	            --        me.TituloMapaEstrategico, un.NomeUnidadeNegocio
                --        ,oe.CodigoObjetoEstrategia
                --        [5] --oe.* 
	            --   FROM 
		        --        {0}.{1}.ObjetoEstrategia			oe 
        		--
		        --        INNER JOIN {0}.{1}.TipoObjetoEstrategia toe on 
			    --        ( toe.CodigoTipoObjetoEstrategia = oe.CodigoTipoObjetoEstrategia)
        		--	
		        --        INNER JOIN {0}.{1}.MapaEstrategico  me ON 
		        --        ( me.CodigoMapaEstrategico = oe.CodigoMapaEstrategico)
        		--
			    --        INNER JOIN {0}.{1}.UnidadeNegocio   un ON 
			    --        (un.CodigoUnidadeNegocio = me.CodigoUnidadeNegocio)
                --  WHERE
	            --            toe.IniciaisTipoObjeto  = '{2}'
	            --        AND un.CodigoEntidade	    = {3}
	            --        {4} --AND oe.[CodigoMapaEstrategico] = {4}
                --  ORDER BY me.TituloMapaEstrategico, TituloObjetoEstrategia
                ", bancodb, Ownerdb, iniciaisTO, codigoEntidade, codigoME);

            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public DataSet getObjetoIndicadorParaDarPermissao(string codigoEntidade, string codigoME)
    {
        try
        {
            string comandoSQL = string.Format(@"
                    DECLARE
	                          @CodigoMapaEstrategico    Int
                            , @CodigoEntidade           Int

                    SET @CodigoMapaEstrategico  = {3}
                    SET @CodigoEntidade         = {2}

                    SELECT 
		                  i.[CodigoIndicador]                                                       AS [CodigoObjeto]
	                    , i.[NomeIndicador]                                                         AS [NomeObjeto]
	                    , oPai.[CodigoObjetoEstrategia]                                             AS [CodigoObjetoPai]
	                    , CAST( 'Objetivo : ' + oPai.[DescricaoObjetoEstrategia] AS Varchar(265))   AS [NomeObjetoPai]
                        , me.TituloMapaEstrategico                                                  AS [DoMapaEstrategico]
                    FROM 
	                    {0}.{1}.[Indicador]                                     AS [i]
                    	
		                    INNER JOIN {0}.{1}.[IndicadorUnidade]               AS [iu]     ON
			                    ( iu.[CodigoIndicador]          = i.[CodigoIndicador] )
                    	
		                    LEFT JOIN {0}.{1}.[IndicadorObjetivoEstrategico]    AS [ioe]    ON 
			                    (ioe.[CodigoIndicador]          = i.[CodigoIndicador])
                    		
		                    LEFT JOIN {0}.{1}.[objetoEstrategia]                AS [oPai]	ON 
			                    (oPai.[CodigoObjetoEstrategia]  = ioe.[CodigoObjetivoEstrategico])
                    			
		                    LEFT JOIN {0}.{1}.[MapaEstrategico]                 AS [me]		ON 
			                    (me.[CodigoMapaEstrategico]     = oPai.[CodigoMapaEstrategico] )
                    			
		                    LEFT JOIN {0}.{1}.[UnidadeNegocio]                  AS [un]		ON 
			                    (un.CodigoUnidadeNegocio        = me.[CodigoUnidadeNegocio])
                    	
                    WHERE
			                    i.[DataExclusao]                    IS NULL
	                    AND (   @CodigoMapaEstrategico              = -1 
				                    OR oPai.CodigoMapaEstrategico   = @CodigoMapaEstrategico )
	                    AND     iu.[CodigoUnidadeNegocio]           = @CodigoEntidade
	                    AND (   un.[CodigoEntidade]                 IS NULL 
                                    OR un.[CodigoEntidade]          = @CodigoEntidade	)
                    ORDER BY i.[NomeIndicador]
                ", bancodb, Ownerdb, codigoEntidade, codigoME);

            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    #endregion

    #region _Estrategia - Wizard - menuMapa - PerspectivasMapa.aspx

    public bool incluiPerspectivaMapa(int codigoMapaEstrategico, int codigoVersaoMapa, string iniciaisTipoObjeto
                                    , string TituloObjetoEstrategia, string glossario, int idUsuarioInclusao
                                    , int idResponsavel
                                    , ref string codigoNovoObjetivoEstrategico, ref string resultado, int codigoEntidade)
    {
        bool retorno = false;

        string comandoSQL = string.Format(@"
            IF EXISTS(SELECT 1 FROM {0}.{1}.ObjetoEstrategia WHERE TituloObjetoEstrategia = '{5}' AND CodigoMapaEstrategico = {2} AND DataExclusao IS NULL)
                BEGIN
                    SELECT -1 AS Codigo, 'ERROR' AS Resultado
                END
            ELSE
                BEGIN
                    DECLARE     @CodigoObjetoEstrategia      INT
                            ,   @CodigoMapaEstrategico          INT
                            ,   @CodigoVersaoMapaEstrategico    INT
                            ,   @CodigoTipoObjeto               INT
                            ,   @CodigoObjetoSuperior           INT

                    SET @CodigoMapaEstrategico          = {2}
                    SET @CodigoVersaoMapaEstrategico    = {3}
                    SELECT @CodigoTipoObjeto        = CodigoTipoObjetoEstrategia    FROM {0}.{1}.TipoObjetoEstrategia   WHERE IniciaisTipoObjeto = '{4}'
                    SELECT TOP 1 @CodigoObjetoSuperior    = CodigoObjetoEstrategia        FROM {0}.{1}.ObjetoEstrategia       WHERE CodigoMapaEstrategico = @CodigoMapaEstrategico AND CodigoObjetoEstrategiaSuperior IS NULL ORDER BY CodigoObjetoEstrategia

                    INSERT INTO {0}.{1}.ObjetoEstrategia( 
                                oe.CodigoMapaEstrategico
                            ,   oe.CodigoVersaoMapaEstrategico
                            ,   TituloObjetoEstrategia
                            ,   GlossarioObjeto
                            ,   CodigoTipoObjetoEstrategia
                            ,   AlturaObjetoEstrategia
                            ,   LarguraObjetoEstrategia 
                            ,   TopoObjetoEstrategia
                            ,   EsquerdaObjetoEstrategia
                            ,   CorFundoObjetoEstrategia
                            ,   CorBordaObjetoEstrategia
                            ,   CorFonteObjetoEstrategia
                            ,   OrdemObjeto
                            ,   CodigoUsuarioInclusao
                            ,   DataInclusao
                            ,   CodigoResponsavelObjeto
                            ,   CodigoObjetoEstrategiaSuperior
                            )
                    VALUES (
                            @CodigoMapaEstrategico
                        ,   @CodigoVersaoMapaEstrategico
                        ,   '{5}'
                        ,   {6}
                        ,   @CodigoTipoObjeto
                        ,   170
                        ,   786
                        ,   188
                        ,   4
                        ,   '#FFFFFF'
                        ,   '#800000'
                        ,   '#000000'
                        ,   1
                        ,   {7}
                        ,   GETDATE()
                        ,   {8}
                        ,   @CodigoObjetoSuperior
                        )

                    SET @CodigoObjetoEstrategia = SCOPE_IDENTITY()

                    SELECT @CodigoObjetoEstrategia AS Codigo, 'OK' AS Resultado

                    --Em seguida à gravação dos dados, deve ser acionada a proc [p_ProcessaMatrizAcessos] 
                    --para que a matriz de acesso do usuário logado seja refeita para o mapa cujos dados estão sendo gravadas;
                    
                    DECLARE @RC int
                    DECLARE @in_iniciaisTipoObjeto char(2)
                        DECLARE @in_codigoObjetoPai bigint
                    DECLARE @in_codigoEntidade int
                    DECLARE @in_codigoPermissao smallint
                    DECLARE @in_codigoUsuario int
                    DECLARE @in_codigoPerfil int
                    DECLARE @in_codigoObjeto INT

                    SET @in_iniciaisTipoObjeto =  'PP'       
                    SET @in_codigoObjeto = @CodigoObjetoEstrategia
                    SET @in_codigoObjetoPai = 0
                    SET @in_codigoEntidade = {9}
                    SET @in_codigoPermissao = NULL
                    SET @in_codigoUsuario = {7}
                    SET @in_codigoPerfil = NULL

                    EXECUTE @RC = [dbo].[p_ProcessaMatrizAcessos] 
                        @in_iniciaisTipoObjeto
                        ,@in_codigoObjeto
                        ,@in_codigoObjetoPai
                        ,@in_codigoEntidade
                        ,@in_codigoPermissao
                        ,@in_codigoUsuario
                        ,@in_codigoPerfil

                   --Em seguida ao acionamento da proc do item anterior, deve ser acionada outra proc, 
                   --a [p_incluiJobProcessaMatrizAcessos] para que seja incluído um job que referá a matriz de permissão do mapa para todos os usuários;

                    EXECUTE @RC = [dbo].[p_incluiJobProcessaMatrizAcessos] 
                       @in_iniciaisTipoObjeto
                      ,@in_codigoObjeto
                      ,@in_codigoObjetoPai
                      ,@in_codigoEntidade
                      ,@in_codigoPermissao
                      ,NULL
                      ,@in_codigoPerfil
                      ,NULL

                END
            ", bancodb, Ownerdb, codigoMapaEstrategico, codigoVersaoMapa, iniciaisTipoObjeto
             , TituloObjetoEstrategia
             , glossario != "" ? "'" + glossario.Trim().Replace("'", "''") + "'" : "NULL"
             , idUsuarioInclusao
             , idResponsavel != -1 ? idResponsavel.ToString() : "NULL"
             , codigoEntidade);

        try
        {
            DataSet ds = getDataSet(comandoSQL);
            codigoNovoObjetivoEstrategico = ds.Tables[0].Rows[0]["Codigo"].ToString();
            resultado = ds.Tables[0].Rows[0]["Resultado"].ToString();
            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaPerspectivaMapa(string tituloObjeto, int idUsuarioEdicao, string glossario
                                        , int idObjetoEstrategia, int idMapaEstrategia, int idVersaoMapa
                                        , int idResponsavel, ref string resultado, int codigoEntidade)
    {
        bool retorno = false;

        string comandoSQL = string.Format(@"
            IF EXISTS(SELECT 1 FROM {0}.{1}.ObjetoEstrategia WHERE TituloObjetoEstrategia = '{2}' AND CodigoObjetoEstrategia != {5} AND CodigoMapaEstrategico = {6} AND DataExclusao IS NULL)
                BEGIN
                    SELECT 'ERROR' AS Resultado
                END
            ELSE
                BEGIN
                    UPDATE {0}.{1}.ObjetoEstrategia

                    SET     TituloObjetoEstrategia          = '{2}'
                        ,   GlossarioObjeto                 = {3}
                        ,   CodigoUsuarioUltimaAlteracao    = {4}
                        ,   DataUltimaAlteracao             = GETDATE()
                        ,   CodigoResponsavelObjeto         = {8}

                    WHERE   CodigoObjetoEstrategia      = {5}
                        AND CodigoMapaEstrategico       = {6}
                        AND CodigoVersaoMapaEstrategico = {7}


--Em seguida à gravação dos dados, deve ser acionada a proc [p_ProcessaMatrizAcessos] 
                    --para que a matriz de acesso do usuário logado seja refeita para o mapa cujos dados estão sendo gravadas;
                    
                    DECLARE @RC int
                    DECLARE @in_iniciaisTipoObjeto char(2)
                        DECLARE @in_codigoObjetoPai bigint
                    DECLARE @in_codigoEntidade int
                    DECLARE @in_codigoPermissao smallint
                    DECLARE @in_codigoUsuario int
                    DECLARE @in_codigoPerfil int
                    DECLARE @in_codigoObjeto INT

                    SET @in_iniciaisTipoObjeto =  'PP'       
                    SET @in_codigoObjeto = {5}
                    SET @in_codigoObjetoPai = 0
                    SET @in_codigoEntidade = {9}
                    SET @in_codigoPermissao = NULL
                    SET @in_codigoUsuario = {7}
                    SET @in_codigoPerfil = NULL

                    EXECUTE @RC = [dbo].[p_ProcessaMatrizAcessos] 
                        @in_iniciaisTipoObjeto
                        ,@in_codigoObjeto
                        ,@in_codigoObjetoPai
                        ,@in_codigoEntidade
                        ,@in_codigoPermissao
                        ,@in_codigoUsuario
                        ,@in_codigoPerfil

                   --Em seguida ao acionamento da proc do item anterior, deve ser acionada outra proc, 
                   --a [p_incluiJobProcessaMatrizAcessos] para que seja incluído um job que referá a matriz de permissão do mapa para todos os usuários;

                    EXECUTE @RC = [dbo].[p_incluiJobProcessaMatrizAcessos] 
                       @in_iniciaisTipoObjeto
                      ,@in_codigoObjeto
                      ,@in_codigoObjetoPai
                      ,@in_codigoEntidade
                      ,@in_codigoPermissao
                      ,NULL
                      ,@in_codigoPerfil
                      ,NULL


                    SELECT 'OK' AS Resultado
                END
                ", bancodb, Ownerdb, tituloObjeto
                 , (glossario != "" ? "'" + glossario.Trim().Replace("'", "''") + "'" : "NULL")
                 , idUsuarioEdicao, idObjetoEstrategia
                 , idMapaEstrategia, idVersaoMapa
                 , idResponsavel != -1 ? idResponsavel.ToString() : "NULL"
         , codigoEntidade);

        try
        {
            DataSet ds = getDataSet(comandoSQL);
            resultado = ds.Tables[0].Rows[0]["Resultado"].ToString();
            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool excluiPerspectivaMapa(int idObjetoEstrategia, int idMapaEstrategia, int idVersaoMapa, int idUsuarioExclusao
                                    , ref string resultado)
    {
        bool retorno = false;

        string comandoSQL = string.Format(@"
            IF EXISTS(SELECT 1 FROM {0}.{1}.ObjetoEstrategia where CodigoObjetoEstrategiaSuperior = {3} AND DataExclusao IS NULL)
                BEGIN
                    SELECT 'ERROR' AS Resultado
                END
            ELSE
                BEGIN
                    UPDATE {0}.{1}.ObjetoEstrategia

                    SET     CodigoUsuarioExclusao = {2}
                        ,   DataExclusao = GETDATE()

                    WHERE   CodigoObjetoEstrategia      = {3}
                        AND CodigoMapaEstrategico       = {4}
                        AND CodigoVersaoMapaEstrategico = {5}
                    
                    SELECT 'OK' AS Resultado

                END
                ", bancodb, Ownerdb, idUsuarioExclusao
                 , idObjetoEstrategia, idMapaEstrategia, idVersaoMapa);

        try
        {
            DataSet ds = getDataSet(comandoSQL);
            resultado = ds.Tables[0].Rows[0]["Resultado"].ToString();
            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    #endregion
}
}