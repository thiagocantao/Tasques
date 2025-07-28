using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;

/// <summary>
/// Summary description for myDados
/// </summary>


namespace BriskPtf
{
public partial class dados
{
    #region _Projetos - Visão Corporativa

    public DataSet getResumoDesempenhoGeralEntidade(int codigoUsuario, int codigoEntidade, int ano, string where)
    {
        string colunaNoAno = "";

        if (ano != -1)
            colunaNoAno = "NoAno";

        string comandoSQL = string.Format(
             @"BEGIN
                      DECLARE @DesempenhoTrabalho Numeric(18,4),
                              @TrabalhoLBData     Numeric(18,4),
                              @TrabalhoRealData   Numeric(18,4),
                              @UltimoStatusTrabalho SmallInt,
                              @CorTrabalho        Varchar(30)

                      DECLARE @DesempenhoCusto Numeric(18,4),
                              @CustoLBData    Numeric(18,4),
                              @CustoRealData   Numeric(18,4),
                              @UltimoStatusCusto SmallInt,
                              @CorCusto        Varchar(30)

                      DECLARE @DesempenhoReceita Numeric(18,4),
                              @ReceitaLBData    Numeric(18,4),
                              @ReceitaRealData   Numeric(18,4),
                              @UltimoStatusReceita SmallInt,
                              @CorReceita        Varchar(30)

                         
                         SELECT @TrabalhoLBData = Sum(rp.TrabalhoLBData),
                                @TrabalhoRealData = Sum(rp.TrabalhoRealTotal),
                                @CustoLBData = Sum(rp.CustoLBData{5}),
                                @CustoRealData = Sum(rp.CustoRealTotal{5}),
                                @ReceitaLBData = Sum(rp.ReceitaLBData{5}),
                                @ReceitaRealData = Sum(rp.ReceitaRealData{5})
                           FROM {0}.{1}.ResumoProjeto rp,
                                {0}.{1}.Projeto p
                           INNER JOIN {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                    AND s.IndicaAcompanhamentoExecucao = 'S')         
                           WHERE rp.codigoProjeto = p.codigoProjeto
                             AND p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ')
                            {4}/*Somente os projetos em execução*/
                      
                      
                         IF @TrabalhoLBData = 0
                            SET @DesempenhoTrabalho = 0
                         ELSE
                            SET @DesempenhoTrabalho = @TrabalhoRealData / @TrabalhoLBData


                         SELECT TOP 1 @UltimoStatusTrabalho = par_ind.codigotipostatus 
                           FROM {0}.{1}.parametroindicadores par_ind
                          WHERE (@DesempenhoTrabalho*100 >= par_ind.valorinicial
                            AND @DesempenhoTrabalho*100 <= par_ind.valorfinal)
                            AND par_ind.tipoindicador = 'TRA'          

                         SELECT @CorTrabalho = ca.CorApresentacao
                           FROM {0}.{1}.TipoStatusAnalise AS tsa 
                           INNER JOIN {0}.{1}.CorApresentacao AS ca ON ca.CodigoCorApresentacao = tsa.CodigoCorApresentacao
                           WHERE CodigoTipoStatus = @UltimoStatusTrabalho

                     
                         IF @CustoLBData = 0
                            SET @DesempenhoCusto = 0
                         ELSE
                            SET @DesempenhoCusto = @CustoRealData / @CustoLBData


                         SELECT TOP 1 @UltimoStatusCusto = par_ind.codigotipostatus 
                           FROM {0}.{1}.parametroindicadores par_ind
                          WHERE (@DesempenhoCusto*100 >= par_ind.valorinicial
                            AND @DesempenhoCusto*100 <= par_ind.valorfinal)
                            AND par_ind.tipoindicador = 'FIN'          

                         SELECT @CorCusto = ca.CorApresentacao
                           FROM {0}.{1}.TipoStatusAnalise AS tsa 
                           INNER JOIN {0}.{1}.CorApresentacao AS ca ON ca.CodigoCorApresentacao = tsa.CodigoCorApresentacao
                           WHERE CodigoTipoStatus = @UltimoStatusCusto


                         IF @ReceitaLBData = 0
                            SET @DesempenhoReceita = 0
                         ELSE
                            SET @DesempenhoReceita = @ReceitaRealData / @ReceitaLBData


                         SELECT TOP 1 @UltimoStatusReceita = par_ind.codigotipostatus 
                           FROM {0}.{1}.parametroindicadores par_ind
                          WHERE (@DesempenhoReceita*100 >= par_ind.valorinicial
                            AND @DesempenhoReceita*100 <= par_ind.valorfinal)
                            AND par_ind.tipoindicador = 'REC'          

                         SELECT @CorReceita = ca.CorApresentacao
                           FROM {0}.{1}.TipoStatusAnalise AS tsa 
                           INNER JOIN {0}.{1}.CorApresentacao AS ca ON ca.CodigoCorApresentacao = tsa.CodigoCorApresentacao
                           WHERE CodigoTipoStatus = @UltimoStatusReceita

                         SELECT @CorTrabalho AS CorFisico, @CorCusto AS CorFinanceiro, @CorReceita AS CorReceita,
                                ISNULL(Sum(rp.TrabalhoLBData), 0) AS TotalTrabalhoPrevisto,
                                ISNULL(Sum(rp.TrabalhoLBTotal), 0) AS TotalTrabalhoPrevistoGeral,
                                ISNULL(Sum(rp.TrabalhoRealTotal), 0) AS TotalTrabalhoReal,
                                ISNULL(Sum(rp.CustoLBData{5}), 0) AS TotalCustoOrcado,
                                ISNULL(Sum(rp.CustoLBTotal{5}), 0) AS TotalCustoOrcadoGeral,
                                ISNULL(Sum(rp.CustoRealTotal{5}), 0) AS TotalCustoReal,
                                ISNULL(Sum(rp.ReceitaLBData{5}), 0) AS TotalReceitaOrcada,
                                ISNULL(Sum(rp.ReceitaLBTotal{5}), 0) AS TotalReceitaOrcadaGeral,
                                ISNULL(Sum(rp.ReceitaRealData{5}), 0) AS TotalReceitaReal
                           FROM {0}.{1}.ResumoProjeto AS rp INNER JOIN 
                                {0}.{1}.Projeto AS p ON p.CodigoProjeto = rp.CodigoProjeto INNER JOIN 
                                {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                    AND s.IndicaAcompanhamentoExecucao = 'S')                    
                          WHERE p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ')
                            {4} 

                    END", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where, colunaNoAno);
        return getDataSet(comandoSQL);
    }

    public DataSet getOrcamentoTrabalhoProjetos(int codigoUsuario, int codigoEntidade, int ano, string where)
    {
        string colunaNoAno = "";

        if (ano != -1)
            colunaNoAno = "NoAno";

        string comandoSQL = string.Format(
             @"SELECT ISNULL(Sum(rp.TrabalhoLBData), 0) AS TotalTrabalhoPrevisto,
                     ISNULL(Sum(rp.TrabalhoLBTotal), 0) AS TotalTrabalhoPrevistoGeral,
                     ISNULL(Sum(rp.TrabalhoRealTotal), 0) AS TotalTrabalhoReal,
                     ISNULL(Sum(rp.CustoLBData{4}), 0) AS TotalCustoOrcado,
                     ISNULL(Sum(rp.CustoLBTotal{4}), 0) AS TotalCustoOrcadoGeral,
                     ISNULL(Sum(rp.CustoRealTotal{4}), 0) AS TotalCustoReal,
                     ISNULL(Sum(rp.ReceitaLBData{4}), 0) AS TotalReceitaOrcada,
                     ISNULL(Sum(rp.ReceitaLBTotal{4}), 0) AS TotalReceitaOrcadaGeral,
                     ISNULL(Sum(rp.ReceitaRealData{4}), 0) AS TotalReceitaReal
                FROM {0}.{1}.ResumoProjeto AS rp INNER JOIN 
                     {0}.{1}.Projeto AS p ON p.CodigoProjeto = rp.CodigoProjeto INNER JOIN 
                     {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                    AND s.IndicaAcompanhamentoExecucao = 'S')                    
               WHERE p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ')
                 {5} 
               ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, colunaNoAno, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getCoresBulletsVCProjetos(int codigoUsuario, int codigoEntidade, int ano, string where)
    {
        string colunaNoAno = "";

        if (ano != -1)
            colunaNoAno = "NoAno";

        string comandoSQL = string.Format(
             @"BEGIN
                      DECLARE @DesempenhoTrabalho Numeric(18,4),
                              @TrabalhoLBData     Numeric(18,4),
                              @TrabalhoRealData   Numeric(18,4),
                              @UltimoStatusTrabalho SmallInt,
                              @CorTrabalho        Varchar(30)

                      DECLARE @DesempenhoCusto Numeric(18,4),
                              @CustoLBData    Numeric(18,4),
                              @CustoRealData   Numeric(18,4),
                              @UltimoStatusCusto SmallInt,
                              @CorCusto        Varchar(30)

                      DECLARE @DesempenhoReceita Numeric(18,4),
                              @ReceitaLBData    Numeric(18,4),
                              @ReceitaRealData   Numeric(18,4),
                              @UltimoStatusReceita SmallInt,
                              @CorReceita        Varchar(30)

                         
                         SELECT @TrabalhoLBData = Sum(rp.TrabalhoLBData),
                                @TrabalhoRealData = Sum(rp.TrabalhoRealTotal),
                                @CustoLBData = Sum(rp.CustoLBData{5}),
                                @CustoRealData = Sum(rp.CustoRealTotal{5}),
                                @ReceitaLBData = Sum(rp.ReceitaLBData{5}),
                                @ReceitaRealData = Sum(rp.ReceitaRealData{5})
                           FROM {0}.{1}.ResumoProjeto rp,
                                {0}.{1}.Projeto p
                           INNER JOIN {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                    AND s.IndicaAcompanhamentoExecucao = 'S')         
                           WHERE rp.codigoProjeto = p.codigoProjeto
                             AND p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ')
                            {4}/*Somente os projetos em execução*/
                      
                      
                         IF @TrabalhoLBData = 0
                            SET @DesempenhoTrabalho = 0
                         ELSE
                            SET @DesempenhoTrabalho = @TrabalhoRealData / @TrabalhoLBData


                         SELECT TOP 1 @UltimoStatusTrabalho = par_ind.codigotipostatus 
                           FROM {0}.{1}.parametroindicadores par_ind
                          WHERE (@DesempenhoTrabalho*100 >= par_ind.valorinicial
                            AND @DesempenhoTrabalho*100 <= par_ind.valorfinal)
                            AND par_ind.tipoindicador = 'TRA'          

                         SELECT @CorTrabalho = ca.CorApresentacao
                           FROM {0}.{1}.TipoStatusAnalise AS tsa 
                           INNER JOIN {0}.{1}.CorApresentacao AS ca ON ca.CodigoCorApresentacao = tsa.CodigoCorApresentacao
                           WHERE CodigoTipoStatus = @UltimoStatusTrabalho

                     
                         IF @CustoLBData = 0
                            SET @DesempenhoCusto = 0
                         ELSE
                            SET @DesempenhoCusto = @CustoRealData / @CustoLBData


                         SELECT TOP 1 @UltimoStatusCusto = par_ind.codigotipostatus 
                           FROM {0}.{1}.parametroindicadores par_ind
                          WHERE (@DesempenhoCusto*100 >= par_ind.valorinicial
                            AND @DesempenhoCusto*100 <= par_ind.valorfinal)
                            AND par_ind.tipoindicador = 'FIN'          

                         SELECT @CorCusto = ca.CorApresentacao
                           FROM {0}.{1}.TipoStatusAnalise AS tsa 
                           INNER JOIN {0}.{1}.CorApresentacao AS ca ON ca.CodigoCorApresentacao = tsa.CodigoCorApresentacao
                           WHERE CodigoTipoStatus = @UltimoStatusCusto


                         IF @ReceitaLBData = 0
                            SET @DesempenhoReceita = 0
                         ELSE
                            SET @DesempenhoReceita = @ReceitaRealData / @ReceitaLBData


                         SELECT TOP 1 @UltimoStatusReceita = par_ind.codigotipostatus 
                           FROM {0}.{1}.parametroindicadores par_ind
                          WHERE (@DesempenhoReceita*100 >= par_ind.valorinicial
                            AND @DesempenhoReceita*100 <= par_ind.valorfinal)
                            AND par_ind.tipoindicador = 'REC'          

                         SELECT @CorReceita = ca.CorApresentacao
                           FROM {0}.{1}.TipoStatusAnalise AS tsa 
                           INNER JOIN {0}.{1}.CorApresentacao AS ca ON ca.CodigoCorApresentacao = tsa.CodigoCorApresentacao
                           WHERE CodigoTipoStatus = @UltimoStatusReceita

                         SELECT @CorTrabalho AS CorFisico, @CorCusto AS CorFinanceiro, @CorReceita AS CorReceita

                    END", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where, colunaNoAno);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoProjetosVC(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT [Verde] AS [Satisfatorio], [Amarelo] AS [Atencao], [Laranja] AS Finalizando, 
                [Azul] AS [Excelente], [Branco] AS [Sem_Acompanhamento], [Vermelho] AS [Critico]
	        FROM
            ( SELECT rp.CorGeral, 1 AS [Qtde]
		            FROM {0}.{1}.ResumoProjeto as rp  
			            INNER JOIN {0}.{1}.Projeto p ON p.CodigoProjeto = rp.CodigoProjeto  
               WHERE p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ') 
                 AND p.CodigoStatusProjeto = 3
                {4}
            ) AS [TabelaOrigem]
            PIVOT (
                Count([Qtde]) FOR CorGeral IN ([Verde], [Amarelo], [Laranja], [Azul], [Branco], [Vermelho])
            ) AS TPIVOT
            ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoProjetosVCComCor(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
        @"BEGIN
            DECLARE @tblRetorno TABLE
		            (Situacao Varchar(50),
                     Quantidade Int)

            DECLARE @satisafatorio AS int, 
                    @Atencao AS int, 
                    @Finalizando AS int,
                    @Excelente AS int, 
                    @Sem_Acompanhamento as int,
                    @Critico AS int,
                    @contador as int

            DECLARE crWorks CURSOR LOCAL FAST_FORWARD FOR 

            SELECT [Verde] AS [Satisfatorio], 
                   [Amarelo] AS [Atencao], 
                   [Laranja] AS Finalizando, 
                   [Azul] AS [Excelente], 
                   [Branco] AS [Sem_Acompanhamento], 
                   [Vermelho] AS [Critico]
	          FROM
            ( 
               SELECT rp.CorGeral, 1 AS [Qtde]
		         FROM {0}.{1}.ResumoProjeto as rp  
		   INNER JOIN {0}.{1}.Projeto p ON p.CodigoProjeto = rp.CodigoProjeto  
               WHERE p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto 
                                               FROM TipoProjeto tp 
                                              WHERE tp.IndicaTipoProjeto = 'PRJ') 
                 AND p.CodigoStatusProjeto = 3
                {4}
            ) AS [TabelaOrigem]
            PIVOT (
                Count([Qtde]) FOR CorGeral IN ([Verde], [Amarelo], [Laranja], [Azul], [Branco], [Vermelho])
            ) AS TPIVOT
            
	OPEN crWorks;

		FETCH NEXT FROM crWorks INTO  @satisafatorio , 
									  @Atencao  , 
									  @Finalizando  ,
									  @Excelente  , 
									  @Sem_Acompanhamento  ,
									  @Critico  
		WHILE( @@FETCH_STATUS = 0) 
		BEGIN
                INSERT INTO @tblRetorno(Situacao, Quantidade) VALUES ('Satisfatório', @satisafatorio)
				INSERT INTO @tblRetorno(Situacao, Quantidade) VALUES ('Atenção', @Atencao)
				INSERT INTO @tblRetorno(Situacao, Quantidade) VALUES ('Finalizando', @Finalizando)
				INSERT INTO @tblRetorno(Situacao, Quantidade) VALUES ('Excelente', @Excelente)
				INSERT INTO @tblRetorno(Situacao, Quantidade) VALUES ('Sem Informação', @Sem_Acompanhamento)
			    INSERT INTO @tblRetorno(Situacao, Quantidade) VALUES ('Crítico', @Critico)
                FETCH NEXT FROM crWorks INTO  @satisafatorio , 
											  @Atencao  , 
											  @Finalizando  ,
											  @Excelente  , 
											  @Sem_Acompanhamento  ,
											  @Critico  
        END
        SELECT * FROM @tblRetorno
 END", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoMetasVC(int codigoEntidade, int codigoUsuario, char buscaSomenteResponsavel)
    {
        string comandoSQL = string.Format(
            @"BEGIN 

                SELECT 
			                f.[Atingidas]			AS Satisfatorio
		                , f.[Abaixo]				AS Atencao
		                , f.[Superadas]				AS Excelente
		                , CAST( 0 AS Int)			AS Laranja
		                , f.[Desatualizadas]	    AS Sem_Acompanhamento
		                , f.[MuitoAbaixo]			AS Critico
	                FROM 
		                {0}.{1}.f_GetNumerosDesempenhoMetasEstratégicas({3}, {2}, '{4}') AS [f] 

             END
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, buscaSomenteResponsavel);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoProjetosUnidadeAtendimentoVC(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT ISNULL(CodigoEntidade,-1) AS Codigo, ISNULL(SiglaUnidadeNegocio,'N/D') AS Descricao,  
                [Verde] AS [Verdes], [Amarelo] AS [Amarelos], [Vermelho] AS [Vermelhos], [Laranja] AS [Laranjas], [Branco] AS [Sem_Acompanhamento]
	        FROM
            (SELECT un.CodigoEntidade , un2.SiglaUnidadeNegocio, rp.CorGeral, 1 AS [Qtde]
		            FROM {0}.{1}.ResumoProjeto as rp  
			            INNER JOIN {0}.{1}.Projeto p ON (p.CodigoProjeto = rp.CodigoProjeto) 
                        INNER JOIN {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto AND s.IndicaAcompanhamentoExecucao = 'S') 
                        LEFT JOIN {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = p.CodigoUnidadeAtendimento)  
                        LEFT JOIN {0}.{1}.UnidadeNegocio AS un2 ON (un2.CodigoUnidadeNegocio = un.CodigoEntidade) 
               WHERE p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ') 
                {4}
            ) AS [TabelaOrigem]
            PIVOT (
                Count([Qtde]) FOR CorGeral IN ([Verde], [Amarelo], [Vermelho], [Laranja], [Branco])
            ) AS TPIVOT
            ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoProjetosUnidadeVC(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT CodigoUnidadeNegocio AS Codigo, SiglaUnidadeNegocio AS Descricao, 
                [Verde] AS [Verdes], [Amarelo] AS [Amarelos], [Vermelho] AS [Vermelhos], [Laranja] AS [Laranjas], [Branco] AS [Sem_Acompanhamento]
	        FROM
            ( SELECT un.CodigoUnidadeNegocio, un.SiglaUnidadeNegocio, rp.CorGeral, 1 AS [Qtde]
		            FROM {0}.{1}.ResumoProjeto as rp  
			            INNER JOIN {0}.{1}.Projeto p ON (p.CodigoProjeto = rp.CodigoProjeto) 
                        INNER JOIN {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto AND s.IndicaAcompanhamentoExecucao = 'S') 
                        INNER JOIN {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio) 
               WHERE p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ') 
                {4}
            ) AS [TabelaOrigem]
            PIVOT (
                Count([Qtde]) FOR CorGeral IN ([Verde], [Amarelo], [Vermelho], [Laranja], [Branco])
            ) AS TPIVOT
            ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoObrasUnidadeVC(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT CodigoUnidadeNegocio AS Codigo, SiglaUnidadeNegocio AS Descricao, 
                [Verde] AS [Verdes], [Amarelo] AS [Amarelos], [Vermelho] AS [Vermelhos], [Laranja] AS [Laranjas], [Branco] AS [Sem_Acompanhamento]
	        FROM
            ( SELECT un.CodigoUnidadeNegocio, un.SiglaUnidadeNegocio, rp.CorGeral, 1 AS [Qtde]
		            FROM {0}.{1}.ResumoProjeto as rp  
			            INNER JOIN {0}.{1}.Projeto p ON (p.CodigoProjeto = rp.CodigoProjeto) 
                        INNER JOIN {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto) 
                        INNER JOIN {0}.{1}.Obra AS o ON (o.CodigoProjeto = p.CodigoProjeto) 
                        INNER JOIN {0}.{1}.TipoServico AS ts ON (ts.CodigoTipoServico = o.CodigoTipoServico
                                                     AND ts.IndicaObra = 'S')    
                        INNER JOIN {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio) 
               WHERE p.DataExclusao IS NULL
                 AND s.IniciaisStatus NOT IN ('CANCELADO','SUSPENSO') 
                 AND p.CodigoEntidade = {3}    
                {4}
            ) AS [TabelaOrigem]
            PIVOT (
                Count([Qtde]) FOR CorGeral IN ([Verde], [Amarelo], [Vermelho], [Laranja], [Branco])
            ) AS TPIVOT
            ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getHistoricoDesempenhoProjetosVC(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT * FROM (
                SELECT top 6                       
                       RIGHT('0' + CONVERT(VarChar, p.Mes), 2) + '/' + CONVERT(VarChar, p.Ano) AS Periodo,
	                   ISNULL(Sum(CASE WHEN p.CorGeral = 'Verde' THEN 1 ELSE 0 END), 0) AS Verdes,
	                   ISNULL(Sum(CASE WHEN p.CorGeral = 'Amarelo' THEN 1 ELSE 0 END), 0) AS Amarelos,
	                   ISNULL(Sum(CASE WHEN p.CorGeral = 'Vermelho' THEN 1 ELSE 0 END), 0) AS Vermelhos,
	                   ISNULL(Sum(CASE WHEN p.CorGeral = 'Laranja' THEN 1 ELSE 0 END), 0) AS Laranjas, 
	                   ISNULL(Sum(CASE WHEN p.CorGeral = 'Branco' THEN 1 ELSE 0 END), 0) AS Sem_Acompanhamento, 
					   p.Ano, p.Mes
                  FROM {0}.{1}.HistoricoResumoProjeto p INNER JOIN
                       {0}.{1}.Projeto prj ON p.CodigoProjeto = prj.CodigoProjeto INNER JOIN
                       {0}.{1}.TipoProjeto tp ON (prj.CodigoTipoProjeto = tp.CodigoTipoProjeto 
                                              AND tp.IndicaTipoProjeto = 'PRJ')
                 WHERE p.IndicaPrograma = 'N'
                   {4}
                 GROUP BY RIGHT('0' + CONVERT(VarChar, p.Mes), 2) + '/' + CONVERT(VarChar, p.Ano), CONVERT(VarChar, p.Ano) + '/' + RIGHT('0' + CONVERT(VarChar, p.Mes), 2),
						  p.Ano, p.Mes
                 ORDER BY CONVERT(VarChar, p.Ano) + '/' + RIGHT('0' + CONVERT(VarChar, p.Mes), 2) desc) As tb ORDER BY Ano, Mes", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getHistoricoDesempenhoProjetosVC_ordena(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT  top 6
                       p.Ano * 100 + p.Mes   as codigoOrdenacao,
                       RIGHT('0' + CONVERT(VarChar, p.Mes), 2) + '/' + CONVERT(VarChar, p.Ano) AS Periodo,
	                   ISNULL(Sum(CASE WHEN p.CorGeral = 'Verde' THEN 1 ELSE 0 END), 0) AS Verdes,
	                   ISNULL(Sum(CASE WHEN p.CorGeral = 'Amarelo' THEN 1 ELSE 0 END), 0) AS Amarelos,
	                   ISNULL(Sum(CASE WHEN p.CorGeral = 'Vermelho' THEN 1 ELSE 0 END), 0) AS Vermelhos,
	                   ISNULL(Sum(CASE WHEN p.CorGeral = 'Laranja' THEN 1 ELSE 0 END), 0) AS Laranjas,
	                   ISNULL(Sum(CASE WHEN p.CorGeral = 'Branco' THEN 1 ELSE 0 END), 0) AS Sem_Acompanhamento
                  FROM {0}.{1}.HistoricoResumoProjeto p INNER JOIN
                       {0}.{1}.Projeto prj ON p.CodigoProjeto = prj.CodigoProjeto INNER JOIN
                       {0}.{1}.TipoProjeto tp ON (prj.CodigoTipoProjeto = tp.CodigoTipoProjeto 
                                              AND tp.IndicaTipoProjeto = 'PRJ')
                 WHERE p.IndicaPrograma = 'N'
                   {4}
                 --GROUP BY RIGHT('0' + CONVERT(VarChar, p.Mes), 2) + '/' + CONVERT(VarChar, p.Ano), CONVERT(VarChar, p.Ano) + '/' + RIGHT('0' + CONVERT(VarChar, p.Mes), 2)
                 GROUP BY p.Mes, p.ano
                  ORDER BY 1 desc --CONVERT(VarChar, p.Ano) + '/' + RIGHT('0' + CONVERT(VarChar, p.Mes), 2) asc", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }


    public DataSet getProdutosTaiPorStatusVC(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT tp.DescricaoSituacaoProduto, COUNT(tai.CodigoProjeto) AS Quantidade, CodigoCor
                  FROM  {0}.{1}.TipoSitucaoProduto AS tp 
                       LEFT JOIN {0}.{1}.tai_ProdutosAcoesIniciativa AS tai
                          ON ( tai.CodigoSituacaoProduto = tp.CodigoSituacaoProduto )
                       LEFT JOIN {0}.{1}.Projeto p
                          ON ( p.CodigoProjeto = tai.CodigoProjeto
                          AND p.CodigoEntidade = {2} )
                       LEFT JOIN {0}.{1}.Status s
                          ON ( s.CodigoStatus = p.CodigoStatusProjeto )
                WHERE ISNULL(s.IndicaAcompanhamentoExecucao,'S') = 'S'
                  {3}
                GROUP BY tp.DescricaoSituacaoProduto, CodigoCor
                ORDER BY 1", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoProdutosTai(DataTable dt, string titulo, int fonte)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();


        int codigoUsuarioResponsavel = int.Parse(getInfoSistema("IDUsuarioLogado").ToString());
        int codigoEntidadeUsuarioResponsavel = int.Parse(getInfoSistema("CodigoEntidade").ToString());

        try
        {

            foreach (DataRow dr in dt.Rows)
            {
                xmlAux.Append(string.Format(@"<set label=""{0}"" value=""{1}"" color=""{2}""/>", dr["DescricaoSituacaoProduto"].ToString()
                                                                                               , dr["Quantidade"].ToString()
                                                                                               , dr["CodigoCor"].ToString()));

            }

        }
        catch
        {
            return "";
        }

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
        xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\"" +
            " chartTopMargin=\"5\" chartRightMargin=\"8\" inThousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
            " adjustDiv=\"1\"  slantLabels=\"1\" labelDisplay=\"ROTATE\" canvasBorderThickness=\"1\" canvasBorderColor=\"A1A1A1\" " +
            " showValues=\"1\" " + usarGradiente + usarBordasArredondadas + exportar + " showShadow=\"0\" showLabels=\"1\" decimals=\"0\" baseFontSize=\"" + fonte + "\">");
        xml.Append(xmlAux.ToString());
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

    public DataSet getOrcamentoTrabalhoPorUnidade(int codigoUsuario, int codigoEntidade, int ano, string where)
    {
        string colunaNoAno = "";

        if (ano != -1)
            colunaNoAno = "NoAno";

        string comandoSQL = string.Format(
               @"SELECT u.CodigoUnidadeNegocio AS Codigo, u.SiglaUnidadeNegocio AS Descricao,
	                 ISNULL(Sum(rp.TrabalhoLBData), 0) AS TrabalhoPrevisto,
                     ISNULL(Sum(rp.TrabalhoRealTotal), 0) AS TrabalhoReal,
                     ISNULL(Sum(rp.CustoLBData{5}), 0) AS CustoPrevisto,
                     ISNULL(Sum(rp.CustoRealTotal{5}), 0) AS CustoReal,
                     ISNULL(Sum(rp.ReceitaLBData{5}), 0) AS ReceitaPrevista,
                     ISNULL(Sum(rp.ReceitaRealData{5}), 0) AS ReceitaReal
                  FROM {0}.{1}.UnidadeNegocio u INNER JOIN
					   {0}.{1}.Projeto p ON p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio INNER JOIN
	                   {0}.{1}.ResumoProjeto rp ON rp.CodigoProjeto = p.CodigoProjeto INNER JOIN
                       {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                    AND s.IndicaAcompanhamentoExecucao = 'S')                     
                WHERE p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ')
                  {4} 
                GROUP BY u.CodigoUnidadeNegocio, u.SiglaUnidadeNegocio
            ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where, colunaNoAno);
        return getDataSet(comandoSQL);
    }

    public string getGraficoPrevistoReal(DataTable dt, string colunaPrevisto, string colunaReal, string titulo, int fonte, bool linkLista)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();
        int i = 0;

        try
        {

            int codigoUsuarioResponsavel = int.Parse(getInfoSistema("IDUsuarioLogado").ToString());
            int codigoEntidadeUsuarioResponsavel = int.Parse(getInfoSistema("CodigoEntidade").ToString());

            bool podeAcessarLink = linkLista == true ? VerificaPermissaoUsuario(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "NULL", "EN", 0, "NULL", "AcsLstPrj") : false;

            linkLista = (linkLista == true && podeAcessarLink == true);

            xmlAux.Append("<categories>");

            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append("<category label=\"" + dt.Rows[i]["Descricao"] + "\"/>");

            }

            xmlAux.Append("</categories>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xmlAux.Append("<dataset seriesName=\"Previsto\" color=\"" + corPrevisto + "\">");

            string link = "";

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?CUN={0}&Programas=N"" ", dt.Rows[i]["Codigo"].ToString());

                xmlAux.Append(string.Format(@"<set label=""Previsto"" value=""{0}"" {1}/>", dt.Rows[i][colunaPrevisto].ToString()
                                                                                          , link));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Realizado\" color=\"" + corReal + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?CUN={0}&Programas=N"" ", dt.Rows[i]["Codigo"].ToString());

                xmlAux.Append(string.Format(@"<set label=""Realizado"" value=""{0}"" {1}/>", dt.Rows[i][colunaReal].ToString()
                                                                                          , link));
            }

            xmlAux.Append("</dataset>");
        }
        catch
        {
            return "";
        }

        float qtdProjetos = float.Parse(i.ToString()) / 2;

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
        xml.Append("<chart use3DLighting=\"0\" showShadow=\"0\" caption=\"" + titulo + "\" palette=\"1\" inDecimalSeparator=\",\" scrollHeight=\"12\" yAxisNamePadding=\"4\" chartLeftMargin=\"4\"" +
            " legendPadding=\"2\" showBorder=\"0\" baseFontSize=\"" + fonte + "\" " + //numVisiblePlot=\"10\"" +
            " chartRightMargin=\"20\" showLegend=\"1\" BgColor=\"F7F7F7\" slantLabels=\"1\" labelDisplay=\"ROTATE\" " +
            " canvasBgColor=\"F7F7F7\" chartTopMargin=\"10\" numVisiblePlot=\"6\" canvasBorderThickness=\"1\" canvasBorderColor=\"A1A1A1\" " +
            " decimalSeparator=\".\" " + usarGradiente + usarBordasArredondadas + exportar + " shownames=\"1\" adjustDiv=\"1\" showvalues=\"0\" decimals=\"2\" chartBottomMargin=\"10\">");

        xml.Append(xmlAux.ToString());
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

    public DataSet getNumerosVisaoProjetos(int codigoUsuario, int codigoEntidade, int ano, string where)
    {

        string colunaNoAno = "";

        if (ano != -1)
            colunaNoAno = "NoAno";

        string comandoSQL = string.Format(
               @"SELECT 
                        (SELECT count(1) 
                          FROM {0}.{1}.ResumoProjeto comp
                         WHERE comp.CodigoProjeto IN(SELECT p.codigoProjeto FROM {0}.{1}.Projeto p INNER JOIN 
                                                                                 {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                                                                            AND s.IndicaAcompanhamentoExecucao = 'S')                     
                                                                           WHERE p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ') 
                                                                             {4})) AS qtdProjetos,                                 
                        isNull(Sum(CustoLBData{5})     , 0) AS CustoOrcadoData,
                        isNull(Sum(CustoLBTotal{5})    , 0) AS CustoOrcadoTotal,
                        isNull(Sum(CustoRealData{5})   , 0) AS CustoRealData,
                        isNull(Sum(CustoRealTotal{5})  , 0) AS CustoRealTotal,
                        isNull(Sum(TrabalhoLBData)  , 0) AS TrabalhoOrcadoData,
                        isNull(Sum(TrabalhoLBTotal) , 0) AS TrabalhoOrcadoTotal,
                        isNull(Sum(TrabalhoRealData), 0) AS TrabalhoRealData,
                        isNull(Sum(TrabalhoRealTotal), 0) AS TrabalhoRealTotal,
                        isNull(Sum(ReceitaLBData{5})   , 0) AS ReceitaOrcadoData,
                        isNull(Sum(ReceitaLBTotal{5})  , 0) AS ReceitaOrcadoTotal,
                        isNull(Sum(ReceitaRealData{5}) , 0) AS ReceitaRealData,
                        isNull(Sum(ReceitaRealTotal{5}), 0) AS ReceitaRealTotal                                
                FROM {0}.{1}.ResumoProjeto rp INNER JOIN
                     {0}.{1}.Projeto p ON p.CodigoProjeto = rp.CodigoProjeto INNER JOIN 
                     {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                    AND s.IndicaAcompanhamentoExecucao = 'S')                     
               WHERE p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ')
                 {4}
            ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where, colunaNoAno);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region _Projetos - DadosProjeto - AnaliseDasMetas.aspx

    public DataSet getAnalisesCriticas_old(string codigoProjeto, int tipo)
    {
        //se tipo = 1 verifica se o projeto tem cr associado,
        //se tipo = 2 busca as analises do projeto em questão
        // Busca o nome do banco de dados do sistema de orçamento
        string nomeBancoSistemaOrcamento = "dbCdisOrcamento";

        DataSet dsParametros = getParametrosSistema("nomeBancoSistemaOrcamento");

        if (DataSetOk(dsParametros) && DataTableOk(dsParametros.Tables[0]) && dsParametros.Tables[0].Rows[0]["nomeBancoSistemaOrcamento"] + "" != "")
            nomeBancoSistemaOrcamento = dsParametros.Tables[0].Rows[0]["nomeBancoSistemaOrcamento"].ToString();
        string comandoSQL = "";
        if (tipo == 1)
        {
            comandoSQL = string.Format(@"
            
                DECLARE @CodigoProjeto INT
                    SET @CodigoProjeto = {2}

               select p.CodigoProjeto
                    from  {0}.{1}.Projeto p inner join
                          {0}.{1}.ProjetoCR pcr on (pcr.codigoProjeto = p.CodigoProjeto) 
                    where p.CodigoProjeto = @CodigoProjeto
        ", bancodb, Ownerdb, codigoProjeto);
        }
        else
        {
            comandoSQL = string.Format(@"
            
                DECLARE @CodigoProjeto INT
                    SET @CodigoProjeto = {2}

               select c.codigoCR, c.NomeCR,  ado.mes mes, ado.CausasProblemas
                    from  {0}.{1}.Projeto p inner join
                          {0}.{1}.ProjetoCR pcr on (pcr.codigoProjeto = p.CodigoProjeto) inner join
                          {3}.dbo.CR c on ( c.CodigoCR = pcr.CodigoCR) inner join
                          {3}.dbo.AnaliseDesvioOrcamento ado on (ado.CodigoCR = pcr.codigoCR)
                    where p.CodigoProjeto = @CodigoProjeto
                    order by ado.mes desc, c.NomeCR 
        ", bancodb, Ownerdb, codigoProjeto, nomeBancoSistemaOrcamento);
        }
        return getDataSet(comandoSQL);
    }

    public DataSet getAnalisesCriticas(string codigoProjeto)
    {

        // Busca o nome do banco de dados do sistema de orçamento
        string nomeBancoSistemaOrcamento = "dbCdisOrcamento";

        DataSet dsParametros = getParametrosSistema("nomeBancoSistemaOrcamento");

        if (DataSetOk(dsParametros) && DataTableOk(dsParametros.Tables[0]) && dsParametros.Tables[0].Rows[0]["nomeBancoSistemaOrcamento"] + "" != "")
            nomeBancoSistemaOrcamento = dsParametros.Tables[0].Rows[0]["nomeBancoSistemaOrcamento"].ToString();
        string comandoSQL = "";

        comandoSQL = string.Format(@"
            
                DECLARE @CodigoProjeto INT
                    SET @CodigoProjeto = {2}

                select distinct pcr.CodigoCR
                    from  {0}.{1}.Projeto p inner join
                          {0}.{1}.ProjetoCR pcr on (pcr.codigoProjeto = p.CodigoProjeto) 
                    where p.CodigoProjeto = @CodigoProjeto

               select c.codigoCR, c.NomeCR,  mo.ano, ado.mes mes, ado.CausasProblemas, 
               REPLICATE('0', 2 - DATALENGTH(convert(varchar(2), ado.mes)))+convert(varchar(2), ado.mes)+'/'+convert(varchar(4),mo.ano) as periodo
                    from  {0}.{1}.Projeto p inner join
                          {0}.{1}.ProjetoCR pcr on (pcr.codigoProjeto = p.CodigoProjeto) inner join
                          {3}.dbo.CR c on ( c.CodigoCR = pcr.CodigoCR) inner join
                          {3}.dbo.AnaliseDesvioOrcamento ado on (ado.CodigoCR = pcr.codigoCR) inner join
                          {3}.dbo.movimentoorcamento mo on mo.codigomovimentoorcamento = c.codigomovimentoorcamento 
                    where p.CodigoProjeto = @CodigoProjeto
                    order by mo.ano desc, ado.mes desc, c.NomeCR 
        ", bancodb, Ownerdb, codigoProjeto, nomeBancoSistemaOrcamento);

        return getDataSet(comandoSQL);
    }

    public DataSet getAnalisePerformanceObjeto(string codigoObjeto, string ano, string mes, string tipoAssociacao)
    {
        string where = "";

        if (ano != "")
            where += " AND Ano = " + ano;

        if (mes != "")
            where += " AND Mes = " + mes;

        comandoSQL = string.Format(@"
            --Select UM dados do [AnalisePerformanceProjeto], filtrado pelo CodigoIndicador-CodigoProjeto-Ano-Mes
                DECLARE @CodigoObjeto INT
                DECLARE @CodigoTipoAssociacao AS INT
                    SET @CodigoObjeto = {2}

                SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao FROM TipoAssociacao WHERE IniciaisTipoAssociacao = '{3}'

                SELECT app.*, ui.NomeUsuario As UsuarioInclusao,  ua.NomeUsuario As UsuarioAlteracao, 
                (CASE WHEN EXISTS(SELECT 1 FROM {0}.{1}.StatusReport sr WHERE sr.CodigoAnalisePerformance = app.CodigoAnalisePerformance AND sr.CodigoObjeto = @CodigoObjeto AND sr.CodigoTipoAssociacaoObjeto = @CodigoTipoAssociacao AND sr.DataExclusao IS NULL) THEN 'S' ELSE 'N' END) AS ExisteVinculoStatusReport
                FROM        {0}.{1}.[AnalisePerformance]    AS app
                LEFT JOIN   {0}.{1}.[Usuario]               AS ui   ON app.CodigoUsuarioInclusao = ui.CodigoUsuario
                LEFT JOIN   {0}.{1}.[Usuario]               AS ua   ON app.CodigoUsuarioUltimaAlteracao = ua.CodigoUsuario
                WHERE   CodigoObjetoAssociado   = @CodigoObjeto
                    AND CodigoTipoAssociacao    = @CodigoTipoAssociacao
                    AND app.DataExclusao is null
                    {4}
                ORDER BY app.DataAnalisePerformance DESC
        ", bancodb, Ownerdb, codigoObjeto, tipoAssociacao, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getAnalisesIndicador(string codigoIndicador, string where)
    {
        comandoSQL = string.Format(@"
            BEGIN
              DECLARE @CodigoObjeto INT
  
              SELECT @CodigoObjeto = CodigoObjetoEstrategia 
                FROM {0}.{1}.IndicadorObjetoEstrategia 
               WHERE CodigoIndicador = {2}
  
              SELECT ap.CodigoAnalisePerformance,
                     ap.Analise,
                     ap.Recomendacoes,
                     CONVERT(VarChar(10), IsNull(ap.DataUltimaAlteracao, ap.DataInclusao), 103) AS DataAnalise,
                     u.NomeUsuario AS Responsavel,
                     ap.CodigoCorStatusObjetoAssociado,
                     ca.CorApresentacao
                FROM {0}.{1}.AnalisePerformance AS ap INNER JOIN
                     {0}.{1}.Usuario AS u ON (u.CodigoUsuario = ISNULL(ap.CodigoUsuarioUltimaAlteracao,ap.CodigoUsuarioInclusao)) LEFT JOIN
                     {0}.{1}.CorApresentacao ca ON ca.CodigoCorApresentacao = ap.CodigoCorStatusObjetoAssociado
               WHERE ap.DataExclusao IS NULL
                 AND ap.CodigoObjetoAssociado = @CodigoObjeto
                 AND ap.CodigoTipoAssociacao IN (SELECT CodigoTipoAssociacao FROM {0}.{1}.TipoAssociacao WHERE IniciaisTipoAssociacao IN ('PP','OB'))
                 {3}
            END

        ", bancodb, Ownerdb, codigoIndicador, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiAnalisePerformanceIndicador(int codigoIndicador, int codigoUsuario, string analise, string recomendacoes, int codigoCorStatus)
    {
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
            BEGIN
                DECLARE @CodigoTipoAssociacao AS INT,
                        @CodigoObjetoEstrategia INT

                SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao, @CodigoObjetoEstrategia = CodigoObjetoEstrategia
                  FROM {0}.{1}.IndicadorObjetoEstrategia 
                 WHERE CodigoIndicador = {2}

                INSERT INTO {0}.{1}.AnalisePerformance(CodigoIndicador, CodigoObjetoAssociado, CodigoTipoAssociacao
                                                        , DataAnalisePerformance, Analise, Recomendacoes
                                                        , CodigoUsuarioInclusao, DataInclusao, IndicaRegistroEditavel, CodigoCorStatusObjetoAssociado)
                VALUES ({2}, @CodigoObjetoEstrategia, @CodigoTipoAssociacao, GETDATE(), '{3}', '{4}', {5}, GETDATE(), 'S', {6})
            END
            ", bancodb, Ownerdb, codigoIndicador, analise.Replace("'", "''"), recomendacoes.Replace("'", "''"), codigoUsuario, codigoCorStatus);

            DataSet ds = getDataSet(comandoSQL);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool atualizaAnalisePerformanceIndicador(int codigoAnalisePerformance, int codigoUsuario, string analise, string recomendacoes, int codigoCorStatus)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"                
                UPDATE {0}.{1}.[AnalisePerformance]
                   SET DataAnalisePerformance         = GETDATE()
                      ,Analise                        = '{3}'
                      ,Recomendacoes                  = '{4}'
                      ,CodigoUsuarioUltimaAlteracao   = {5}
                      ,DataUltimaAlteracao            = GETDATE()
                      ,CodigoCorStatusObjetoAssociado = {6}
                WHERE CodigoAnalisePerformance = {2}
               ", bancodb, Ownerdb, codigoAnalisePerformance, analise.Replace("'", "''"), recomendacoes.Replace("'", "''"), codigoUsuario, codigoCorStatus);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool excluiAnalisePerformanceIndicador(int codigoAnalisePerformance, int codigoUsuario)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"                
                UPDATE {0}.{1}.[AnalisePerformance]
                   SET CodigoUsuarioExclusao   = {3}
                      ,DataExclusao            = GETDATE()
                WHERE CodigoAnalisePerformance = {2}
               ", bancodb, Ownerdb, codigoAnalisePerformance, codigoUsuario);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public DataSet getPeriodicidadeIndicadorOperacional(int codigoMeta)
    {
        string comandoSQL = string.Format(@"            
            DECLARE @CodigoTipoAssociacao AS INT
            
            SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao FROM TipoAssociacao WHERE IniciaisTipoAssociacao = 'MT'

                SELECT rmo.CodigoMetaOperacional, mo.CodigoProjeto, mo.CodigoIndicador, rmo.Ano, rmo.Mes, 'E' AS TipoEdicao
                        , {0}.{1}.f_GetNomePeriodoMetaOperacional(rmo.CodigoMetaOperacional, rmo.Ano, rmo.Mes, 1, 4) AS Periodo
                        , rmo.ValorMetaPeriodo AS MetaMes, rmo.ValorResultadoPeriodo AS ResultadoMes
                        , {0}.{1}.f_GetCorDesempenhoResultado(rmo.ValorMetaPeriodo, rmo.ValorResultadoPeriodo, i.Polaridade, 'IO',mo.CodigoIndicador) AS Desempenho
                        -- 30/09/2010 : campos adicionados. Referencia ao acumulados
                        , rmo.ValorMetaAcumuladaAno AS MetaAcumuladaAno, rmo.ValorResultadoAcumuladoAno AS ResultadoAcumuladoAno
                        , {0}.{1}.f_GetCorDesempenhoResultado(rmo.ValorMetaAcumuladaAno, rmo.ValorResultadoAcumuladoAno, i.Polaridade, 'IO', mo.CodigoIndicador) AS DesempenhoAculumado
                        , tum.SiglaUnidadeMedida
                        , i.CasasDecimais

                FROM        {0}.{1}.ResumoMetaOperacional  AS rmo 
                INNER JOIN  MetaOperacional mo ON mo.CodigoMetaOperacional = rmo.CodigoMetaOperacional
                INNER JOIN  {0}.{1}.IndicadorOperacional        AS i    ON i.CodigoIndicador = mo.CodigoIndicador 
                INNER JOIN  {0}.{1}.[PeriodoAnalisePortfolio]   AS per  ON per.CodigoEntidade = i.CodigoEntidade AND per.[Ano] = rmo.[Ano]
                INNER JOIN  {0}.{1}.TipoUnidadeMedida           AS tum  ON tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida 

                WHERE   rmo.CodigoMetaOperacional       = {2}
                    AND per.[IndicaAnoAtivo]    = 'S'
                    AND MONTH(dbo.f_GetDataReferenciaPeriodoMetaOperacional(rmo.CodigoMetaOperacional, rmo.Ano, rmo.Mes)) = rmo.Mes
                    AND EXISTS(SELECT 1 FROM AnalisePerformance app
                                WHERE app.CodigoIndicador   = mo.CodigoIndicador
                                    AND app.CodigoObjetoAssociado	= rmo.CodigoMetaOperacional
                                    AND app.CodigoTipoAssociacao	= @CodigoTipoAssociacao
                                    AND app.Ano                     = rmo.Ano
                                    AND app.Mes                     = rmo.Mes)

                UNION

                SELECT rmo.CodigoMetaOperacional, mo.CodigoProjeto, mo.CodigoIndicador, rmo.Ano, rmo.Mes, 'I'
                    , {0}.{1}.f_GetNomePeriodoMetaOperacional(rmo.CodigoMetaOperacional, rmo.Ano, rmo.Mes, 1, 4) AS Periodo
                    , rmo.ValorMetaPeriodo AS MetaMes, rmo.ValorResultadoPeriodo AS ResultadoMes
                    , {0}.{1}.f_GetCorDesempenhoResultado(rmo.ValorMetaPeriodo, rmo.ValorResultadoPeriodo, i.Polaridade, 'IO',mo.CodigoIndicador) AS Desempenho
                    -- 30/09/2010 : campos adicionados. Referencia ao acumulados
                    , rmo.ValorMetaAcumuladaAno AS MetaAcumuladaAno, rmo.ValorResultadoAcumuladoAno AS ResultadoAcumuladoAno
                    , {0}.{1}.f_GetCorDesempenhoResultado(rmo.ValorMetaAcumuladaAno, rmo.ValorResultadoAcumuladoAno, i.Polaridade, 'IO', mo.CodigoIndicador) AS DesempenhoAculumado
                    , tum.SiglaUnidadeMedida
                    , i.CasasDecimais

                FROM        {0}.{1}.ResumoMetaOperacional  AS rmo 
                INNER JOIN  {0}.{1}.MetaOperacional mo ON mo.CodigoMetaOperacional = rmo.CodigoMetaOperacional
                INNER JOIN  {0}.{1}.IndicadorOperacional        AS i    ON i.CodigoIndicador = mo.CodigoIndicador
                INNER JOIN  {0}.{1}.[PeriodoAnalisePortfolio]   AS per  ON per.CodigoEntidade = i.CodigoEntidade AND per.[Ano] = rmo.[Ano]
                INNER JOIN  {0}.{1}.TipoUnidadeMedida           AS tum  ON tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida 

                  WHERE rmo.CodigoMetaOperacional       = {2}
                    AND per.[IndicaAnoAtivo]    = 'S'
                    AND MONTH(dbo.f_GetDataReferenciaPeriodoMetaOperacional(rmo.CodigoMetaOperacional, rmo.Ano, rmo.Mes)) = rmo.Mes
                    AND NOT EXISTS(SELECT 1 FROM AnalisePerformance app
                                    WHERE   app.CodigoObjetoAssociado	= rmo.CodigoMetaOperacional
                                        AND app.CodigoTipoAssociacao	= @CodigoTipoAssociacao                                        
                                        AND app.Ano                     = rmo.Ano
                                        AND app.Mes                     = rmo.Mes)

                 ORDER BY rmo.Ano, rmo.Mes        
        ", bancodb, Ownerdb, codigoMeta);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getPeriodicidadeHistoricoAnalises(int codigoMeta, string where)
    {
        string comandoSQL = string.Format(@"            
            DECLARE @CodigoTipoAssociacao AS INT
            
            SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao FROM TipoAssociacao WHERE IniciaisTipoAssociacao = 'MT'

                SELECT rmo.CodigoMetaOperacional, mo.CodigoProjeto, mo.CodigoIndicador, rmo.Ano, rmo.Mes, 'E' AS TipoEdicao
                        , {0}.{1}.f_GetNomePeriodoMetaOperacional(rmo.CodigoMetaOperacional, rmo.Ano, rmo.Mes, 1, 4) AS Periodo
                        , mo.MetaNumerica AS MetaMes, rmo.ValorResultadoPeriodo AS ResultadoMes
                        , {0}.{1}.f_GetCorDesempenhoResultado(mo.MetaNumerica, rmo.ValorResultadoPeriodo, i.Polaridade, 'IO', i.CodigoIndicador) AS Desempenho
                        -- 30/09/2010 : campos adicionados. Referencia ao acumulados
                        , mo.MetaNumerica AS MetaAcumuladaAno, rmo.ValorResultadoAcumuladoAno AS ResultadoAcumuladoAno
                        , {0}.{1}.f_GetCorDesempenhoResultado(mo.MetaNumerica, rmo.ValorResultadoPeriodo, i.Polaridade, 'IO', i.CodigoIndicador) AS DesempenhoAculumado
                        , tum.SiglaUnidadeMedida
                        , i.CasasDecimais
                FROM        {0}.{1}.ResumoMetaOperacional  AS rmo 
                INNER JOIN  MetaOperacional mo ON mo.CodigoMetaOperacional = rmo.CodigoMetaOperacional
                INNER JOIN  {0}.{1}.IndicadorOperacional        AS i    ON i.CodigoIndicador = mo.CodigoIndicador 
                INNER JOIN  {0}.{1}.[PeriodoAnalisePortfolio]   AS per  ON per.CodigoEntidade = i.CodigoEntidade AND per.[Ano] = rmo.[Ano]
                INNER JOIN  {0}.{1}.TipoUnidadeMedida           AS tum  ON tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida 

                WHERE rmo.CodigoMetaOperacional       = {2}
                  AND rmo.ValorResultadoPeriodo IS NOT NULL
                  AND per.[IndicaAnoAtivo]    = 'S'
                    {3}
                ORDER BY rmo.Ano DESC, rmo.Mes DESC      

        ", bancodb, Ownerdb, codigoMeta, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getResultadosMeta(int codigoMeta, string where)
    {
        string comandoSQL = string.Format(@"            
            DECLARE @CodigoTipoAssociacao AS INT
            
            SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao FROM TipoAssociacao WHERE IniciaisTipoAssociacao = 'MT'

                SELECT TOP 12 {0}.{1}.f_GetNomePeriodoMetaOperacional(rmo.CodigoMetaOperacional, rmo.Ano, rmo.Mes, 1, 4) AS Periodo
                      ,rmo.ValorResultadoPeriodo AS ResultadoMes, rmo.Ano, rmo.Mes
                      ,{0}.{1}.f_GetCorDesempenhoResultado(mo.MetaNumerica, rmo.ValorResultadoPeriodo, i.Polaridade, 'IO', i.CodigoIndicador) AS Desempenho
                FROM        {0}.{1}.ResumoMetaOperacional  AS rmo 
                INNER JOIN  MetaOperacional mo ON mo.CodigoMetaOperacional = rmo.CodigoMetaOperacional
                INNER JOIN  {0}.{1}.IndicadorOperacional        AS i    ON i.CodigoIndicador = mo.CodigoIndicador 
                INNER JOIN  {0}.{1}.[PeriodoAnalisePortfolio]   AS per  ON per.CodigoEntidade = i.CodigoEntidade AND per.[Ano] = rmo.[Ano]
                WHERE rmo.CodigoMetaOperacional       = {2}
                  AND rmo.ValorResultadoPeriodo IS NOT NULL
                  AND per.[IndicaAnoAtivo]    = 'S'
                    {3}
                ORDER BY rmo.Ano DESC, rmo.Mes DESC

        ", bancodb, Ownerdb, codigoMeta, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public string getGraficoPeriodicidadeHistoricoAnalisesLog(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot, string unidadeMedida)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        int i = 0;

        string formaNumero = "";

        if (unidadeMedida == "%")
            formaNumero = @" numberSuffix=""%"" ";
        else
        {
            if (unidadeMedida.Contains("$"))
                formaNumero = string.Format(@" numberPrefix=""{0} "" ", unidadeMedida);
            else
                formaNumero = "";
        }

        double menorValor = 999999999999999;

        DataRow[] drs = dt.Select("", "Ano, Mes");

        for (i = 0; i < drs.Length; i++)
        {
            string valorAtual = drs[i]["ResultadoMes"].ToString();

            if (valorAtual != "" && double.Parse(valorAtual) != 0 && double.Parse(valorAtual) < menorValor)
                menorValor = double.Parse(valorAtual);


        }

        xml.Append(string.Format(@"<chart palette=""2"" valuePadding=""9"" caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""3"" chartLeftMargin=""1"" chartRightMargin=""1"" chartTopMargin=""5"" canvasBgColor=""F2F2F2"" canvasLeftMargin=""0"" canvasRightMargin=""20"" canvasBottomMargin=""2""
                                    numDivLines=""3"" showMinorDivLineValues=""0"" numMinorDivLines=""5""  divLineThickness=""1"" showBorder=""0"" alternateHGridColor=""F2F2F2"" BgColor=""F2F2F2""  decimalPlaces=""{7}"" showLimits=""1""
                                    inDecimalSeparator="",""  connectNullData=""1"" divLineColor=""CCCCCC"" showLegend=""0"" inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" {6}  plotGradientColor="""">", ""
                                                                                                                                                          , titulo
                                                                                                                                                          , fonte
                                                                                                                                                          , scrollHeight
                                                                                                                                                          , numDivLines
                                                                                                                                                          , numVisiblePlot
                                                                                                                                                          , formaNumero
                                                                                                                                                          , casasDecimais
                                                                                                                                                          , menorValor));
        xml.Append(string.Format(@"<categories>"));

        try
        {
            for (i = 0; i < drs.Length; i++)
            {
                xml.Append(string.Format(@"<category label=""{0}"" />", drs[i]["Periodo"].ToString()));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));

        double valorDivisao = 10;

        for (i = 0; i < casasDecimais; i++)
            valorDivisao = valorDivisao * 10;

        xml.Append(string.Format(@"<dataset seriesName=""Resultado"">"));

        try
        {
            for (i = 0; i < drs.Length; i++)
            {

                string valorResultado = drs[i]["ResultadoMes"].ToString();

                string cor = drs[i]["Desempenho"].ToString().ToUpper().Trim();

                switch (cor)
                {
                    case "VERDE":
                        cor = corSatisfatorio;
                        break;
                    case "AMARELO":
                        cor = corAtencao;
                        break;
                    case "VERMELHO":
                        cor = corCritico;
                        break;
                    case "LARANJA":
                        cor = "EC8D00";
                        break;
                    case "AZUL":
                        cor = corExcelente;
                        break;
                    case "BRANCO":
                        cor = "FFFFFF";
                        break;
                }

                string link = "";

                string displayResultado = "";

                if (unidadeMedida == "%")
                    displayResultado = valorResultado == "" ? "-" : string.Format("{0:n" + casasDecimais + "}%", double.Parse(valorResultado));
                else
                {
                    if (unidadeMedida.Contains("$"))
                        displayResultado = valorResultado == "" ? "-" : string.Format(unidadeMedida + "{0:n" + casasDecimais + "}", double.Parse(valorResultado));
                    else
                        displayResultado = valorResultado == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorResultado));
                }

                if (menorValor < 1)
                    menorValor = 1 / valorDivisao;
                else
                    menorValor = 0.1;

                if (valorResultado != "" && double.Parse(valorResultado) == 0)
                    valorResultado = (menorValor).ToString();

                xml.Append(string.Format(@"<set displayValue=""{1}"" anchorRadius='8' anchorSides='20' anchorBorderColor='CCCCCC' anchorBgColor='{3}' value=""{0}"" {2} toolText=""Resultado: {1}"" /> ", valorResultado, displayResultado, link, cor));


            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontValues\" type=\"font\" face=\"Verdana\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Verdana\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("</definition>");
        xml.Append("<application>");
        xml.Append("<apply toObject=\"DATALABELS\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"DATAVALUES\" styles=\"fontValues\" />");
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

    public string getGraficoPeriodicidadeHistoricoAnalises(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot, string unidadeMedida)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        int i = 0;

        string formaNumero = "";

        if (unidadeMedida == "%")
            formaNumero = @" numberSuffix=""%"" ";
        else
        {
            if (unidadeMedida.Contains("$"))
                formaNumero = string.Format(@" numberPrefix=""{0} "" ", unidadeMedida);
            else
                formaNumero = "";
        }

        DataRow[] drs = dt.Select("", "Ano, Mes");



        xml.Append(string.Format(@"<chart palette=""2"" valuePadding=""9"" caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""3"" chartLeftMargin=""1"" chartRightMargin=""1"" chartTopMargin=""5"" canvasBgColor=""F2F2F2"" canvasLeftMargin=""0"" canvasRightMargin=""20"" canvasBottomMargin=""2""
                                    numDivLines=""3"" showMinorDivLineValues=""0"" numMinorDivLines=""5"" showvalues=""1"" divLineThickness=""1"" showBorder=""0"" alternateHGridColor=""F2F2F2"" BgColor=""F2F2F2"" labelDisplay=""NONE"" decimalPlaces=""{7}"" showLimits=""1""
                                    inDecimalSeparator="",""  connectNullData=""1"" divLineColor=""CCCCCC"" showLegend=""0"" inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" {6}  plotGradientColor="""">", ""
                                                                                                                                                          , titulo
                                                                                                                                                          , fonte
                                                                                                                                                          , scrollHeight
                                                                                                                                                          , numDivLines
                                                                                                                                                          , numVisiblePlot
                                                                                                                                                          , formaNumero
                                                                                                                                                          , casasDecimais
                                                                                                                                                          , 0));
        xml.Append(string.Format(@"<categories>"));

        try
        {
            for (i = 0; i < drs.Length; i++)
            {
                xml.Append(string.Format(@"<category label=""{0}"" />", drs[i]["Periodo"].ToString()));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));

        xml.Append(string.Format(@"<dataset seriesName=""Resultado"" >"));

        try
        {
            for (i = 0; i < drs.Length; i++)
            {

                string valorResultado = drs[i]["ResultadoMes"].ToString();

                string cor = drs[i]["Desempenho"].ToString().ToUpper().Trim();

                switch (cor)
                {
                    case "VERDE":
                        cor = corSatisfatorio;
                        break;
                    case "AMARELO":
                        cor = corAtencao;
                        break;
                    case "VERMELHO":
                        cor = corCritico;
                        break;
                    case "LARANJA":
                        cor = "EC8D00";
                        break;
                    case "AZUL":
                        cor = corExcelente;
                        break;
                    case "BRANCO":
                        cor = "FFFFFF";
                        break;
                }

                string link = "";

                string displayResultado = "";

                if (unidadeMedida == "%")
                    displayResultado = valorResultado == "" ? "-" : string.Format("{0:n" + casasDecimais + "}%", double.Parse(valorResultado));
                else
                {
                    if (unidadeMedida.Contains("$"))
                        displayResultado = valorResultado == "" ? "-" : string.Format(unidadeMedida + "{0:n" + casasDecimais + "}", double.Parse(valorResultado));
                    else
                        displayResultado = valorResultado == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorResultado));
                }

                xml.Append(string.Format(@"<set displayValue=""{1}"" anchorRadius='8' anchorSides='20' anchorBorderColor='CCCCCC' anchorBgColor='{3}' value=""{0}"" {2} toolText=""Resultado: {1}"" /> ", valorResultado, displayResultado, link, cor));


            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontValues\" type=\"font\" face=\"Verdana\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Verdana\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("</definition>");
        xml.Append("<application>");
        xml.Append("<apply toObject=\"DATALABELS\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"DATAVALUES\" styles=\"fontValues\" />");
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

    public bool incluirAnalisePerformanceMeta(string codigoIndicador, string codigoMeta, string ano, string mes
                                                , string analise, string recomendacoes, string idUsuario
                                                , string TipoAssociacao, string indicaTipoIndicador)
    {
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
            --inserir un novo registro ao [AnalisePerformanceProjeto]
            DECLARE @CodigoTipoAssociacao AS INT
            
            SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao FROM TipoAssociacao WHERE IniciaisTipoAssociacao = '{9}'

            INSERT INTO {0}.{1}.AnalisePerformance(CodigoIndicador, CodigoObjetoAssociado, CodigoTipoAssociacao
                                                    , Ano, Mes, DataAnalisePerformance, Analise, Recomendacoes
                                                    , CodigoUsuarioInclusao, DataInclusao, IndicaTipoIndicador,IndicaRegistroEditavel)
            VALUES ({2}, {3}, @CodigoTipoAssociacao, {4}, {5}, GETDATE(), {6}, {7}, {8}, GETDATE(), '{10}','S')
            ", bancodb, Ownerdb, codigoIndicador, codigoMeta, ano, mes
             , analise == "" ? "NULL" : "'" + analise.Replace("'", "''") + "'"
             , recomendacoes == "" ? "NULL" : "'" + recomendacoes.Replace("'", "''") + "'"
             , idUsuario, TipoAssociacao, indicaTipoIndicador);

            DataSet ds = getDataSet(comandoSQL);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool atualizaAnalisePerformanceMeta(string codigoIndicador, string codigoMeta, string ano
                                                , string mes, string analise, string recomendacoes
                                                , string idUsuario, string TipoAssociacao)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                --editar un registro do [AnalisePerformanceProjeto]
                DECLARE @CodigoTipoAssociacao AS INT

                SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao FROM TipoAssociacao WHERE IniciaisTipoAssociacao = '{9}'

                UPDATE {0}.{1}.[AnalisePerformance]
                SET     DataAnalisePerformance      = GETDATE()
                    , Analise                       = {2}
                    , Recomendacoes                 = {3}
                    , CodigoUsuarioUltimaAlteracao  = {4}
                    , DataUltimaAlteracao           = GETDATE()

                WHERE   CodigoIndicador         = {5}
                    AND CodigoObjetoAssociado   = {6}
                    AND CodigoTipoAssociacao    = @CodigoTipoAssociacao
                    AND Ano                     = {7}
                    AND Mes                     = {8}
               ", bancodb, Ownerdb
                , analise == "" ? "NULL" : "'" + analise.Replace("'", "''") + "'"
                , recomendacoes == "" ? "NULL" : "'" + recomendacoes.Replace("'", "''") + "'"
                , idUsuario, codigoIndicador, codigoMeta, ano, mes, TipoAssociacao);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool excluirAnalisePerformanceMeta(string codigoIndicador, string codigoMeta, string ano, string mes, string TipoAssociacao)
    {
        string comandoSQL = string.Format(@"
            --excluir un registro do [AnalisePerformanceProjeto]
            DECLARE @CodigoTipoAssociacao AS INT

            SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao FROM TipoAssociacao WHERE IniciaisTipoAssociacao = '{6}'

            DELETE FROM {0}.{1}.[AnalisePerformance]
            WHERE   CodigoIndicador         = {2}
                AND CodigoObjetoAssociado   = {3}
                AND CodigoTipoAssociacao    = @CodigoTipoAssociacao
                AND Ano                     = {4}
                AND Mes                     = {5}
            ", bancodb, Ownerdb, codigoIndicador, codigoMeta, ano, mes, TipoAssociacao);
        int regAfetados = 0;
        try
        {
            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }


    public bool incluiAnaliseProjeto(int codigoProjeto, string analise, int idUsuario)
    {
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
            --inserir un novo registro ao [AnalisePerformanceProjeto]
DECLARE @CodigoTipoAssociacao INT
DECLARE @CodigoObjeto INT 
DECLARE @CodigoStatusReport INT
DECLARE @CodigoAnalisePerformance INT
    SET @CodigoObjeto = {2}
            
 SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao FROM TipoAssociacao WHERE IniciaisTipoAssociacao = 'PR'

 INSERT INTO {0}.{1}.AnalisePerformance(CodigoObjetoAssociado, CodigoTipoAssociacao, DataAnalisePerformance, Analise, CodigoUsuarioInclusao, DataInclusao,IndicaRegistroEditavel)
 VALUES (@CodigoObjeto, @CodigoTipoAssociacao, GETDATE(), '{3}', {4}, GETDATE(), '{5}')

    SET @CodigoAnalisePerformance = @@IDENTITY
            
 SELECT TOP 1 
        @CodigoStatusReport = sr.CodigoStatusReport
   FROM StatusReport sr 
  WHERE sr.CodigoObjeto = @CodigoObjeto
    AND sr.CodigoTipoAssociacaoObjeto = @CodigoTipoAssociacao
  ORDER BY 
        DataGeracao DESC
        
 UPDATE StatusReport 
    SET CodigoAnalisePerformance = @CodigoAnalisePerformance 
  WHERE CodigoStatusReport = @CodigoStatusReport
    AND DataExclusao IS NULL
    AND CodigoAnalisePerformance IS NULL
    AND DataPublicacao IS NULL
            ", bancodb, Ownerdb, codigoProjeto
             , analise.Replace("'", "''")
             , idUsuario
             , "S");

            DataSet ds = getDataSet(comandoSQL);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool atualizaAnaliseProjeto(int codigoAnalise, string analise, int idUsuario)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                --editar un registro do [AnalisePerformanceProjeto]
                DECLARE @CodigoTipoAssociacao AS INT

                SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao FROM TipoAssociacao WHERE IniciaisTipoAssociacao = 'PR'

                UPDATE {0}.{1}.[AnalisePerformance]
                SET   DataAnalisePerformance      = GETDATE()
                    , Analise                       = '{2}'
                    , CodigoUsuarioUltimaAlteracao  = {3}
                    , DataUltimaAlteracao           = GETDATE()
                WHERE CodigoAnalisePerformance = {4}
               ", bancodb, Ownerdb
                , analise.Replace("'", "''")
                , idUsuario, codigoAnalise);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool excluiAnaliseProjeto(int codigoAnalisePerformance)
    {
        string comandoSQL = string.Format(@"
            UPDATE {0}.{1}.[AnalisePerformance]
               SET [DataExclusao] = GETDATE()
             WHERE [CodigoAnalisePerformance] = {2}
            ", bancodb, Ownerdb, codigoAnalisePerformance);
        int regAfetados = 0;
        try
        {
            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region _Projetos - DadosProjeto - TarefasToDoList.aspx

    public string getDescricaoStatusTarefaToDoList(int codigoStatus)
    {
        string comandoSQL = string.Format(
                      @"SELECT DescricaoStatusTarefa 
                          FROM {0}.{1}.StatusTarefa 
                         WHERE CodigoStatusTarefa = {2}
               ", bancodb, Ownerdb, codigoStatus);

        DataSet ds = getDataSet(comandoSQL);

        string textoPadrao = "";

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            textoPadrao = ds.Tables[0].Rows[0]["DescricaoStatusTarefa"].ToString();

        return textoPadrao;
    }

    public DataSet getStatusTarefas(string where)
    {
        comandoSQL = string.Format(@"
                    SELECT   CodigoStatusTarefa
                            ,DescricaoStatusTarefa
                    FROM {0}.{1}.StatusTarefa
                    WHERE 1 = 1 {2}", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getProjetosToDoList(int codigoEntidade, int codigoProjeto)
    {
        comandoSQL = string.Format(
            @"select * from {0}.{1}.f_GetTarefasToDoListProjeto({3}, {2},-1)"
            , bancodb, Ownerdb, codigoProjeto, codigoEntidade);

        return getDataSet(comandoSQL);
    }

    public DataSet getGerenteProjeto(int codigoProjeto)
    {
        comandoSQL = string.Format(@"
            SELECT  p.CodigoGerenteProjeto, u.NomeUsuario as NomeGerenteProjeto 
               FROM {0}.{1}.projeto p
         INNER JOIN {0}.{1}.usuario u on (u.CodigoUsuario = p.CodigoGerenteProjeto)
              WHERE p.CodigoProjeto = {2}"
            , bancodb, Ownerdb, codigoProjeto);

        return getDataSet(comandoSQL);
    }

    public DataSet getResponsavelTarefaProjeto(int codigoProjeto)
    {
        comandoSQL = string.Format(@"
            SELECT  CodigoUsuarioResponsavelTarefa
            FROM    {0}.{1}.TarefaToDoList
            WHERE   CodigoProjeto = {2}"
            , bancodb, Ownerdb, codigoProjeto);

        return getDataSet(comandoSQL);
    }

    public bool IncluiTarefaToDoListProjeto(int codigoEntidade, int codigoProjeto, int codigoUsuarioInclusao, string nomeTarefa, int codigoResponsavel, string prioridade, string statusTarefa
        , string inicioPrevisto, string terminoPrevisto, string esforcoPrevisto, string esforcoReal, string custoPrevisto, string custoReal
        , string inicioReal, string terminoReal, string anotacoes, ref string msgErro)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                BEGIN

                    DECLARE @CodigoToDoList int,
                            @CodigoTipoAssociacao int

                    SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao
                      FROM {0}.{1}.TipoAssociacao 
                     WHERE IniciaisTipoAssociacao = 'PR'
                    
                    IF NOT EXISTS(SELECT 1 FROM {0}.{1}.ToDoList AS tdl WHERE tdl.CodigoObjetoAssociado = {15} AND CodigoTipoAssociacao = @CodigoTipoAssociacao)
                    BEGIN
                        INSERT INTO {0}.{1}.ToDoList(DataInclusao, CodigoUsuarioInclusao, CodigoEntidade, CodigoObjetoAssociado, CodigoTipoAssociacao)
                                VALUES(GETDATE(), {16}, {17}, {15}, @CodigoTipoAssociacao)
                        SELECT @CodigoToDoList = SCOPE_IDENTITY()
                    END
                        ELSE
                            SELECT TOP 1 @CodigoToDoList = CodigoToDoList 
                              FROM {0}.{1}.ToDoList AS tdl 
                             WHERE tdl.CodigoObjetoAssociado = {15} 
                               AND CodigoTipoAssociacao = @CodigoTipoAssociacao                        

                    
                    
                    INSERT INTO {0}.{1}.TarefaToDoList(CodigoToDoList, DescricaoTarefa, InicioPrevisto, TerminoPrevisto, InicioReal, TerminoReal
	                                                  ,CodigoUsuarioResponsavelTarefa, PercentualConcluido, Anotacoes, Prioridade, CodigoStatusTarefa
                                                      ,EsforcoPrevisto, EsforcoReal, CustoPrevisto, CustoReal, CodigoProjeto, CodigoUsuarioInclusao,DataInclusao)
                                        VALUES(@CodigoToDoList, '{2}', {3}, {4}, {5}, {6}, {7}, 0, '{8}', '{9}', {10}, {11}, {12}, {13}, {14}, {15},{16}, GetDate())
                END"
                , bancodb, Ownerdb, nomeTarefa, inicioPrevisto, terminoPrevisto, inicioReal, terminoReal, codigoResponsavel, anotacoes, prioridade
                , statusTarefa, esforcoPrevisto.Replace(".", "").Replace(',', '.'), esforcoReal.Replace(".", "").Replace(',', '.')
                , custoPrevisto.Replace(".", "").Replace(',', '.'), custoReal.Replace(".", "").Replace(',', '.')
                , codigoProjeto, codigoUsuarioInclusao, codigoEntidade);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch (Exception ex)
        {
            msgErro = getErroIncluiRegistro(ex.Message);
            return false;
        }
    }

    public bool atualizaTarefaToDoList(string chave, string InicioReal, string TerminoReal, string EsforcoReal, string Anotacoes, string codigoStatusTarefa, int codigUsuarioLogado, ref string msgErro)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                UPDATE {0}.{1}.TarefaToDoList
                SET
	                 InicioReal =  {2}
	                ,TerminoReal = {3}
	                ,EsforcoReal = {4}
	                ,Anotacoes = '{5}'
	                ,CodigoStatusTarefa = {6}
                    ,DataUltimaAlteracao = getdate()
                    ,[CodigoUsuarioUltimaAlteracao] = {8}
                WHERE CodigoTarefa = {7}"
                , bancodb, Ownerdb, InicioReal, TerminoReal
                , EsforcoReal.Replace(',', '.'), Anotacoes, codigoStatusTarefa, chave, codigUsuarioLogado);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch (Exception ex)
        {
            msgErro = getErroAtualizaRegistro(ex.Message);
            return false;
        }
    }

    public bool atualizaTarefaToDoListKanban(string codigoTarefa, string InicioReal, string TerminoReal, string EsforcoReal, string Anotacoes, ref string msgErro)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                UPDATE {0}.{1}.TarefaToDoList
                SET
	                 InicioRealKanban =  {2}
	                ,TerminoRealKanban = {3}
	                ,EsforcoRealKanban = {4}
	                ,Anotacoes = '{5}'
                WHERE CodigoTarefa = {6}"
                , bancodb, Ownerdb, InicioReal, TerminoReal
                , EsforcoReal.Replace(',', '.'), Anotacoes, codigoTarefa);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch (Exception ex)
        {
            msgErro = getErroAtualizaRegistro(ex.Message);
            return false;
        }
    }

    public bool atualizaTarefaToDoListProjeto(string chave, int codigoUsuarioAlteracao, string nomeTarefa, int codigoResponsavel, string prioridade, string statusTarefa
        , string inicioPrevisto, string terminoPrevisto, string esforcoPrevisto, string esforcoReal, string custoPrevisto, string custoReal
        , string inicioReal, string terminoReal, string anotacoes, ref string msgErro)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                UPDATE {0}.{1}.TarefaToDoList
                SET DescricaoTarefa = '{3}'
                  , InicioPrevisto = {4}
                  , TerminoPrevisto = {5}
                  , InicioReal = {6}
                  , TerminoReal = {7}
	              , CodigoUsuarioResponsavelTarefa = {8}
                  , Anotacoes = '{9}'
                  , Prioridade = '{10}'
                  , CodigoStatusTarefa = {11}
                  , EsforcoPrevisto = {12}
                  , EsforcoReal = {13}
                  , CustoPrevisto = {14}
                  , CustoReal = {15}
                  , DataUltimaAlteracao = GetDate()
                  , CodigoUsuarioUltimaAlteracao = {16}
                WHERE CodigoTarefa = {2}"
                , bancodb, Ownerdb, chave, nomeTarefa, inicioPrevisto, terminoPrevisto, inicioReal, terminoReal, codigoResponsavel, anotacoes, prioridade
                , statusTarefa, esforcoPrevisto.Replace(".", "").Replace(',', '.'), esforcoReal.Replace(".", "").Replace(',', '.')
                , custoPrevisto.Replace(".", "").Replace(',', '.'), custoReal.Replace(".", "").Replace(',', '.'), codigoUsuarioAlteracao);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch (Exception ex)
        {
            msgErro = getErroAtualizaRegistro(ex.Message);
            return false;
        }
    }

    public bool excluiToDoList(string chave, int usuarioExclusao, int codigoObjetivoEstrategico, ref string msgErro)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                UPDATE {0}.{1}.ToDoList
                SET
	                 DataExclusao = GETDATE()
	                ,CodigoUsuarioExclusao = {3}
                WHERE CodigoToDoList = {2}


        DELETE from LinkObjeto
        WHERE CodigoObjetoLink = {2} and --CodigoObjeto = {4}
		CodigoTipoObjeto = (Select CodigoTipoAssociacao from TipoAssociacao where IniciaisTipoAssociacao = 'OB') and 
		CodigoTipoObjetoLink = (select CodigoTipoAssociacao from TipoAssociacao where IniciaisTipoAssociacao = 'TD') and 
		CodigoTipoLink = (select CodigoTipoLink from TipoLinkObjeto where IniciaisTipoLink = 'AS')"
                , bancodb, Ownerdb, chave, usuarioExclusao, codigoObjetivoEstrategico);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch (Exception ex)
        {
            msgErro = getErroExcluiRegistro(ex.Message);
            return false;
        }
    }


    public bool excluiToDoList(string chave, int usuarioExclusao, ref string msgErro)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                UPDATE {0}.{1}.ToDoList
                SET
	                 DataExclusao = GETDATE()
	                ,CodigoUsuarioExclusao = {3}
                WHERE CodigoToDoList = {2}


        DELETE from LinkObjeto
        WHERE CodigoObjetoLink = {2} and --CodigoObjeto = 60
		CodigoTipoObjeto = (Select CodigoTipoAssociacao from TipoAssociacao where IniciaisTipoAssociacao = 'OB') and 
		CodigoTipoObjetoLink = (select CodigoTipoAssociacao from TipoAssociacao where IniciaisTipoAssociacao = 'TT') and 
		CodigoTipoLink = (select CodigoTipoLink from TipoLinkObjeto where IniciaisTipoLink = 'AS')"
                , bancodb, Ownerdb, chave, usuarioExclusao);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch (Exception ex)
        {
            msgErro = getErroExcluiRegistro(ex.Message);
            return false;
        }
    }



    public bool excluiTarefaToDoList(string chave, int usuarioExclusao, ref string msgErro)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                UPDATE {0}.{1}.TarefaToDoList
                SET
	                 DataExclusao = GETDATE()
	                ,CodigoUsuarioExclusao = {3}
                WHERE CodigoTarefa = {2}"
                , bancodb, Ownerdb, chave, usuarioExclusao);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch (Exception ex)
        {
            msgErro = getErroExcluiRegistro(ex.Message);
            return false;
        }
    }

    #endregion

    #region _Projetos - DadosProjeto - AnexosProjetos.aspx

    public DataSet getAnexos(int CodigoObjetoAssociado, int codigoTipoAssociacao, int codigoEntidade)
    {
        string comandoSQL = string.Format(@"
                    SELECT a.CodigoAnexo, a.DescricaoAnexo, convert(varchar(20), a.DataInclusao, 113) as DataInclusao, 
                           a.CodigoUsuarioInclusao, a.Nome, a.CodigoEntidade, a.IndicaPasta, 
                           CASE WHEN aa.IndicaLinkCompartilhado = 'S' THEN aa.CodigoPastaLink ELSE a.CodigoPastaSuperior END AS CodigoPastaSuperior,
                           a.IndicaControladoSistema, U.NomeUsuario, PSUP.Nome AS NomePastaSuperior, aa.IndicaLinkCompartilhado,
                           a.dataCheckOut, a.codigoUsuarioCheckOut, a.dataCheckIn, UCK.NomeUsuario as nomeUsuarioCheckout,
                           (select MAX(numeroVersao) from AnexoVersao where codigoAnexo = a.CodigoAnexo) UltimaVersao
                      FROM {0}.{1}.Anexo AS a INNER JOIN
                           {0}.{1}.AnexoAssociacao AS aa ON (aa.CodigoAnexo = a.CodigoAnexo ) INNER JOIN
                           {0}.{1}.Usuario AS U ON a.CodigoUsuarioInclusao = U.CodigoUsuario LEFT OUTER JOIN
                           {0}.{1}.Anexo AS PSUP ON a.CodigoPastaSuperior = PSUP.CodigoAnexo LEFT OUTER JOIN
                           {0}.{1}.Usuario AS UCK ON a.codigoUsuarioCheckOut = UCK.CodigoUsuario
                     WHERE a.dataExclusao is null
                       AND aa.CodigoObjetoAssociado = {2} 
                       AND aa.CodigoTipoAssociacao = {3} 
                       
                       -- AND a.CodigoEntidade = {4}   -- deixado de levar em conta a entidade do anexo uma vez que todos os anexos estão associados a um objeto
                   ORDER BY CodigoPastaSuperior, IndicaPasta desc, Nome"
                    , bancodb, Ownerdb, CodigoObjetoAssociado, codigoTipoAssociacao, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getAnexosOpcaoLink(int codigoEntidade, int CodigoObjetoAssociado, int codigoTipoAssociacao)
    {
        string comandoSQL = string.Format(@"
                    SELECT a.CodigoAnexo, a.DescricaoAnexo, a.DataInclusao, 
                           a.Nome, a.CodigoEntidade, U.NomeUsuario
                      FROM {0}.{1}.Anexo AS a 
					  INNER JOIN {0}.{1}.Usuario AS U ON a.CodigoUsuarioInclusao = U.CodigoUsuario
                      INNER JOIN {0}.{1}.AnexoAssociacao AS ass ON (ass.CodigoAnexo = a.CodigoAnexo)
					  inner join {0}.{1}.TipoAssociacao as ta on (ass.CodigoTipoAssociacao = ta.CodigoTipoAssociacao)

					 WHERE a.dataExclusao is null
					   AND a.IndicaLink = 'S'
                       AND a.IndicaPasta = 'N'
					   AND a.CodigoEntidade = {2}
					   and ta.CodigoTipoAssociacao = dbo.f_GetCodigoTipoAssociacao('EN')
                       AND a.CodigoAnexo NOT in(SELECT aa.CodigoAnexo FROM {0}.{1}.AnexoAssociacao aa 
					                   WHERE aa.CodigoObjetoAssociado = {3} 
									     AND aa.CodigoTipoAssociacao = {4}
										 AND aa.CodigoAnexo = a.CodigoAnexo)
					   ORDER BY Nome"
                    , bancodb, Ownerdb, codigoEntidade, CodigoObjetoAssociado, codigoTipoAssociacao);
        return getDataSet(comandoSQL);
    }

    public DataSet getVersoesAnexo(int codigoAnexo)
    {
        string comandoSQL = string.Format(@"
                    SELECT av.CodigoAnexo, av.codigoSequencialAnexo, numeroVersao, nomeArquivo, u.NomeUsuario, dataVersao
                      FROM {0}.{1}.AnexoVersao av INNER JOIN
			               {0}.{1}.Usuario u ON u.CodigoUsuario = av.codigoUsuarioInclusao
                     WHERE codigoAnexo = {2} and av.DataExclusao is null
                     ORDER BY numeroVersao
                    ", bancodb, Ownerdb, codigoAnexo);
        return getDataSet(comandoSQL);
    }

    public DataSet getInformacoesAnexo(int CodigoAnexo)
    {
        string comandoSQL = string.Format(@"
                    SELECT a.CodigoAnexo, a.DescricaoAnexo, convert(varchar(20), a.DataInclusao, 113) as DataInclusao, 
                           a.CodigoUsuarioInclusao, a.Nome, a.CodigoEntidade, a.CodigoPastaSuperior, a.IndicaPasta, 
                           a.IndicaControladoSistema, U.NomeUsuario, PSUP.Nome AS NomePastaSuperior, a.IndicaLink,
                           a.dataCheckOut, a.codigoUsuarioCheckOut, a.dataCheckIn, UCK.NomeUsuario as nomeUsuarioCheckout,
                           (select MAX(numeroVersao) from AnexoVersao where codigoAnexo = a.CodigoAnexo) UltimaVersao,
                           a.PalavraChave, a.IndicaAnexoPublicoExterno
                      FROM {0}.{1}.Anexo AS a INNER JOIN
                           {0}.{1}.Usuario AS U ON a.CodigoUsuarioInclusao = U.CodigoUsuario LEFT OUTER JOIN
                           {0}.{1}.Anexo AS PSUP ON a.CodigoPastaSuperior = PSUP.CodigoAnexo LEFT OUTER JOIN
                           {0}.{1}.Usuario AS UCK ON a.codigoUsuarioCheckOut = UCK.CodigoUsuario
                     WHERE a.CodigoAnexo = {2}
                       AND a.dataExclusao is null"
                    , bancodb, Ownerdb, CodigoAnexo);
        return getDataSet(comandoSQL);
    }

    public string incluirAnexoFisico(string descricaoAnexo, string codigoUsuarioInclusao, string nomeAnexo, string codigoEntidade,
                             int? codigoPastaSuperior, char indicaPasta, char IndicaControladoSistema, int CodigoTipoAssociacao,
                             string codigoProjeto, ref int codigoNovoAnexo)
    {
        string comandoSQL = "";
        //int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN
                            DECLARE @CodigoAnexo AS bigint;
                            DECLARE @codigoSequencialAnexo AS bigint;

                            DECLARE @CodigoPastaSuperior AS INT;

                            INSERT INTO {0}.{1}.Anexo
                                   (DescricaoAnexo
                                   ,DataInclusao
                                   ,CodigoUsuarioInclusao
                                   ,Nome
                                   ,CodigoEntidade
                                   ,CodigoPastaSuperior
                                   ,IndicaPasta
                                   ,IndicaControladoSistema)
                            VALUES
                                   ('{2}', GETDATE(), {3}, '{4}', {5}, {6}, '{7}', '{8}')
                            
                            SELECT @CodigoAnexo = scope_identity()
                            
                            INSERT into {0}.{1}.AnexoAssociacao (CodigoAnexo, CodigoObjetoAssociado, CodigoTipoAssociacao)
                            VALUES (@CodigoAnexo, {9}, {10})

                            --FAZ UM SELECT PRA VER QUAIS SÃO AS PERMISSOES DA PASTA SUPERIOR E AO MESMO TEMPO INSERE
                            --AS MESMAS PERMISSOES DA PASTA SUPERIOR NO ANEXO ATUAL.
                            --vai inserindo o  codigo do anexo atual junto com as entidades associadas a entidade superior.
                            INSERT INTO {0}.{1}.AnexoAssociacao (CodigoAnexo, CodigoObjetoAssociado, CodigoTipoAssociacao)
                                                          SELECT @CodigoAnexo, CodigoObjetoAssociado, CodigoTipoAssociacao
                                                            FROM {0}.{1}.AnexoAssociacao 
                                                           WHERE CodigoAnexo = {6}--codigo da pasta superior
                                                             AND CodigoObjetoAssociado != {9} --Todos as entidades associadas, menos a entidade atual
                                                             AND CodigoTipoAssociacao = {10} 
                                  
                            -- CONTROLE DE VERSAO, registra com primeira versão - 18/11/2011 (acg)
                            INSERT INTO {0}.{1}.AnexoVersao (codigoAnexo, numeroVersao, dataVersao, codigoUsuarioInclusao, nomeArquivo)
                              VALUES (@CodigoAnexo, 1, getdate(), {3}, '{4}')

                            SELECT @codigoSequencialAnexo = scope_identity()

                            SELECT @CodigoAnexo as CodigoAnexo, @codigoSequencialAnexo as CodigoSequencialAnexo
                    END
                    ", bancodb, Ownerdb, descricaoAnexo.Replace("'", "''"), codigoUsuarioInclusao
                     , nomeAnexo.Replace("*", "").Replace("|", "").Replace("\\", "").Replace(":", "").Replace("\"", "").Replace("&lt;", "").Replace("&gt;", "").Replace("?", "").Replace("/", "")
                     .Replace("'", "''"), codigoEntidade, codigoPastaSuperior.HasValue ? codigoPastaSuperior.Value.ToString() : "null", indicaPasta, IndicaControladoSistema, codigoProjeto, CodigoTipoAssociacao);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                codigoNovoAnexo = int.Parse(ds.Tables[0].Rows[0]["CodigoAnexo"].ToString());
            }

            return "";
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }


    //##############################################################

    public bool VerificaDuplicadeAnexo(string codigoUsuarioResponsavel, string nomeNovoAnexo, string codigoEntidadeUsuarioResponsavel, int CodigoPastaDestino, int codigoTipoAssociacao, string IDObjetoAssociado)
    {
        DataSet dsParamatro = getParametrosSistema(int.Parse(codigoEntidadeUsuarioResponsavel), "TamanhoGravacaoArquivoFisico", "DiretorioGravacaoAnexosEmDisco");
        comandoSQL = string.Format(@"
                                        select * from Anexo AS ANE
                                        INNER JOIN 
                                        AnexoAssociacao as ASS ON ANE.CodigoAnexo = ASS.CodigoAnexo

                                        where ANE.Nome = '{3}'   AND
                                        ANE.CodigoEntidade = {4} AND
                                        ANE.DataExclusao IS NULL AND                                        
                                        Ass.CodigoObjetoAssociado = {6} AND
                                        ANE.CodigoPastaSuperior = {7}
                                        ", bancodb, Ownerdb, codigoUsuarioResponsavel, nomeNovoAnexo, codigoEntidadeUsuarioResponsavel, codigoTipoAssociacao, IDObjetoAssociado, CodigoPastaDestino
                                   );
        DataSet ds = getDataSet(comandoSQL);
        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    //##############################################################

    public string incluirAnexo(string descricaoAnexo, string codigoUsuarioInclusao, string nomeAnexo, string codigoEntidade,
                             int? codigoPastaSuperior, char indicaPasta, char IndicaControladoSistema, int CodigoTipoAssociacao, string indicaLink, string palavraChave,
                             string codigoProjeto, byte[] arquivo, string indicaPublica, Boolean arquivoReuniao = false, string codigoItemArquivoReuniao = "", string codigoTipoArquivoReuniao = "")
    {
        string comandoSQL = "";
        string comandoArquivoReuniao = "";
        int tamanhoArquivo = 0;
        int tamanhoMaximoGravacaoBanco = 999999999;
        string diretorioGravacaoAnexosEmDisco = string.Empty;
        bool gravaFS = false;
        DataSet dsParamatro = getParametrosSistema(int.Parse(codigoEntidade), "TamanhoGravacaoArquivoFisico", "DiretorioGravacaoAnexosEmDisco");

        if (DataSetOk(dsParamatro) && DataTableOk(dsParamatro.Tables[0]) && dsParamatro.Tables[0].Rows[0]["TamanhoGravacaoArquivoFisico"].ToString() != "")
        {
            tamanhoMaximoGravacaoBanco = int.Parse(dsParamatro.Tables[0].Rows[0]["TamanhoGravacaoArquivoFisico"].ToString()) * 1024 * 1024;
            diretorioGravacaoAnexosEmDisco = dsParamatro.Tables[0].Rows[0]["DiretorioGravacaoAnexosEmDisco"] as string;
            if (!string.IsNullOrEmpty(diretorioGravacaoAnexosEmDisco))
            {
                diretorioGravacaoAnexosEmDisco = diretorioGravacaoAnexosEmDisco
                    .Replace("%PathPortal%", HostingEnvironment.ApplicationPhysicalPath);
            }
        }

        tamanhoArquivo = (arquivo == null) ? 0 : arquivo.Length;

        try
        {
            gravaFS = tamanhoArquivo >= tamanhoMaximoGravacaoBanco;

            comandoSQL = string.Format(@"
                    BEGIN
                        BEGIN TRAN
                        BEGIN TRY

                            DECLARE @CodigoAnexo AS bigint;
                            DECLARE @codigoSequencialAnexo AS bigint;

                            DECLARE @CodigoPastaSuperior AS INT;

                            INSERT INTO {0}.{1}.Anexo
                                   (DescricaoAnexo
                                   ,DataInclusao
                                   ,CodigoUsuarioInclusao
                                   ,Nome
                                   ,CodigoEntidade
                                   ,CodigoPastaSuperior
                                   ,IndicaPasta
                                   ,IndicaControladoSistema
                                   ,IndicaLink
                                   ,PalavraChave
                                   ,IndicaAnexoPublicoExterno)
                            VALUES
                                   ('{2}', GETDATE(), {3}, '{4}', {5}, {6}, '{7}', '{8}', {11}, '{12}', '{13}')
                            
                            SELECT @CodigoAnexo = scope_identity()
                            
                            INSERT into {0}.{1}.AnexoAssociacao (CodigoAnexo, CodigoObjetoAssociado, CodigoTipoAssociacao)
                            VALUES (@CodigoAnexo, {9}, {10})

                            --FAZ UM SELECT PRA VER QUAIS SÃO AS PERMISSOES DA PASTA SUPERIOR E AO MESMO TEMPO INSERE
                            --AS MESMAS PERMISSOES DA PASTA SUPERIOR NO ANEXO ATUAL.
                            --vai inserindo o  codigo do anexo atual junto com as entidades associadas a entidade superior.
                            INSERT INTO {0}.{1}.AnexoAssociacao (CodigoAnexo, CodigoObjetoAssociado, CodigoTipoAssociacao)
                                                          SELECT @CodigoAnexo, CodigoObjetoAssociado, CodigoTipoAssociacao
                                                            FROM {0}.{1}.AnexoAssociacao 
                                                           WHERE CodigoAnexo = {6}--codigo da pasta superior
                                                             AND CodigoObjetoAssociado != {9} --Todos as entidades associadas, menos a entidade atual
                                                             AND CodigoTipoAssociacao = {10} 
                                  
                            -- CONTROLE DE VERSAO, registra com primeira versão - 18/11/2011 (acg)
                            INSERT INTO {0}.{1}.AnexoVersao (codigoAnexo, numeroVersao, dataVersao, codigoUsuarioInclusao, nomeArquivo
                                   ,IndicaDestinoGravacaoAnexo)
                              VALUES (@CodigoAnexo, 1, getdate(), {3}, '{4}', '{14}')

                            SELECT @codigoSequencialAnexo = scope_identity()

                    
                    ", bancodb, Ownerdb, descricaoAnexo.Replace("'", "''"), codigoUsuarioInclusao
                     , nomeAnexo.Replace("'", "''").Replace("*", "").Replace("|", "").Replace("\\", "").Replace(":", "").Replace("\"", "").Replace("&lt;", "").Replace("&gt;", "").Replace("?", "").Replace("/", "")
                     , codigoEntidade, codigoPastaSuperior.HasValue ? codigoPastaSuperior.Value.ToString() : "null"
                     , indicaPasta, IndicaControladoSistema, codigoProjeto, CodigoTipoAssociacao, indicaLink, palavraChave, indicaPublica, gravaFS ? "DI" : "BD");

            ///DataSet ds = getDataSet(comandoSQL);
			// se tem arquivo... vamos inserir no banco de dados
            if (arquivo != null)
            {
                SqlConnection conexao = new SqlConnection(strConn);
                SqlCommand comando = new SqlCommand();

                if (gravaFS)
                {
                    conexao.Open();
                    comando.Connection = conexao;
                    comando.CommandType = CommandType.Text;
                    comando.CommandText = comandoSQL + string.Format(
                                                     @"
                        COMMIT;
                        END TRY
                        BEGIN CATCH

                            DECLARE @ErrorMessage NVarchar(4000), @ErrorSeverity Int, @ErrorState Int;
                            SET @ErrorMessage = ERROR_MESSAGE();
                            SET @ErrorSeverity = ERROR_SEVERITY();
                            SET @ErrorState = ERROR_STATE();
                            
                            ROLLBACK;
    
		                    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);

                        END CATCH
                    END");

                    comando.CommandTimeout = CDIS.ClasseDados.TimeOutSqlCommand;
                    comando.ExecuteNonQuery();

                    string nomeArquivo = string.Format("{0}_{1}_{2}_V1_{3}", codigoPastaSuperior, codigoProjeto, CodigoTipoAssociacao, nomeAnexo);
                    string caminhoArquivo = Path.Combine(diretorioGravacaoAnexosEmDisco, nomeArquivo);
                    File.WriteAllBytes(caminhoArquivo, arquivo);
                }
                else
                {
                    if (arquivoReuniao)
                    {
                        comandoArquivoReuniao = string.Format("INSERT INTO {0}.{1}.AnexoItemPautaReuniao VALUES (@CodigoAnexo, {2}, {3}, {4})", bancodb, Ownerdb, codigoProjeto, codigoItemArquivoReuniao, codigoTipoArquivoReuniao);
                    }


                    comando.Connection = conexao;
                    comando.CommandType = CommandType.Text;
                    comando.CommandText = comandoSQL + string.Format(
                                                     @"INSERT INTO {0}.{1}.ConteudoAnexo
                                                    (CodigoSequencialAnexo
                                                    , Anexo) 
                                            VALUES (@codigoSequencialAnexo, @Anexo){2}
                        COMMIT;
                        END TRY
                        BEGIN CATCH

                            DECLARE @ErrorMessage NVarchar(4000), @ErrorSeverity Int, @ErrorState Int;
                            SET @ErrorMessage = ERROR_MESSAGE();
                            SET @ErrorSeverity = ERROR_SEVERITY();
                            SET @ErrorState = ERROR_STATE();
                            
                            ROLLBACK;
    
		                    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);

                        END CATCH
                    END", bancodb, Ownerdb, comandoArquivoReuniao);
                    // comando.Parameters.Add(new SqlParameter("@CodigoSequencialAnexo", SqlDbType.Int, 0, ParameterDirection.Input, 0, 0, "CodigoSequencialAnexo", DataRowVersion.Current, false, null, "", "", ""));
                    comando.Parameters.Add(new SqlParameter("@Anexo", SqlDbType.Image, 0, ParameterDirection.Input, 0, 0, "Anexo", DataRowVersion.Current, false, null, "", "", ""));

                    comando.Parameters[0].Value = arquivo;

                    comando.CommandTimeout = CDIS.ClasseDados.TimeOutSqlCommand;

                    conexao.Open();
                    comando.ExecuteNonQuery();
                }


            }
            else
            {
                // se o arquivo estiver nulo e não for uma pasta => exceção!!!
                if (indicaPasta != 'S')
                {
                    throw new Exception("Não foi possível detectar o conteúdo do arquivo a incluir!");
                }
                comandoSQL += @"
                        COMMIT;
                        END TRY
                        BEGIN CATCH

                            DECLARE @ErrorMessage NVarchar(4000), @ErrorSeverity Int, @ErrorState Int;
                            SET @ErrorMessage = ERROR_MESSAGE();
                            SET @ErrorSeverity = ERROR_SEVERITY();
                            SET @ErrorState = ERROR_STATE();

                            ROLLBACK;

                            RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);

                        END CATCH
                    END";
                int regAf = 0;

                execSQL(comandoSQL, ref regAf);
            }
            return "";
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    /// <summary>
    /// Retorna a imagem do arquivo anexo
    /// </summary>
    /// <param name="codigoAnexo">Codigo do anexo</param>
    /// <param name="codigoSequencialAnexo">Código da versão. Se for nulo, será retornada a imagem da versão mais recente</param>
    /// <param name="NomeArquivo"></param>
    /// <returns></returns>
    public byte[] getConteudoAnexo(int codigoAnexo, int? codigoSequencialAnexo, ref string NomeArquivo, string IndicaDestinoGravacaoAnexo)
    {
        byte[] ImagemArmazenada = null;

        if (IndicaDestinoGravacaoAnexo != "DI")
        {
            string comandoObtemUltimaVersao =
                @"DECLARE @codigoSequencialAnexo bigint
              ";
            // se não informou o codigoSequencial, pega a última versão
            if (!codigoSequencialAnexo.HasValue)
                comandoObtemUltimaVersao += string.Format("SELECT @codigoSequencialAnexo = max(codigoSequencialAnexo) from {0}.{1}.AnexoVersao where codigoAnexo = {2} ", bancodb, Ownerdb, codigoAnexo);
            else
                comandoObtemUltimaVersao += string.Format("SELECT @codigoSequencialAnexo = {0} ", codigoSequencialAnexo.Value);


            string ComandoSQL = string.Format(
                 @"{2}

               SELECT AV.nomeArquivo, CA.Anexo 
                 FROM {0}.{1}.Anexo AS A INNER JOIN
                      {0}.{1}.AnexoVersao AS AV ON AV.codigoAnexo = A.codigoAnexo INNER JOIN
                      {0}.{1}.ConteudoAnexo AS CA ON AV.codigoSequencialAnexo = CA.codigoSequencialAnexo
                WHERE     (CA.codigoSequencialAnexo = @codigoSequencialAnexo )", bancodb, Ownerdb, comandoObtemUltimaVersao);

            DataSet ds = getDataSet(ComandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                NomeArquivo = ds.Tables[0].Rows[0][0].ToString();
                ImagemArmazenada = (byte[])ds.Tables[0].Rows[0][1];
            }
        }
        else
        {
            string comandoObtemUltimaVersao =
                @"DECLARE @codigoSequencialAnexo bigint
              ";
            // se não informou o codigoSequencial, pega a última versão
            if (!codigoSequencialAnexo.HasValue)
                comandoObtemUltimaVersao += string.Format("SELECT @codigoSequencialAnexo = max(codigoSequencialAnexo) from {0}.{1}.AnexoVersao where codigoAnexo = {2} ", bancodb, Ownerdb, codigoAnexo);
            else
                comandoObtemUltimaVersao += string.Format("SELECT @codigoSequencialAnexo = {0} ", codigoSequencialAnexo.Value);


            string ComandoSQL = string.Format(
                 @"{2}

               SELECT a.Nome, a.CodigoEntidade, aa.CodigoObjetoAssociado, aa.CodigoTipoAssociacao, a.CodigoPastaSuperior, AV.NumeroVersao
                 FROM {0}.{1}.Anexo AS a INNER JOIN
                      {0}.{1}.AnexoVersao AS AV ON AV.codigoAnexo = a.codigoAnexo INNER JOIN
                      {0}.{1}.AnexoAssociacao AS aa ON (aa.CodigoAnexo = a.CodigoAnexo ) 
                WHERE     (AV.codigoSequencialAnexo = @codigoSequencialAnexo )", bancodb, Ownerdb, comandoObtemUltimaVersao);


            DataSet ds = getDataSet(ComandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                NomeArquivo = ds.Tables[0].Rows[0][0].ToString();
                string codigoEntidade = ds.Tables[0].Rows[0]["CodigoEntidade"].ToString();
                string codigoObjeto = ds.Tables[0].Rows[0]["CodigoObjetoAssociado"].ToString();
                string codigoAssociacao = ds.Tables[0].Rows[0]["CodigoTipoAssociacao"].ToString();
                string codigoPasta = ds.Tables[0].Rows[0]["CodigoPastaSuperior"].ToString();
                string numeroVersao = ds.Tables[0].Rows[0]["NumeroVersao"].ToString();

                string diretorioGravacaoAnexosEmDisco = string.Empty;
                DataSet dsParamatro = getParametrosSistema(int.Parse(codigoEntidade), "DiretorioGravacaoAnexosEmDisco");

                if (DataSetOk(dsParamatro) && DataTableOk(dsParamatro.Tables[0]))
                {
                    diretorioGravacaoAnexosEmDisco = dsParamatro.Tables[0].Rows[0]["DiretorioGravacaoAnexosEmDisco"] as string;
                    if (!string.IsNullOrEmpty(diretorioGravacaoAnexosEmDisco))
                    {
                        diretorioGravacaoAnexosEmDisco = diretorioGravacaoAnexosEmDisco
                            .Replace("%PathPortal%", HostingEnvironment.ApplicationPhysicalPath);
                    }
                }
                string nomeArquivo = string.Format("{0}_{1}_{2}_V{3}_{4}", codigoPasta, codigoObjeto, codigoAssociacao, numeroVersao, NomeArquivo);
                string caminhoArquivo = Path.Combine(diretorioGravacaoAnexosEmDisco, nomeArquivo);

                ImagemArmazenada = File.ReadAllBytes(caminhoArquivo);

            }
        }
        return ImagemArmazenada;
    }

    public bool anexoEDaEntidadeAtual(int codigoAnexo, int codigoEntidadeAtual, ref string erros)
    {
        bool retorno = false;
        try
        {
            int codigoEntidadeCriacaoAnexo = 0;
            comandoSQL = string.Format(@"SELECT CodigoEntidade FROM {0}.{1}.Anexo WHERE CodigoAnexo = {2}", bancodb, Ownerdb, codigoAnexo);
            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                codigoEntidadeCriacaoAnexo = int.Parse(ds.Tables[0].Rows[0]["CodigoEntidade"].ToString());
            }
            if (codigoEntidadeCriacaoAnexo == codigoEntidadeAtual)
            {
                retorno = true;
            }
            else
            {
                retorno = false;
            }

        }
        catch (Exception ex)
        {
            erros = ex.Message;
            retorno = false;
        }
        return retorno;
    }

    public bool anexoEstaCompartilhadoEmEntidades(int codigoAnexo, int codigoEntidade, int codigoTipoAssociacao, int codigoObjetoAssociado)
    {
        comandoSQL = string.Format(@"SELECT 1 FROM {0}.{1}.AnexoAssociacao WHERE CodigoAnexo = {2} and CodigoObjetoAssociado != {3} and CodigoTipoAssociacao = (select CodigoTipoAssociacao from {0}.{1}.TipoAssociacao where IniciaisTipoAssociacao = 'EN')", bancodb, Ownerdb, codigoAnexo, codigoEntidade);
        DataSet ds = getDataSet(comandoSQL);
        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            comandoSQL = string.Format(@"SELECT 1 FROM {0}.{1}.AnexoAssociacao WHERE CodigoObjetoAssociado = {2} AND CodigoTipoAssociacao = {3} AND IndicaLinkCompartilhado = 'S'", bancodb, Ownerdb, codigoObjetoAssociado, codigoTipoAssociacao);

            ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    public bool anexoEstaLinkado(int codigoAnexo, int codigoEntidade)
    {
        comandoSQL = string.Format(@"SELECT 1 FROM {0}.{1}.AnexoAssociacao WHERE IndicaLinkCompartilhado = 'S' AND CodigoAnexo = {2} and CodigoObjetoAssociado != {3} and CodigoTipoAssociacao != (select CodigoTipoAssociacao from {0}.{1}.TipoAssociacao where IniciaisTipoAssociacao = 'EN')", bancodb, Ownerdb, codigoAnexo, codigoEntidade);
        DataSet ds = getDataSet(comandoSQL);
        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            return true;

        }
        else
        {
            return false;
        }
    }

    public bool anexoEPasta(int codigoAnexo)
    {
        bool retorno = false;
        try
        {
            string indicapasta = "";
            comandoSQL = string.Format(@"SELECT IndicaPasta FROM {0}.{1}.Anexo WHERE CodigoAnexo = {2}", bancodb, Ownerdb, codigoAnexo);
            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                indicapasta = ds.Tables[0].Rows[0]["IndicaPasta"].ToString();
            }
            retorno = (indicapasta == "S");

        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool excluiAnexoProjeto(char indicaPasta, int codigoAnexo, int codigoUsuarioExclusao, int codigoEntidade, int codigoProjeto, int codigoTipoAssociacao, ref string erros)
    {
        bool retorno = false;
        try
        {
            if (indicaPasta != 'S' && indicaPasta != 'N')
            {
                erros = "A indicação (IndicaPasta) do tipo de objeto a ser inserido não é válida. Utilize 'S' ou 'N'." + Delimitador_Erro;
                retorno = false;
            }

            int registrosAfetados = 0;

            // se for pasta, verifica se ela não possui "filhos"
            if (indicaPasta == 'S')
            {
                comandoSQL = string.Format(
                    @"SELECT codigoAnexo
                        FROM {0}.{1}.Anexo 
                       WHERE CodigoPastaSuperior = {3}
                         AND CodigoEntidade = {4}
                       --  AND IndicaControladoSistema = 'N'
                         AND DataExclusao is null
                         AND CodigoPastaSuperior <> CodigoAnexo                              
                            ", bancodb, Ownerdb, codigoAnexo, codigoAnexo, codigoEntidade);
                DataSet ds = getDataSet(comandoSQL);
                if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                {
                    //throw new Exception("Só é possível excluir pastas que estejam vazias." + Delimitador_Erro);
                    erros = "Só é possível excluir pastas que estejam vazias.";
                    retorno = false;
                }
                else
                {
                    comandoSQL = string.Format(
                                            @"
                      BEGIN TRAN
                       BEGIN TRY  

                          DELETE FROM {0}.{1}.[AnexoAssociacao]
                          WHERE CodigoAnexo = {4}
                            AND CodigoObjetoAssociado = {5}
                            AND CodigoTipoAssociacao = {6}

                         DELETE FROM {0}.{1}.[AnexoVersao] WHERE CodigoAnexo = {2}

                         DELETE FROM {0}.{1}.Anexo WHERE CodigoAnexo = {2} and CodigoEntidade = {7}
                     
			             commit tran
		               END TRY
		               BEGIN CATCH
                            ROLLBACK
                            DECLARE @ErrorMessage NVARCHAR(4000);
                            DECLARE @ErrorSeverity INT;
                            DECLARE @ErrorState INT;

                            SELECT @ErrorMessage = CHAR(13)+'Houve um erro ao efetivar a exclusão do anexo ou pasta, para o projeto.' + CHAR(13)+'Favor entrar em contato com o administrador ' + 
                                    'do Sistema.' + CHAR(13)+'Mensagem original do erro:' + CHAR(13) + CHAR(13) + ERROR_MESSAGE();
                            SELECT @ErrorSeverity = ERROR_SEVERITY();
                            SELECT @ErrorState = ERROR_STATE();
                            RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
		               END CATCH  
                    ", bancodb, Ownerdb, codigoAnexo, codigoUsuarioExclusao, codigoAnexo, codigoProjeto, codigoTipoAssociacao, codigoEntidade);
                    execSQL(comandoSQL, ref registrosAfetados);
                    retorno = true;
                }
            }
            else // arquivo -- Arquivo não é excluído fisicamente
            {

                comandoSQL = string.Format(
                   @"
                  BEGIN
                    IF EXISTS(SELECT 1 FROM {0}.{1}.AnexoAssociacao WHERE CodigoAnexo = {2} AND CodigoObjetoAssociado = {5} AND CodigoTipoAssociacao = {6} AND IndicaLinkCompartilhado = 'S')
                        DELETE FROM {0}.{1}.AnexoAssociacao WHERE CodigoAnexo = {2} AND CodigoObjetoAssociado = {5} AND CodigoTipoAssociacao = {6} AND IndicaLinkCompartilhado = 'S'
                    ELSE
                        UPDATE {0}.{1}.Anexo
                            SET DataExclusao = getdate()
                               ,[Nome] = LEFT([Nome], 220) + ' - EXCLUÍDO EM ' + CONVERT(varchar(30), GETDATE(), 120)-- Ex:  NomeAnexo.pdf - EXCLUÍDO EM 2018 - 02 - 05 14:28:10
                              , codigoUsuarioExclusao = {4}
                          WHERE CodigoAnexo = {2} 
                            AND CodigoEntidade = {3}
                 END", bancodb, Ownerdb, codigoAnexo, codigoEntidade, codigoUsuarioExclusao, codigoProjeto, codigoTipoAssociacao);

                execSQL(comandoSQL, ref registrosAfetados);
                retorno = true;
            }
        }
        catch (Exception ex)
        {
            erros = getErroExcluiRegistro(ex.Message);
            retorno = false;
        }
        return retorno;
    }

    public string getNomeProjeto(string codigoProjeto, string where)
    {
        string nomeProjeto = "";
        comandoSQL = string.Format(@"
                    SELECT  NomeProjeto
                    FROM    {0}.{1}.Projeto
                    WHERE   CodigoProjeto = {2} {3}
                    ", bancodb, Ownerdb, codigoProjeto, where);
        DataSet ds = getDataSet(comandoSQL);
        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            nomeProjeto = ds.Tables[0].Rows[0]["nomeProjeto"].ToString();

        return nomeProjeto;
    }

    public bool atualizaAnexo(string descricao, int codigoAnexo)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"UPDATE {0}.{1}.Anexo
                                           SET DescricaoAnexo = '{2}'
                                         WHERE CodigoAnexo = {3}", bancodb, Ownerdb, descricao, codigoAnexo);
            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Controle de versão - Chekout
    public bool registraCheckoutArquivoAnexo(int codigoAnexo, int codigoUsuario)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"DECLARE @DataATual datetime
                                             SET @DataATual  = getDate()

                                        UPDATE {0}.{1}.Anexo
                                           SET dataCheckOut = @DataATual
                                             , dataCheckIn = null
                                             , codigoUsuarioCheckOut = {3}
                                         WHERE CodigoAnexo = {2}

                                         DECLARE @numeroUltimaVersao int
                                             SET @numeroUltimaVersao = (SELECT max(numeroVersao) FROM {0}.{1}.AnexoVersao WHERE codigoAnexo = {2} )

                                        UPDATE {0}.{1}.AnexoVersao
                                           SET dataCheckOut = @DataAtual
                                             , dataCheckIn = null
                                             , codigoUsuarioCheckOut = {3}
                                         WHERE CodigoAnexo = {2}
                                           and dataCheckIn is null
                                              ", bancodb, Ownerdb, codigoAnexo, codigoUsuario);
            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Controle de versão - Chekin
    public bool registraCheckinArquivoAnexo(int codigoAnexo, int codigoUsuario, string nomeNovoAnexo, byte[] arquivo)
    {
        string comandoSQL = "";
        int tamanhoArquivo = 0;
        int tamanhoMaximoGravacaoBanco = 999999999;
        string diretorioGravacaoAnexosEmDisco = string.Empty;
        bool gravaFS = false;
        DataSet dsParamatro = getParametrosSistema(int.Parse(getInfoSistema("CodigoEntidade").ToString()), "TamanhoGravacaoArquivoFisico", "DiretorioGravacaoAnexosEmDisco");

        if (DataSetOk(dsParamatro) && DataTableOk(dsParamatro.Tables[0]) && dsParamatro.Tables[0].Rows[0]["TamanhoGravacaoArquivoFisico"].ToString() != "")
        {
            tamanhoMaximoGravacaoBanco = int.Parse(dsParamatro.Tables[0].Rows[0]["TamanhoGravacaoArquivoFisico"].ToString()) * 1024 * 1024;
            diretorioGravacaoAnexosEmDisco = dsParamatro.Tables[0].Rows[0]["DiretorioGravacaoAnexosEmDisco"] as string;
            if (!string.IsNullOrEmpty(diretorioGravacaoAnexosEmDisco))
            {
                diretorioGravacaoAnexosEmDisco = diretorioGravacaoAnexosEmDisco
                    .Replace("%PathPortal%", HostingEnvironment.ApplicationPhysicalPath);
            }
        }

        tamanhoArquivo = arquivo.Length;

        try
        {
            gravaFS = tamanhoArquivo >= tamanhoMaximoGravacaoBanco;

            if (gravaFS)
            {
                comandoSQL = string.Format(@"
                                    BEGIN
                                         
                                         DECLARE @numeroUltimaVersao int
                                             SET @numeroUltimaVersao = (SELECT max(numeroVersao) FROM {0}.{1}.AnexoVersao WHERE codigoAnexo = {2} )
                                     
                                     SELECT @numeroUltimaVersao + 1 AS NumeroVersao

                                    END", bancodb, Ownerdb, codigoAnexo);

                DataSet ds = getDataSet(comandoSQL);

                if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                {
                    string versaoAnexo = ds.Tables[0].Rows[0]["NumeroVersao"].ToString();
                    comandoSQL = string.Format(
                @"SELECT a.Nome, a.CodigoEntidade, aa.CodigoObjetoAssociado, aa.CodigoTipoAssociacao, a.CodigoPastaSuperior
                          FROM Anexo AS a INNER JOIN
                               AnexoAssociacao AS aa ON (aa.CodigoAnexo = a.CodigoAnexo ) 
                         WHERE a.CodigoAnexo = {2}", bancodb, Ownerdb, codigoAnexo);

                    ds = getDataSet(comandoSQL);
                    if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                    {
                        string NomeArquivo = ds.Tables[0].Rows[0][0].ToString();
                        string codigoEntidade = ds.Tables[0].Rows[0]["CodigoEntidade"].ToString();
                        string codigoObjeto = ds.Tables[0].Rows[0]["CodigoObjetoAssociado"].ToString();
                        string codigoAssociacao = ds.Tables[0].Rows[0]["CodigoTipoAssociacao"].ToString();
                        string codigoPasta = ds.Tables[0].Rows[0]["CodigoPastaSuperior"].ToString();


                        string nomeArquivo = string.Format("{0}_{1}_{2}_V{3}_{4}", codigoPasta, codigoObjeto, codigoAssociacao, versaoAnexo, NomeArquivo);
                        string caminhoArquivo = Path.Combine(diretorioGravacaoAnexosEmDisco, nomeArquivo);
                        FileStream fs = new FileStream(caminhoArquivo, FileMode.Create);

                        foreach (byte bt in arquivo)
                            fs.WriteByte(bt);

                        fs.Close();
                    }
                }

                comandoSQL = string.Format(@"
                                    BEGIN TRAN
                                         DECLARE @codigoSequencialAnexo AS bigint;

                                         DECLARE @DataATual datetime
                                             SET @DataATual  = getDate()

                                        UPDATE {0}.{1}.Anexo
                                           SET dataCheckIn = @DataATual                                              
                                         WHERE CodigoAnexo = {2}

                                         DECLARE @numeroUltimaVersao int
                                             SET @numeroUltimaVersao = (SELECT max(numeroVersao) FROM {0}.{1}.AnexoVersao WHERE codigoAnexo = {2} )

                                        UPDATE {0}.{1}.AnexoVersao
                                           SET dataCheckIn = @DataAtual
                                         WHERE CodigoAnexo = {2}
                                           AND numeroVersao = @numeroUltimaVersao 

                                        INSERT INTO {0}.{1}.AnexoVersao (codigoAnexo, numeroVersao, dataVersao, codigoUsuarioInclusao, nomeArquivo, IndicaDestinoGravacaoAnexo)
                                            VALUES ({2}, @numeroUltimaVersao + 1, @DataATual, {3}, '{4}','DI')

                                        SELECT @codigoSequencialAnexo = scope_identity()

                                        

                                    COMMIT TRAN", bancodb, Ownerdb, codigoAnexo, codigoUsuario, nomeNovoAnexo);

                SqlConnection conexao = new SqlConnection(strConn);
                SqlCommand comando = new SqlCommand();

                comando.Connection = conexao;
                comando.CommandType = CommandType.Text;
                comando.CommandText = comandoSQL;
                comando.Parameters.Add(new SqlParameter("@Anexo", SqlDbType.Image, 0, ParameterDirection.Input, 0, 0, "Anexo", DataRowVersion.Current, false, null, "", "", ""));

                comando.Parameters[0].Value = arquivo;

                comando.CommandTimeout = CDIS.ClasseDados.TimeOutSqlCommand;
                conexao.Open();
                comando.ExecuteNonQuery();
            }
            else
            {
                comandoSQL = string.Format(@"
                                    BEGIN TRAN
                                         DECLARE @codigoSequencialAnexo AS bigint;

                                         DECLARE @DataATual datetime
                                             SET @DataATual  = getDate()

                                        UPDATE {0}.{1}.Anexo
                                           SET dataCheckIn = @DataATual
                                         WHERE CodigoAnexo = {2}

                                         DECLARE @numeroUltimaVersao int
                                             SET @numeroUltimaVersao = (SELECT max(numeroVersao) FROM {0}.{1}.AnexoVersao WHERE codigoAnexo = {2} )

                                        UPDATE {0}.{1}.AnexoVersao
                                           SET dataCheckIn = @DataAtual
                                         WHERE CodigoAnexo = {2}
                                           AND numeroVersao = @numeroUltimaVersao 

                                        INSERT INTO {0}.{1}.AnexoVersao (codigoAnexo, numeroVersao, dataVersao, codigoUsuarioInclusao, nomeArquivo)
                                            VALUES ({2}, @numeroUltimaVersao + 1, @DataATual, {3}, '{4}')

                                        SELECT @codigoSequencialAnexo = scope_identity()

                                        INSERT INTO {0}.{1}.ConteudoAnexo
                                                    (CodigoSequencialAnexo, Anexo) 
                                            VALUES (@codigoSequencialAnexo, @Anexo)

                                    COMMIT TRAN", bancodb, Ownerdb, codigoAnexo, codigoUsuario, nomeNovoAnexo);

                SqlConnection conexao = new SqlConnection(strConn);
                SqlCommand comando = new SqlCommand();

                comando.Connection = conexao;
                comando.CommandType = CommandType.Text;
                comando.CommandText = comandoSQL;
                comando.Parameters.Add(new SqlParameter("@Anexo", SqlDbType.Image, 0, ParameterDirection.Input, 0, 0, "Anexo", DataRowVersion.Current, false, null, "", "", ""));

                comando.Parameters[0].Value = arquivo;

                comando.CommandTimeout = CDIS.ClasseDados.TimeOutSqlCommand;

                conexao.Open();
                comando.ExecuteNonQuery();
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string registraCheckinIncondicionalAnexo(int codigoAnexo, int codigoUsuario)
    {
        string comandoSQL = "";

        try
        {

            comandoSQL = string.Format(@"
                                    BEGIN TRAN

                                         DECLARE @DataATual datetime
                                             SET @DataATual  = getDate()

                                        UPDATE {0}.{1}.Anexo
                                           SET dataCheckIn = @DataATual
                                         WHERE CodigoAnexo = {2}
                                       
                                    COMMIT TRAN", bancodb, Ownerdb, codigoAnexo, codigoUsuario);

            SqlConnection conexao = new SqlConnection(strConn);
            SqlCommand comando = new SqlCommand();

            comando.Connection = conexao;
            comando.CommandType = CommandType.Text;
            comando.CommandText = comandoSQL;
            comando.CommandTimeout = CDIS.ClasseDados.TimeOutSqlCommand;

            conexao.Open();
            int rows = comando.ExecuteNonQuery();

            return "OK";
        }
        catch (Exception e)
        {
            return ("Erro ao desbloquear o arquivo.\\n\\n" + e.Message.Replace(Environment.NewLine, "\n")).Replace("'", "");
        }
    }


    public void download(int codigoAnexo, int? codigoSequencialAnexo, Page page, HttpResponse Response, HttpRequest Request, bool forceDownload)
    {
        string comandoSQL = "";

        if (codigoSequencialAnexo.HasValue)
            comandoSQL = string.Format(@"SELECT IndicaDestinoGravacaoAnexo FROM {0}.{1}.AnexoVersao WHERE CodigoSequencialAnexo = {2}", getDbName()
                , getDbOwner()
                , codigoSequencialAnexo.Value);
        else
            comandoSQL = string.Format(@"
            BEGIN
                DECLARE @codigoSequencialAnexo AS bigint;
                DECLARE @numeroUltimaVersao int
                SET @numeroUltimaVersao = (SELECT max(numeroVersao) FROM {0}.{1}.AnexoVersao WHERE codigoAnexo = {2} )
                SELECT IndicaDestinoGravacaoAnexo FROM {0}.{1}.AnexoVersao WHERE CodigoAnexo = {2} AND numeroVersao = @numeroUltimaVersao
            END", getDbName()
                , getDbOwner()
                , codigoAnexo);

        DataSet ds = getDataSet(comandoSQL);
        string IndicaDestinoGravacaoAnexo = "BD";

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            IndicaDestinoGravacaoAnexo = ds.Tables[0].Rows[0]["IndicaDestinoGravacaoAnexo"].ToString();
        }

        string NomeArquivo = "";
        // aqui ele pega o nome do arquivo no 'ref NomeArquivo'
        byte[] imagem = getConteudoAnexo(codigoAnexo, codigoSequencialAnexo, ref NomeArquivo, IndicaDestinoGravacaoAnexo);

        NomeArquivo = NomeArquivo.Replace("/", "").Replace("\\", "").Replace("*", "").Replace("?", "").Replace(":", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");

        if (NomeArquivo != "")
        {
            string arquivo = Request.ServerVariables["APPL_PHYSICAL_PATH"] + "ArquivosTemporarios\\" + NomeArquivo;
            //string arquivo2 = "../../ArquivosTemporarios/" + NomeArquivo;
            string arquivo2 = "~/ArquivosTemporarios/" + NomeArquivo;
            FileStream fs = new FileStream(arquivo, FileMode.Create, FileAccess.Write);
            fs.Write(imagem, 0, imagem.Length);
            fs.Close();

            ForceDownloadFile(arquivo2, page, Response, true, true);
        }
    }

    public void ForceDownloadFile(string caminhoVirtual, Page page, HttpResponse Response, bool forceDownload, bool incluirJSFecharJanela)
    {
        string path = page.MapPath(caminhoVirtual);
        string name = Path.GetFileName(path);
        string ext = Path.GetExtension(path);
        string type = "application/octet-stream";
        if (forceDownload)
        {
            Response.AppendHeader("content-disposition",
                "attachment; filename=\"" + name + "\"");
        }
        Response.ContentType = type;
        Response.WriteFile(path);
        Response.Flush();
        Response.End();
        if (incluirJSFecharJanela)
        {
            Response.Write("<SCRIPT LANGUAGE='JavaScript'>");
            Response.Write(" window.top.fechaModal();");
            Response.Write("</SCRIPT>");
        }

    }


    #endregion

    #region _Projetos - DadosProjeto - Riscos e Questões

    public DataSet getTipoRiscoQuestao(char indicaRiscoQuestao)
    {
        comandoSQL = string.Format(
            @"SELECT CodigoTipoRiscoQuestao
                   , DescricaoTipoRiscoQuestao
                   , IndicaControladoSistema
                   , IndicaRiscoQuestao
                   , Polaridade
                   , CAST( CASE Polaridade WHEN 'POS' THEN 'Positiva' ELSE 'Negativa' END AS Varchar(10) ) AS PolaridadeExtenso
               FROM {0}.{1}.[TipoRiscoQuestao]
              WHERE IndicaRiscoQuestao = '{2}'
              ORDER by DescricaoTipoRiscoQuestao", bancodb, Ownerdb, indicaRiscoQuestao.ToString());

        return getDataSet(comandoSQL);
    }

    public DataSet getRiscosQuestoes(int codigoProjeto, char tipoRiscoQuestao)
    {
        int? codigoUsuario = null;
        int? codigoEntidade = null;
        return getRiscosQuestoes(codigoProjeto, tipoRiscoQuestao, codigoUsuario, codigoEntidade);
    }

    public DataSet getQuantidadeRiscosQuestoesCriticas(int codigoProjeto, int codigoEntidade)
    {
        comandoSQL = string.Format(
           @"SELECT ISNULL(SUM(CASE WHEN IndicaRiscoQuestao = 'R' THEN 1 ELSE 0 END), 0) AS Riscos
	               ,ISNULL(SUM(CASE WHEN IndicaRiscoQuestao = 'Q' THEN 1 ELSE 0 END), 0) AS Questoes
              FROM {0}.{1}.RiscoQuestao
             WHERE {0}.{1}.f_GetCorRiscoQuestao(CodigoRiscoQuestao) = 'Vermelho'
               AND (CodigoStatusRiscoQuestao = 'QA' OR CodigoStatusRiscoQuestao = 'RA')
               AND DataExclusao IS NULL
               AND CodigoProjeto = {2}
               AND CodigoEntidade = {3}"
            , bancodb, Ownerdb, codigoProjeto, codigoEntidade);

        return getDataSet(comandoSQL);
    }

    public DataSet getRiscosQuestoes(int? codigoProjeto, char tipoRiscoQuestao, int? codigoUsuario, int? codigoEntidade)
    {
        string sWhere = "";
        string whereStatusRisco = "";

        if (codigoProjeto.HasValue)
        {
            sWhere += string.Format(@" AND  (RQ.CodigoProjeto in 
                                            (
                                            select codigoprojetofilho from LinkProjeto LP inner join 
                                                                           projeto pr on pr.codigoprojeto = lp.codigoprojetofilho 
                                                                                     and pr.CodigoStatusProjeto = 3   
                                                                                              
                                            where LP.CodigoProjetoPai = {0}
                                            and LP.TipoLink = 'PP'
                                            )
                                            or RQ.CodigoProjeto = {0} ) ", codigoProjeto);
            whereStatusRisco = string.Format(" AND (dbo.f_verificaObjetoValido(RQ.CodigoRiscoQuestao, NULL, 'RQ', 0, RQ.CodigoEntidade, {0}, NULL, '{1}', 0) = 1) ", codigoProjeto, getIniciaisTipoProjeto(codigoProjeto.Value));
        }
        else
        {
            whereStatusRisco = " AND (dbo.f_verificaObjetoValido(RQ.CodigoRiscoQuestao, NULL, 'RQ', 0, RQ.CodigoEntidade, RQ.CodigoEntidade, NULL, 'EN', 0) = 1) ";
        }

        if ((codigoUsuario.HasValue) && (codigoEntidade.HasValue))
            sWhere += string.Format(@" 
				AND PRJ.[CodigoEntidade]		= {0}
				AND RQ.CodigoUsuarioResponsavel = {1} ", codigoEntidade, codigoUsuario);

        comandoSQL = string.Format(
            @"SELECT  RQ.CodigoRiscoQuestao, RQ.DescricaoRiscoQuestao, RQ.DetalheRiscoQuestao, CAST( CASE TRQ.Polaridade WHEN 'POS' THEN '{6}' ELSE '{7}' END AS Varchar(10) ) AS Polaridade, 
                  RQ.ProbabilidadePrioridade, RQ.ImpactoUrgencia, RQ.CodigoTipoRiscoQuestao, 
                  RQ.CodigoUsuarioResponsavel, RQ.DataStatusRiscoQuestao, RQ.DataPublicacao, SRQ.DescricaoStatusRiscoQuestao, TRQ.DescricaoTipoRiscoQuestao, 
                  UResp.NomeUsuario AS NomeUsuarioResponsavel, RQ.ProbabilidadePrioridade * RQ.ImpactoUrgencia as Severidade, RQ.ProbabilidadePrioridade, RQ.ImpactoUrgencia,
                  CONVERT(varchar, RQ.DataLimiteResolucao, 103) as DataLimiteResolucao, SRQ.CodigoStatusRiscoQuestao,
                  CASE WHEN SRQ.CodigoStatusRiscoQuestao IN ('RE', 'QR', 'RC', 'QC', 'RT') THEN RQ.DataStatusRiscoQuestao ELSE NULL END AS DataEliminacaoCancelamento, 
                  CONVERT(varchar, RQ.DataInclusao, 103) + ' - ' + UINC.NomeUsuario as IncluidoEmPor,
                  CONVERT(varchar, RQ.DataPublicacao, 103) + ' - ' + UPUB.NomeUsuario as PublicaoEmPor,
                  RQ.ConsequenciaRiscoQuestao, RQ.TratamentoRiscoQuestao, {1}.f_GetCorRiscoQuestao(RQ.CodigoRiscoQuestao) as CorRiscoQuestao,
                  CONVERT(varchar, GETDATE(), 103) as hoje, CASE WHEN RQ.DataPublicacao IS NOT NULL THEN '{8}' ELSE '{9}' END AS Publicado, PRJ.[CodigoProjeto], PRJ.NomeProjeto
                  , PRJ.IndicaPrograma, RQ.CodigoTipoRespostaRisco, RQ.CustoRiscoQuestao, RQ.CodigoRiscoQuestaoSuperior,
                  RQSUP.DescricaoRiscoQuestao as DescricaoRiscoQuestaoSuperior, TRR.DescricaoRespostaRisco
            FROM  {0}.{1}.RiscoQuestao AS RQ INNER JOIN
                  {0}.{1}.Projeto as PRJ on (PRJ.CodigoProjeto = RQ.CodigoProjeto) inner join
                  {0}.{1}.StatusRiscoQuestao AS SRQ ON RQ.CodigoStatusRiscoQuestao = SRQ.CodigoStatusRiscoQuestao INNER JOIN
                  {0}.{1}.TipoRiscoQuestao AS TRQ ON RQ.CodigoTipoRiscoQuestao = TRQ.CodigoTipoRiscoQuestao INNER JOIN
                  {0}.{1}.Usuario AS UResp ON RQ.CodigoUsuarioResponsavel = UResp.CodigoUsuario LEFT OUTER JOIN
                  {0}.{1}.Usuario AS UINC ON RQ.CodigoUsuarioInclusao = UINC.CodigoUsuario LEFT OUTER JOIN
                  {0}.{1}.Usuario AS UPUB ON RQ.CodigoUsuarioPublicacao = UPUB.CodigoUsuario LEFT OUTER JOIN
                  {0}.{1}.RiscoQuestao AS RQSUP ON (RQSUP.CodigoRiscoQuestao = RQ.CodigoRiscoQuestaoSuperior) LEFT OUTER JOIN
                  {0}.{1}.TipoRespostaRisco AS TRR ON (TRR.CodigoTipoRespostaRisco = RQ.CodigoTipoRespostaRisco)
            WHERE RQ.DataExclusao IS NULL
              AND RQ.IndicaRiscoQuestao = '{3}' {4} {5} 
           ORDER by RQ.DescricaoRiscoQuestao"
            , bancodb, Ownerdb, codigoProjeto, tipoRiscoQuestao.ToString(), sWhere, whereStatusRisco,
            Resources.traducao.positiva,
            Resources.traducao.negativa,
            Resources.traducao.sim,
            Resources.traducao.nao);

        return getDataSet(comandoSQL);
    }

    public DataSet getRiscoQuestao(int CodigoRiscoQuestao)
    {
        comandoSQL = string.Format(
            @"SELECT  Projeto.NomeProjeto, RQ.DescricaoRiscoQuestao, RQ.DetalheRiscoQuestao, RQ.ProbabilidadePrioridade, RQ.ImpactoUrgencia, RQ.CodigoTipoRiscoQuestao, 
                      RQ.CodigoUsuarioResponsavel, RQ.DataStatusRiscoQuestao, RQ.DataPublicacao, SRQ.CodigoStatusRiscoQuestao, SRQ.DescricaoStatusRiscoQuestao, TRQ.DescricaoTipoRiscoQuestao, 
                      UResp.NomeUsuario AS NomeUsuarioResponsavel, RQ.ProbabilidadePrioridade * RQ.ImpactoUrgencia as Severidade, 
                      CONVERT(varchar, RQ.DataLimiteResolucao, 103) as DataLimiteResolucao,{0}.{1}.f_GetCorRiscoQuestao(RQ.CodigoRiscoQuestao) as CorRiscoQuestao,
                      CASE WHEN SRQ.CodigoStatusRiscoQuestao IN ('RE', 'QR', 'RC', 'QC', 'RT') THEN RQ.DataStatusRiscoQuestao ELSE NULL END AS DataEliminacaoCancelamento, 
                      CONVERT(varchar, RQ.DataInclusao, 103) + ' - ' + UINC.NomeUsuario as IncluidoEmPor,
                      CONVERT(varchar, RQ.DataPublicacao, 103) + ' - ' + UPUB.NomeUsuario as PublicaoEmPor,
                      RQ.ConsequenciaRiscoQuestao, RQ.TratamentoRiscoQuestao,
                      Comentario.DescricaoComentario, Comentario.DataComentario, Comentario.DataComentarioData, Comentario.CodigoUsuarioComentario, Comentario.NomeUsuarioComentario
                FROM  {0}.{1}.Usuario AS UPUB RIGHT OUTER JOIN 
                             (
                                  {0}.{1}.Projeto INNER JOIN
                                  {0}.{1}.RiscoQuestao AS RQ INNER JOIN
                                  {0}.{1}.StatusRiscoQuestao AS SRQ ON RQ.CodigoStatusRiscoQuestao = SRQ.CodigoStatusRiscoQuestao INNER JOIN
                                  {0}.{1}.TipoRiscoQuestao AS TRQ ON RQ.CodigoTipoRiscoQuestao = TRQ.CodigoTipoRiscoQuestao INNER JOIN
                                  {0}.{1}.Usuario AS UResp ON RQ.CodigoUsuarioResponsavel = UResp.CodigoUsuario ON Projeto.CodigoProjeto = RQ.CodigoProjeto 
                             ) ON UPUB.CodigoUsuario = RQ.CodigoUsuarioPublicacao LEFT OUTER JOIN
                      {0}.{1}.Usuario AS UINC ON RQ.CodigoUsuarioInclusao = UINC.CodigoUsuario LEFT JOIN
                      ( SELECT top 1 CRQ.CodigoRiscoQuestao, CRQ.DescricaoComentario, convert(varchar, CRQ.DataComentario, 103) as DataComentario, CRQ.DataComentario AS DataComentarioData, CRQ.CodigoUsuarioComentario, U.NomeUsuario as NomeUsuarioComentario
						  FROM {0}.{1}.ComentarioRiscoQuestao AS CRQ INNER JOIN
							   {0}.{1}.Usuario AS U ON CRQ.CodigoUsuarioComentario = U.CodigoUsuario 
					     WHERE CRQ.CodigoRiscoQuestao = {2}
						 ORDER by CRQ.DataComentario desc  ) as Comentario on (Comentario.CodigoRiscoQuestao = RQ.CodigoRiscoQuestao)
                WHERE RQ.CodigoRiscoQuestao = {2}
                  AND RQ.DataExclusao is null"
            , bancodb, Ownerdb, CodigoRiscoQuestao);

        return getDataSet(comandoSQL);
    }

    public DataSet getComentarioRiscosQuestoes(int codigoRiscoQuestao)
    {
        comandoSQL = string.Format(
            @"SELECT CRQ.CodigoComentario, CRQ.DescricaoComentario, CRQ.DataComentario, 
                     CRQ.CodigoUsuarioComentario, U.NomeUsuario
                FROM {0}.{1}.ComentarioRiscoQuestao AS CRQ INNER JOIN
                     {0}.{1}.Usuario AS U ON CRQ.CodigoUsuarioComentario = U.CodigoUsuario 
                WHERE CRQ.CodigoRiscoQuestao = {2}
               ORDER by CRQ.DataComentario DESC"
            , bancodb, Ownerdb, codigoRiscoQuestao);

        return getDataSet(comandoSQL);
    }

    #endregion

    #region _Projetos - DadosProjeto - Caracterização

    public bool atualizaCaracterizacaoProjeto(int codigoProjeto, string codigoCategoria, int codigoUnidadeNegocio, int codigoGerenteProjeto, string nomeProjeto)
    {
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                             UPDATE {0}.{1}.Projeto SET
                                        CodigoUnidadeNegocio = {3}
                                       ,CodigoGerenteProjeto = {4}
                                       ,CodigoCategoria      = {5}
                                       ,DataUltimaAlteracao  = getdate()
                                       ,NomeProjeto          = '{6}'
                                 WHERE  CodigoProjeto   = {2}
                        END        
            ", bancodb, Ownerdb, codigoProjeto, codigoUnidadeNegocio, codigoGerenteProjeto, codigoCategoria, nomeProjeto);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion

    #region _Projetos - DadosProjeto - Permissões do Projetos


    public DataSet getUsuarioCorporativoDaEntidadeAtiva(string codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT u.CodigoUsuario
                      ,u.NomeUsuario
                      ,u.EMail

                FROM        {0}.{1}.usuario                 AS u 
                INNER JOIN  {0}.{1}.UsuarioUnidadeNegocio   AS uun ON u.CodigoUsuario = uun.CodigoUsuario

                WHERE uun.CodigoUnidadeNegocio              = {2}
                  AND uun.IndicaUsuarioAtivoUnidadeNegocio  = 'S'
                  AND u.[CodigoUsuario] IN (
						SELECT rc.[CodigoUsuario] FROM {0}.{1}.[RecursoCorporativo] AS [rc] 
						WHERE rc.[CodigoEntidade]					=  {2} 
							AND rc.[CodigoTipoRecurso]			= 1 
							AND rc.[DataDesativacaoRecurso]	IS NULL)
                  {3}

                ORDER BY u.NomeUsuario 
                ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public int getEntidadUnidadeNegocio(int codigoUnidade)
    {
        string comandoSQL = string.Format(@"
                SELECT CodigoEntidade
                  FROM UnidadeNegocio
                 WHERE CodigoUnidadeNegocio = {0}
                ", codigoUnidade);

        int codigoEntidade = -1;

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            codigoEntidade = int.Parse(ds.Tables[0].Rows[0]["CodigoEntidade"].ToString());

        return codigoEntidade;
    }

    public DataSet getUsuarioDaEntidadeAtiva(string codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT u.CodigoUsuario
                      ,u.NomeUsuario
                      ,u.EMail

                FROM        {0}.{1}.usuario                 AS u 
                INNER JOIN  {0}.{1}.UsuarioUnidadeNegocio   AS uun ON u.CodigoUsuario = uun.CodigoUsuario

                WHERE uun.CodigoUnidadeNegocio              = {2}
                  AND uun.IndicaUsuarioAtivoUnidadeNegocio  = 'S'
                  AND u.DataExclusao IS NULL
                  {3}

                ORDER BY u.NomeUsuario 
                ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getUsuarioDaEntidadeAtivaParaEnvioDeEmails(string codigoEntidade, string codigoEvento, int idProjeto)
    {
        string comandoSQL = string.Format(@"
                select distinct CodigoResponsavelEvento as CodigoUsuario from  dbo.Evento 
                WHERE CodigoEvento = {3} and CodigoEntidade = {2} and CodigoObjetoAssociado = {4}
                UNION
                select distinct CodigoUsuario as CodigoUsuario
                      FROM {0}.{1}.Usuario u INNER JOIN
		                   {0}.{1}.ParticipanteEvento pe ON pe.CodigoParticipante = u.CodigoUsuario
                      WHERE CodigoEvento = {3}
                            AND CodigoEntidadeAcessoPadrao = {2}
                ", bancodb, Ownerdb, codigoEntidade, codigoEvento, idProjeto);
        return getDataSet(comandoSQL);
    }

    public DataSet getUnidadesDaEntidadeAtiva(string codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT u.CodigoUnidadeNegocio
                      ,u.SiglaUnidadeNegocio
                      ,u.NomeUnidadeNegocio

                FROM        {0}.{1}.unidadeNegocio          AS u
                WHERE u.CodigoEntidade              = {2}
                  {3}

                ORDER BY u.SiglaUnidadeNegocio 
                ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getMenuProjetoUsuario(string codigoUsuario, string codigoProjeto, string codigoEntidade)
    {
        string comandoSQL = string.Format(@"
                SELECT * FROM {0}.{1}.f_GetMenuProjeto({2},{3}, {4})
            ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, codigoProjeto);
        return getDataSet(comandoSQL);
    }

    public DataSet getMenuObjeto(string codigoUsuario, string codigoObjeto, string iniciaisObjeto, string codigoEntidade)
    {
        string comandoSQL = string.Format(@"
                SELECT * FROM {0}.{1}.f_GetMenuObjeto({2}, {3}, {4}, '{5}')
            ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, codigoObjeto, iniciaisObjeto);
        return getDataSet(comandoSQL);
    }

    public bool getIndicaProjetoObra(string codigoProjeto)
    {
        string comandoSQL = string.Format(@"
                SELECT 1 
                  FROM {0}.{1}.Projeto p
                 WHERE EXISTS(SELECT 1 
                                FROM {0}.{1}.Obra o 
                               WHERE o.CodigoProjeto = p.CodigoProjeto
                                 AND o.CodigoProjeto = {2})
            ", bancodb, Ownerdb, codigoProjeto);
        DataSet ds = getDataSet(comandoSQL);

        return DataSetOk(ds) && DataTableOk(ds.Tables[0]);
    }

    public DataSet GetPermissoesUsuarioProjeto(string codigoUsuario, string codigoProjeto, string codigoEntidade)
    {
        string comandoSQL = string.Format(@"
                BEGIN
	                DECLARE @MenuCronograma int,
                            @MenuRH int,
                            @MenuFinanceiro int,
                            @MenuToDoList int,
                            @MenuCaracterizacao int,
                            @MenuPermissao int,
                            @MenuRiscos int,
                            @MenuQuestoes int,
                            @MenuFormulario int,
                            @MenuFluxo int,
                            @MenuAnexo int,
                            @MenuMensage int,
                            @MenuReuniao int,
                            @MenuParametros int,
                            @EdicaoCronograma Int,
                            @CodigoProjeto int,
                            @CodigoUsuario int,
                            @CodigoEntidade int,
                            @MenuConfigurarRelatorioStatus int,
                            @MenuEnviarDestinatariosRelatorioStatus int,
                            @MenuPublicarRelatorioStatus int,
                            @MenuEditarComentariosRelatorioStatus int,
                            @MenuStatusProjeto int,
                            @MenuAtualizacaoMetas int,
                            @MenuAtualizacaoResultados int,
                            @MenuAnaliseMetas int,
                            @MenuAcompanhamentoMetas int,
                            @MenuDesbloqueio int,
                            @MenuEAP int,
                            @MenuAnalises int,
                            @EdicaoEAP Int,
                            @MenuInterdependencia Int,
                            @MenuEditarPrevisao Int
                            
                					
	                SET @CodigoProjeto = {3}
	                SET @CodigoUsuario = {2}
                    SET @CodigoEntidade = {4}


	                --menu principal
                    SELECT @MenuStatusProjeto = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsStt');
	                SELECT @MenuCronograma = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsCrn');
	                SELECT @MenuRH = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_AcsTlaRHU');
	                SELECT @MenuFinanceiro = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsFin');
	                SELECT @MenuToDoList = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsTdl');
	                SELECT @MenuCaracterizacao = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_AltCaract');
	                SELECT @MenuPermissao = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_AdmPrs');
	                --menu Riscos
	                SELECT @MenuRiscos = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsRQ1');
	                SELECT @MenuQuestoes = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsRQ2');
	                --menu formulario
	                SELECT @MenuFormulario = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsCnuFrm');
	                --menu fluxo
	                SELECT @MenuFluxo = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsFlx');
	                --menu mensagem
	                SELECT @MenuAnexo = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsAnx');
	                SELECT @MenuMensage = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsMsg');
	                SELECT @MenuReuniao = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsReu');
	                SELECT @MenuParametros = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsCRsRel');
	                SELECT @EdicaoCronograma = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CadCrn');
                    SELECT @MenuConfigurarRelatorioStatus = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_AltDstSttRpt');
                    SELECT @MenuEnviarDestinatariosRelatorioStatus = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_EnvRptStt');
                    SELECT @MenuPublicarRelatorioStatus = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_PubSttRpt');
                    SELECT @MenuEditarComentariosRelatorioStatus = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CmtSttRpt');
                    SELECT @MenuAtualizacaoMetas = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_DefMta');
                    SELECT @MenuAtualizacaoResultados = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_RegRes');
                    SELECT @MenuAnaliseMetas = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_AnlMta');
                    SELECT @MenuAcompanhamentoMetas = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_AcmMta');
                    SELECT @MenuDesbloqueio = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_DesBlq');
                    SELECT @MenuEAP = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsEAP'); 
                    SELECT @MenuAnalises = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsAnl'); 
                    SELECT @EdicaoEAP = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CadEAP');                   
                    SELECT @MenuInterdependencia = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_CnsInterdep');
                    SELECT @MenuEditarPrevisao = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PR', 0, null, 'PR_AltPrvOrc');



	                SELECT  @MenuStatusProjeto AS MenuStatusProjeto,
                            @MenuCronograma AS VisualizarCronograma, 
                            @MenuRH AS MenuRH,
                            @MenuFinanceiro AS MenuFinanceiro,
                            @MenuToDoList AS MenuToDoList,
                            @MenuCaracterizacao AS MenuCaracterizacao,
                            @MenuPermissao AS MenuPermissao,
                            @MenuRiscos AS MenuRiscos,
                            @MenuQuestoes AS MenuQuestoes,
                            @MenuFormulario AS MenuFormulario,
                            @MenuFluxo AS MenuFluxo,
                            @MenuAnexo AS MenuAnexo,
                            @MenuMensage AS MenuMensage,
                            @MenuReuniao AS MenuReuniao,
                            @MenuParametros AS MenuParametros,
                            @EdicaoCronograma AS EditarCronograma,
                            @MenuConfigurarRelatorioStatus AS MenuConfigurarRelatorioStatus,
                            @MenuEnviarDestinatariosRelatorioStatus AS MenuEnviarDestinatariosRelatorioStatus,
                            @MenuPublicarRelatorioStatus AS MenuPublicarRelatorioStatus,
                            @MenuEditarComentariosRelatorioStatus AS MenuEditarComentariosRelatorioStatus,
                            @MenuAtualizacaoMetas AS MenuAtualizacaoMetas,
                            @MenuAtualizacaoResultados AS MenuAtualizacaoResultados,
                            @MenuAnaliseMetas AS MenuAnaliseMetas,
                            @MenuAcompanhamentoMetas AS MenuAcompanhamentoMetas,
                            @MenuDesbloqueio AS MenuDesbloqueio,
                            @MenuEAP AS VisualizarEAP,
                            @MenuAnalises AS MenuAnalises,
                            @EdicaoEAP AS EditarEAP,
                            @MenuInterdependencia AS MenuInterdependencia,
                            @MenuEditarPrevisao AS MenuEditarPrevisao
                END
            ", bancodb, Ownerdb, codigoUsuario, codigoProjeto, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet GetPermissoesUsuarioDemanda(string codigoUsuario, string codigoProjeto, string codigoEntidade)
    {
        string comandoSQL = string.Format(@"
                BEGIN
	                DECLARE @MenuCronograma int,
                            @MenuRH int,
                            @MenuFinanceiro int,
                            @MenuToDoList int,
                            @MenuCaracterizacao int,
                            @MenuPermissao int,
                            @MenuRiscos int,
                            @MenuQuestoes int,
                            @MenuFormulario int,
                            @MenuFluxo int,
                            @MenuAnexo int,
                            @MenuMensage int,
                            @MenuReuniao int,
                            @MenuParametros int,
                            @EdicaoCronograma Int,
                            @CodigoProjeto int,
                            @CodigoUsuario int,
                            @CodigoEntidade int,
                            @MenuConfigurarRelatorioStatus int,
                            @MenuEnviarDestinatariosRelatorioStatus int,
                            @MenuPublicarRelatorioStatus int,
                            @MenuEditarComentariosRelatorioStatus int,
                            @MenuStatusProjeto int,
                            @MenuAtualizacaoMetas int,
                            @MenuAtualizacaoResultados int,
                            @MenuAnaliseMetas int,
                            @MenuDesbloqueio int
                            
                					
	                SET @CodigoProjeto = {3}
	                SET @CodigoUsuario = {2}
                    SET @CodigoEntidade = {4}


	                --menu principal
                    SELECT @MenuStatusProjeto = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_Cns');
	                SELECT @MenuCronograma = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_CnsCrn');
	                SELECT @MenuRH = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_AcsTlaRHU');
	                SELECT @MenuFinanceiro = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_CnsFin');
	                SELECT @MenuToDoList = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_CnsTdl');
	                SELECT @MenuCaracterizacao = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_AltCaract');
	                SELECT @MenuPermissao = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_AdmPrs');
	                --menu Riscos
	                SELECT @MenuRiscos = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_CnsRQ1');
	                SELECT @MenuQuestoes = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_CnsRQ2');
	                --menu formulario
	                SELECT @MenuFormulario = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_CnsCnuFrm');
	                --menu fluxo
	                SELECT @MenuFluxo = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_CnsFlx');
	                --menu mensagem
	                SELECT @MenuAnexo = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_CnsAnx');
	                SELECT @MenuMensage = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_CnsMsg');
	                SELECT @MenuReuniao = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_CnsReu');
	                SELECT @MenuParametros = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_CnsCRsRel');
	                SELECT @EdicaoCronograma = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_AltCrn');
                    SELECT @MenuConfigurarRelatorioStatus = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_CmtSttRpt');
                    SELECT @MenuEnviarDestinatariosRelatorioStatus = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_EnvRptStt');
                    SELECT @MenuPublicarRelatorioStatus = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_PubSttRpt');
                    SELECT @MenuEditarComentariosRelatorioStatus = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_CmtSttRpt');
                    SELECT @MenuAtualizacaoMetas = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_DefMta');
                    SELECT @MenuAtualizacaoResultados = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_RegRes');
                    SELECT @MenuAnaliseMetas = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_AnlMta');
                    SELECT @MenuDesbloqueio = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'DC', 0, null, 'DC_DesBlq');
                    
	                SELECT  @MenuStatusProjeto AS MenuStatusProjeto,
                            @MenuCronograma AS VisualizarCronograma, 
                            @MenuRH AS MenuRH,
                            @MenuFinanceiro AS MenuFinanceiro,
                            @MenuToDoList AS MenuToDoList,
                            @MenuCaracterizacao AS MenuCaracterizacao,
                            @MenuPermissao AS MenuPermissao,
                            @MenuRiscos AS MenuRiscos,
                            @MenuQuestoes AS MenuQuestoes,
                            @MenuFormulario AS MenuFormulario,
                            @MenuFluxo AS MenuFluxo,
                            @MenuAnexo AS MenuAnexo,
                            @MenuMensage AS MenuMensage,
                            @MenuReuniao AS MenuReuniao,
                            @MenuParametros AS MenuParametros,
                            @EdicaoCronograma AS EditarCronograma,
                            @MenuConfigurarRelatorioStatus AS MenuConfigurarRelatorioStatus,
                            @MenuEnviarDestinatariosRelatorioStatus AS MenuEnviarDestinatariosRelatorioStatus,
                            @MenuPublicarRelatorioStatus AS MenuPublicarRelatorioStatus,
                            @MenuEditarComentariosRelatorioStatus AS MenuEditarComentariosRelatorioStatus,
                            @MenuAtualizacaoMetas AS MenuAtualizacaoMetas,
                            @MenuAtualizacaoResultados AS MenuAtualizacaoResultados,
                            @MenuAnaliseMetas AS MenuAnaliseMetas,
                            @MenuDesbloqueio AS MenuDesbloqueio
                END
            ", bancodb, Ownerdb, codigoUsuario, codigoProjeto, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet GetPermissoesUsuarioProcesso(string codigoUsuario, string codigoProjeto, string codigoEntidade)
    {
        string comandoSQL = string.Format(@"
                BEGIN
	                DECLARE @MenuCronograma int,
                            @MenuRH int,
                            @MenuFinanceiro int,
                            @MenuToDoList int,
                            @MenuCaracterizacao int,
                            @MenuPermissao int,
                            @MenuRiscos int,
                            @MenuQuestoes int,
                            @MenuFormulario int,
                            @MenuFluxo int,
                            @MenuAnexo int,
                            @MenuMensage int,
                            @MenuReuniao int,
                            @MenuParametros int,
                            @EdicaoCronograma Int,
                            @CodigoProjeto int,
                            @CodigoUsuario int,
                            @CodigoEntidade int,
                            @MenuConfigurarRelatorioStatus int,
                            @MenuEnviarDestinatariosRelatorioStatus int,
                            @MenuPublicarRelatorioStatus int,
                            @MenuEditarComentariosRelatorioStatus int,
                            @MenuStatusProjeto int,
                            @MenuAtualizacaoMetas int,
                            @MenuAtualizacaoResultados int,
                            @MenuAnaliseMetas int,
                            @MenuDesbloqueio int
                            
                					
	                SET @CodigoProjeto = {3}
	                SET @CodigoUsuario = {2}
                    SET @CodigoEntidade = {4}


	                --menu principal
                    SELECT @MenuStatusProjeto = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_Cns');
	                SELECT @MenuCronograma = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_CnsCrn');
	                SELECT @MenuRH = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_AcsTlaRHU');
	                SELECT @MenuFinanceiro = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_CnsFin');
	                SELECT @MenuToDoList = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_CnsTdl');
	                SELECT @MenuCaracterizacao = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_AltCaract');
	                SELECT @MenuPermissao = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_AdmPrs');
	                --menu Riscos
	                SELECT @MenuRiscos = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_CnsRQ1');
	                SELECT @MenuQuestoes = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_CnsRQ2');
	                --menu formulario
	                SELECT @MenuFormulario = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_CnsCnuFrm');
	                --menu fluxo
	                SELECT @MenuFluxo = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_CnsFlx');
	                --menu mensagem
	                SELECT @MenuAnexo = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_CnsAnx');
	                SELECT @MenuMensage = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_CnsMsg');
	                SELECT @MenuReuniao = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_CnsReu');
	                SELECT @MenuParametros = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_CnsCRsRel');
	                SELECT @EdicaoCronograma = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_AltCrn');
                    SELECT @MenuConfigurarRelatorioStatus = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_CmtSttRpt');
                    SELECT @MenuEnviarDestinatariosRelatorioStatus = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_EnvRptStt');
                    SELECT @MenuPublicarRelatorioStatus = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_PubSttRpt');
                    SELECT @MenuEditarComentariosRelatorioStatus = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_CmtSttRpt');
                    SELECT @MenuAtualizacaoMetas = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_DefMta');
                    SELECT @MenuAtualizacaoResultados = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_RegRes');
                    SELECT @MenuAnaliseMetas = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_AnlMta');
                    SELECT @MenuDesbloqueio = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjeto, null, 'PC', 0, null, 'PC_DesBlq');
                    
	                SELECT  @MenuStatusProjeto AS MenuStatusProjeto,
                            @MenuCronograma AS VisualizarCronograma, 
                            @MenuRH AS MenuRH,
                            @MenuFinanceiro AS MenuFinanceiro,
                            @MenuToDoList AS MenuToDoList,
                            @MenuCaracterizacao AS MenuCaracterizacao,
                            @MenuPermissao AS MenuPermissao,
                            @MenuRiscos AS MenuRiscos,
                            @MenuQuestoes AS MenuQuestoes,
                            @MenuFormulario AS MenuFormulario,
                            @MenuFluxo AS MenuFluxo,
                            @MenuAnexo AS MenuAnexo,
                            @MenuMensage AS MenuMensage,
                            @MenuReuniao AS MenuReuniao,
                            @MenuParametros AS MenuParametros,
                            @EdicaoCronograma AS EditarCronograma,
                            @MenuConfigurarRelatorioStatus AS MenuConfigurarRelatorioStatus,
                            @MenuEnviarDestinatariosRelatorioStatus AS MenuEnviarDestinatariosRelatorioStatus,
                            @MenuPublicarRelatorioStatus AS MenuPublicarRelatorioStatus,
                            @MenuEditarComentariosRelatorioStatus AS MenuEditarComentariosRelatorioStatus,
                            @MenuAtualizacaoMetas AS MenuAtualizacaoMetas,
                            @MenuAtualizacaoResultados AS MenuAtualizacaoResultados,
                            @MenuAnaliseMetas AS MenuAnaliseMetas,
                            @MenuDesbloqueio AS MenuDesbloqueio
                END
            ", bancodb, Ownerdb, codigoUsuario, codigoProjeto, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region _Projetos - DadosProjeto - IntegracaoOrcamentoERP.aspx

    public DataSet getGridIntegracaoOrcamento(string codigoProjeto, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT DISTINCT mo.DescricaoMovimentoOrcamento
			          ,mop.CodigoReservado
			          ,mo.Ano
			          ,mop.NomeCR
                      ,mop.CodigoCR
                      ,un.SiglaUnidadeNegocio
                FROM {0}.{1}.vi_MovimentoOrcamento AS mo
	                 INNER JOIN {0}.{1}.vi_MovimentoOrcamentoProjeto AS mop
		                     ON mo.CodigoMovimentoOrcamento = mop.CodigoMovimentoOrcamento AND mop.DataExclusao IS NULL
	                 INNER JOIN ProjetoCR AS pcr
		                     ON mop.CodigoCR = pcr.CodigoCR 
                     INNER JOIN UnidadeNegocio AS un
                             ON ( un.CodigoEntidade = mop.CodigoEntidade   AND
                                  un.CodigoEntidade = un.CodigoUnidadeNegocio )
  
                WHERE pcr.CodigoProjeto = {2}
                  {3}
                ORDER BY mo.Ano desc, mop.NomeCR
        ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getMovimientoOrcamentario(string where)
    {
        string comandoSQL = string.Format(@"
            SELECT Distinct CodigoMovimentoOrcamento, DescricaoMovimentoOrcamento  
              FROM {0}.{1}.vi_MovimentoOrcamento 
             WHERE CodigoStatusMovimentoOrcamento in (1, 3)
               {2} 
             ORDER BY DescricaoMovimentoOrcamento
        ", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getListagemCR(string codigoMovimentoOrcamentario, int codigoProjeto, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
        BEGIN
            DECLARE @CodigoEntidadeOrcamento int

            SELECT TOP 1 @CodigoEntidadeOrcamento = CodigoEntidadeOrcamento 
              FROM {0}.{1}.vi_MovimentoOrcamento
             WHERE CodigoMovimentoOrcamento = {2}
               AND CodigoEntidade = {3}

            SELECT DISTINCT CodigoCR, nomeCR, CodigoReservado
              FROM {0}.{1}.vi_MovimentoOrcamentoProjeto 
             WHERE CodigoMovimentoOrcamento = {2}
               AND DataExclusao IS NULL
               AND NomeCR IS NOT NULL
               AND CodigoEntidadeOrcamento = @CodigoEntidadeOrcamento
                {5}
             ORDER BY NomeCR
        END
        ", bancodb, Ownerdb, codigoMovimentoOrcamentario, codigoEntidade, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }


    public DataSet getCRseleccionado(string codigCR, string idProjeto, int codigoEntidade)
    {
        string comandoSQL = string.Format(@"
            SELECT CodigoCR, pCR.CodigoProjeto, pr.NomeProjeto, pr.CodigoEntidade, un.NomeUnidadeNegocio AS desUnidadeNegocio
              FROM ProjetoCR AS pCR 
				   INNER JOIN Projeto AS pr
						   ON (pCR.CodigoProjeto = pr.CodigoProjeto
                               AND pr.CodigoEntidade = {4})
				   INNER JOIN UnidadeNegocio un
						   ON pr.CodigoEntidade = un.CodigoEntidade
             WHERE CodigoCR = {2}
               --AND CodigoProjeto = {3}
        ", bancodb, Ownerdb, codigCR, idProjeto, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public bool incluirProjetoIntegraCR(string codigoProjeto, string codigoCR)
    {
        bool retorno = true;
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
                  IF NOT EXISTS(SELECT 1 FROM {0}.{1}.ProjetoCR 
                                 WHERE CodigoProjeto = {2} 
                                   AND CodigoCR = {3})
                  BEGIN
                     INSERT INTO {0}.{1}.ProjetoCR(CodigoProjeto, CodigoCR)
                     VALUES({2}, {3})
				  END", bancodb, Ownerdb, codigoProjeto, codigoCR);

            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(getErroIncluiRegistro(ex.Message));
        }
        return retorno;
    }

    public bool excluiProjetoIntegraCR(string codigoProjeto, string codigoCR)
    {
        int registrosAfetados = 0;
        bool retorno = false;
        string mensagem = "";
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
                    DELETE From {0}.{1}.ProjetoCR
                     WHERE CodigoCR = {2}
                       AND CodigoProjeto = {3}
                ", bancodb, Ownerdb, codigoCR, codigoProjeto);
            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            mensagem = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaCuboOrcamentoAnoCorrente(string codigoEntidade)
    {
        bool retorno = true;
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
                  EXEC {0}.{1}.p_AtualizaCuboOrcamento {2}, 1, {3}
				", bancodb, Ownerdb, DateTime.Now.Year, codigoEntidade);

            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(getErroIncluiRegistro(ex.Message));
        }
        return retorno;
    }

    #endregion
    #region _Projetos - DadosProjeto - Contratos - Previsao Financeira

    public DataSet getSomaPrevisaoFinanceira(int codigoContrato)
    {
        string comandoSQL = string.Format(@"
                SELECT SUM(ISNULL(ValorPrevisto,0)) AS Soma
                  FROM {0}.{1}.PrevisaoFinanceira 
                 WHERE CodigoContrato = {2}
                   AND DataExclusao IS NULL
                ", bancodb, Ownerdb, codigoContrato);
        return getDataSet(comandoSQL);
    }

    public DataSet getPrevisaoFinanceira(string where)
    {
        string comandoSQL = string.Format(@"
                SELECT   pf.[CodigoPrevisaoFinanceira]
                        ,pf.[CodigoContrato]
                        ,pf.[DataInclusao]
                        ,pf.[CodigoUsuarioInclusao]
                        ,pf.[ValorPrevisto]
                        ,pf.[DataExclusao]
                        ,pf.[CodigoUsuarioExclusao]
                        ,pf.[ObservacoesPrevisaoFinanceira]
                        ,pf.[DataPrevisao]
	                    ,ui.NomeUsuario AS UsuarioInclusao
                        
                 FROM {0}.{1}.PrevisaoFinanceira pf LEFT JOIN
	                  {0}.{1}.Usuario ui ON ui.CodigoUsuario = pf.CodigoUsuarioInclusao
                WHERE pf.DataExclusao IS NULL
                  {2}
                 ORDER BY pf.[DataPrevisao] 
                ", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    public bool incluirPrevisaoFinanceiraContratoObra(int codigoContrato, string valorPrevisao, string dataPrevisao, string observacoes, int codigoUsuarioInclusao, ref string msgErro)
    {
        DataSet ds;
        bool retorno = true;
        try
        {

            comandoSQL = string.Format(@"
              BEGIN
                DECLARE @CodigoAditivo int

                INSERT INTO {0}.{1}.[PrevisaoFinanceira]
                                   ([CodigoContrato]
                                   ,[DataInclusao]
                                   ,[CodigoUsuarioInclusao]
                                   ,[DataPrevisao]
                                   ,[ValorPrevisto]
                                   ,[ObservacoesPrevisaoFinanceira])
                     VALUES({2}, GetDate(), {3}, {4}, {5}, '{6}')

               END
				", bancodb, Ownerdb
                 , codigoContrato
                 , codigoUsuarioInclusao
                 , dataPrevisao
                 , valorPrevisao.Replace(".", "").Replace(",", ".")
                 , observacoes.Replace("'", "''"));

            ds = getDataSet(comandoSQL);

            msgErro = "";

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = getErroIncluiRegistro(ex.Message);
        }
        return retorno;
    }


    public bool atualizaPrevisaoFinanceiraContratoObra(int codigoPrevisao, int codigoContrato, string valorPrevisao, string dataPrevisao, string observacoes, ref string msgErro)
    {
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"
              BEGIN

                UPDATE {0}.{1}.[PrevisaoFinanceira] SET CodigoContrato = {3}
                                                 , [DataPrevisao] = {4}
                                                 , [ValorPrevisto] = {5}
                                                 , [ObservacoesPrevisaoFinanceira] = '{6}'
                     WHERE CodigoPrevisaoFinanceira = {2}

               END
				", bancodb, Ownerdb
                 , codigoPrevisao
                 , codigoContrato
                 , dataPrevisao
                 , valorPrevisao.Replace(".", "").Replace(",", ".")
                 , observacoes.Replace("'", "''"));

            int registrosAfetados = 0;
            execSQL(comandoSQL, ref registrosAfetados);

            msgErro = "";

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = getErroIncluiRegistro(ex.Message);
        }
        return retorno;
    }


    public bool excluiPrevisaoFinanceiraContratoObra(int codigoPrevisao, int codigoUsuario, ref string msgErro)
    {
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"
              BEGIN

                UPDATE {0}.{1}.PrevisaoFinanceira SET CodigoUsuarioExclusao = {3}
                                                 , DataExclusao = GetDate()
                     WHERE CodigoPrevisaoFinanceira = {2}
               END
				", bancodb, Ownerdb
                 , codigoPrevisao, codigoUsuario);

            int registrosAfetados = 0;
            execSQL(comandoSQL, ref registrosAfetados);

            msgErro = "";

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = getErroIncluiRegistro(ex.Message);
        }
        return retorno;
    }

    #endregion

    #region _Projetos - DadosProjeto - Contratos



    public DataSet getContratosAquisicoes(int codigoUnidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT    usu.NomeUsuario
                        , un.CodigoUnidadeNegocio
                        , un.NomeUnidadeNegocio
                        , frfi.NomeFonte
                        , taq.CodigoTipoAquisicao
                        , taq.DescricaoTipoAquisicao
                        , cont.CodigoContrato
                        , proj.NomeProjeto
                        , cont.NumeroContrato
                        , cont.StatusContrato 
                        , pe.NomePessoa as Fornecedor
                        , cont.DataInicio
                        , cont.DataTermino
                        , cont.CodigoUsuarioResponsavel
                        , cont.DescricaoObjetoContrato
                        , cont.CodigoFonteRecursosFinanceiros
                        , cont.Observacao
                        , cont.NumeroContratoSAP
                        , cont.CodigoProjeto
                        , cont.CodigoTipoContrato
                        , tc.DescricaoTipoContrato
                        , cont.ValorContrato
                        , cont.ValorContratoOriginal
                        ,CASE WHEN cont.StatusContrato = 'A' THEN 'Ativo' ELSE 'Encerrado' END AS SituacaoContrato
                        , {0}.{1}.f_VerificaAcessoConcedido({3}, {2}, cont.CodigoContrato, NULL, 'CT', 0, NULL, 'CT_Alt') * 2 + 
						  {0}.{1}.f_VerificaAcessoConcedido({3}, {2}, cont.CodigoContrato, NULL, 'CT', 0, NULL, 'CT_Exc') * 4 +
						  {0}.{1}.f_VerificaAcessoConcedido({3}, {2}, cont.CodigoContrato, NULL, 'CT', 0, NULL, 'CT_AdmPrs') * 8 AS [Permissoes]
                        , cont.CodigoPessoaContratada
                        , cont.TipoPessoa
                        , (select count(1) from ParcelaContrato pc where pc.CodigoContrato = cont.CodigoContrato AND pc.[DataExclusao] IS NULL) as TemParcelas
                        , cont.IndicaRevisaoPrevia
                        , tc.DescricaoTipoContrato
                        , (SELECT SUM( ISNULL(pc.ValorPago, 0) ) 
                             FROM ParcelaContrato pc 
                            WHERE pc.DataPagamento IS NOT NULL 
                              AND pc.CodigoContrato = cont.CodigoContrato AND pc.[DataExclusao] IS NULL ) as ValorPago
                        , cont.CodigoStatusComplementarContrato
                        , scc.DescricaoStatusComplementarContrato
                        , ISNULL(cont.ValorContrato, 0) - ISNULL( (SELECT SUM( ISNULL(pc.ValorPago, 0) ) 
                                                             FROM ParcelaContrato pc 
                                                            WHERE pc.DataPagamento IS NOT NULL 
                                                              AND pc.CodigoContrato = cont.CodigoContrato AND pc.[DataExclusao] IS NULL ),0) as Saldo
                        
                FROM        {0}.{1}.Contrato                    AS cont
                LEFT JOIN   {0}.{1}.Pessoa                     AS pe ON cont.CodigoPessoaContratada                   = pe.CodigoPessoa 
                LEFT JOIN   {0}.{1}.[PessoaEntidade] AS [pen] ON (
			                                                        pen.[CodigoPessoa] = cont.[CodigoPessoaContratada]
			                                                        AND pen.codigoEntidade = cont.[CodigoEntidade]
			                                                        )
                LEFT JOIN   {0}.{1}.Projeto                     AS proj ON cont.CodigoProjeto                   = proj.CodigoProjeto
                LEFT JOIN  {0}.{1}.TipoAquisicao               AS taq  ON cont.CodigoTipoAquisicao             = taq.CodigoTipoAquisicao
                LEFT JOIN  {0}.{1}.FonteRecursosFinanceiros    AS frfi ON cont.CodigoFonteRecursosFinanceiros  = frfi.CodigoFonteRecursosFinanceiros
                LEFT JOIN  {0}.{1}.TipoContrato                AS tc   ON (tc.CodigoTipoContrato                = cont.CodigoTipoContrato)
                INNER JOIN  {0}.{1}.UnidadeNegocio              AS un   ON cont.CodigoUnidadeNegocio            = un.CodigoUnidadeNegocio
                INNER JOIN  {0}.{1}.Usuario                     AS usu  ON cont.CodigoUsuarioResponsavel        = usu.CodigoUsuario
                LEFT JOIN {0}.{1}.StatusComplementarContrato   AS scc ON (scc.CodigoStatusComplementarContrato  = cont.CodigoStatusComplementarContrato)
                WHERE cont.CodigoContrato IN ( SELECT f.[CodigoContrato] FROM {0}.{1}.f_GetContratosUsuario({3}, {2}) AS [f] ) 
                      {4}

                ORDER BY cont.NumeroContrato
                ", bancodb, Ownerdb, codigoUnidade, codigoUsuario, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getListaContratosEstendidos(int codigoEntidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(@"SELECT * FROM {0}.{1}.f_GetListaContratosEstendidos({2},{3}) WHERE (1=1) {4} ORDER BY AnoContrato, NumeroContrato
                ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getContratosExtendidos(int codigoEntidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT cont.CodigoContrato, cont.CodigoTipoContrato, cont.NumeroContrato  
	                  ,cont.StatusContrato, p.CodigoStatusProjeto, cont.CodigoPessoaContratada
	                  ,cont.DescricaoObjetoContrato, o.CodigoMunicipioObra, o.CodigoSegmentoObra
	                  ,cont.DataInicio, cont.DataTermino, YEAR(cont.DataTermino) AS AnoTermino, cont.ValorContrato, cont.DataBaseReajuste
                      ,cont.CodigoCriterioReajusteContrato, o.CodigoTipoServico
	                  ,o.CodigoOrigemObra, cont.CodigoFonteRecursosFinanceiros
	                  ,cont.CodigoCriterioMedicaoContrato, cont.CodigoUsuarioResponsavel, cont.CodigoUnidadeNegocio
	                  ,prs.NomeContato, prs.NomePessoa, prs.NomeFantasia, mrsOrigem.NomeMunicipio NomeMunicipioOrigem, o.NumeroTrabalhadoresDiretos
	                  ,cont.Observacao, tc.DescricaoTipoContrato, u.NomeUsuario AS GestorContrato, cont.CodigoProjeto
                      ,CASE WHEN cont.StatusContrato = 'A' THEN 'Ativo' 
                            WHEN cont.StatusContrato = 'I' THEN 'Encerrado' ELSE 'Previsto' END AS SituacaoContrato
                      ,CASE WHEN o.CodigoProjeto IS NULL THEN 'N' ELSE 'S' END AS IndicaObra
                      ,(SELECT COUNT(1) FROM {0}.{1}.AditivoContrato ac WHERE ac.CodigoContrato = cont.CodigoContrato AND ac.DataExclusao IS NULL) AS QuantidadeAditivos
                      ,cont.DataAssinatura, cont.DataTerminoOriginal, cont.ValorContratoOriginal
                      ,{0}.{1}.f_VerificaAcessoConcedido({2}, {3}, cont.CodigoContrato, NULL, 'CT', 0, NULL, 'CT_Alt') * 2 + 
				       {0}.{1}.f_VerificaAcessoConcedido({2}, {3}, cont.CodigoContrato, NULL, 'CT', 0, NULL, 'CT_AdmPrs') * 4 +
                       {0}.{1}.f_VerificaAcessoConcedido({2}, {3}, cont.CodigoContrato, NULL, 'CT', 0, NULL, 'CT_Enc') * 8 AS [Permissoes]
                      , (SELECT isnull(SUM(isnull(ValorPrevisto,0)),0) FROM {0}.{1}.ParcelaContrato parc where parc.CodigoContrato = cont.CodigoContrato AND parc.[DataExclusao] IS NULL) AS ValorMedido 
                      ,{0}.{1}.f_VerificaAcessoConcedido({2}, {3}, cont.CodigoContrato, NULL, 'CT', 0, NULL, 'CT_AltNumCtr') As [PodeAlterarNumero]
                      ,{0}.{1}.f_VerificaAcessoConcedido({2}, {3}, cont.CodigoContrato, NULL, 'CT', 0, NULL, 'CT_Exc') As [PodeExcluirContrato]
                      ,CASE WHEN EXISTS(SELECT 1 FROM {0}.{1}.ParcelaContrato pc WHERE pc.CodigoContrato = cont.CodigoContrato AND pc.[DataExclusao] IS NULL)
							  OR EXISTS(SELECT 1 FROM {0}.{1}.AditivoContrato ac WHERE ac.CodigoContrato = cont.CodigoContrato )
							  OR EXISTS(SELECT 1 FROM {0}.{1}.ComentarioContrato cc WHERE cc.CodigoContrato = cont.CodigoContrato)
							  OR EXISTS(SELECT 1 FROM {0}.{1}.AnexoAssociacao aa
                                          INNER JOIN {0}.{1}.Anexo AN ON (an.CodigoAnexo = aa.CodigoAnexo and AN.DataExclusao IS NULL  )  
                                         WHERE aa.CodigoObjetoAssociado = cont.CodigoContrato
                                           AND aa.CodigoTipoAssociacao = dbo.f_GetCodigoTipoAssociacao('CT')
                                                                        )
							THEN 'S'
							ELSE 'N'													
                        END [ContratoPossuiVinculo]--comentários, anexos, adtivos e parcelas
					  ,o.CodigoObra
                      ,cont.NumeroInterno2, cont.NumeroInterno3,
                       mrs.NomeMunicipio, p.NomeProjeto, VigenciaContrato
                      , (SELECT isnull(SUM(isnull(ValorPago,0)),0) FROM {0}.{1}.ParcelaContrato parc where parc.CodigoContrato = cont.CodigoContrato AND parc.[DataExclusao] IS NULL) AS ValorPago
                      ,CASE WHEN (SELECT COUNT(1) FROM {0}.{1}.AditivoContrato ac WHERE ac.CodigoContrato = cont.CodigoContrato AND ac.DataExclusao IS NULL AND ac.tipoAditivo = 'TC'   ) != 0 THEN 'S' ELSE 'N' END AS TemAditivoTEC
                      ,cont.UnidadeVigenciaContrato
                      ,cont.NumeroContratoSAP 
                  FROM {0}.{1}.Contrato cont LEFT JOIN
	                   {0}.{1}.Projeto p ON (p.CodigoProjeto = cont.CodigoProjeto
																AND p.DataExclusao IS NULL)  LEFT JOIN
	                   {0}.{1}.Obra o ON (o.CodigoContrato = cont.CodigoContrato
																AND o.DataExclusao IS NULL) INNER JOIN
	                   {0}.{1}.Pessoa prs ON prs.CodigoPessoa = cont.CodigoPessoaContratada INNER JOIN 
                       {0}.{1}.[PessoaEntidade] AS [pe] ON (
			                    pe.[CodigoPessoa] = cont.[CodigoPessoaContratada]
			                    AND pe.codigoEntidade = {3}
                                --AND pe.IndicaFornecedor = 'S'
			                    ) INNER JOIN
                       {0}.{1}.Municipio mrsOrigem ON mrsOrigem.CodigoMunicipio = prs.CodigoMunicipioEnderecoPessoa LEFT JOIN
                       {0}.{1}.Municipio mrs ON mrs.CodigoMunicipio = o.CodigoMunicipioObra INNER JOIN
                       {0}.{1}.TipoContrato tc ON tc.CodigoTipoContrato = cont.CodigoTipoContrato INNER JOIN
                       {0}.{1}.Usuario u ON u.CodigoUsuario = cont.CodigoUsuarioResponsavel 
                 WHERE cont.CodigoEntidade = {3}
                    AND {0}.{1}.f_VerificaAcessoConcedido({2}, {3}, cont.CodigoContrato, NULL, 'CT', 0, NULL, 'CT_Cns') = 1
                   {4}
                 ORDER BY AnoContrato, NumeroContrato
                ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDadosContratoExtendido(int codigoContrato, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT cont.CodigoContrato, cont.CodigoTipoContrato, cont.NumeroContrato  
	                  ,cont.StatusContrato, p.CodigoStatusProjeto, cont.CodigoPessoaContratada
	                  ,cont.DescricaoObjetoContrato, o.CodigoMunicipioObra, o.CodigoSegmentoObra
	                  ,cont.DataInicio, cont.DataTermino, cont.ValorContrato, cont.DataBaseReajuste
	                  ,cont.CodigoCriterioReajusteContrato, o.CodigoTipoServico
	                  ,o.CodigoOrigemObra, cont.CodigoFonteRecursosFinanceiros
	                  ,cont.CodigoCriterioMedicaoContrato, cont.CodigoUsuarioResponsavel, cont.CodigoUnidadeNegocio
	                  ,prs.NomeContato, prs.NomePessoa, prs.NomeFantasia, mrsOrigem.NomeMunicipio NomeMunicipioOrigem, o.NumeroTrabalhadoresDiretos
	                  ,cont.Observacao, tc.DescricaoTipoContrato, cont.CodigoProjeto
                      ,CASE WHEN o.CodigoProjeto IS NULL THEN 'N' ELSE 'S' END AS IndicaObra
                      ,cont.DataAssinatura, cont.DataTerminoOriginal, cont.ValorContratoOriginal                      
					  ,o.CodigoObra, cont.NumeroInterno2, cont.NumeroInterno3, u.NomeUsuario AS GestorContrato
                      ,CASE WHEN (SELECT COUNT(1) FROM {0}.{1}.AditivoContrato ac WHERE ac.CodigoContrato = cont.CodigoContrato AND ac.DataExclusao IS NULL AND ac.tipoAditivo = 'TC'   ) != 0 THEN 'S' ELSE 'N' END AS TemAditivoTEC
                      ,cont.NumeroContratoSAP
                      ,cont.SubContratacao
                      ,cont.ClassificacaoFornecedor
                      ,cont.TipoCusto
                      ,cont.ContaContabil
                      ,cont.TipoPagamento
                      ,cont.RetencaoGarantia 
                      ,cont.ValorGarantia
                      ,cont.PercGarantia
                      ,cont.dtInicioGarantia
                      ,cont.dtFimGarantia 
                      ,cont.CentroCusto 
                      ,CASE WHEN (SELECT COUNT(1) FROM {0}.{1}.AditivoContrato ac WHERE ac.CodigoContrato = cont.CodigoContrato AND ac.DataExclusao IS NULL  ) != 0 THEN 'S' ELSE 'N' END AS TemAditivo  
                        
                  FROM {0}.{1}.Contrato cont LEFT JOIN
	                   {0}.{1}.Projeto p ON (p.CodigoProjeto = cont.CodigoProjeto
																AND p.DataExclusao IS NULL)  LEFT JOIN
	                   {0}.{1}.Obra o ON (o.CodigoContrato = cont.CodigoContrato
																AND o.DataExclusao IS NULL) INNER JOIN
	                   {0}.{1}.Pessoa prs ON prs.CodigoPessoa = cont.CodigoPessoaContratada INNER JOIN  
                       {0}.{1}.[PessoaEntidade] AS [pen] ON (
			                pen.[CodigoPessoa] = cont.[CodigoPessoaContratada]
			                AND pen.codigoEntidade = cont.codigoEntidade
                            --AND pen.IndicaFornecedor = 'S'
			                ) INNER JOIN
                       {0}.{1}.Municipio mrsOrigem ON mrsOrigem.CodigoMunicipio = prs.CodigoMunicipioEnderecoPessoa left JOIN
                       {0}.{1}.Municipio mrs ON mrs.CodigoMunicipio = o.CodigoMunicipioObra INNER JOIN
                       {0}.{1}.TipoContrato tc ON tc.CodigoTipoContrato = cont.CodigoTipoContrato INNER JOIN
                       {0}.{1}.Usuario u ON u.CodigoUsuario = cont.CodigoUsuarioResponsavel 
                 WHERE cont.CodigoContrato = {2}
                   {3}
                ", bancodb, Ownerdb, codigoContrato, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getInformacoesContrato(int codigoContrato)
    {
        string comandoSQL = string.Format(@"
                SELECT cont.ValorContrato, cont.ValorContratoOriginal, CONVERT(VarChar(10), cont.DataTermino, 103) AS DataTermino, cont.DataTerminoOriginal
                      ,cont.NumeroContrato, cont.StatusContrato
                  FROM {0}.{1}.Contrato cont LEFT JOIN
				       {0}.{1}.Pessoa pe ON pe.CodigoPessoa = cont.CodigoPessoaContratada LEFT JOIN 
                       {0}.{1}.[PessoaEntidade] AS [pen] ON (
			        pen.[CodigoPessoa] = cont.[CodigoPessoaContratada]
			        AND pen.codigoEntidade = cont.codigoEntidade
                    --AND pen.IndicaFornecedor = 'S'
			        ) 
                 WHERE cont.CodigoContrato = {2}
                ", bancodb, Ownerdb, codigoContrato);
        return getDataSet(comandoSQL);
    }



    public DataSet getContratosAtivosProjeto(int codigoEntidade, int codigoUsuario, int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT cont.CodigoContrato, cont.NumeroContrato 
                  FROM {0}.{1}.Contrato cont LEFT JOIN
				             {0}.{1}.Pessoa pe ON pe.CodigoPessoa = cont.CodigoPessoaContratada LEFT JOIN 
                            {0}.{1}.[PessoaEntidade] AS [pen] ON (
			                pen.[CodigoPessoa] = cont.[CodigoPessoaContratada]
			                AND pen.codigoEntidade = cont.codigoEntidade
                            --AND pen.IndicaFornecedor = 'S'
			                ) 
                 WHERE cont.CodigoContrato IN (SELECT f.CodigoContrato FROM {0}.{1}.f_GetContratosUsuario({2}, {3}) AS f) 
                   AND cont.CodigoProjeto = {4}
                   AND cont.StatusContrato = 'A'
                   {5}
                 ORDER BY cont.NumeroContrato
                ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public bool verificaExistenciaContrato(string numeroContrato, int codigoContrato, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
            SELECT 1
              FROM {0}.{1}.Contrato c LEFT JOIN
				             {0}.{1}.Pessoa pe ON pe.CodigoPessoa = c.CodigoPessoaContratada LEFT JOIN 
                            {0}.{1}.[PessoaEntidade] AS [pen] ON (
			                pen.[CodigoPessoa] = c.[CodigoPessoaContratada]
			                AND pen.codigoEntidade = c.codigoEntidade
                            --AND pen.IndicaFornecedor = 'S'
			                ) 
             WHERE c.NumeroContrato = '{2}'
               AND c.CodigoContrato <> {3}
               AND c.CodigoEntidade = {4} 
               {5}",
            bancodb, Ownerdb, numeroContrato, codigoContrato, codigoEntidade, where);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            return true;

        return false;
    }

    public bool validaFormatoNumeroContrato(string numeroContrato, string where)
    {
        string comandoSQL = string.Format(@"
            SELECT 1
             WHERE SUBSTRING('{2}',3,1) = '-'
               AND SUBSTRING('{2}',5,1) = '-'
               AND SUBSTRING('{2}',9,1) = '/'
               {3}",
            bancodb, Ownerdb, numeroContrato, where);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            return true;

        return false;
    }
    public bool validaSequenciaContrato(string numeroContrato, string where, int codigoEntidade)
    {
        string comandoSQL = string.Format(@"
            SELECT 1
              FROM {0}.{1}.SequenciaContrato s
             WHERE s.ano = SUBSTRING('{2}',10,4)
               AND s.SequenciaContrato < SUBSTRING('{2}',6,3)
               AND CodigoUnidadeNegocio = {3}
               {4}",
            bancodb, Ownerdb, numeroContrato, codigoEntidade, where);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            return true;

        return false;
    }


    public bool verificaExistenciaSubContratada(int codigoContrato, string where)
    {
        string comandoSQL = string.Format(@"
            SELECT 1
              FROM {0}.{1}.SubContratada sc
             WHERE sc.CodigoContrato = {2}
               {3}",
            bancodb, Ownerdb, codigoContrato, where);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            return true;

        return false;
    }

    public bool incluirContratoExtendido(int tipoContrato, string numeroContrato, string situacao, int razaoSocial, string objeto, int municipio, int segmento
         , string inicio, string termino, string assinatura, string valor, string dataBase, int criterioReajuste, int tipoContratacao, int origem, int fonte, int criterioMedicao
        , int gestor, string numeroTrabalhadores, string observacoes, int codigoUsuarioInclusao, string codigoUnidade, int codigoEntidade, int codigoProjeto, ref int novoCodigoContrato, ref string msgErro,
        string numeroInterno2, string numeroInterno3, string numeroContratoSAP, string permitirSub, string ClassificacaoFornecedor, string centroCusto, int tipoPagamento
        , string tipoCusto, string contaContabil, string retencaoGarantia, string valorGarantia, string percGarantia, string inicioGarantia, string terminoGarantia)
    {
        DataSet ds;
        bool retorno = true;
        try
        {
            string codigoProjetoAssociado = "NULL";

            if (codigoProjeto != -1)
            {
                codigoProjetoAssociado = codigoProjeto.ToString();
            }


            comandoSQL = string.Format(@"
                BEGIN
                    DECLARE @CodigoContrato INT
                    DECLARE @SequenciaContrato INT
                    DECLARE @NumeroContrato varchar(50)
                    DECLARE @IndicaTipoContratacao char(1) 
                    DECLARE @Origem   Varchar(50)  
                    DECLARE @UsaNumeracaoAutomatica Char(1)
                    DECLARE @CodigoProjeto int
                    DECLARE @CodigoObra int
                    DECLARE @CodigoStatusComplementarContrato smallint

                    SELECT @CodigoStatusComplementarContrato = [CodigoStatusComplementarContrato] 
                      FROM {0}.{1}.[StatusComplementarContrato] AS [scc] 
                      WHERE scc.[IndicaStatusContrato] = '{4}';
                    
                    SET @CodigoObra = -1
                    
                    --Verificando se o contrato que está sendo incluído é um contrato de Obra
                    SELECT @CodigoProjeto = CodigoProjeto, 
                           @CodigoObra = CodigoObra
                      FROM {0}.{1}.[Obra] ob
                     WHERE ob.CodigoProjeto = {24}     

                    select @UsaNumeracaoAutomatica = valor 
                      from ParametroConfiguracaoSistema par 
                     where par.CodigoEntidade = {23}
                       and par.parametro = 'numeracaoAutomaticaContratos';

                    SET @NumeroContrato = '{3}'
                  BEGIN TRY 
                    BEGIN TRAN
                      if (@UsaNumeracaoAutomatica = 'S')
                      begin
                        UPDATE {0}.{1}.[SequenciaContrato] SET [SequenciaContrato] = (ISNULL([SequenciaContrato], 0) + 1) WHERE [Ano] = YEAR(GETDATE()) and [CodigoUnidadeNegocio] = {23}   
                        
                        SELECT @SequenciaContrato = [SequenciaContrato]  FROM {0}.{1}.[SequenciaContrato] WHERE [Ano] = YEAR(GETDATE()) and [CodigoUnidadeNegocio] = {23}

                        SELECT @IndicaTipoContratacao = ts.[IndicaTipoContratacao] 
                             FROM {0}.{1}.[TipoServico] ts
                                where ts.CodigoTipoServico = {14}
                                  and ts.codigoEntidade = {23}  

                        SELECT @Origem = p.Valor
			                from dbo.[ParametroConfiguracaoSistema] p
			                where p.[CodigoEntidade] = {23}
			                  and p.[Parametro] = 'PrefixoNumeracaoContrato'
                        
                        SELECT @NumeroContrato = @Origem+'-'+@IndicaTipoContratacao+'-'+RIGHT( '000' + CONVERT(VARCHAR(10), @SequenciaContrato), 3)+'/'+CONVERT(VARCHAR(5),YEAR(GETDATE())) 
                       end
                        INSERT INTO {0}.{1}.Contrato (CodigoTipoContrato, NumeroContrato, StatusContrato, CodigoPessoaContratada, DescricaoObjetoContrato, DataInicio, DataTermino
                                                     ,ValorContrato, DataBaseReajuste, CodigoCriterioReajusteContrato, CodigoFonteRecursosFinanceiros, CodigoCriterioMedicaoContrato
                                                     ,CodigoUsuarioResponsavel, Observacao, DataInclusao, CodigoUsuarioInclusao, CodigoUnidadeNegocio, CodigoEntidade, CodigoProjeto, DataAssinatura
                                                     ,DataTerminoOriginal, ValorContratoOriginal, NumeroInterno2, NumeroInterno3, NumeroContratoSAP, SubContratacao, ClassificacaoFornecedor
                                                     ,CentroCusto, TipoPagamento, TipoCusto, ContaContabil, RetencaoGarantia, ValorGarantia, PercGarantia
                                                     ,dtInicioGarantia, dtFimGarantia, tipoPessoa, CodigoStatusComplementarContrato)
                             VALUES({2}, @NumeroContrato, '{4}', {5}, '{6}', {9}, {10}, 
                                    {11}, {12},{13}, {16}, {17}, 
                                    {18}, '{20}', GETDATE(), {21}, {22}, {23}, {24}, {25}, 
                                    {10}, {11}, '{26}','{27}', '{28}', '{29}', '{30}',
                                   '{31}', {32},'{33}','{34}','{35}','{36}','{37}',
                                    {38},{39},'F',@CodigoStatusComplementarContrato)

                        
                        SELECT @CodigoContrato = SCOPE_IDENTITY()              

                        if (@CodigoObra > 0)
                        begin
                            UPDATE {0}.{1}.Obra SET     CodigoMunicipioObra = {7}
                                                      , CodigosegmentoObra = {8}
                                                      , CodigoTipoServico = {14}
                                                      , CodigoOrigemObra = {15}
                                                      , NumeroTrabalhadoresDiretos = {19}
                                                      , CodigoContrato = @CodigoContrato
                                                        
                                                      
                            WHERE CodigoProjeto = @CodigoProjeto
                              AND CodigoObra = @CodigoObra 
                        end
                        else   
                        begin 

                            INSERT INTO {0}.{1}.Obra(CodigoContrato, CodigoMunicipioObra, CodigoSegmentoObra, CodigoTipoServico, CodigoOrigemObra, NumeroTrabalhadoresDiretos, CodigoProjeto)
                                 VALUES(@CodigoContrato, {7}, {8}, {14}, {15}, {19}, {24})
                        end
                        SELECT @CodigoContrato AS [CodigoContrato]                          
                    COMMIT TRAN; 
                  END TRY
                  BEGIN CATCH
                       ROLLBACK TRAN;
                  END CATCH                         
                END
				", bancodb, Ownerdb
                 , tipoContrato, numeroContrato.Replace("'", "''"), situacao
                 , razaoSocial, objeto.Replace("'", "''"), municipio, segmento, inicio, termino, valor.Replace(".", "").Replace(",", "."), dataBase, criterioReajuste
                 , tipoContratacao, origem, fonte, criterioMedicao, gestor, numeroTrabalhadores.Replace(".", ""), observacoes.Replace("'", "''"), codigoUsuarioInclusao
                 , codigoUnidade, codigoEntidade, codigoProjetoAssociado, assinatura, numeroInterno2, numeroInterno3, numeroContratoSAP, permitirSub, ClassificacaoFornecedor
                 , centroCusto, tipoPagamento, tipoCusto, contaContabil, retencaoGarantia, valorGarantia.Replace(".", "").Replace(",", "."), percGarantia.Replace(".", "").Replace(",", ".")
                 , inicioGarantia, terminoGarantia);

            //execSQL(comandoSQL, ref registrosAfetados);
            ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigoContrato = int.Parse(ds.Tables[0].Rows[0]["CodigoContrato"].ToString());


            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = getErroIncluiRegistro(ex.Message);
        }
        return retorno;
    }

    public bool atualizaContratoExtendido(int codigoContrato, int tipoContrato, string situacao
       , int razaoSocial, string objeto, int municipio, int segmento, string inicio, string termino, string assinatura, string valor, string dataBase, int criterioReajuste
       , int tipoContratacao, int origem, int fonte, int criterioMedicao, int gestor, string numeroTrabalhadores, string observacoes, int codigoUsuarioAlteracao, string codigoUnidade, int codigoProjeto,
        string valorcAditivo, string datacAditivo, ref string msgErro, string numeroContrato, string numeroInterno2, string numeroInterno3, string numeroContratoSAP, string permitirSub,
        string ClassificacaoFornecedor, string centroCusto, int tipoPagamento, string tipoCusto, string contaContabil, string retencaoGarantia, string valorGarantia, string percGarantia
       , string inicioGarantia, string terminoGarantia)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            string codigoProjetoAssociado = "NULL";

            if (codigoProjeto != -1)
            {
                codigoProjetoAssociado = codigoProjeto.ToString();
            }

            comandoSQL = string.Format(@"
                BEGIN
                    DECLARE @CodigoProjeto int
                    DECLARE @CodigoObra int
                    DECLARE @CodigoStatusComplementarContrato smallint

                    SELECT @CodigoStatusComplementarContrato = [CodigoStatusComplementarContrato] 
                      FROM {0}.{1}.[StatusComplementarContrato] AS [scc] 
                      WHERE scc.[IndicaStatusContrato] = '{3}';
                    
                    SET @CodigoObra = -1
                    
                    --Verificando se o contrato que está sendo alterado é um contrato de Obra

                    SELECT @CodigoProjeto = CodigoProjeto, 
                           @CodigoObra = CodigoObra
                      FROM {0}.{1}.[Obra] ob
                     WHERE ob.CodigoProjeto = {30}       
                    
                    UPDATE {0}.{1}.Contrato SET CodigoTipoContrato = {2}
                                          , StatusContrato = '{3}'
                                          , CodigoPessoaContratada = {4}
                                          , DescricaoObjetoContrato = '{5}'
                                          , DataInicio = {8}
                                          , DataTerminoOriginal = {9}
                                          , ValorContratoOriginal = {10}
                                          , DataBaseReajuste = {11}
                                          , CodigoCriterioReajusteContrato = {12}
                                          , CodigoFonteRecursosFinanceiros = {15}
                                          , CodigoCriterioMedicaoContrato = {16}
                                          , CodigoUsuarioResponsavel = {17}
                                          , Observacao = '{19}'
                                          , DataUltimaAlteracao = GETDATE()
                                          , CodigoUsuarioUltimaAlteracao = {20}
                                          , CodigoUnidadeNegocio =  {21}
                                          {23}
                                          , DataAssinatura = {24}
                                          , ValorContrato = {25}
                                          , DataTermino = {26} 
                                          , NumeroContrato = '{27}'
                                          , NumeroInterno2 = '{28}' 
                                          , NumeroInterno3 = '{29}'
                                          , NumeroContratoSAP = '{31}'
                                          , SubContratacao = '{32}'
                                          , ClassificacaoFornecedor = '{33}'
                                          , CentroCusto = '{34}'
                                          , TipoPagamento = {35}
                                          , TipoCusto = '{36}'
                                          , ContaContabil = '{37}'
                                          , RetencaoGarantia = '{38}'
                                          , ValorGarantia = '{39}'
                                          , PercGarantia = '{40}'
                                          , dtInicioGarantia = {41}
                                          , dtFimGarantia = {42}
                                          , TipoPessoa = 'F'
                                          , CodigoStatusComplementarContrato = @CodigoStatusComplementarContrato
               
                WHERE CodigoContrato = {22}
                
                if ('{32}' = 'N')
                BEGIN
                    DELETE FROM {0}.{1}.[SubContratada]
                     WHERE CodigoContrato = {22}
                END

                --if ({30} > 0)
                if (@CodigoObra > 0)
                BEGIN
                    UPDATE {0}.{1}.Obra SET CodigoMunicipioObra = {6}
                                          , CodigosegmentoObra = {7}
                                          , CodigoTipoServico = {13}
                                          , CodigoOrigemObra = {14}
                                          , NumeroTrabalhadoresDiretos = {18}
                                          , CodigoContrato = {22}
                       WHERE CodigoProjeto  = {30}
                      
                    --DELETE {0}.{1}.Obra WHERE CodigoContrato = {22} AND CodigoProjeto IS NULL        
                    UPDATE {0}.{1}.Obra 
                      SET DataExclusao =  GETDATE(),
                          CodigoUsuarioExclusao = {20}
                    WHERE CodigoContrato = {22} AND CodigoProjeto IS NULL   
                END 
                ELSE 
                BEGIN 
                   UPDATE {0}.{1}.Obra SET CodigoMunicipioObra = {6}
                                        , CodigosegmentoObra = {7}
                                        , CodigoTipoServico = {13}
                                        , CodigoOrigemObra = {14}
                                        , NumeroTrabalhadoresDiretos = {18}
                                        {23}
                    WHERE CodigoContrato = {22}
                END  
  
            END

				", bancodb, Ownerdb
                 , tipoContrato, situacao
                 , razaoSocial, objeto.Replace("'", "''"), municipio, segmento, inicio, termino, valor.Replace(".", "").Replace(",", "."), dataBase, criterioReajuste
                 , tipoContratacao, origem, fonte, criterioMedicao, gestor, numeroTrabalhadores.Replace(".", ""), observacoes.Replace("'", "''"), codigoUsuarioAlteracao
                 , codigoUnidade, codigoContrato, codigoProjetoAssociado == "NULL" ? "" : ", CodigoProjeto = " + codigoProjetoAssociado, assinatura, valorcAditivo.Replace(".", "").Replace(",", "."), datacAditivo, numeroContrato
                 , numeroInterno2, numeroInterno3, codigoProjetoAssociado, numeroContratoSAP, permitirSub, ClassificacaoFornecedor, centroCusto, tipoPagamento
                 , tipoCusto, contaContabil, retencaoGarantia, valorGarantia.Replace(".", "").Replace(",", "."), percGarantia.Replace(".", "").Replace(",", ".")
                 , inicioGarantia, terminoGarantia);

            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = getErroAtualizaRegistro(ex.Message);
        }
        return retorno;
    }

    public bool atualizaStatusContratoExtendido(int codigoContrato, string situacao, string comentario, int codigoUsuarioAlteracao)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"   
                BEGIN            
                    INSERT INTO {0}.{1}.ComentarioContrato (CodigoContrato, Comentario, DataInclusao, CodigoUsuarioInclusao)
                                                    VALUES ({2}, '{4}', GetDate(), {5})

                    UPDATE {0}.{1}.Contrato SET StatusContrato = '{3}'
                                          , DataUltimaAlteracao = GETDATE()
                                          , CodigoUsuarioUltimaAlteracao = {5}
                    WHERE CodigoContrato = {2}               
                END

				", bancodb, Ownerdb, codigoContrato, situacao, comentario.Replace("'", "''"), codigoUsuarioAlteracao);

            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(getErroIncluiRegistro(ex.Message));
        }
        return retorno;
    }


    public bool incluirContratoObra(string numeroContrato, int razaoSocial, string objeto, string inicio, string termino, string valor, string dataBase, int criterioReajuste
        , int criterioMedicao, int gestor, string numeroTrabalhadores, string observacoes, int codigoUsuarioInclusao, int codigoEntidade, int codigoProjeto, int codigoUnidadePadrao, string dataAssinatura, ref string msgErro,
        string numeroInterno2, string numeroInterno3, string vigencia, string fonteContratacao, string unidadeVigenciaContrato)
    {
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"
              BEGIN

                
                DECLARE @CodigoContrato int
                DECLARE @SequenciaContrato INT
                DECLARE @NumeroContrato varchar(50)
                DECLARE @IndicaTipoContratacao char(1) 
                DECLARE @Origem   Varchar(50)
                DECLARE @UsaNumeracaoAutomatica Char(1)
                DECLARE @CodigoFonteRecursos int

                      select @UsaNumeracaoAutomatica = valor 
                      from ParametroConfiguracaoSistema par 
                      where par.CodigoEntidade = {15}
                        and par.parametro = 'numeracaoAutomaticaContratos';

                SELECT @CodigoFonteRecursos = CodigoFonteRecursosFinanceiros
                  FROM {0}.{1}.FonteRecursosFinanceiros
                 WHERE NomeFonte = '{22}'
                   AND CodigoEntidade = {15}

                    SET @NumeroContrato = '{2}'
                  BEGIN TRY 
                    BEGIN TRAN
                      if (@UsaNumeracaoAutomatica = 'S')
                      begin
                        UPDATE {0}.{1}.[SequenciaContrato] SET [SequenciaContrato] = (ISNULL([SequenciaContrato], 0) + 1) WHERE [Ano] = YEAR(GETDATE()) and [CodigoUnidadeNegocio] = {15}   
                        
                        SELECT @SequenciaContrato = [SequenciaContrato]  FROM {0}.{1}.[SequenciaContrato] WHERE [Ano] = YEAR(GETDATE()) and [CodigoUnidadeNegocio] = {15}
                            
                        
                        SELECT @IndicaTipoContratacao = ts.[IndicaTipoContratacao] 
                             FROM {0}.{1}.[TipoServico] ts, {0}.{1}.[obra] ob
                                where ob.codigoProjeto = {16}  
                                  and ts.CodigoTipoServico = ob.codigoTipoServico
                                  and ts.codigoEntidade = {15} 
                                                   
                        SELECT @Origem = p.Valor
			            from dbo.[ParametroConfiguracaoSistema] p
			            where p.[CodigoEntidade] = {15}
			              and p.[Parametro] = 'PrefixoNumeracaoContrato'	  

                        
                        SELECT @NumeroContrato = @Origem+'-'+@IndicaTipoContratacao+'-'+RIGHT( '000' + CONVERT(VARCHAR(10), @SequenciaContrato), 3)+'/'+CONVERT(VARCHAR(5),YEAR(GETDATE())) 
                            
                      end
                        INSERT INTO {0}.{1}.Contrato (NumeroContrato, CodigoPessoaContratada, DescricaoObjetoContrato
                                                        ,ValorContrato, DataBaseReajuste, CodigoCriterioReajusteContrato, CodigoCriterioMedicaoContrato
                                                        , CodigoUsuarioResponsavel, Observacao, DataInclusao, CodigoUsuarioInclusao, CodigoUnidadeNegocio
                                                        , CodigoEntidade, CodigoProjeto, CodigoTipoContrato, StatusContrato, TipoContratado, DataAssinatura
                                                        , ValorContratoOriginal, NumeroInterno2, NumeroInterno3, VigenciaContrato, CodigoFonteRecursosFinanceiros, UnidadeVigenciaContrato, TipoPessoa)
                                VALUES(@NumeroContrato, {3}, '{4}', {7}, {8}
                                    ,{9}, {10}, {11}, '{13}', GETDATE(), {14}, {18}, {15}, {16}, 1, 'P', 'F', {17}, {7}, '{19}', '{20}', {21}, @CodigoFonteRecursos,'{23}','F')

                        SELECT @CodigoContrato = SCOPE_IDENTITY()

                        UPDATE {0}.{1}.Obra SET CodigoContrato = @CodigoContrato
                                                ,NumeroTrabalhadoresDiretos = {12} 
                            WHERE CodigoProjeto = {16}
                    COMMIT TRAN; 
                END TRY
                BEGIN CATCH
                     ROLLBACK TRAN;
                END CATCH           
               END
				", bancodb, Ownerdb
                 , numeroContrato.Replace("'", "''")
                 , razaoSocial, objeto.Replace("'", "''"), inicio, termino, valor.Replace(".", "").Replace(",", "."), dataBase, criterioReajuste
                 , criterioMedicao, gestor, numeroTrabalhadores.Replace(".", ""), observacoes.Replace("'", "''"), codigoUsuarioInclusao
                 , codigoEntidade, codigoProjeto, dataAssinatura, codigoUnidadePadrao, numeroInterno2, numeroInterno3, vigencia, fonteContratacao, unidadeVigenciaContrato);

            int registrosAfetados = 0;
            execSQL(comandoSQL, ref registrosAfetados);

            msgErro = "";

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = getErroIncluiRegistro(ex.Message);
        }
        return retorno;
    }

    public bool atualizaContratoObra(int codigoContrato, string numeroContrato, int razaoSocial, string objeto, string inicio, string termino, string valor, string dataBase, int criterioReajuste
        , int criterioMedicao, int gestor, string numeroTrabalhadores, string observacoes, int codigoUsuarioAlteracao, int codigoEntidade, int codigoProjeto, string dataAssinatura, ref string msgErro,
        string numeroInterno2, string numeroInterno3, string vigencia, string unidadeVigenciaContrato)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"
                BEGIN
                    UPDATE {0}.{1}.Contrato SET CodigoPessoaContratada = {3}
                                          , DescricaoObjetoContrato = '{4}'
                                          --, DataInicio = {5}
                                          --, DataTerminoOriginal = {6}
                                          --, DataTermino = {6}
                                          , ValorContratoOriginal = {7}
                                          , ValorContrato = {7}
                                          , DataBaseReajuste = {8}
                                          , CodigoCriterioReajusteContrato = {9}
                                          , CodigoCriterioMedicaoContrato = {10}
                                          , CodigoUsuarioResponsavel = {11}
                                          , Observacao = '{13}'
                                          , DataUltimaAlteracao = GETDATE()
                                          , CodigoUsuarioUltimaAlteracao = {14}
                                          , DataAssinatura = {15}
                                          , NumeroContrato = '{2}'
                                          , NumeroInterno2 = '{17}'
                                          , NumeroInterno3 = '{18}'
                                          , VigenciaContrato = {19}
                                          , UnidadeVigenciaContrato = '{20}'
                                          , TipoPessoa = 'F'
                    WHERE CodigoContrato = {16}
                
                   UPDATE {0}.{1}.Obra SET NumeroTrabalhadoresDiretos = {12}
               END               
				", bancodb, Ownerdb
                 , numeroContrato.Replace("'", "''")
                 , razaoSocial, objeto.Replace("'", "''"), inicio, termino, valor.Replace(".", "").Replace(",", "."), dataBase, criterioReajuste
                 , criterioMedicao, gestor, numeroTrabalhadores.Replace(".", ""), observacoes.Replace("'", "''"), codigoUsuarioAlteracao, dataAssinatura, codigoContrato,
                 numeroInterno2, numeroInterno3, vigencia, unidadeVigenciaContrato);

            execSQL(comandoSQL, ref registrosAfetados);

            msgErro = "";

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = getErroIncluiRegistro(ex.Message);
        }
        return retorno;
    }


    public bool incluirAditivoContratoObra(int codigoContrato, int codigoTipoInstrumento, string numeroContrato, string tipoAditivo, string novoValor, string valorContrato, string valorAditivo, string dataPrazo, string observacoes, int codigoUsuarioInclusao, int codigoWorflow, int codigoInstanciaWf, ref int novoCodigoAditivo, ref string msgErro)
    {
        DataSet ds;
        bool retorno = true;
        try
        {
            string cWF = codigoWorflow == -1 ? "NULL" : codigoWorflow.ToString();
            string cInstancia = codigoInstanciaWf == -1 ? "NULL" : codigoInstanciaWf.ToString();

            string comandoEncerrarContrato = "";

            if (tipoAditivo == "TC")
            {
                comandoEncerrarContrato = string.Format(@"UPDATE {0}.{1}.Contrato SET StatusContrato = 'I' WHERE CodigoContrato = {2}", bancodb, Ownerdb, codigoContrato);
            }

            comandoSQL = string.Format(@"
              BEGIN
                DECLARE @CodigoAditivo int

                INSERT INTO {0}.{1}.AditivoContrato (CodigoContrato, CodigoTipoContratoAditivo, NumeroContratoAditivo, TipoAditivo
                                                    , NovoValorContrato, ValorContrato, ValorAditivo, NovaDataVigencia, DescricaoMotivoAditivo, DataInclusao, CodigoUsuarioInclusao, CodigoWorkflow, CodigoInstanciaWf
                                                    , DataAprovacaoAditivo, CodigoUsuarioAprovacao)
                     VALUES({2}, {3}, '{4}', '{5}', {6}, {7}, {8}, {9}, '{10}', GetDate(), {11}, {12}, {13}, GetDate(), {11})

                SELECT @CodigoAditivo = SCOPE_IDENTITY()

                SELECT @CodigoAditivo AS CodigoAditivo

                {14}

               END
				", bancodb, Ownerdb
                 , codigoContrato, codigoTipoInstrumento, numeroContrato.Replace("'", "''"), tipoAditivo
                 , novoValor.Replace(".", "").Replace(",", "."), valorContrato.Replace(".", "").Replace(",", "."), valorAditivo.Replace(".", "").Replace(",", ".")
                 , dataPrazo, observacoes.Replace("'", "''"), codigoUsuarioInclusao, cWF, cInstancia
                 , comandoEncerrarContrato);

            ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigoAditivo = int.Parse(ds.Tables[0].Rows[0]["CodigoAditivo"].ToString());

            msgErro = "";

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = getErroIncluiRegistro(ex.Message);
        }
        return retorno;
    }

    public bool atualizaAditivoContratoObra(int codigoAditivo, int codigoContrato, int codigoTipoInstrumento, string numeroContrato, string tipoAditivo, string novoValor, string valorContrato, string valorAditivo, string dataPrazo, string observacoes, ref string msgErro)
    {
        bool retorno = true;
        try
        {

            string comandoEncerrarContrato = "";

            if (tipoAditivo == "TC")
            {
                comandoEncerrarContrato = string.Format(@"UPDATE {0}.{1}.Contrato SET StatusContrato = 'I' WHERE CodigoContrato = {2}", bancodb, Ownerdb, codigoContrato);
            }


            comandoSQL = string.Format(@"
              BEGIN

                UPDATE {0}.{1}.AditivoContrato SET CodigoContrato = {3}
                                                 , CodigoTipoContratoAditivo = {4}
                                                 , NumeroContratoAditivo = '{5}'
                                                 , TipoAditivo = '{6}'
                                                 , NovoValorContrato = {7}
                                                 , ValorContrato = {8}
                                                 , ValorAditivo = {9}
                                                 , NovaDataVigencia = {10}
                                                 , DescricaoMotivoAditivo = '{11}'
                     WHERE CodigoAditivoContrato = {2}

                {12}
               END
				", bancodb, Ownerdb
                 , codigoAditivo, codigoContrato, codigoTipoInstrumento, numeroContrato.Replace("'", "''"), tipoAditivo
                 , novoValor.Replace(".", "").Replace(",", "."), valorContrato.Replace(".", "").Replace(",", "."), valorAditivo.Replace(".", "").Replace(",", ".")
                 , dataPrazo, observacoes.Replace("'", "''")
                 , comandoEncerrarContrato);

            int registrosAfetados = 0;
            execSQL(comandoSQL, ref registrosAfetados);

            msgErro = "";

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = getErroIncluiRegistro(ex.Message);
        }
        return retorno;
    }

    public bool atualizaDadosAditivo(int codigoAditivo, int codigoTipoInstrumento, string numeroContrato, string tipoAditivo, string novoValor, string valorContrato, string valorAditivo, string dataPrazo, string observacoes, ref string msgErro)
    {
        bool retorno = true;
        try
        {

            comandoSQL = string.Format(@"
              BEGIN

                UPDATE {0}.{1}.AditivoContrato SET CodigoTipoContratoAditivo = {3}
                                                 , NumeroContratoAditivo = '{4}'
                                                 , TipoAditivo = '{5}'
                                                 , NovoValorContrato = {6}
                                                 , ValorContrato = {7}
                                                 , ValorAditivo = {8}
                                                 , NovaDataVigencia = {9}
                                                 , DescricaoMotivoAditivo = '{10}'
                     WHERE CodigoAditivoContrato = {2}

               END
				", bancodb, Ownerdb
                 , codigoAditivo, codigoTipoInstrumento, numeroContrato.Replace("'", "''"), tipoAditivo
                 , novoValor.Replace(".", "").Replace(",", "."), valorContrato.Replace(".", "").Replace(",", "."), valorAditivo.Replace(".", "").Replace(",", ".")
                 , dataPrazo, observacoes.Replace("'", "''"));

            int registrosAfetados = 0;
            execSQL(comandoSQL, ref registrosAfetados);

            msgErro = "";

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = getErroIncluiRegistro(ex.Message);
        }
        return retorno;
    }

    public bool atualizaInstanciaAditivoContratoObra(int codigoAditivo, int codigoInstanciaWf, ref string msgErro)
    {
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"
              BEGIN

                UPDATE {0}.{1}.AditivoContrato SET CodigoInstanciaWf = {3}
                     WHERE CodigoAditivoContrato = {2}
               END
				", bancodb, Ownerdb
                 , codigoAditivo, codigoInstanciaWf);

            int registrosAfetados = 0;
            execSQL(comandoSQL, ref registrosAfetados);

            msgErro = "";

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = getErroIncluiRegistro(ex.Message);
        }
        return retorno;
    }

    public bool excluiAditivoContratoObra(int codigoAditivo, int codigoUsuario, ref string msgErro)
    {
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"
              BEGIN

                UPDATE {0}.{1}.AditivoContrato SET CodigoUsuarioExclusao = {3}
                                                 , DataExclusao = GetDate()
                     WHERE CodigoAditivoContrato = {2}
               END
				", bancodb, Ownerdb
                 , codigoAditivo, codigoUsuario);

            int registrosAfetados = 0;
            execSQL(comandoSQL, ref registrosAfetados);

            msgErro = "";

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = getErroIncluiRegistro(ex.Message);
        }
        return retorno;
    }

    public DataSet getComentariosContratos(string where)
    {
        string comandoSQL = string.Format(@"
                SELECT cc.CodigoComentarioContrato, cc.Comentario, cc.DataInclusao, ui.NomeUsuario AS UsuarioInclusao
                 FROM {0}.{1}.ComentarioContrato cc LEFT JOIN
	                  {0}.{1}.Usuario ui ON ui.CodigoUsuario = cc.CodigoUsuarioInclusao
                WHERE 1 = 1
                  {2}
                ", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    public bool incluirComentarioContrato(int codigoContrato, string comentario, int codigoUsuario)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"   
                INSERT INTO {0}.{1}.ComentarioContrato (CodigoContrato, Comentario, DataInclusao, CodigoUsuarioInclusao)
                                                    VALUES ({2}, '{3}', GetDate(), {4})                
				", bancodb, Ownerdb, codigoContrato, comentario.Replace("'", "''"), codigoUsuario);

            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(getErroIncluiRegistro(ex.Message));
        }
        return retorno;
    }

    public bool atualizaComentarioContrato(int codigoComentarioContrato, string comentario)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"   
                UPDATE {0}.{1}.ComentarioContrato SET Comentario = '{3}'
                 WHERE CodigoComentarioContrato = {2}               
				", bancodb, Ownerdb, codigoComentarioContrato, comentario.Replace("'", "''"));

            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(getErroAtualizaRegistro(ex.Message));
        }
        return retorno;
    }

    public bool excluiReajusteContrato(int codigoIndiceReajusteContrato)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"   
            DELETE FROM {0}.{1}.IndiceReajusteContrato
            WHERE CodigoIndiceReajusteContrato = {2}", bancodb, Ownerdb, codigoIndiceReajusteContrato);
            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(getErroExcluiRegistro(ex.Message));
        }
        return retorno;
    }

    public bool excluiComentarioContrato(int codigoComentarioContrato)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"   
                DELETE FROM {0}.{1}.ComentarioContrato
                 WHERE CodigoComentarioContrato = {2}               
				", bancodb, Ownerdb, codigoComentarioContrato);

            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(getErroIncluiRegistro(ex.Message));
        }
        return retorno;
    }

    public DataSet getIndiceReajusteContrato(int codigoContrato, string where)
    {
        string comandoSQL = string.Format(@"
        SELECT CodigoContrato,
	           NomeIndiceReajusteContrato,
		       PercentualReajuste,
		       DataAplicacaoReajuste,
		       DataInclusao,
		       CodigoUsuarioInclusao,
               CodigoIndiceReajusteContrato
         FROM {0}.{1}.IndiceReajusteContrato 
        WHERE CodigoContrato = {2}
                  {3}
         ORDER BY DataAplicacaoReajuste desc", bancodb, Ownerdb, codigoContrato, where);

        return getDataSet(comandoSQL);
    }

    public bool incluiIndiceReajusteContrato(int codigoContrato, string nomeIndiceReajusteContrato, double percentualReajuste, string dataAplicacaoReajuste, string indicaReajusteAplicado, int codigoUsuarioInclusao)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"   
            INSERT INTO {0}.{1}.IndiceReajusteContrato
            (CodigoContrato,NomeIndiceReajusteContrato,PercentualReajuste
            ,DataAplicacaoReajuste,IndicaReajusteAplicado,DataInclusao
            ,CodigoUsuarioInclusao)
            VALUES
           ({2},'{3}',{4}
           , CONVERT(DATETIME,'{5}',103),'{6}',getDate()
           ,{7})", bancodb, Ownerdb, codigoContrato, nomeIndiceReajusteContrato, percentualReajuste.ToString().Replace(',', '.'), dataAplicacaoReajuste, indicaReajusteAplicado, codigoUsuarioInclusao);
            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(getErroIncluiRegistro(ex.Message));
        }
        return retorno;
    }

    public bool atualizaIndiceReajusteContrato(int codigoIndiceReajusteContrato, string nomeIndiceReajusteContrato, double percentualReajuste, string dataAplicacaoReajuste, string indicaReajusteAplicado)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"   
            UPDATE {0}.{1}.IndiceReajusteContrato
            SET 
               NomeIndiceReajusteContrato = '{3}'
               ,PercentualReajuste = {4}
               ,DataAplicacaoReajuste = CONVERT(DATETIME,'{5}',103)
               ,IndicaReajusteAplicado = '{6}'
         WHERE CodigoIndiceReajusteContrato = {2}", bancodb, Ownerdb, codigoIndiceReajusteContrato, nomeIndiceReajusteContrato, percentualReajuste.ToString().Replace(',', '.'), dataAplicacaoReajuste, indicaReajusteAplicado);
            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(getErroIncluiRegistro(ex.Message));
        }
        return retorno;
    }

    public DataSet getAditivosContratos(string where)
    {
        string comandoSQL = string.Format(@"
                SELECT ac.CodigoAditivoContrato, ac.CodigoContrato, ac.CodigoTipoContratoAditivo
	                  ,ac.CodigoUsuarioAprovacao, ac.DataAprovacaoAditivo, ac.CodigoUsuarioInclusao, ac.DataInclusao
	                  ,ac.DescricaoMotivoAditivo, ac.NovaDataVigencia, ac.NovoValorContrato
	                  ,ac.NumeroContratoAditivo, ac.TipoAditivo, ap.NomeUsuario AS UsuarioAprovacao, CodigoWorkflow
	                  ,ui.NomeUsuario AS UsuarioInclusao, tc.DescricaoTipoContrato, ac.ValorAditivo, ac.ValorContrato
	                  ,(CASE WHEN ({0}.{1}.f_ctt_EhUltimoAditivo(ac.CodigoAditivoContrato, ac.TipoAditivo) = 1) THEN 'S' ELSE 'N' END) AS Editavel
                     ,CASE WHEN ac.TipoAditivo = 'PR' THEN 'Prazo' 
                           WHEN ac.TipoAditivo = 'VL' THEN 'Valor' 
                           WHEN ac.TipoAditivo = 'PV' THEN 'Prazo/Valor' 
                           WHEN ac.TipoAditivo = 'TM' THEN 'Troca de Material' 
                           WHEN ac.TipoAditivo = 'SC' THEN 'Escopo' 
                           ELSE ' ' END AS DescricaoTipoAditivo
                 FROM {0}.{1}.AditivoContrato ac LEFT JOIN
	                  {0}.{1}.Usuario ap ON ap.CodigoUsuario = ac.CodigoUsuarioAprovacao INNER JOIN
	                  {0}.{1}.Usuario ui ON ui.CodigoUsuario = ac.CodigoUsuarioInclusao INNER JOIN
                      {0}.{1}.TipoContrato tc ON tc.CodigoTipoContrato = CodigoTipoContratoAditivo
                WHERE ac.DataExclusao IS NULL
                  AND ac.DataAprovacaoAditivo IS NOT NULL
                  {2}
                ", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getAditivo(int codigoAditivo, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT ac.CodigoAditivoContrato, ac.CodigoContrato, ac.CodigoTipoContratoAditivo
	                  ,ac.CodigoUsuarioAprovacao, ac.DataAprovacaoAditivo, ac.CodigoUsuarioInclusao, ac.DataInclusao
	                  ,ac.DescricaoMotivoAditivo, ac.NovaDataVigencia, ac.NovoValorContrato
	                  ,ac.NumeroContratoAditivo, ac.TipoAditivo, ap.NomeUsuario AS UsuarioAprovacao
	                  ,ui.NomeUsuario AS UsuarioInclusao, ac.ValorAditivo, ac.ValorContrato
                 FROM {0}.{1}.AditivoContrato ac LEFT JOIN
	                  {0}.{1}.Usuario ap ON ap.CodigoUsuario = ac.CodigoUsuarioAprovacao INNER JOIN
	                  {0}.{1}.Usuario ui ON ui.CodigoUsuario = ac.CodigoUsuarioInclusao
                WHERE ac.CodigoAditivoContrato = {2}
                  {3}
                ", bancodb, Ownerdb, codigoAditivo, where);

        return getDataSet(comandoSQL);
    }

    public int getCodigoAditivoFluxo(int codigoWf, int codigoInstanciaWf)
    {
        string comandoSQL = string.Format(@"
                SELECT ac.CodigoAditivoContrato
                 FROM {0}.{1}.AditivoContrato ac
                WHERE ac.CodigoWorkflow = {2}
                  AND ac.CodigoInstanciaWF = {3}
                ", bancodb, Ownerdb, codigoWf, codigoInstanciaWf);

        int codigoAditivo = -1;

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            codigoAditivo = int.Parse(ds.Tables[0].Rows[0]["CodigoAditivoContrato"].ToString());

        return codigoAditivo;
    }

    public int getCodigoProjetoInstanciaFluxo(int codigoWf, int codigoInstanciaWf)
    {
        string comandoSQL = string.Format(@"
               SELECT IdentificadorProjetoRelacionado 
                 FROM {0}.{1}.InstanciasWorkflows
                WHERE CodigoWorkflow = {2}
                  AND CodigoInstanciaWF = {3}
                ", bancodb, Ownerdb, codigoWf, codigoInstanciaWf);

        int codigoProjeto = -1;

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            codigoProjeto = int.Parse(ds.Tables[0].Rows[0]["IdentificadorProjetoRelacionado"].ToString());

        return codigoProjeto;
    }

    public bool verificaPendenciaAditivoFluxo(int codigoContrato)
    {
        string comandoSQL = string.Format(@"
                SELECT 1
                 FROM {0}.{1}.AditivoContrato ac
                WHERE ac.CodigoContrato = {2}
                  AND ac.DataAprovacaoAditivo IS NULL
                  AND ac.DataExclusao IS NULL
                ", bancodb, Ownerdb, codigoContrato);

        bool possuiPendenciaFluxo = false;

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            possuiPendenciaFluxo = true;

        return possuiPendenciaFluxo;
    }

    public DataSet getTiposAquisicao(string codigoUnidade)
    {
        string comandoSQL = string.Format(@"
                SELECT CodigoTipoAquisicao, DescricaoTipoAquisicao 
                FROM {0}.{1}.TipoAquisicao
                WHERE CodigoEntidade = {2}
                ORDER BY DescricaoTipoAquisicao
                ", bancodb, Ownerdb, codigoUnidade);

        return getDataSet(comandoSQL);
    }

    public DataSet getParcelaContrato(string codigoContrato)
    {
        string comandoSQL = string.Format(@"
                SELECT 
			[CodigoContrato]
		,	[NumeroParcela]
		,	[ValorPrevisto]
		,	[DataVencimento]
		,	[DataPagamento]
		,	[ValorPago]
		,	[HistoricoParcela]
		,	[NumeroAditivoContrato]
		,	[TipoRegistro]
		,	[NumeroNF]
		,	[ValorRetencao]
		,	[ValorMultas]
		,	[CodigoMedicao]
		,	[CodigoConta]
		,	[CodigoProjetoParcela]
		,	[DataEmissaoNF]
		,	[CodigoLancamentoFinanceiro]
		,	[IndicaTipoIntegracao]
		,	[IndicaDadoIntegracaoAtualizado]
		, [DataUltimaIntegracao]
                FROM {0}.{1}.[f_GetParcelaContrato]({2}) 
             ORDER BY NumeroAditivoContrato, NumeroParcela
                ", bancodb, Ownerdb, codigoContrato);

        return getDataSet(comandoSQL);
    }

    public DataSet getFontePagadora(string where)
    {
        string comandoSQL = string.Format(@"
                SELECT frf.CodigoFonteRecursosFinanceiros, frf.NomeFonte 
                FROM {0}.{1}.FonteRecursosFinanceiros AS frf
                {2}
                ORDER BY NomeFonte", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public bool incluirContratoAquisicoes(int codigoEntidade, string numeroContrato, string codigoProjeto, string codigoTipoAquisicao
                                           , string fornecedor, string descricaoObjetoContrato, string codigoFonteRecursosFinancieros
                                           , string codigoUnidadeNegocio, DateTime dataInicio, DateTime dataTermino
                                           , string codigoUsuarioResponsavel, string codigoUsuarioInclusao, string statusContrato
                                           , string observacoes, string numeroContratoSAP, string codigoTipoContrato
                                           , string valorContrato, string tipoPessoa, string codigoPessoa, string indicaRevisaoPrevia, string codigoStatusComplementarContrato, ref int novoCodigoContrato)
    {

        DataSet ds;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"
                DECLARE @CodigoContrato INT
                DECLARE @CodigoStatusComplementarContrato INT
                    SET @CodigoStatusComplementarContrato = {22}

                DECLARE @IndicaStatusContrato CHAR(1)
                 
                  SELECT @IndicaStatusContrato = scc.IndicaStatusContrato 
                    FROM StatusComplementarContrato scc 
                   WHERE CodigoStatusComplementarContrato =  @CodigoStatusComplementarContrato

                INSERT INTO {0}.{1}.Contrato (NumeroContrato, CodigoProjeto, CodigoTipoAquisicao, Fornecedor, DescricaoObjetoContrato
											  , CodigoFonteRecursosFinanceiros, CodigoUnidadeNegocio, DataInicio, DataTermino
											  , CodigoUsuarioResponsavel, DataInclusao, CodigoUsuarioInclusao, StatusContrato
                                              , Observacao, NumeroContratoSAP, CodigoTipoContrato, ValorContrato, CodigoEntidade, CodigoPessoaContratada, TipoPessoa, indicaRevisaoPrevia, CodigoStatusComplementarContrato, ValorContratoOriginal)
                VALUES('{2}',{3} ,{4} ,{5},'{6}'
		                ,{7} ,{8},{9} ,{10}
		                ,{11},GETDATE() ,{12}, @IndicaStatusContrato
                        ,'{14}','{15}',{16},{17},{18},{19},'{20}','{21}',@CodigoStatusComplementarContrato, {17})

                SELECT @CodigoContrato = SCOPE_IDENTITY()
                SELECT @CodigoContrato AS [CodigoContrato]

				", bancodb, Ownerdb
                 , numeroContrato.Replace("'", "''")
                 , codigoProjeto, codigoTipoAquisicao
                 , fornecedor
                 , descricaoObjetoContrato.Replace("'", "''")
                 , codigoFonteRecursosFinancieros, codigoUnidadeNegocio
                 , dataInicio == DateTime.MinValue ? "NULL" : string.Format("CONVERT(DATETIME, '{0:dd/MM/yyyy}', 103)", dataInicio)
                 , dataTermino == DateTime.MinValue ? "NULL" : string.Format("CONVERT(DATETIME, '{0:dd/MM/yyyy}', 103)", dataTermino)
                 , codigoUsuarioResponsavel
                 , codigoUsuarioInclusao, statusContrato
                 , observacoes.Replace("'", "''")
                 , numeroContratoSAP.Replace("'", "''")
                 , codigoTipoContrato, valorContrato, codigoEntidade, codigoPessoa, tipoPessoa, indicaRevisaoPrevia, codigoStatusComplementarContrato);

            //execSQL(comandoSQL, ref registrosAfetados);
            ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigoContrato = int.Parse(ds.Tables[0].Rows[0]["CodigoContrato"].ToString());


            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(getErroIncluiRegistro(ex.Message));
        }
        return retorno;
    }

    public bool atualizaContratoAquisicoes(string numeroContrato, string codigoTipoAquisicao
                                           , string fornecedor, string descricaoObjetoContrato, string codigoFonteRecursosFinancieros
                                           , string codigoUnidadeNegocio, DateTime dataInicio, DateTime dataTermino
                                           , string codigoUsuarioResponsavel, string codigoUsuarioUltimaAlteracao
                                           , string statusContrato, string codigoContratoAquisicao, string observacoes
                                           , string numeroContratoSAP, string codigoProjeto, string codigoTipoContrato
                                           , string valorContrato, string sqlParcelas, string tipoPessoa, string codigoPessoa, string IndicaRevisaoPrevia, string codigoStatusComplementarContrato)
    {
        string comandoSQL = "";
        bool retorno = true;
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                        DECLARE @CodigoContrato     INT,
                                @CodigoUsuario      INT

                        SET @CodigoContrato = {13}
                        SET @CodigoUsuario = {11}

                DECLARE @CodigoStatusComplementarContrato INT
                    SET @CodigoStatusComplementarContrato = {22}

                DECLARE @IndicaStatusContrato CHAR(1)
                 
                  SELECT @IndicaStatusContrato = scc.IndicaStatusContrato 
                    FROM StatusComplementarContrato scc 
                   WHERE CodigoStatusComplementarContrato =  @CodigoStatusComplementarContrato

                        UPDATE {0}.{1}.Contrato
                        SET
	                          NumeroContrato                    = '{2}'
                            , CodigoProjeto                     = {16}
                            , CodigoTipoAquisicao               = {3}
                            , fornecedor                        = {4}
                            , DescricaoObjetoContrato           = '{5}'
                            , CodigoFonteRecursosFinanceiros    = {6}
                            , CodigoUnidadeNegocio              = {7}
                            , DataInicio                        = {8}
                            , DataTermino                       = {9}
                            , CodigoUsuarioResponsavel          = {10}
                            , DataUltimaAlteracao               = GETDATE()
                            , CodigoUsuarioUltimaAlteracao      = @CodigoUsuario
                            , StatusContrato                    = @IndicaStatusContrato
                            , Observacao                        = '{14}'
                            , NumeroContratoSAP                 = '{15}'
                            , CodigoTipoContrato                = {18}
                            
                            , TipoPessoa                        = '{19}'
                            , CodigoPessoaContratada            = {20}
                            , IndicaRevisaoPrevia               = '{21}'
                            , CodigoStatusComplementarContrato  = @CodigoStatusComplementarContrato
                        where
	                        CodigoContrato = @CodigoContrato
                        
                        ------
                        {17}
                        ", bancodb, Ownerdb
                         , numeroContrato.Replace("'", "''")
                         , codigoTipoAquisicao
                         , fornecedor
                         , descricaoObjetoContrato.Replace("'", "''")
                         , codigoFonteRecursosFinancieros, codigoUnidadeNegocio
                         , dataInicio == DateTime.MinValue ? "NULL" : string.Format("CONVERT(DATETIME, '{0:dd/MM/yyyy}', 103)", dataInicio)
                         , dataTermino == DateTime.MinValue ? "NULL" : string.Format("CONVERT(DATETIME, '{0:dd/MM/yyyy}', 103)", dataTermino)
                         , codigoUsuarioResponsavel, codigoUsuarioUltimaAlteracao
                         , statusContrato, codigoContratoAquisicao
                         , observacoes.Replace("'", "''")
                         , numeroContratoSAP.Replace("'", "''")
                         , codigoProjeto
                         , sqlParcelas
                         , codigoTipoContrato
                         , tipoPessoa
                         , codigoPessoa
                         , IndicaRevisaoPrevia
                         , codigoStatusComplementarContrato);


            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(getErroAtualizaRegistro(ex.Message));

        }
        return retorno;
    }

    public bool excluiContratoAquisicoes(string codigoContrato, int codTipoAssociacao, ref string mensagem)
    {
        bool retorno = false;
        string comandoSQL = string.Format(@"
BEGIN TRANSACTION;

BEGIN TRY
    DECLARE @CodigoContrato INT
		SET @CodigoContrato = {2}
		
    --Eliminando o vínculo com a tabela 'Obra'
     UPDATE {0}.{1}.Obra
		SET CodigoContrato = NULL
	  WHERE CodigoContrato = @CodigoContrato
    
    --Excluindo fisicamente os aditivos dos contratos já excluídos logicamente
     DELETE 
	   FROM {0}.{1}.AditivoContrato
	  WHERE CodigoContrato = @CodigoContrato
        AND DataExclusao IS NOT NULL

    --Excluindo fisicamente as parcelas já excluídas logicamente
     DELETE 
	   FROM {0}.{1}.ParcelaContrato
	  WHERE CodigoContrato = @CodigoContrato
        AND DataExclusao IS NOT NULL

    --Excluindo Contrato
     DELETE 
	   FROM {0}.{1}.Contrato
	  WHERE CodigoContrato = @CodigoContrato

     SELECT 'Sucesso' AS Resultado
END TRY
BEGIN CATCH
    SELECT 'Falha' AS Resultado
          ,ERROR_NUMBER() AS ErrorNumber
          ,ERROR_SEVERITY() AS ErrorSeverity
          ,ERROR_STATE() AS ErrorState
          ,ERROR_PROCEDURE() AS ErrorProcedure
          ,ERROR_LINE() AS ErrorLine
          ,ERROR_MESSAGE() AS ErrorMessage;

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
END CATCH;

IF @@TRANCOUNT > 0
    COMMIT TRANSACTION;
", bancodb, Ownerdb, codigoContrato);
        DataSet ds = getDataSet(comandoSQL);

        DataRow drResultado = ds.Tables[0].Rows[0];
        retorno = drResultado["Resultado"].Equals("Sucesso");

        if (!retorno)
        {
            mensagem = string.Format(@"
Número do erro: {0}
Gravidade:      {1}
Estado:         {2}
Linha:          {3}
Mensagem:       {4}"
                , drResultado["ErrorNumber"]
                , drResultado["ErrorSeverity"]
                , drResultado["ErrorState"]
                , drResultado["ErrorLine"]
                , drResultado["ErrorMessage"]);
        }

        return retorno;
    }

    public bool incluiParcelaContratoAquisicoes(int codigoContrato, string numeroParcela, string valorPrevisto, string strVencto, string strPagto
        , string valorPago, string historico, string numeroAditivoContrato, int codigoUsuario, string numeroNF, string valorretencao, string valorMulta, string codigoConta, string codigoProjetoParcela, string CodigoCronogramaProjetoPacoteEAP, string CodigoTarefaPacoteEAP, string CodigoRecursoProjeto, string strDataEmissao, ref string mensagemErro)
    {
        bool retorno = true;
        //CodigoCronogramaProjetoPacoteEAP, string CodigoTarefaPacoteEAP, string CodigoRecursoProjeto
        comandoSQL = string.Format(@"
              DECLARE
                  @CodigoContrato				    int
                , @NumeroAditivoContrato			smallint
                , @NumeroParcela					smallint
                , @ValorPrevisto					decimal(18,4)
                , @DataVencimento					datetime
                , @DataPagamento					datetime
                , @ValorPago						decimal(18,4)
                , @HistoricoParcela					varchar(500)
                , @CodigoUsuarioInclusao			int
                , @NumeroNF							varchar(20)
                , @ValorRetencao					decimal(18,4)
                , @ValorMultas						decimal(18,4)
                , @CodigoConta						int
                , @CodigoProjetoParcela				int
                , @CodigoCronogramaProjetoPacoteEAP	varchar(64)
                , @CodigoTarefaPacoteEAP			int
                , @CodigoRecursoProjeto				int
                , @DataEmissaoNF					datetime

              SET @CodigoContrato								    = {2}
              SET @NumeroAditivoContrato							= {9}
              SET @NumeroParcela									= {3}
              SET @ValorPrevisto									= {4}
              SET @DataVencimento							        = {5}
              SET @DataPagamento									= {6}
              SET @ValorPago										= {7}
              SET @HistoricoParcela							        = {8}
              SET @CodigoUsuarioInclusao							= {10}
              SET @NumeroNF										    = {11}
              SET @ValorRetencao									= {12}
              SET @ValorMultas									    = {13}
              SET @CodigoConta								        = {14}
              SET @CodigoProjetoParcela							    = {15}
              SET @CodigoCronogramaProjetoPacoteEAP	                = {16}
              SET @CodigoTarefaPacoteEAP						    = {17}
              SET @CodigoRecursoProjeto							    = {18}
              SET @DataEmissaoNF									= {19}

              EXEC {0}.{1}.[p_insereParcelaContrato]
                    @in_CodigoContrato								= @CodigoContrato
                , @in_NumeroAditivoContrato							= @NumeroAditivoContrato
                , @in_NumeroParcela									= @NumeroParcela
                , @in_ValorPrevisto									= @ValorPrevisto
                , @in_DataVencimento							    = @DataVencimento
                , @in_DataPagamento									= @DataPagamento
                , @in_ValorPago										= @ValorPago
                , @in_HistoricoParcela							    = @HistoricoParcela
                , @in_CodigoUsuarioInclusao							= @CodigoUsuarioInclusao
                , @in_NumeroNF										= @NumeroNF
                , @in_ValorRetencao									= @ValorRetencao
                , @in_ValorMultas									= @ValorMultas
                , @in_CodigoConta								    = @CodigoConta
                , @in_CodigoProjetoParcela							= @CodigoProjetoParcela
                , @in_CodigoCronogramaProjetoPacoteEAP	            = @CodigoCronogramaProjetoPacoteEAP	
                , @in_CodigoTarefaPacoteEAP							= @CodigoTarefaPacoteEAP
                , @in_CodigoRecursoProjeto							= @CodigoRecursoProjeto
                , @in_DataEmissaoNF									= @DataEmissaoNF

                                ", bancodb, Ownerdb
                             ,/*{2}*/ codigoContrato, /*{3}*/numeroParcela, /*{4}*/valorPrevisto.Replace(",", ".")
                             ,/*{5}*/ strVencto, /*{6}*/strPagto, /*{7}*/valorPago != "" ? "'" + valorPago.Replace(",", ".") + "'" : "NULL"
                             ,/*{8}*/ historico != "" ? "'" + historico.Replace("'", "''") + "'" : "NULL"
                             ,/*{9}*/numeroAditivoContrato
                             ,/*{10}*/codigoUsuario
                             ,/*{11}*/numeroNF
                             ,/*{12}*/valorretencao != "" ? "'" + valorretencao.Replace(",", ".") + "'" : "NULL"
                             ,/*{13}*/valorMulta != "" ? "'" + valorMulta.Replace(",", ".") + "'" : "NULL"
                             ,/*{14}*/codigoConta != "" ? codigoConta : "NULL"
                             ,/*{15}*/codigoProjetoParcela != "" ? codigoProjetoParcela : "NULL"
                             ,/*{16}*/CodigoCronogramaProjetoPacoteEAP
                             ,/*{17}*/CodigoTarefaPacoteEAP
                             ,/*{18}*/CodigoRecursoProjeto
                             ,/*{19}*/strDataEmissao);
        string comandoSQLFinal = geraBlocoBeginTran() +
            Environment.NewLine +
            comandoSQL +
            Environment.NewLine +
            geraBlocoEndTran();

        DataSet ds = getDataSet(comandoSQLFinal);
        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            if (ds.Tables[0].Rows[0][0].ToString().ToLower().Trim() == "ok")
            {
                mensagemErro = "";
                retorno = true;
            }
            else
            {
                mensagemErro = ds.Tables[0].Rows[0][0].ToString();
                if (mensagemErro.Contains("Violação da restrição PRIMARY KEY"))
                {
                    mensagemErro = "Erro ao inserir o registro, já existe um lançamento com mesmo número de aditivo e parcela de contrato.";
                }
                else
                {
                    getErroIncluiRegistro(mensagemErro);
                }
                retorno = false;
            }
        }
        return retorno;
    }

    public bool atualizaParcelaContratoAquisicoes(int codigoContrato, string numeroParcela, string valorPrevisto, string strVencto, string strPagto
        , string valorPago, string historico, string numeroAditivoContrato, int codigoUsuario, string numeroNF, string valorretencao, string valorMulta, string codigoConta, string codigoProjetoParcela, string CodigoCronogramaProjetoPacoteEAP, string CodigoTarefaPacoteEAP, string CodigoRecursoProjeto, string strDataEmissao, ref string mensagemErro)
    {
        bool retorno = true;
        comandoSQL = string.Format(@"
                                UPDATE {0}.{1}.ParcelaContrato
                                SET
                                      ValorPrevisto     = {4}
                                    , DataVencimento    = {5}
                                    , DataPagamento     = {6}
                                    , ValorPago         = {7}
                                    , HistoricoParcela  = {8}
                                    , DataUltimaAlteracao           = GETDATE()
                                    , CodigoUsuarioUltimaAlteracao  =  {10}
                                    , numeroNF = {11}
                                    , valorRetencao = {12}
                                    , valorMultas = {13}
                                    , CodigoConta = {14}
                                    , CodigoProjetoParcela = {15}
                                    ,CodigoCronogramaProjetoPacoteEAP = {16}
                                    ,CodigoTarefaPacoteEAP = {17}
                                    ,CodigoRecursoProjeto = {18}
                                    ,DataEmissaoNF = {19}

                                WHERE   CodigoContrato          = {2}
                                  AND   NumeroParcela           = {3}
                                  AND   NumeroAditivoContrato   = {9}

                                ", bancodb, Ownerdb
                                 , codigoContrato
                                 , numeroParcela
                                 , valorPrevisto.Replace(",", ".")
                                 , strVencto
                                 , strPagto
                                 , valorPago != "" ? "'" + valorPago.Replace(",", ".") + "'" : "NULL"
                                 , historico != "" ? "'" + historico.Replace("'", "''") + "'" : "NULL"
                                 , numeroAditivoContrato
                                 , codigoUsuario
                                 , numeroNF
                                 , valorretencao != "" ? "'" + valorretencao.Replace(",", ".") + "'" : "NULL"
                                 , valorMulta != "" ? "'" + valorMulta.Replace(",", ".") + "'" : "NULL"
                                 ,/*{14}*/codigoConta != "" ? codigoConta : "NULL"
                                 ,/*{15}*/codigoProjetoParcela != "" ? codigoProjetoParcela : "NULL"
                                 ,/*{16}*/ CodigoCronogramaProjetoPacoteEAP
                                 ,/*{17}*/CodigoTarefaPacoteEAP
                                 ,/*{18}*/CodigoRecursoProjeto
                                 ,/*{19}*/strDataEmissao);

        string comandoSQLFinal = geraBlocoBeginTran() +
            Environment.NewLine +
            comandoSQL +
            Environment.NewLine +
            geraBlocoEndTran();

        DataSet ds = getDataSet(comandoSQLFinal);
        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            if (ds.Tables[0].Rows[0][0].ToString().ToLower() == "ok")
            {
                mensagemErro = "";
                retorno = true;
            }
            else
            {
                mensagemErro = ds.Tables[0].Rows[0][0].ToString();
                getErroAtualizaRegistro(mensagemErro);
                retorno = false;
            }
        }
        return retorno;
    }

    public bool excluiParcelaContratoAquisicoes(int codigoContrato, string numeroParcela, string numeroAditivoContrato)
    {
        bool retorno = true;
        try
        {

            comandoSQL = string.Format(@"
                                UPDATE ParcelaContrato
                               SET DataExclusao = GetDate()
                                   ,CodigoUsuarioexclusao = {3}
                                WHERE   CodigoContrato          = {0}
                                  AND   NumeroParcela           = {1}
                                  AND   NumeroAditivoContrato   = {2}
								

                                DECLARE @sequencialParcela as int
							   SET @sequencialParcela = -1     
                               (SELECT @sequencialParcela = SequencialParcela 
											                    FROM ParcelaContrato
                                                               WHERE CodigoContrato = {0}
                                                                 AND NumeroParcela = {1}
                                                                 AND NumeroAditivoContrato = {2})
							    IF(@sequencialParcela <> -1)
                                BEGIN
                                     UPDATE LancamentoFinanceiro 
                                       SET DataExclusao = GetDate()
                                          ,CodigoUsuarioexclusao = {3}
								      WHERE dbo.f_GetIniciaisTipoAssociacao(CodigoTipoAssociacao) = 'PA' 
									   AND CodigoObjetoAssociado = @sequencialParcela
                                END", codigoContrato
                                 , numeroParcela
                                 , numeroAditivoContrato
                                 , getInfoSistema("IDUsuarioLogado"));

            int registrosAfetados = 0;
            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            throw new Exception(getErroIncluiRegistro(ex.Message));
        }
        return retorno;
    }

    public bool VerificaAcessoEmContratosDoProjeto(int codigoProjeto, int codigoUsuarioResponsavel, int codigoEntidadeLogada)
    {
        string comandaSql = string.Format(@"
DECLARE @CodigoProjeto INT
DECLARE @CodigoUsuario INT
DECLARE @CodigoEntidade INT
		SET @CodigoUsuario = {2}
		SET @CodigoProjeto = {3}
		SET @CodigoEntidade = {4}

 SELECT {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, c.CodigoContrato, NULL, 'CT', 0, NULL, 'CT_Cns') AS AcessoConcedido1,
		{0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, p.CodigoUnidadeNegocio, NULL, 'UN', 0, NULL, 'CT_Cns') AS AcessoConcedido2,
		{0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, p.CodigoUnidadeNegocio, NULL, 'UN', 0, NULL, 'UN_IncCtt') AS AcessoConcedido3
   FROM {0}.{1}.Projeto p LEFT JOIN 
		{0}.{1}.Contrato c ON c.CodigoProjeto = p.CodigoProjeto
  WHERE p.CodigoProjeto = @CodigoProjeto"
            , bancodb, Ownerdb, codigoUsuarioResponsavel, codigoProjeto, codigoEntidadeLogada);

        DataSet ds = getDataSet(comandaSql);
        foreach (DataRow dr in ds.Tables[0].Rows)
        {
            if ((bool)dr["AcessoConcedido1"] || (bool)dr["AcessoConcedido2"] || (bool)dr["AcessoConcedido3"])
                return true;
        }
        return false;
    }

    #endregion

    #region SENAR
    public DataSet verificaVinculacaoProcesso(int CodigoContrato, string NumeroProcesso, string CodigoTipoFolder)
    {
        comandoSQL = string.Format(@"SELECT {0}.{1}.f_Senar_VerificaVinculacaoProcesso({2},{3},'{4}')", bancodb, Ownerdb, CodigoContrato,CodigoTipoFolder, NumeroProcesso);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet buscarProcessoVinculadoSenarDocs(int CodigoContrato)
    {
        comandoSQL = string.Format(@"EXEC {0}.{1}.p_SENAR_BuscaProcessoVinculadoSenarDocs {2}
                    ", bancodb, Ownerdb, CodigoContrato);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet buscarAnexosProcessoSenarDocs(int CodigoContrato)
    {
        comandoSQL = string.Format(@"EXEC {0}.{1}.p_SENAR_BuscaAnexosProcessoSenarDocs {2}
                    ", bancodb, Ownerdb, CodigoContrato);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet vincularProcesso(int CodigoContrato, string TipoFolder, string NumeroProcesso, string Assunto, string Fornecedor, int CodigoUsuarioLogado)
    {
        comandoSQL = string.Format(@"EXEC {0}.{1}.p_SENAR_VinculaProcesso {2},{3},'{4}','{5}','{6}',{7}", bancodb, Ownerdb, CodigoContrato, TipoFolder, NumeroProcesso, Assunto, Fornecedor, CodigoUsuarioLogado);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet incluirAnexoProcesso(int CodigoContrato, string NomeDocumento, string URLDocumento, DateTime DataDocumento)
    {
        comandoSQL = string.Format(@"EXEC {0}.{1}.p_SENAR_IncluiAnexoProcesso {2},'{3}','{4}','{5}'
                    ", bancodb, Ownerdb, CodigoContrato, NomeDocumento, URLDocumento, DataDocumento);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet desvincularProcesso(int CodigoContrato, int CodigoUsuarioLogado)
    {
        comandoSQL = string.Format(@"EXEC {0}.{1}.p_SENAR_DesvinculaVinculaProcesso {2},'{3}'
                    ", bancodb, Ownerdb, CodigoContrato, CodigoUsuarioLogado);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getTipoProcesso()
    {
        comandoSQL = string.Format(@"SELECT CodigoTipoFolder, DescricaoTipoFolder FROM {0}.{1}.f_Senar_GetListaTipoFolder()", bancodb, Ownerdb);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }
    #endregion
}
}