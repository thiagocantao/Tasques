using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

/// <summary>
/// Summary description for myDados
/// </summary>


namespace BriskPtf
{
    public partial class dados
    {
        #region Portfólios

        public DataSet getPortfolios(int codigoEntidade, string where)
        {
            comandoSQL = string.Format(
               @"SELECT p.[CodigoPortfolio]
                  ,p.[DescricaoPortfolio]
                  ,p.[CodigoPortfolioSuperior]
                  ,p.[CodigoUsuarioGerente]
                  ,p.[CodigoStatusPortfolio]
                  ,p.[CodigoEntidade]
                  ,s.DescricaoStatus AS Status
                  ,pSup.DescricaoPortfolio AS PortfolioSuperior
                  ,u.NomeUsuario AS UsuarioGerente
                  ,p.CodigoCarteiraAssociada
                  ,cart.NomeCarteira
                  ,ISNULL(p.CodigoCarteiraAssociada, -1) AS CarteiraAssociada
              FROM {0}.{1}.Portfolio p INNER JOIN
	               {0}.{1}.Status s ON s.CodigoStatus = p.CodigoStatusPortfolio INNER JOIN
	               {0}.{1}.Usuario u ON u.CodigoUsuario = p.CodigoUsuarioGerente LEFT JOIN
	               {0}.{1}.Portfolio pSup ON pSup.CodigoPortfolio = p.CodigoPortfolioSuperior LEFT JOIN
                   {0}.{1}.Carteira cart ON (cart.CodigoCarteira = p.CodigoCarteiraAssociada)
             WHERE p.CodigoEntidade = {2}
               {3}", bancodb, Ownerdb, codigoEntidade, where);
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getPortfoliosFiltroPorCenario(int codigoEntidade, string where, int codigoPortfolio)
        {
            comandoSQL = string.Format(
               @"SELECT p.[CodigoPortfolio]
                  ,p.[DescricaoPortfolio]
                  ,p.[CodigoPortfolioSuperior]
                  ,p.[CodigoUsuarioGerente]
                  ,p.[CodigoStatusPortfolio]
                  ,p.[CodigoEntidade]
                  ,s.DescricaoStatus AS Status
                  ,pSup.DescricaoPortfolio AS PortfolioSuperior
                  ,u.NomeUsuario AS UsuarioGerente
                  ,p.CodigoCarteiraAssociada
                  ,cart.NomeCarteira
                  ,ISNULL(p.CodigoCarteiraAssociada, -1) AS CarteiraAssociada
              FROM {0}.{1}.Portfolio p INNER JOIN
	               {0}.{1}.Status s ON s.CodigoStatus = p.CodigoStatusPortfolio INNER JOIN
	               {0}.{1}.Usuario u ON u.CodigoUsuario = p.CodigoUsuarioGerente LEFT JOIN
	               {0}.{1}.Portfolio pSup ON pSup.CodigoPortfolio = p.CodigoPortfolioSuperior LEFT JOIN
                   {0}.{1}.Carteira cart ON (cart.CodigoCarteira = p.CodigoCarteiraAssociada)
INNER JOIN  f_GetProjetosSelecaoBalanceamento({4}, -1, {2}) AS GPSB  ON (GPSB._CodigoProjeto IS NOT NULL )

             WHERE p.CodigoEntidade = {2} {3}", bancodb, Ownerdb, codigoEntidade, where, codigoPortfolio);
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getPeriodoAnalisePortfolio(int codigoEntidade, string where)
        {
            comandoSQL = string.Format(
               @"SELECT Ano, IndicaAnoPeriodoEditavel, IndicaTipoDetalheEdicao
              FROM {0}.{1}.PeriodoAnalisePortfolio
             WHERE CodigoEntidade = {2}
               {3}", bancodb, Ownerdb, codigoEntidade, where);
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getStatusPortfolios(string where)
        {
            comandoSQL = string.Format(
               @"SELECT [CodigoStatus]
                  ,[DescricaoStatus]
                  ,[IndicaControladoSistema]
                  ,[TipoStatus]
              FROM {0}.{1}.[Status]
             WHERE 1 = 1 {2} order by DescricaoStatus ", bancodb, Ownerdb, where);
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public bool incluiPortfolio(string descricaoPortfolio, int codigoGerente, int codigoStatus, int codigoPortfolioSuperior, int codigoUnidade, ref string mensagem)
        {
            int regAfetados = 0;
            try
            {
                comandoSQL = string.Format(@"INSERT INTO {0}.{1}.[Portfolio]
                                                       ([DescricaoPortfolio]
                                                       ,[CodigoPortfolioSuperior]
                                                       ,[CodigoUsuarioGerente]
                                                       ,[CodigoStatusPortfolio]
                                                       ,[CodigoEntidade])
                                                 VALUES('{2}'
                                                       , {3}
                                                       , {4}
                                                       , {5}
                                                       , {6})"
                    , bancodb, Ownerdb, descricaoPortfolio, codigoPortfolioSuperior == -1 ? "NULL" : codigoPortfolioSuperior.ToString(), codigoGerente, codigoStatus, codigoUnidade);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                mensagem = ex.Message;
                return false;
            }
        }

        public bool atualizaPortfolio(int codigoPortfolio, string descricaoPortfolio, int codigoGerente, int codigoStatus, int codigoPortfolioSuperior, int codigoUnidade, ref string msg)
        {
            int regAfetados = 0;
            try
            {
                comandoSQL = string.Format(@"UPDATE {0}.{1}.[Portfolio]
                                            SET [DescricaoPortfolio] = '{3}'
                                               ,[CodigoPortfolioSuperior] = {4}
                                               ,[CodigoUsuarioGerente] = {5}
                                               ,[CodigoStatusPortfolio] = {6}
                                               ,[CodigoEntidade] = {7}
                                          WHERE CodigoPortfolio = {2}"
                   , bancodb, Ownerdb, codigoPortfolio, descricaoPortfolio,
                   codigoPortfolioSuperior == -1 ? "NULL" : codigoPortfolioSuperior.ToString(), codigoGerente, codigoStatus, codigoUnidade);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public bool excluiPortfolio(int codigoPortfolio, ref string msg)
        {
            int regAfetados = 0;
            try
            {
                comandoSQL = string.Format(@"SELECT 1 FROM {0}.{1}.[Portfolio] WHERE CodigoPortfolioSuperior = {2}", bancodb, Ownerdb, codigoPortfolio);

                DataSet ds = getDataSet(comandoSQL);

                if (DataSetOk(ds) && DataTableOk(ds.Tables[0]) && ds.Tables[0].Rows.Count > 0)
                {
                    msg = "O portfólio não pode ser excluído, pois é portfólio superior de outros portfólios!";
                    return false;
                }

                comandoSQL = string.Format(@"DELETE FROM {0}.{1}.[Portfolio] WHERE CodigoPortfolio = {2}", bancodb, Ownerdb, codigoPortfolio);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }

        }

        public DataSet getListaProjetosPortfolios(int codigoUsuario, int codigoEntidade, int codigoCarteira, string where)
        {
            string comandoSQL = string.Format(
                @"BEGIN
 
                 DECLARE @CodigoEntidade int
                 DECLARE @CodigoUsuario int
                 SET @CodigoEntidade = {2}
                 SET @CodigoUsuario = {3}
                 SET @CodigoCarteira = {4} 
                 DECLARE @tbResumo Table(Codigo VarChar(10),
			                Descricao VarChar(255),
			                ScoreCriterios Decimal(10,2),
			                ScoreRiscos Decimal(10,2),
                            Despesa Decimal(18,2),
                            Receita Decimal(18,2),
                            Desempenho VarChar(10),
			                Status VarChar(50),
			                CodigoPai VarChar(10),
			                CodigoCategoria int,
			                CodigoPortfolio int,
							CodigoProjeto int)

                INSERT INTO @tbResumo													 
		                 SELECT 'C.' + CONVERT(VarChar, p.CodigoProjeto) AS Codigo, 
			                    NomeProjeto, 
                                rp.ScoreCriterios, 
                                rp.ScoreRiscos, 
                                rp.CustoTendenciaTotal, 
                                rp.ReceitaTendenciaTotal, 
                                rp.CorGeral, s.DescricaoStatus,
			                    'B.' + CONVERT(VarChar, ISNUll(p.CodigoCategoria, 0)) + '.'  + CONVERT(VarChar, ISNUll(pp.CodigoPortfolio, 0)),
			                    ISNUll(p.CodigoCategoria, 0),  ISNUll(pp.CodigoPortfolio, 0), p.CodigoProjeto			 
		                  FROM {0}.{1}.Projeto as p INNER JOIN 
			                   {0}.{1}.PortfolioProjeto pp ON pp.CodigoProjeto = p.CodigoProjeto INNER JOIN
			                   {0}.{1}.Status s ON s.CodigoStatus = p.CodigoStatusProjeto INNER JOIN
                               {0}.{1}.ResumoProjeto rp ON rp.CodigoProjeto = p.CodigoProjeto INNER JOIN
                               {0}.{1}.f_GetProjetosUsuario(@CodigoUsuario, @CodigoEntidade, @CodigoCarteira) as PU ON (PU.CodigoProjeto = p.CodigoProjeto) 
		                 WHERE p.CodigoEntidade = @CodigoEntidade
                           AND p.DataExclusao IS NULL
		                   AND s.IndicaAcompanhamentoPortfolio = 'S'
                					
                INSERT INTO @tbResumo													 
		                 SELECT 'B.' + CONVERT(VarChar, ISNUll(t.CodigoCategoria, 0)) + '.'  + CONVERT(VarChar, ISNUll(t.CodigoPortfolio, 0)),
			                    c.DescricaoCategoria, 
                                Avg(rp.ScoreCriterios), 
                                Avg(rp.ScoreRiscos), 
                                SUM(rp.CustoTendenciaTotal),  
                                SUM(rp.ReceitaTendenciaTotal), null, null,
			                    'A.' + CONVERT(VarChar, ISNUll(t.CodigoPortfolio, 0)),
			                    t.CodigoCategoria, ISNUll(t.CodigoPortfolio, 0), 0
		                   FROM {0}.{1}.Categoria as c LEFT JOIN 
		  	                            @tbResumo as t ON t.CodigoCategoria = c.CodigoCategoria INNER JOIN
                                {0}.{1}.ResumoProjeto rp ON rp.CodigoProjeto = t.CodigoProjeto
		                WHERE t.CodigoCategoria <> 0
		                  AND c.CodigoEntidade = @CodigoEntidade
                          AND c.DataExclusao IS NULL
		                GROUP By t.CodigoCategoria, ISNUll(t.CodigoPortfolio, 0), DescricaoCategoria
                		
                				
                INSERT INTO @tbResumo													 
		                 SELECT 'A.' + CONVERT(VarChar, t.CodigoPortfolio),
			                 p.DescricaoPortfolio, 
                             Avg(rp.ScoreCriterios), 
                             Avg(rp.ScoreRiscos), 
                             SUM(rp.CustoTendenciaTotal),  
                             SUM(rp.ReceitaTendenciaTotal), null,
			                 null, null,
			                 null, ISNUll(t.CodigoPortfolio, 0), 0
		                FROM {0}.{1}.Portfolio as p LEFT JOIN 
			                @tbResumo as t ON t.CodigoPortfolio = p.CodigoPortfolio INNER JOIN
                            {0}.{1}.ResumoProjeto rp ON rp.CodigoProjeto = t.CodigoProjeto
		                WHERE t.CodigoPortfolio <> 0
		                  AND p.CodigoEntidade = @CodigoEntidade
		                GROUP By t.CodigoPortfolio, p.DescricaoPortfolio					
                SELECT * FROM @tbResumo 
                  ORDER By CodigoPortfolio, CodigoCategoria, Codigo
                END", bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoCarteira);

            return getDataSet(comandoSQL);
        }


        #endregion

        #region Unidades Negocio

        public int getUnidadeMaisAntigaUsuario(int idUsuarioLogado)
        {
            int retorno = -1;
            comandoSQL = string.Format(@"SELECT CodigoUnidadeNegocio FROM {0}.{1}.UsuarioUnidadeNegocio
                                        WHERE DataAtivacaoPerfilUnidadeNegocio
                                    IN(SELECT MIN(DataAtivacaoPerfilUnidadeNegocio) FROM {0}.{1}.UsuarioUnidadeNegocio
            WHERE CodigoUsuario = {2} AND IndicaUsuarioAtivoUnidadeNegocio = 'S')", bancodb, Ownerdb, idUsuarioLogado);
            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                retorno = int.Parse(ds.Tables[0].Rows[0][0].ToString());
            }
            return retorno;
        }

        public DataSet getGerenteEntidade(string where, int codigoEntidade)
        {
            comandoSQL = string.Format(@"
            SELECT DISTINCT u.CodigoUsuario,
                            u.NomeUsuario,
                            u.EMail 
            FROM {0}.{1}.Usuario AS u

                INNER JOIN {0}.{1}.UsuarioUnidadeNegocio    AS uun 
                        ON u.CodigoUsuario = uun.CodigoUsuario
                INNER JOIN {0}.{1}.UnidadeNegocio           AS un 
                        ON un.CodigoUnidadeNegocio = uun.CodigoUnidadeNegocio

            WHERE (uun.IndicaUsuarioAtivoUnidadeNegocio = 'S'
              AND un.CodigoEntidade = {2}) 
              {3}

            ORDER BY u.NomeUsuario
        ", bancodb, Ownerdb, codigoEntidade, where);

            return classeDados.getDataSet(comandoSQL);
        }

        public DataSet getUnidadeSuperiorPermitidas(int codigoEntidad, int codigoUnidadeAtual, string where)
        {
            comandoSQL = string.Format(@"
                DECLARE
		                @CodigoEntidadeLogada   Int
	                ,   @CodigoUnidadeAtual     Int

                    SET @CodigoEntidadeLogada   = {2}
	                SET @CodigoUnidadeAtual     = {3}	
                        	
	                SELECT  un.[CodigoUnidadeNegocio]
			            ,   un.[NomeUnidadeNegocio]
			            ,   un.[SiglaUnidadeNegocio]

		            FROM {0}.{1}.[UnidadeNegocio]   AS un

		            WHERE   un.[CodigoEntidade]             = @CodigoEntidadeLogada
			          AND   un.[DataExclusao]               IS NULL
			          AND   un.[CodigoUnidadeNegocio]       != @CodigoUnidadeAtual
                      AND   un.[IndicaUnidadeNegocioAtiva]  = 'S'
			          AND   {0}.{1}.[f_GetUnidadeSuperior]  (un.[CodigoUnidadeNegocio], @CodigoUnidadeAtual) IS NULL
                      {4}
                       order by un.[SiglaUnidadeNegocio]
        ", bancodb, Ownerdb, codigoEntidad, codigoUnidadeAtual, where);
            return classeDados.getDataSet(comandoSQL);
        }
        public DataSet getUnidadeSuperiorAnalises(int codigoEntidade)
        {
            string comandoSQL = string.Format(@"SELECT  
			un.[CodigoUnidadeNegocio]
		   ,un.[SiglaUnidadeNegocio]
           ,un.NomeUnidadeNegocio
	FROM 
		{0}.{1}.[UnidadeNegocio]								AS [un]
	WHERE
			un.[CodigoUnidadeNegocioSuperior]	= {2}
        AND un.[CodigoUnidadeNegocio]	        != un.[CodigoUnidadeNegocioSuperior]
		AND un.[DataExclusao]					IS NULL
		AND un.[IndicaUnidadeNegocioAtiva]		= 'S'
        AND (un.[CodigoUnidadeNegocio] =un.[CodigoEntidade]) OR un.[CodigoUnidadeNegocio] = {2}", bancodb, Ownerdb, codigoEntidade);
            return classeDados.getDataSet(comandoSQL);
        }


        public DataSet getUnidadeSuperior(string where, int codigoEntidade)
        {
            comandoSQL = string.Format(
                @"SELECT
                CodigoUnidadeNegocio,
                SiglaUnidadeNegocio, 
                CASE WHEN CodigoEntidade = CodigoUnidadeNegocio THEN 0 ELSE 1 END AS Ordem	
              FROM {0}.{1}.UnidadeNegocio
              WHERE (IndicaUnidadeNegocioAtiva = 'S'
                AND CodigoEntidade = {2}
              --AND CodigoUnidadeNegocio = {3}
                AND DataExclusao is null) 
                {3}
              ORDER BY Ordem, SiglaUnidadeNegocio", bancodb, Ownerdb, codigoEntidade, where);
            return classeDados.getDataSet(comandoSQL);
        }

        public DataSet getUnidadesNegocioEntidade(int codigoEntidade, string where)
        {
            comandoSQL = string.Format(@"
                SELECT      un.CodigoUnidadeNegocio
                        ,   un.NomeUnidadeNegocio
                        ,   (SELECT TOP 1 uns.NomeUnidadeNegocio FROM {0}.{1}.UnidadeNegocio AS uns WHERE uns.CodigoUnidadeNegocio = un.CodigoUnidadeNegocioSuperior) AS NomeUnidadeNegocioSuperior
                        ,   un.SiglaUnidadeNegocio
                        ,   un.IndicaUnidadeNegocioAtiva
                        ,   un.Observacoes
                        ,   un.CodigoUnidadeNegocioSuperior
                        ,   un.CodigoUsuarioGerente
                        ,   un.NivelHierarquicoUnidade
                        ,   un.CodigoReservado

                FROM {0}.{1}.UnidadeNegocio un

                WHERE un.CodigoEntidade = {2}
                  AND un.DataExclusao   IS NULL AND IndicaUnidadeNegocioAtiva = 'S'
                  {3} ORDER BY un.SiglaUnidadeNegocio
              ", bancodb, Ownerdb, codigoEntidade, where);
            return getDataSet(comandoSQL);
        }

        public DataSet getUnidadesNegocioListaProjetos(int codigoEntidade, int codigoUsuario, int codigoCarteira)
        {
            comandoSQL = string.Format(@"
                SELECT NomeUnidadeNegocio, SiglaUnidadeNegocio, CodigoUnidadeNegocio
                  FROM {0}.{1}.UnidadeNegocio
                 WHERE CodigoEntidade = {2} 
                   AND IndicaUnidadeNegocioAtiva = 'S'
                   AND DataExclusao IS NULL
                 ORDER BY SiglaUnidadeNegocio
              ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoCarteira);
            return getDataSet(comandoSQL);
        }

        /// <summary>
        /// Metodo para o controle da tela, no momento de fazer referencia ao botão de permissão.
        /// </summary>
        /// <param name="codigoEntidade"></param>
        /// <param name="where"></param>
        /// <param name="permissao"></param>
        /// <returns></returns>
        public DataSet getUnidadesNegocioTela(int codigoEntidade, int idUsuario, string where)
        {
            comandoSQL = string.Format(@"
                SELECT  un.CodigoUnidadeNegocio
                    ,   un.NomeUnidadeNegocio
                    ,   (SELECT TOP 1 uns.NomeUnidadeNegocio FROM {0}.{1}.UnidadeNegocio AS uns WHERE uns.CodigoUnidadeNegocio = un.CodigoUnidadeNegocioSuperior) AS NomeUnidadeNegocioSuperior
                    ,   un.SiglaUnidadeNegocio
                    ,   un.IndicaUnidadeNegocioAtiva
                    ,   un.Observacoes
                    ,   un.CodigoUnidadeNegocioSuperior
                    ,   un.CodigoUsuarioGerente
                    ,   un.NivelHierarquicoUnidade
                    ,   un.CodigoReservado
                    ,   {0}.{1}.f_VerificaAcessoConcedido({4}, {2}, un.CodigoUnidadeNegocio, NULL, 'UN', 0, NULL, 'UN_AdmPrs')   *   8	-- 8	administrar permissões
                            AS [Permissoes]
                    ,   u.NomeUsuario as NomeUsuarioGerente
                    ,   un.Latitude
                    ,   un.Longitude
                FROM {0}.{1}.UnidadeNegocio un
           LEFT JOIN {0}.{1}.Usuario u on (u.CodigoUsuario = un.CodigoUsuarioGerente)
                WHERE un.CodigoEntidade = {2}
                  AND un.DataExclusao   IS NULL
                  {3}

                ORDER BY un.SiglaUnidadeNegocio
              ", bancodb, Ownerdb, codigoEntidade, where, idUsuario);
            return getDataSet(comandoSQL);
        }

        public DataSet getEstruturaHierarquicaPai(int codigoUnidadeNegocio, string where)
        {
            comandoSQL = string.Format(
               @"SELECT  NivelHierarquicoUnidade
              FROM {0}.{1}.UnidadeNegocio
              WHERE (CodigoUnidadeNegocio = {2}){3} ", bancodb, Ownerdb, codigoUnidadeNegocio, where);
            return classeDados.getDataSet(comandoSQL);
        }

        public DataSet incluiUnidadeNegocio(string nomeUnidadeNegocio, string siglaUnidadeNegocio,
                                                bool codigoTipoUnidadeNegocio, int nivelHierarquicoUnidade,
                                                char indicaUnidadeNegocioAtiva, int codigoUsuarioInclusao,
                                                string observacoes, int codigoEntidade, int? codigoUnidadeNegocioSuperior,
                                                ref int codigoUnidadeNegocio, int codigoGerente, string codigoReservado,
                                                ref string mensagem)
        {
            int tipoUnidade = 0;

            string comandoSQLNivelUnidade = "";

            //se possui unidade Superior, busca o nível dela.
            if (codigoUnidadeNegocioSuperior.HasValue)
            {
                comandoSQLNivelUnidade = string.Format(@"
                    SELECT  @NivelHierarquicoUnidade = NivelHierarquicoUnidade 
                    FROM    {0}.{1}.UnidadeNegocio 
                    WHERE   CodigoUnidadeNegocio = {2}
                    ", bancodb, Ownerdb, codigoUnidadeNegocioSuperior.Value);
            }

            //        if (codigoTipoUnidadeNegocio == true)
            //        {
            //            tipoUnidade = 2;
            //            comandoSQLNivelUnidade = @"
            //                DECLARE @CodigoTipoUnidadeNegocio INT
            //                
            //                SET @CodigoTipoUnidadeNegocio = 2 --(SELECT CodigoTipoUnidadeNegocio FROM TipoUnidadeNegocio WHERE IndicaEntidade = 'N')
            //                ";
            //        }
            //        else
            //        {
            tipoUnidade = 1;

            comandoSQLNivelUnidade = @"
                DECLARE @CodigoTipoUnidadeNegocio INT
                
                SET @CodigoTipoUnidadeNegocio = (SELECT MIN(CodigoTipoUnidadeNegocio)
                                                   FROM TipoUnidadeNegocio 
                                                  WHERE IndicaEntidade = 'N')
                ";
            //}

            comandoSQL = string.Format(@"
                    BEGIN

                        DECLARE @NivelHierarquicoUnidade INT

                        SET @NivelHierarquicoUnidade = 1 

                        {2}

                        INSERT INTO {0}.{1}.UnidadeNegocio(
                                    NomeUnidadeNegocio
                                ,   SiglaUnidadeNegocio
                                ,   CodigoTipoUnidadeNegocio
                                ,   NivelHierarquicoUnidade
                                ,   IndicaUnidadeNegocioAtiva
                                ,   DataInclusao
                                ,   CodigoUsuarioInclusao
                                ,   Observacoes
                                ,   CodigoEntidade
                                ,   CodigoUsuarioGerente
                                ,   CodigoReservado
                                ,   CodigoUnidadeNegocioSuperior


                                )
                        VALUES(
                                '{3}'
                            ,   '{4}' 
                            ,   @CodigoTipoUnidadeNegocio --{5}
                            ,   @NivelHierarquicoUnidade
                            ,   '{6}'
                            ,   GETDATE()
                            ,   {7}
                            ,   '{8}'
                            ,   {9}
                            ,   {10}
                            ,   {11}
                            ,   {12}


                            )

                        SELECT scope_identity() as CodigoUnidadeNegocio

                    END
                ", bancodb, Ownerdb, comandoSQLNivelUnidade, nomeUnidadeNegocio, siglaUnidadeNegocio, tipoUnidade
                     , indicaUnidadeNegocioAtiva, codigoUsuarioInclusao, observacoes, codigoEntidade
                     , (codigoGerente == 0) ? "NULL" : codigoGerente.ToString()
                     , (codigoReservado == "") ? "NULL" : "'" + codigoReservado + "'"
                     , codigoUnidadeNegocioSuperior.HasValue ? codigoUnidadeNegocioSuperior.Value.ToString() : "NULL");


            return getDataSet(comandoSQL);
        }


        public int getUnidadePossueFilho(int codigoUnidadeNegocio)
        {
            comandoSQL = string.Format(@"SELECT COUNT(EstruturaHierarquica)   
                                            FROM {0}.{1}.UnidadeNegocio
                                         WHERE  EstruturaHierarquica like (SELECT EstruturaHierarquica
												                            FROM {0}.{1}.UnidadeNegocio
												                            WHERE	CodigoUnidadeNegocio = {2})	+ '.%'
                                         AND DataExclusao is null
                                            ", bancodb, Ownerdb, codigoUnidadeNegocio);

            DataSet ds = classeDados.getDataSet(comandoSQL);
            return int.Parse(ds.Tables[0].Rows[0][0].ToString());
        }

        public DataSet atualizaUnidadeNegocio(string nomeUnidadeNegocio, string siglaUnidadeNegocio, int codigoUsuarioAlteracao, int nivelHierarquicoUnidade, char indicaUnidadeNegocioAtiva, string observacoes, int codigoUnidadeNegocio, int? codigoUnidadeNegocioSuperior, int codigoGerente, string codigoReservado, ref string msg)
        {
            string comandoSQLNivelUnidade = "";

            //se possui unidade Superior, busca o nível dela.
            if (codigoUnidadeNegocioSuperior.HasValue)
            {
                comandoSQLNivelUnidade = string.Format(@"
                    SELECT  @NivelHierarquicoUnidade = NivelHierarquicoUnidade 
                    FROM    {0}.{1}.UnidadeNegocio 
                    WHERE   CodigoUnidadeNegocio = {2}
                    ", bancodb, Ownerdb, codigoUnidadeNegocioSuperior.Value);
            }

            comandoSQL = string.Format(@"
                BEGIN
                    DECLARE @NivelHierarquicoUnidade INT

                    SET @NivelHierarquicoUnidade = 1 

                    {2}

                    UPDATE {0}.{1}.UnidadeNegocio
                       SET  NomeUnidadeNegocio              = '{3}'
                        ,   SiglaUnidadeNegocio             = '{4}'
                        ,   DataUltimaAlteracao             = GETDATE()
                        ,   CodigoUsuarioUltimaAlteracao    = {5}
                        ,   IndicaUnidadeNegocioAtiva       = '{6}'
                        ,   Observacoes                     = '{7}'
                        ,   CodigoUnidadeNegocioSuperior    = {8}
                        ,   CodigoUsuarioGerente            = {9}
                        ,   CodigoReservado                 = {10}
                        ,   NivelHierarquicoUnidade         = @NivelHierarquicoUnidade


                     WHERE CodigoUnidadeNegocio     = {11}

                    -- tem que fazer um cursor para atualuzar os niveis das unidades filhas
                    SELECT CodigoUnidadeNegocio FROM {0}.{1}.UnidadeNegocio

                END
            ", bancodb, Ownerdb, comandoSQLNivelUnidade, nomeUnidadeNegocio, siglaUnidadeNegocio, codigoUsuarioAlteracao
                 , indicaUnidadeNegocioAtiva, observacoes
                 , codigoUnidadeNegocioSuperior.HasValue ? codigoUnidadeNegocioSuperior.Value.ToString() : "NULL"
                 , (codigoGerente == 0) ? "NULL" : codigoGerente.ToString()
                 , (codigoReservado == "") ? "NULL" : "'" + codigoReservado + "'"
                 , codigoUnidadeNegocio);

            return getDataSet(comandoSQL);
        }

        public bool excluiUnidadeNegocio(int codigoUnidadeNegocio, int codigoUsuarioExclusao, ref int regAfetados, ref string mensagem)
        {
            try
            {
                comandoSQL = string.Format(@"
                    DECLARE @RC int
                    DECLARE @in_codigoUnidadeNegocio int
                    DECLARE @in_codigoUsuarioExclusao int
                    DECLARE @in_tipoUnidade char(1)

                    SET @in_codigoUnidadeNegocio    = {3}
                    SET @in_codigoUsuarioExclusao   = {2}
                    SET @in_tipoUnidade             = 'U'

                    EXECUTE @RC = {0}.{1}.[p_ExcluiUnidadeNegocio] 
                       @in_codigoUnidadeNegocio
                      ,@in_codigoUsuarioExclusao
                      ,@in_tipoUnidade
            ", bancodb, Ownerdb, codigoUsuarioExclusao, codigoUnidadeNegocio);

                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                mensagem = ex.Message;
                return false;
            }
        }

        public System.Drawing.Image ObtemLogoEntidade(int codigoUsuario)
        {

            int codEntidade = Convert.ToInt32(this.getInfoSistema("CodigoEntidade"));
            DataSet dsLogoUnidade = this.getLogoUnidade(codigoUsuario, codEntidade);
            if (this.DataSetOk(dsLogoUnidade) && this.DataTableOk(dsLogoUnidade.Tables[0]) && dsLogoUnidade.Tables[0].Rows[0]["LogoUnidadeNegocio"] + "" != "")
            {
                try
                {
                    byte[] bytesLogo = (byte[])dsLogoUnidade.Tables[0].Rows[0]["LogoUnidadeNegocio"];
                    System.IO.MemoryStream stream = new System.IO.MemoryStream(bytesLogo);
                    System.Drawing.Image logo = System.Drawing.Image.FromStream(stream);
                    return logo;
                }
                catch
                {
                    return null;
                }
            }
            else
                return null;
        }

        public System.Drawing.Image ObtemLogoEntidade()
        {
            int codEntidade = Convert.ToInt32(this.getInfoSistema("CodigoEntidade"));
            DataSet dsLogoUnidade = this.getLogoEntidade(codEntidade, "");

            if (this.DataSetOk(dsLogoUnidade) && this.DataTableOk(dsLogoUnidade.Tables[0]) && dsLogoUnidade.Tables[0].Rows[0]["LogoUnidadeNegocio"] + "" != "")
            {
                try
                {
                    byte[] bytesLogo = (byte[])dsLogoUnidade.Tables[0].Rows[0]["LogoUnidadeNegocio"];
                    System.IO.MemoryStream stream = new System.IO.MemoryStream(bytesLogo);
                    System.Drawing.Image logo = System.Drawing.Image.FromStream(stream);
                    return logo;
                }
                catch
                {
                    return null;
                }
            }
            else
                return null;
        }

        public DataSet getLogoEntidade(int codigoUnidade, object where)
        {
            string comandoSQL = string.Format(
                  @"SELECT     
                UN.CodigoUnidadeNegocio, 
                UN.NomeUnidadeNegocio, 
                UN.LogoUnidadeNegocio
             FROM {0}.{1}.UnidadeNegocio AS UN 
            WHERE     (UN.CodigoUnidadeNegocio =  {2}) 
              AND UN.LogoUnidadeNegocio is not null 
               {3}",
                      bancodb, Ownerdb, codigoUnidade, where);

            return getDataSet(comandoSQL);

        }

        //todo: 10/11/2010 - alejandro :: Definir la consulta para informar si a unidade pode ser Inactiva o Excluida.
        public DataSet getPodeExcluirDesativarUnidadeNegocio(int codigoUnidadeAtual)
        {
            DataSet ds = null;

            string commandoSQL = string.Format(@"
        BEGIN
            DECLARE @CodigoUnidadeAtual INT

            SET @CodigoUnidadeAtual = {2}

            ------------------------------------------------------------------
            SELECT	rc.NomeRecursoCorporativo 
            FROM {0}.{1}.RecursoCorporativo AS rc

                INNER JOIN {0}.{1}.UnidadeNegocio AS un
                        ON un.CodigoUnidadeNegocio = rc.CodigoUnidadeNegocio

            WHERE rc.DataDesativacaoRecurso IS NULL
              AND un.DataExclusao IS NULL
              AND un.CodigoEntidade = @CodigoUnidadeAtual
              AND un.IndicaUnidadeNegocioAtiva = 'S'

            ------------------------------------------------------------------
            SELECT UN.NomeUnidadeNegocio 
            FROM {0}.{1}.UnidadeNegocio AS un
            WHERE un.CodigoUnidadeNegocioSuperior = @CodigoUnidadeAtual
              AND un.DataExclusao IS NULL 
              AND un.IndicaUnidadeNegocioAtiva = 'S'

            ------------------------------------------------------------------
            SELECT me.TituloMapaEstrategico 
            FROM {0}.{1}.MapaEstrategico AS me
            WHERE me.CodigoUnidadeNegocio = @CodigoUnidadeAtual
              AND me.IndicaMapaEstrategicoAtivo = 'S'

            ------------------------------------------------------------------
            SELECT pr.NomeProjeto
            FROM {0}.{1}.Projeto AS pr

                INNER JOIN {0}.{1}.Status           AS sta
                        ON pr.CodigoStatusProjeto   = sta.CodigoStatus
                INNER JOIN {0}.{1}.TipoProjeto      AS tpr
                        ON pr.CodigoTipoProjeto     = tpr.CodigoTipoProjeto
                INNER JOIN {0}.{1}.UnidadeNegocio   AS un
                        ON pr.CodigoEntidade        = un.CodigoUnidadeNegocio

            WHERE tpr.CodigoTipoProjeto     IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ' OR tp.IndicaTipoProjeto = 'PRG')
              AND un.CodigoUnidadeNegocio   = @CodigoUnidadeAtual
              AND pr.DataExclusao           IS NULL
              AND un.DataExclusao           IS NULL
              AND (     sta.IndicaAcompanhamentoPortfolio   = 'S'
                    OR  sta.IndicaSelecaoPortfolio          = 'S'
                    OR  sta.IndicaAcompanhamentoExecucao    = 'S'
                    OR  sta.IndicaAcompanhamentoPosExecucao = 'S' )

            ------------------------------------------------------------------
            SELECT pr.NomeProjeto
            FROM {0}.{1}.Projeto AS pr

                INNER JOIN {0}.{1}.Status           AS sta
                        ON pr.CodigoStatusProjeto   = sta.CodigoStatus
                INNER JOIN {0}.{1}.TipoProjeto      AS tpr
                        ON pr.CodigoTipoProjeto     = tpr.CodigoTipoProjeto
                INNER JOIN {0}.{1}.UnidadeNegocio   AS un
                        ON pr.CodigoEntidade        = un.CodigoUnidadeNegocio

            WHERE tpr.CodigoTipoProjeto     IN (4, 5)
              AND un.CodigoUnidadeNegocio   = @CodigoUnidadeAtual
              AND pr.DataExclusao           IS NULL
              AND un.DataExclusao           IS NULL
              AND (     sta.IndicaAcompanhamentoPortfolio   = 'S'
                    OR  sta.IndicaSelecaoPortfolio          = 'S'
                    OR  sta.IndicaAcompanhamentoExecucao    = 'S'
                    OR  sta.IndicaAcompanhamentoPosExecucao = 'S' )

            ------------------------------------------------------------------
            SELECT con.DescricaoObjetoContrato 
            FROM {0}.{1}.Contrato AS con
            WHERE con.CodigoUnidadeNegocio = @CodigoUnidadeAtual
        END
        ", bancodb, Ownerdb, codigoUnidadeAtual);

            ds = getDataSet(commandoSQL);
            return ds;
        }

        public DataSet getVerificarNomeUnidadeCadastrada(string nomeUnidade, string siglaUnidade, string codigoReservado, string where)
        {
            comandoSQL = string.Format(@"
                    SELECT CodigoUnidadeNegocio, NomeUnidadeNegocio, SiglaUnidadeNegocio, CodigoReservado
                      FROM {0}.{1}.UnidadeNegocio
                     WHERE (    NomeUnidadeNegocio = '{2}'
                            OR  SiglaUnidadeNegocio = '{3}' 
                            OR  CodigoReservado = '{4}' )
                       AND DataExclusao IS NULL
                       {5}
                    ", bancodb, Ownerdb
                         , nomeUnidade.Replace("'", "''")
                         , siglaUnidade.Replace("'", "''")
                         , codigoReservado.Replace("'", "''")
                         , where);
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        #endregion


        #region Categorias

        public DataSet getCriteriosSelecaoUnidade_Disponivel(int codigoCategoria, int codigoUnidade, string where)
        {
            comandoSQL = string.Format(
              @"SELECT CodigoCriterioSelecao, DescricaoCriterioSelecao
              FROM {0}.{1}.CriterioSelecao cs 
             WHERE DataExclusao IS NULL
                 AND CodigoEntidade = {2}
                 {4}
                 AND CodigoCriterioSelecao NOT IN (SELECT ccs.CodigoCriterioSelecao
				                                     FROM {0}.{1}.CategoriaCriterioSelecao ccs
				                                    WHERE ccs.CodigoCategoria = {3})              
             ORDER BY DescricaoCriterioSelecao", bancodb, Ownerdb, codigoUnidade, codigoCategoria, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getCriteriosSelecaoUnidade_Selecionado(int codigoCategoria, int codigoUnidade, string where)
        {
            comandoSQL = string.Format(
              @"SELECT CodigoCriterioSelecao, DescricaoCriterioSelecao
              FROM {0}.{1}.CriterioSelecao cs 
             WHERE DataExclusao IS NULL
                 AND CodigoEntidade = {2}
                 {4}
                 AND CodigoCriterioSelecao IN (SELECT ccs.CodigoCriterioSelecao
				                                 FROM {0}.{1}.CategoriaCriterioSelecao ccs
				                                WHERE ccs.CodigoCategoria = {3})              
             ORDER BY DescricaoCriterioSelecao", bancodb, Ownerdb, codigoUnidade, codigoCategoria, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getRiscosPadroesUnidade_Disponivel(int codigoCategoria, int codigoUnidade, string where)
        {
            comandoSQL = string.Format(
              @"SELECT CodigoRiscoPadrao, DescricaoRiscoPadrao
              FROM {0}.{1}.RiscoPadrao rp 
             WHERE DataExclusao IS NULL
                 AND CodigoEntidade = {2}
                 {4}
                 AND CodigoRiscoPadrao NOT IN (SELECT crp.CodigoRiscoPadrao
				                                 FROM {0}.{1}.CategoriaRiscoPadrao crp
				                                WHERE crp.CodigoCategoria = {3})              
             ORDER BY DescricaoRiscoPadrao", bancodb, Ownerdb, codigoUnidade, codigoCategoria, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getRiscosPadroesUnidade_Selecionado(int codigoCategoria, int codigoUnidade, string where)
        {
            comandoSQL = string.Format(
              @"SELECT CodigoRiscoPadrao, DescricaoRiscoPadrao
              FROM {0}.{1}.RiscoPadrao rp 
             WHERE DataExclusao IS NULL
                 AND CodigoEntidade = {2}
                 {4}
                 AND CodigoRiscoPadrao IN (SELECT crp.CodigoRiscoPadrao
				                             FROM {0}.{1}.CategoriaRiscoPadrao crp
				                            WHERE crp.CodigoCategoria = {3})              
             ORDER BY DescricaoRiscoPadrao", bancodb, Ownerdb, codigoUnidade, codigoCategoria, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getCriteriosSelecaoUnidade(int codigoCategoria, int codigoUnidade, string where)
        {
            comandoSQL = string.Format(
              @"SELECT CodigoCriterioSelecao, DescricaoCriterioSelecao, 'S' AS Selecionado
              FROM {0}.{1}.CriterioSelecao cs 
             WHERE DataExclusao IS NULL
                 AND CodigoEntidade = {2}
                 {4}
                 AND CodigoCriterioSelecao IN (SELECT ccs.CodigoCriterioSelecao
				                                 FROM {0}.{1}.CategoriaCriterioSelecao ccs
				                                WHERE ccs.CodigoCategoria = {3})              
            UNION
            SELECT CodigoCriterioSelecao, DescricaoCriterioSelecao, 'N' AS Selecionado
              FROM {0}.{1}.CriterioSelecao cs 
             WHERE DataExclusao IS NULL
                 AND CodigoEntidade = {2}
                 {4}
                 AND CodigoCriterioSelecao NOT IN (SELECT ccs.CodigoCriterioSelecao
                                                     FROM {0}.{1}.CategoriaCriterioSelecao ccs
				                                    WHERE ccs.CodigoCategoria = {3})
             ORDER BY Selecionado DESC, CodigoCriterioSelecao DESC", bancodb, Ownerdb, codigoUnidade, codigoCategoria, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getRiscosPadroesUnidade(int codigoCategoria, int codigoUnidade, string where)
        {
            comandoSQL = string.Format(
              @"SELECT CodigoRiscoPadrao, DescricaoRiscoPadrao, 'S' AS Selecionado
              FROM {0}.{1}.RiscoPadrao rp 
             WHERE DataExclusao IS NULL
                 AND CodigoEntidade = {2}
                 {4}
                 AND CodigoRiscoPadrao IN (SELECT crp.CodigoRiscoPadrao
				                                 FROM {0}.{1}.CategoriaRiscoPadrao crp
				                                WHERE crp.CodigoCategoria = {3})              
            UNION
            SELECT CodigoRiscoPadrao, DescricaoRiscoPadrao, 'N' AS Selecionado
              FROM {0}.{1}.RiscoPadrao rp 
             WHERE DataExclusao IS NULL
                 AND CodigoEntidade = {2}
                 {4}
                 AND CodigoRiscoPadrao NOT IN (SELECT crp.CodigoRiscoPadrao
				                                 FROM {0}.{1}.CategoriaRiscoPadrao crp
				                                WHERE crp.CodigoCategoria = {3})
             ORDER BY Selecionado DESC, CodigoRiscoPadrao DESC", bancodb, Ownerdb, codigoUnidade, codigoCategoria, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getCategoriasEntidade(int CodigoEntidade, string where)
        {
            comandoSQL = string.Format(
              @"SELECT CodigoCategoria
                  ,DescricaoCategoria
                  ,SiglaCategoria
              FROM {0}.{1}.Categoria
             WHERE CodigoEntidade = {2} 
               AND DataExclusao IS NULL
               {3}
            ORDER BY CodigoCategoria DESC", bancodb, Ownerdb, CodigoEntidade, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getValoresCategoriasUnidade(int codigoCategoria, string where)
        {
            comandoSQL = string.Format(
              @"SELECT RCS.CodigoCategoriaDe, RCS.CodigoCriterioSelecaoDe, RCS.CodigoCriterioSelecaoPara, RCS.ValorRelacaoCriterioDePara
              FROM {0}.{1}.RelacaoCriteriosCategoria AS RCS INNER JOIN
                   {0}.{1}.CriterioSelecao AS CSD ON RCS.CodigoCriterioSelecaoDe = CSD.CodigoCriterioSelecao INNER JOIN
                   {0}.{1}.CriterioSelecao AS CSP ON RCS.CodigoCriterioSelecaoPara = CSP.CodigoCriterioSelecao
             WHERE RCS.CodigoCategoriaDe = {2}
               {3}
             ORDER BY CSD.DescricaoCriterioSelecao, CSP.DescricaoCriterioSelecao", bancodb, Ownerdb, codigoCategoria, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getValoresRiscosUnidade(int codigoCategoria, string where)
        {
            comandoSQL = string.Format(
              @"SELECT CodigoCategoriaDe, CodigoRiscoPadraoDe, 
			 CodigoRiscoPadraoPara, ValorRelacaoRiscoPadraoDePara
              FROM {0}.{1}.RelacaoRiscosCategoria RRC INNER JOIN
                   {0}.{1}.RiscoPadrao AS RPD ON RRC.CodigoRiscoPadraoDe = RPD.CodigoRiscoPadrao INNER JOIN
                   {0}.{1}.RiscoPadrao AS RPP ON RRC.CodigoRiscoPadraoPara = RPP.CodigoRiscoPadrao
             WHERE CodigoCategoriaDe = {2}
               {3}
             ORDER BY RPD.DescricaoRiscoPadrao, RPP.DescricaoRiscoPadrao", bancodb, Ownerdb, codigoCategoria, where);

            return getDataSet(comandoSQL);
        }

        public bool incluiCriteriosCategoria(string descricaoCategoria, string siglaCategoria, int codigoUsuarioInclusao, int codigoUnidade,
            int[] codigoCriterioSelecao, int[] valorRelacaoCriterio,
            int[] codigoRisco, int[] valorRisco, ref string msg, ref int regAfetados)
        {
            try
            {
                string insereCriterios = "";
                string insereValoresRelacao = "";
                string insereRiscos = "";
                string insereValoresRiscos = "";

                int contador = 0;

                for (int i = 0; i < codigoCriterioSelecao.Length; i++)
                {
                    insereCriterios += string.Format(@"INSERT INTO {0}.{1}.CategoriaCriterioSelecao (CodigoCategoria, CodigoCriterioSelecao)
			                VALUES(@NovoCodigoCategoria, {2}); ", bancodb, Ownerdb, codigoCriterioSelecao[i]);

                    for (int j = 0; j < codigoCriterioSelecao.Length; j++)
                    {
                        insereValoresRelacao += string.Format(@"INSERT INTO {0}.{1}.RelacaoCriteriosCategoria (CodigoCategoriaDe, CodigoCriterioSelecaoDe, CodigoCategoriaPara, CodigoCriterioSelecaoPara, ValorRelacaoCriterioDePara)
				                                                VALUES(@NovoCodigoCategoria, {2}, @NovoCodigoCategoria, {3}, {4}); ", bancodb, Ownerdb,
                                                                           codigoCriterioSelecao[i], codigoCriterioSelecao[j],
                                                                           valorRelacaoCriterio[contador]);
                        contador++;

                    }
                }

                contador = 0;

                for (int i = 0; i < codigoRisco.Length; i++)
                {
                    insereRiscos += string.Format(@"INSERT INTO {0}.{1}.CategoriaRiscoPadrao (CodigoCategoria, CodigoRiscoPadrao)
			                VALUES(@NovoCodigoCategoria, {2}); ", bancodb, Ownerdb, codigoRisco[i]);

                    for (int j = 0; j < codigoRisco.Length; j++)
                    {
                        insereValoresRiscos += string.Format(@"INSERT INTO {0}.{1}.RelacaoRiscosCategoria (CodigoCategoriaDe, CodigoRiscoPadraoDe, CodigoCategoriaPara, CodigoRiscoPadraoPara, ValorRelacaoRiscoPadraoDePara)
				                                                VALUES(@NovoCodigoCategoria, {2}, @NovoCodigoCategoria, {3}, {4}); ", bancodb, Ownerdb,
                                                                           codigoRisco[i], codigoRisco[j],
                                                                           valorRisco[contador]);
                        contador++;

                    }
                }

                comandoSQL = string.Format(@" BEGIN
	                                        DECLARE @NovoCodigoCategoria int
                                        	
	                                        INSERT INTO {0}.{1}.Categoria (DescricaoCategoria, DataInclusao, CodigoUsuarioInclusao, CodigoEntidade, SiglaCategoria)
				                                        VALUES('{2}', GetDate(), {3}, {4}, '{9}');
                                        				
	                                        SELECT @NovoCodigoCategoria = scope_identity();	
                                            {5}
                                            {6}
                                            {7}
                                            {8}
                                          END", bancodb, Ownerdb, descricaoCategoria, codigoUsuarioInclusao, codigoUnidade,
                                                    insereCriterios, insereValoresRelacao, insereRiscos, insereValoresRiscos, siglaCategoria);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public bool incluiCategoria(string descricaoCategoria, string siglaCategoria, int codigoUsuarioInclusao, int codigoUnidade,
            ref string msg, ref int regAfetados)
        {
            try
            {

                comandoSQL = string.Format(@"INSERT INTO {0}.{1}.Categoria (DescricaoCategoria, DataInclusao, CodigoUsuarioInclusao, CodigoEntidade, SiglaCategoria)
				                                        VALUES('{2}', GetDate(), {3}, {4}, '{5}');                                        				
	                                        ", bancodb, Ownerdb, descricaoCategoria, codigoUsuarioInclusao, codigoUnidade, siglaCategoria);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public bool atualizaCategoria(int codigoCategoria, string descricaoCategoria, string siglaCategoria, ref string msg, ref int regAfetados)
        {
            try
            {
                comandoSQL = string.Format(@"UPDATE {0}.{1}.Categoria SET DescricaoCategoria = '{3}',
                                                                         SiglaCategoria = '{4}' 
                                             WHERE CodigoCategoria = {2};                                        				
                                           ", bancodb, Ownerdb, codigoCategoria, descricaoCategoria, siglaCategoria);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }

        }


        public bool atualizaCriteriosCategoria(int codigoCategoria, string descricaoCategoria, string siglaCategoria, int[] codigoCriterioSelecao, int[] valorRelacaoCriterio,
            int[] codigoRisco, int[] valorRisco, ref string msg, ref int regAfetados)
        {
            try
            {
                string insereCriterios = string.Format(@"DELETE {0}.{1}.RelacaoCriteriosCategoria WHERE CodigoCategoriaDe = @CodigoCategoria;
                                                     DELETE {0}.{1}.CategoriaCriterioSelecao WHERE CodigoCategoria = @CodigoCategoria; ", bancodb, Ownerdb); ;
                string insereValoresRelacao = "";

                string insereRiscos = string.Format(@"DELETE {0}.{1}.RelacaoRiscosCategoria WHERE CodigoCategoriaDe = @CodigoCategoria; 
                                                  DELETE {0}.{1}.CategoriaRiscoPadrao WHERE CodigoCategoria = @CodigoCategoria; ", bancodb, Ownerdb);
                string insereValoresRiscos = "";

                int contador = 0;

                for (int i = 0; i < codigoCriterioSelecao.Length; i++)
                {
                    insereCriterios += string.Format(@"INSERT INTO {0}.{1}.CategoriaCriterioSelecao (CodigoCategoria, CodigoCriterioSelecao)
			                VALUES(@CodigoCategoria, {2}); ", bancodb, Ownerdb, codigoCriterioSelecao[i]);

                    for (int j = 0; j < codigoCriterioSelecao.Length; j++)
                    {
                        insereValoresRelacao += string.Format(@"INSERT INTO {0}.{1}.RelacaoCriteriosCategoria (CodigoCategoriaDe, CodigoCriterioSelecaoDe, CodigoCategoriaPara, CodigoCriterioSelecaoPara, ValorRelacaoCriterioDePara)
				                                                VALUES(@CodigoCategoria, {2}, @CodigoCategoria, {3}, {4}); ", bancodb, Ownerdb,
                                                                           codigoCriterioSelecao[i], codigoCriterioSelecao[j],
                                                                           valorRelacaoCriterio[contador]);
                        contador++;

                    }
                }

                contador = 0;

                for (int i = 0; i < codigoRisco.Length; i++)
                {
                    insereRiscos += string.Format(@"INSERT INTO {0}.{1}.CategoriaRiscoPadrao (CodigoCategoria, CodigoRiscoPadrao)
			                VALUES(@CodigoCategoria, {2}); ", bancodb, Ownerdb, codigoRisco[i]);

                    for (int j = 0; j < codigoRisco.Length; j++)
                    {
                        insereValoresRiscos += string.Format(@"INSERT INTO {0}.{1}.RelacaoRiscosCategoria (CodigoCategoriaDe, CodigoRiscoPadraoDe, CodigoCategoriaPara, CodigoRiscoPadraoPara, ValorRelacaoRiscoPadraoDePara)
				                                                VALUES(@CodigoCategoria, {2}, @CodigoCategoria, {3}, {4}); ", bancodb, Ownerdb,
                                                                           codigoRisco[i], codigoRisco[j],
                                                                           valorRisco[contador]);
                        contador++;

                    }
                }

                comandoSQL = string.Format(@" BEGIN
	                                        DECLARE @CodigoCategoria int
                                            
                                            SET @CodigoCategoria = {2}                                       	

	                                        UPDATE {0}.{1}.Categoria SET DescricaoCategoria = '{3}',
                                                                         SiglaCategoria = '{8}' 
                                             WHERE CodigoCategoria = @CodigoCategoria;
                                        				
                                            {4}
                                            {5}
                                            {6}
                                            {7}
                                          END", bancodb, Ownerdb, codigoCategoria, descricaoCategoria,
                                                    insereCriterios, insereValoresRelacao, insereRiscos, insereValoresRiscos, siglaCategoria);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }

        }

        public bool excluiCategoria(int codigoCategoria, int codigoUsuarioExclusao, ref string msg, ref int regAfetados)
        {
            try
            {
                comandoSQL = string.Format(@"UPDATE {0}.{1}.Categoria SET DataExclusao = GetDate(),
                                                             CodigoUsuarioExclusao = {3}
                                          WHERE CodigoCategoria = {2}",
                                                    bancodb, Ownerdb, codigoCategoria, codigoUsuarioExclusao);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (SqlException ex)
            {
                msg = ex.Errors[0].Message;
                return false;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }


        public DataSet getMatrizCategoria(int codigoCategoria, string where)
        {
            comandoSQL = string.Format(
              @"SELECT * 
              FROM {0}.{1}.f_getObjetosCriterio({2})
             WHERE 1 = 1
               {3}", bancodb, Ownerdb, codigoCategoria, where);

            return getDataSet(comandoSQL);
        }

        public bool excluiGrupoFator(int codigoGrupo, int codigoUsuarioExclusao, ref int regAfetados)
        {
            try
            {
                comandoSQL = string.Format(@"
                DECLARE @CodigoCategoria SmallInt
                SELECT @CodigoCategoria = CodigoCategoria 
                    FROM {0}.{1}.GrupoCriterioSelecao WHERE CodigoGrupoCriterio = {3}; 

                UPDATE {0}.{1}.GrupoCriterioSelecao 
                    SET DataExclusao = GetDate(),
                        CodigoUsuarioExclusao = {2}
                    WHERE CodigoGrupoCriterio = {3}; 

                DELETE FROM {0}.{1}.RelacaoObjetoCriterioPortfolio
                 WHERE IniciaisTipoObjetoCriterioPai = 'GP'
                   AND CodigoCategoria = @CodigoCategoria
                   AND CodigoObjetoCriterioPai = {3}

                DELETE FROM {0}.{1}.MatrizObjetoCriterio
                 WHERE IniciaisTipoObjetoCriterioPai = 'GP'
                   AND CodigoCategoria = @CodigoCategoria
                   AND CodigoObjetoCriterioPai = {3};

                EXEC {0}.{1}.[p_RecalculaMatrizPesoPortfolio] @CodigoCategoria, {3}, 'GP';
                                                                              				
	            ", bancodb, Ownerdb, codigoUsuarioExclusao, codigoGrupo);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public DataSet getGruposFator(int codigoFator, int codigoCategoria, int codigoEntidade, string where)
        {
            comandoSQL = string.Format(
              @"SELECT CodigoGrupoCriterio, NomeGrupo 
              FROM {0}.{1}.GrupoCriterioSelecao
             WHERE CodigoEntidade = {4}
               AND CodigoCategoria = {3}
               AND CodigoFatorPortfolio = {2}
               AND DataExclusao IS NULL
             ORDER BY NomeGrupo", bancodb, Ownerdb, codigoFator, codigoCategoria, codigoEntidade, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getValoresGruposFator(int codigoFator, int codigoCategoria, int codigoEntidade, string where)
        {
            comandoSQL = string.Format(
              @"SELECT rocp.* 
              FROM {0}.{1}.RelacaoObjetoCriterioPortfolio rocp INNER JOIN
	               {0}.{1}.GrupoCriterioSelecao gpDe ON gpDe.CodigoGrupoCriterio = rocp.CodigoObjetoCriterioDe INNER JOIN
	               {0}.{1}.GrupoCriterioSelecao gpPara ON gpPara.CodigoGrupoCriterio = rocp.CodigoObjetoCriterioPara
             WHERE IniciaisTipoObjetoCriterioPai = 'FT'
               AND rocp.CodigoCategoria = {2}
               AND rocp.CodigoObjetoCriterioPai = {3}
               AND gpDe.DataExclusao IS NULL
               AND gpPara.DataExclusao IS NULL
               AND gpDe.CodigoEntidade = {4}
               AND gpPara.CodigoEntidade = {4}
             ORDER BY gpDe.NomeGrupo, gpPara.NomeGrupo", bancodb, Ownerdb, codigoCategoria, codigoFator, codigoEntidade, where);

            return getDataSet(comandoSQL);
        }

        public bool incluiValoresObjetosCategoria(int codigoCategoria, int codigoObjetoPai, int codigoEntidade, string iniciaisObjetoPai,
            int[] codigos, float[] valoresRelacao, ref string msg, ref int regAfetados)
        {
            try
            {
                string insereValores = "";

                int contador = 0;

                for (int i = 0; i < codigos.Length; i++)
                {
                    for (int j = 0; j < codigos.Length; j++)
                    {
                        insereValores += string.Format(@"INSERT INTO {0}.{1}.RelacaoObjetoCriterioPortfolio (CodigoCategoria, IniciaisTipoObjetoCriterioPai, CodigoObjetoCriterioPai, CodigoObjetoCriterioDe, CodigoObjetoCriterioPara, ValorRelacaoObjetoDePara)
				                                                VALUES({2}, '{3}', {4}, {5}, {6}, {7}); 
                                                    ", bancodb, Ownerdb,
                                                         codigoCategoria, iniciaisObjetoPai, codigoObjetoPai,
                                                         codigos[i], codigos[j],
                                                         valoresRelacao[contador].ToString().Replace(',', '.'));
                        contador++;

                    }
                }



                comandoSQL = string.Format(@" BEGIN
                                            DELETE FROM {0}.{1}.RelacaoObjetoCriterioPortfolio
                                             WHERE IniciaisTipoObjetoCriterioPai = '{2}'
                                               AND CodigoCategoria = {3}
                                               AND CodigoObjetoCriterioPai = {4}

	                                        {5}
                                            
                                            EXEC {0}.{1}.p_RecalculaMatrizPesoPortfolio {3}, {4}, '{2}'                                            

                                          END", bancodb, Ownerdb, iniciaisObjetoPai, codigoCategoria, codigoObjetoPai, insereValores);

                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }


        public DataSet getValoresFatoresCategoria(int codigoCategoria, int codigoEntidade, string where)
        {
            comandoSQL = string.Format(
              @"SELECT rocp.* 
              FROM {0}.{1}.RelacaoObjetoCriterioPortfolio rocp INNER JOIN
	               {0}.{1}.FatorPortfolio fDe ON fDe.CodigoFatorPortfolio = rocp.CodigoObjetoCriterioDe INNER JOIN
	               {0}.{1}.FatorPortfolio fPara ON fPara.CodigoFatorPortfolio = rocp.CodigoObjetoCriterioPara
             WHERE IniciaisTipoObjetoCriterioPai = 'CT'
               AND rocp.CodigoCategoria = {2}
               AND rocp.CodigoObjetoCriterioPai = {3}
               AND fDe.DataExclusao IS NULL
               AND fPara.DataExclusao IS NULL
               AND fDe.CodigoEntidade = {4}
               AND fPara.CodigoEntidade = {4}
             ORDER BY fDe.NomeFatorPortfolio, fPara.NomeFatorPortfolio", bancodb, Ownerdb, codigoCategoria, codigoCategoria, codigoEntidade, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getCriteriosGrupo(int codigoGrupo, int codigoCategoria, int codigoEntidade, string where)
        {
            comandoSQL = string.Format(
              @"SELECT cs.CodigoCriterioSelecao AS CodigoCriterio
		          ,cs.DescricaoCriterioSelecao AS NomeCriterio
	         FROM {0}.{1}.MatrizObjetoCriterio AS m1 INNER JOIN 
                  {0}.{1}.CriterioSelecao AS cs ON(cs.CodigoCriterioSelecao = m1.CodigoObjetoCriterio
					                                AND cs.DataExclusao IS NULL)
	        WHERE m1.CodigoCategoria = {3}
		      AND m1.CodigoObjetoCriterioPai = {2}
              AND cs.CodigoEntidade = {4}
		      AND m1.IniciaisTipoObjetoCriterioPai = 'GP'
              AND cs.DataExclusao IS NULL
              {5}", bancodb, Ownerdb, codigoGrupo, codigoCategoria, codigoEntidade, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getValoresCriteriosGrupo(int codigoGrupo, int codigoCategoria, int codigoEntidade, string where)
        {
            comandoSQL = string.Format(
              @"BEGIN
            DECLARE @tbTemp AS Table(Codigo Int, Nome VarChar(60))

            INSERT INTO @tbTemp
                SELECT cs.CodigoCriterioSelecao AS CodigoCriterio
		          ,cs.DescricaoCriterioSelecao AS NomeCriterio
	             FROM {0}.{1}.MatrizObjetoCriterio AS m1 INNER JOIN 
                      {0}.{1}.CriterioSelecao AS cs ON(cs.CodigoCriterioSelecao = m1.CodigoObjetoCriterio
					                                    AND cs.DataExclusao IS NULL)
	            WHERE m1.CodigoCategoria = {2}
		          AND m1.CodigoObjetoCriterioPai = {3}
                  AND cs.CodigoEntidade = {4}
		          AND m1.IniciaisTipoObjetoCriterioPai = 'GP'
                  AND cs.DataExclusao IS NULL

            SELECT rocp.* 
              FROM {0}.{1}.RelacaoObjetoCriterioPortfolio rocp INNER JOIN
	               @tbTemp gpDe ON gpDe.Codigo = rocp.CodigoObjetoCriterioDe INNER JOIN
	               @tbTemp gpPara ON gpPara.Codigo = rocp.CodigoObjetoCriterioPara
             WHERE IniciaisTipoObjetoCriterioPai = 'GP'
               AND rocp.CodigoCategoria = {2}
               AND rocp.CodigoObjetoCriterioPai = {3}               
             ORDER BY gpDe.Nome, gpPara.Nome
            END", bancodb, Ownerdb, codigoCategoria, codigoGrupo, codigoEntidade, where);

            return getDataSet(comandoSQL);
        }


        #endregion

        #region Entidades

        public DataSet getEntidadesUsuario(int codigoUsuario, string where)
        {
            comandoSQL = string.Format(
                @"SELECT Distinct 
                     Entidade.CodigoUnidadeNegocio, 
                     Entidade.NomeUnidadeNegocio, 
                     TipoUnidadeNegocio.DescricaoTipoUnidadeNegocio, 
                     TipoUnidadeNegocio.IndicaEntidade, 
                     TipoUnidadeNegocio.CodigoTipoUnidadeNegocio, 
                     Entidade.SiglaUnidadeNegocio,
                     Entidade.CodigoReservado 
                FROM {0}.{1}.TipoUnidadeNegocio INNER JOIN
                     {0}.{1}.UnidadeNegocio AS Entidade ON TipoUnidadeNegocio.CodigoTipoUnidadeNegocio = Entidade.CodigoTipoUnidadeNegocio INNER JOIN
                     {0}.{1}.UsuarioUnidadeNegocio ON Entidade.CodigoUnidadeNegocio = UsuarioUnidadeNegocio.CodigoUnidadeNegocio
               WHERE TipoUnidadeNegocio.IndicaEntidade = 'S'
                 AND UsuarioUnidadeNegocio.IndicaUsuarioAtivoUnidadeNegocio = 'S'
                 AND Entidade.IndicaUnidadeNegocioAtiva = 'S'
                 AND Entidade.DataExclusao IS NULL 
                 AND {0}.{1}.f_VerificaAcessoConcedido({2}, Entidade.CodigoUnidadeNegocio, Entidade.CodigoUnidadeNegocio, NULL, 'EN', 0, NULL, 'EN_Acs') = 1 
                 {3} 
               ORDER BY TipoUnidadeNegocio.CodigoTipoUnidadeNegocio, 
                        Entidade.NomeUnidadeNegocio", bancodb, Ownerdb, codigoUsuario, where);
            return getDataSet(comandoSQL);
        }

        public DataSet getEntidadesMapasEstrategicos(int codigoUsuario, int codigoMapa, string where)
        {
            comandoSQL = string.Format(
                @"SELECT DISTINCT 
                     un.CodigoUnidadeNegocio, 
                     un.NomeUnidadeNegocio, 
                     un.SiglaUnidadeNegocio,
                     un.CodigoReservado 
                FROM {0}.{1}.UnidadeNegocio AS un INNER JOIN
                     {0}.{1}.UsuarioUnidadeNegocio ON un.CodigoEntidade = UsuarioUnidadeNegocio.CodigoUnidadeNegocio
               WHERE UsuarioUnidadeNegocio.IndicaUsuarioAtivoUnidadeNegocio = 'S'
                 AND un.IndicaUnidadeNegocioAtiva = 'S'
                 AND un.DataExclusao IS NULL   
                 AND UsuarioUnidadeNegocio.CodigoUsuario = {2} 
                 AND un.CodigoUnidadeNegocio IN (SELECT CodigoUnidadeNegocio 
                                                                         FROM {0}.{1}.PermissaoMapaEstrategicoUnidade
                                                                        WHERE CodigoMapaEstrategico = {3}
                                                                        UNION
                                                                       SELECT CodigoUnidadeNegocio 
                                                                         FROM {0}.{1}.MapaEstrategico
                                                                        WHERE CodigoMapaEstrategico = {3}) 
               ORDER BY un.SiglaUnidadeNegocio", bancodb, Ownerdb, codigoUsuario, codigoMapa, where);
            return getDataSet(comandoSQL);
        }

        //Author: Alejandro
        //Date: 18 / 11 / 2009
        public DataSet getUF(string where)
        {
            comandoSQL = string.Format(@"
                SELECT  SiglaUF
                       ,SiglaUF + ' - ' + NomeUF AS ComboUF
                       ,NomeUF
                       ,IdentificacaoUFMapa
                       ,ImagemMapaUF
                FROM {0}.{1}.[UF] 
                WHERE (1=1) 
                   {2}
                ORDER BY SiglaUF", bancodb, Ownerdb, where);
            return getDataSet(comandoSQL);
        }

        public DataSet getEntidades(string where)
        {
            comandoSQL = string.Format(@"
            SELECT  e.CodigoUnidadeNegocio
                ,   e.SiglaUnidadeNegocio
                ,   e.NomeUnidadeNegocio
                ,   uf.NomeUF
                ,   e.CodigoUsuarioGerente
                ,   e.SiglaUF
                ,   e.IndicaUnidadeNegocioAtiva
                ,   e.Observacoes
                ,   e.CodigoReservado
                ,   u.NomeUsuario
                ,   u.Email
                ,   u.TelefoneContato1
                ,   e.Latitude
                ,   e.Longitude
            FROM        {0}.{1}.UnidadeNegocio  AS e
            LEFT JOIN   {0}.{1}.UF              AS uf   ON e.SiglaUF            = uf.siglaUF 
            INNER JOIN  {0}.{1}.Usuario         AS u    ON e.CodigoSuperUsuario = u.CodigoUsuario

            WHERE e.DataExclusao IS NULL
              {2}

            ORDER BY e.SiglaUnidadeNegocio
            ", bancodb, Ownerdb, where);

            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getDefinicaoUnidade(int codigoEntidade)
        {
            comandoSQL = string.Format(@"
        BEGIN    
                DECLARE @CodigoUnidadeLogada Int
                SET @CodigoUnidadeLogada = {2}
                SELECT TOP 1 tu.[DescricaoTipoUnidadeNegocio], tu.DescricaoPluralTipoUnidade
                  FROM {0}.{1}.[TipoUnidadeNegocio]	AS [tu]
                 WHERE tu.[IndicaEntidade] = 'N'
	               AND tu.[EstruturaHierarquicaEntidade] LIKE ( 
				       SELECT tu2.[EstruturaHierarquicaEntidade] + '%'
				         FROM {0}.{1}.[UnidadeNegocio] AS [un]
				   INNER JOIN {0}.{1}.[TipoUnidadeNegocio] AS [tu2]	ON (tu2.[CodigoTipoUnidadeNegocio] = un.[CodigoTipoUnidadeNegocio])
						WHERE un.[CodigoUnidadeNegocio]	= @CodigoUnidadeLogada )
        END							
        ", bancodb, Ownerdb, codigoEntidade);
            return getDataSet(comandoSQL);
        }

        public DataSet getDefinicaoEntidade(int codigoEntidade)
        {
            comandoSQL = string.Format(@"
            BEGIN
                DECLARE @CodigoEntidade Int,
                        @NivelHierarquico Tinyint

                    SET @CodigoEntidade = {2}
                  
                 SELECT @NivelHierarquico = tun.NivelHierarquicoEntidade
                   FROM {0}.{1}.TipoUnidadeNegocio AS tun INNER JOIN
                        {0}.{1}.UnidadeNegocio AS un ON (un.CodigoTipoUnidadeNegocio = tun.CodigoTipoUnidadeNegocio
                                                         AND un.CodigoUnidadeNegocio = @CodigoEntidade)        
               
               IF EXISTS(SELECT 1 FROM {0}.{1}.TipoUnidadeNegocio
                          WHERE NivelHierarquicoEntidade > @NivelHierarquico 
                            AND IndicaEntidade = 'S')
                    SET @NivelHierarquico = @NivelHierarquico + 1

                 SELECT tun.DescricaoTipoUnidadeNegocio, tun.DescricaoPluralTipoUnidade
                   FROM {0}.{1}.TipoUnidadeNegocio AS tun 
                  WHERE NivelHierarquicoEntidade = @NivelHierarquico
                  AND tun.IndicaEntidade = 'S'
            END", bancodb, Ownerdb, codigoEntidade);
            return getDataSet(comandoSQL);
        }


        public DataSet incluiEntidade(string nomeUsuario, string senha, string emailUsuario, string fone1, int idUsuarioLogado, int codigoUnidadeSuperior,
                                      string nomeUnidadeNegocio, string siglaUnidadeNegocio, int? codigoUsuarioExistente, string siglaUF,
                                      string unidadeAtiva, string Observacoes, int codigoEntidade, string tratarEmailAdministrador, string codigoReservado,
                                      ref int regAfetados, ref string msg)
        {
            string comandoSQL = "";
            string comandoUpdateUsuario = "";
            //SETEAR la setencia SQL, dependendo si o usuario existe o noa no banco de dados.
            if (codigoUsuarioExistente == null) //Fazendo o INSERT... do usuario
            {
                // se o usuário não existe, cria inicialmente para ter seu código gravar na coluna codigoSuperUsuario
                // após incluir a entidade, altera [Usuario].[CodigoEntidadeResponsavelUsuario]
                comandoSQL = string.Format(@"
                        BEGIN
                        DECLARE @novoCodUsuario AS INT

                        --Insero usuario cadastrado como responsavel (pasada como parametro)
                        INSERT INTO {0}.{1}.Usuario
                                (NomeUsuario, TipoAutenticacao, SenhaAcessoAutenticacaoSistema, EMail, TelefoneContato1, TelefoneContato2, 
                                Observacoes, DataInclusao, CodigoUsuarioInclusao, 
                                CodigoEntidadeResponsavelUsuario) --atualizar con entidad atualmente cadastrada
                        VALUES
                                ( '{2}', 'AS', '{3}', '{4}', '{5}', ''
                                , 'Usuario cadastrado na tela Entidade ' + '{7}', GETDATE(), {6}, '{8}')
                        --Recupero o novo ID do Usuario criada al inserir.
                        SET  @novoCodUsuario = SCOPE_IDENTITY()
                        ", bancodb, Ownerdb, nomeUsuario, senha, emailUsuario, fone1, idUsuarioLogado, nomeUnidadeNegocio, codigoEntidade);

                comandoUpdateUsuario = string.Format(@"
                        --altera [Usuario].[CodigoEntidadeResponsavelUsuario]
                        UPDATE {0}.{1}.Usuario
                           SET CodigoEntidadeResponsavelUsuario = @codigoUnidade
                        WHERE CodigoUsuario = @novoCodUsuario
                ", bancodb, Ownerdb);
            }
            else //Fazendo o UPDATE... do usuario
            {
                if ("EX" == tratarEmailAdministrador) //E-mail foi Excluido.
                {
                    comandoSQL = string.Format(@"
                        BEGIN
                        DECLARE @novoCodUsuario AS INT

                        UPDATE {0}.{1}.Usuario
                        SET 
                            CodigoEntidadeResponsavelUsuario = {3}, --atualizar con entidade atualmente cadastrada
                            NomeUsuario             = '{5}',
                            TelefoneContato1        = '{6}',
                            DataExclusao            = NULL,
                            DataUltimaAlteracao     = GETDATE(),
                            CodigoUsuarioUltimaAlteracao = {4}
                        WHERE CodigoUsuario = {2}

                        --Recupero o novo ID do Usuario ja existente.
                        SET  @novoCodUsuario = {2}
                    ", bancodb, Ownerdb, codigoUsuarioExistente, codigoEntidade, idUsuarioLogado, nomeUsuario, fone1);

                    comandoUpdateUsuario = string.Format(@"
                        --altera [Usuario].[CodigoEntidadeResponsavelUsuario]
                        UPDATE {0}.{1}.Usuario
                            SET CodigoEntidadeResponsavelUsuario = @codigoUnidade
                        WHERE CodigoUsuario = @novoCodUsuario
                    ", bancodb, Ownerdb);
                }
                else if ("OE" == tratarEmailAdministrador) //E-mail pertenece a uma outra Entidade.
                {
                    comandoSQL = string.Format(@"
                        BEGIN
                        DECLARE @novoCodUsuario AS INT

                        --Atribui valor à @novoCodUsuario para ser usado mais adiante
                        SET  @novoCodUsuario = {2}
                    ", bancodb, Ownerdb, codigoUsuarioExistente, codigoEntidade, idUsuarioLogado);
                }
                else if ("NE" == tratarEmailAdministrador)//E-mail pertenece a la entidad logada.
                {
                    comandoSQL = string.Format(@" 
                        BEGIN
                        DECLARE @novoCodUsuario AS INT

                        --Actualizo con novos dados do usuario ja existente.
                        UPDATE {0}.{1}.Usuario
                           SET
                                NomeUsuario             = '{2}', 
                                TelefoneContato1        = '{3}', 
                                DataUltimaAlteracao     = GETDATE(),
                                CodigoUsuarioUltimaAlteracao = {4}
                        WHERE CodigoUsuario = {5}

                        --Recupero o novo ID do Usuario ja existente.
                        SET  @novoCodUsuario = {5}
                    ", bancodb, Ownerdb, nomeUsuario, fone1, idUsuarioLogado, codigoUsuarioExistente);

                    //                    comandoUpdateUsuario = string.Format(@"
                    //                            --altera [Usuario].[CodigoEntidadeResponsavelUsuario]
                    //                            UPDATE {0}.{1}.Usuario
                    //                               SET CodigoEntidadeResponsavelUsuario = @codigoUnidade
                    //                            WHERE CodigoUsuario = @novoCodUsuario
                    //                    ", bancodb, Ownerdb);
                }
            }

            //continuo con el seteo da sentencia SQL.
            comandoSQL += string.Format(@"
                        DECLARE @codigoUnidade AS INT
                        DECLARE @extruturaHierarquicaPai AS INT

                        --insero na Unidade de Negocio a nova Entidade (pasado como parametro)
                        INSERT INTO {0}.{1}.UnidadeNegocio(NomeUnidadeNegocio, SiglaUnidadeNegocio, SiglaUF, CodigoUnidadeNegocioSuperior, CodigoTipoUnidadeNegocio, NivelHierarquicoUnidade, IndicaUnidadeNegocioAtiva,DataInclusao, CodigoUsuarioInclusao, Observacoes, CodigoSuperUsuario,CodigoReservado)
                                                    VALUES(             '{4}',               '{5}',     {6},                          {3},                        2,                       2,                       'S',   GETDATE(),                   {2},       '{8}',    @novoCodUsuario,         {10})

                        --Recupero o novo ID da UnidadeNegocio criada al inserir.
                        SET @codigoUnidade = SCOPE_IDENTITY() 

                        --Actualizo o campo EstruturaHierarquica da nova unidade inserida
                        UPDATE {0}.{1}.UnidadeNegocio
                            SET CodigoEntidade = @codigoUnidade -- antes tava parametro codigoUnidadeSuperior
                        WHERE CodigoUnidadeNegocio = @codigoUnidade

                        IF NOT EXISTS(SELECT 1 FROM {0}.{1}.UsuarioUnidadeNegocio AS uun WHERE uun.CodigoUsuario = @novoCodUsuario AND uun.CodigoUnidadeNegocio = @codigoUnidade)
                            INSERT INTO {0}.{1}.UsuarioUnidadeNegocio(CodigoUsuario, CodigoUnidadeNegocio, IndicaUsuarioAtivoUnidadeNegocio, DataAtivacaoPerfilUnidadeNegocio)
                            VALUES(@novoCodUsuario, @codigoUnidade, 'S', GETDATE())
                        ELSE
                            UPDATE {0}.{1}.UsuarioUnidadeNegocio
                               SET IndicaUsuarioAtivoUnidadeNegocio = 'S',
                                   DataAtivacaoPerfilUnidadeNegocio = GETDATE()
                            WHERE CodigoUsuario = @novoCodUsuario
                              AND CodigoUnidadeNegocio = @codigoUnidade

                        {9}

                        --Retorno o CodigoUnidadeNegocio
                        SELECT @codigoUnidade AS codigoUnidade
                        END
                    ", bancodb, Ownerdb, idUsuarioLogado, codigoUnidadeSuperior
                         , nomeUnidadeNegocio, siglaUnidadeNegocio
                         , siglaUF, codigoUnidadeSuperior, Observacoes, comandoUpdateUsuario
                         , codigoReservado.Equals("") ? "NULL" : "'" + codigoReservado + "'");

            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet atualizaEntidade(string nomeUsuario, string senha, string emailUsuario, string fone1, int idUsuarioLogado, int codigoUnidadeSuperior,
                                        string nomeUnidadeNegocio, string siglaUnidadeNegocio, int? codigoUsuarioExistente, string siglaUF,
                                        string unidadeAtiva, string Observacoes, string codigoReservado, ref int regAfetados, ref string msg)
        {
            string comandoSQL = "";

            //SETEAR la setencia SQL, dependendo si o usuario existe o noa no banco de dados.
            if (codigoUsuarioExistente == null) //Fazendo o INSERT... do usuario
            {
                comandoSQL = string.Format(@"
                    BEGIN 
                    DECLARE @novoCodUsuario AS INT

                    --Insero usuario cadastrado como responsavel (pasada como parametro)
                    INSERT INTO {0}.{1}.Usuario
                            (NomeUsuario, TipoAutenticacao, SenhaAcessoAutenticacaoSistema, EMail, TelefoneContato1, TelefoneContato2, 
                            Observacoes, DataInclusao, CodigoUsuarioInclusao, CodigoEntidadeResponsavelUsuario)
                    VALUES
                            ( '{2}', 'AS', '{3}', '{4}', '{5}', '', 
                              'Usuario cadastrado na tela Entidad ' + '{7}', GETDATE(), {6}, )

                    --Recupero o novo ID do Usuario criada al inserir.
                    SET  @novoCodUsuario = scope_identity()
            ", bancodb, Ownerdb, nomeUsuario, senha, emailUsuario, fone1, idUsuarioLogado, nomeUnidadeNegocio, codigoUnidadeSuperior);

            }
            else //Fazendo o UPDATE... do usuario.
            {
                //A atualização do usuario en Entidade so aconteçe si o Usuario pertenece a Entidade editada.
                comandoSQL = string.Format(@" 
                    BEGIN 
                    DECLARE @novoCodUsuario AS INT

                    --Actualizo con novos dados do usuario ja existente.
                    UPDATE {0}.{1}.Usuario
                    SET
                        EMail               = '{6}',
                        NomeUsuario         = '{2}', 
                        TelefoneContato1    = '{3}', 
                        DataUltimaAlteracao = GETDATE(),
                        CodigoUsuarioUltimaAlteracao = {4}
                    WHERE CodigoUsuario = {5}

                    --Recupero o novo ID do Usuario ja existente.
                    SET  @novoCodUsuario = {5}
                ", bancodb, Ownerdb, nomeUsuario, fone1, idUsuarioLogado, codigoUsuarioExistente, emailUsuario);

            }

            //continuo con el seteo da sentencia SQL.
            comandoSQL += string.Format(@"
                    DECLARE @codigoUnidade AS INT
                    DECLARE @extruturaHierarquicaPai AS INT

                    --Recupero o novo ID da UnidadeNegocio.
	                SET @codigoUnidade = {8}

                    --Atualizo os dados da Entidad na Unidade de Negocio (pasado como parametro)
	                UPDATE {0}.{1}.UnidadeNegocio
                                SET 
                                    NomeUnidadeNegocio          = '{4}',
                                    SiglaUnidadeNegocio         = '{5}',
                                    SiglaUF                     = {6},
                                    IndicaUnidadeNegocioAtiva   = '{7}',
                                    DataUltimaAlteracao         = GETDATE(),
                                    CodigoUsuarioUltimaAlteracao = {2},
                                    Observacoes                 = '{9}',
                                    CodigoSuperUsuario          = @novoCodUsuario,
                                    CodigoReservado              = {10}
	                WHERE  
                            CodigoUnidadeNegocio = @codigoUnidade

                    --Retorno o CodigoUnidadeNegocio
                    SELECT @codigoUnidade AS codigoUnidade
                    END
                ", bancodb, Ownerdb, idUsuarioLogado, codigoUnidadeSuperior, nomeUnidadeNegocio
                     , siglaUnidadeNegocio, siglaUF, unidadeAtiva, codigoUnidadeSuperior, Observacoes
                     , codigoReservado.Equals("") ? "NULL" : "'" + codigoReservado + "'");

            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public string getPodeIncluirExcluir(string parametroCodigoEntidade)
        {
            string podeIncluirExcluir = "";
            comandoSQL = string.Format(@"
                    BEGIN
                      DECLARE @CodigoEntidadeAtual Int
                      SET @CodigoEntidadeAtual = {2} --> Parâmetro
                      
                      DECLARE @NivelHierarquicoEntidadeAtual SmallInt,
                              @PossuiEntidadeInferior Char(3)
                              
                      /* Obter o nível hierarquico da entidade atual */
                      SELECT @NivelHierarquicoEntidadeAtual = tun.NivelHierarquicoEntidade
                        FROM            {0}.{1}.TipoUnidadeNegocio  AS tun 

                             INNER JOIN {0}.{1}.UnidadeNegocio      AS un 
                                    ON (tun.CodigoTipoUnidadeNegocio = un.CodigoTipoUnidadeNegocio) 

                       WHERE un.CodigoUnidadeNegocio = @CodigoEntidadeAtual            
                         AND tun.IndicaEntidade = 'S'
                         
                      /* Verifica se existe alguma entidade onde o nível hierarquico é maior que o da entidade
                         atual. */
                         
                      IF EXISTS(SELECT 1
                                  FROM {0}.{1}.TipoUnidadeNegocio
                                 WHERE NivelHierarquicoEntidade > @NivelHierarquicoEntidadeAtual
                                   AND IndicaEntidade = 'S')
                         SET @PossuiEntidadeInferior = 'SIM'
                      ELSE
                         SET @PossuiEntidadeInferior = 'NAO'
                         
                      SELECT @PossuiEntidadeInferior AS PodeIncluirExcluir                    

                    END
            ", bancodb, Ownerdb, parametroCodigoEntidade);

            DataSet ds = getDataSet(comandoSQL);
            podeIncluirExcluir = ds.Tables[0].Rows[0]["PodeIncluirExcluir"].ToString();

            return podeIncluirExcluir;
        }

        /// <summary>
        /// Não permitir a exclusão de filiais onde exista pelo menos um usuário ativo que não seja o responsável (super usuário).
        /// </summary>
        /// <param name="codigoUnidadeNegocio"></param>
        /// <param name="cantReg"></param>
        public void getPodeExcluirSimUsuario(int codigoUnidadeNegocio, ref int cantReg)
        {
            cantReg = 0;
            comandoSQL = string.Format(@"
                    BEGIN

                    DECLARE @CodigoSuperUsuario INT

                    SELECT @CodigoSuperUsuario = CodigoSuperUsuario FROM {0}.{1}.UnidadeNegocio WHERE CodigoUnidadeNegocio = {2}

                    SELECT  1 
                    FROM {0}.{1}.UsuarioUnidadeNegocio
                    WHERE CodigoUnidadeNegocio = {2}
                    AND IndicaUsuarioAtivoUnidadeNegocio = 'S'
                    AND CodigoUsuario NOT IN (@CodigoSuperUsuario)

                    END
                    ", bancodb, Ownerdb, codigoUnidadeNegocio);

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                cantReg = ds.Tables[0].Rows.Count;
        }


        //int existeEmailUsuarioID(string) - verificar la existencia de EMail, caso que exista retorna id do usuario.
        //author: alejandro Fuentes - date: 18 / 11 / 2009
        public int existeEmailUsuarioID(string email)
        {
            comandoSQL = string.Format(@"  
                    SELECT 1, codigoUsuario
                        FROM {0}.{1}.Usuario
                    WHERE Email = '{2}'", bancodb, Ownerdb, email);
            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                return int.Parse(ds.Tables[0].Rows[0]["codigoUsuario"].ToString());
            }
            else
            {
                return 0;
            }
        }

        public bool atualizaLogoUnidade(int CodigoUnidadeNegocio, byte[] imagenLogo)
        {
            int registrosAfetados = 0;
            if (imagenLogo.Length != 0)
            {
                SqlCommand Command = new SqlCommand();
                SqlConnection Connection = new SqlConnection(strConn);

                Command.Connection = Connection;
                Command.CommandType = CommandType.Text;
                //Crio a sentencia UPDATE da tabela que conten a imagen do Logo
                Command.CommandText = string.Format(
                    @"UPDATE {0}.{1}.UnidadeNegocio SET LogoUnidadeNegocio = @ImagemLogo                                          
                   WHERE (CodigoUnidadeNegocio = {2});", bancodb, Ownerdb, CodigoUnidadeNegocio);

                Command.Parameters.Add(new SqlParameter("@ImagemLogo", SqlDbType.Image, 0, ParameterDirection.Input, 0, 0, "ConteudoDocumento", DataRowVersion.Current, false, null, "", "", ""));

                Command.Parameters[0].Value = ((byte[])(imagenLogo));

                Command.CommandTimeout = CDIS.ClasseDados.TimeOutSqlCommand;

                ConnectionState previousConnectionState = Command.Connection.State;

                if (previousConnectionState != ConnectionState.Open)
                {
                    Command.Connection.Open();
                }
                try
                {
                    registrosAfetados = Command.ExecuteNonQuery();
                }
                finally
                {
                    Command.Connection.Close();

                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool excluiEntidade(int CodigoUnidade, int idUsuarioLogado, ref int regAfetados, ref string mensagem)
        {
            try
            {
                comandoSQL = string.Format(@"
                DECLARE @RC int
                DECLARE @in_codigoUnidadeNegocio int
                DECLARE @in_codigoUsuarioExclusao int
                DECLARE @in_tipoUnidade char(1)

                SET @in_codigoUnidadeNegocio    = {3}
                SET @in_codigoUsuarioExclusao   = {2}
                SET @in_tipoUnidade             = 'E'

                EXECUTE @RC = {0}.{1}.[p_ExcluiUnidadeNegocio] 
                   @in_codigoUnidadeNegocio
                  ,@in_codigoUsuarioExclusao
                  ,@in_tipoUnidade", bancodb, Ownerdb, idUsuarioLogado, CodigoUnidade);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                mensagem = ex.Message;
                return false;
            }
        }

        #endregion

        #region Parametros do sistema

        public DataSet getParametrosSistema(params object[] parametros)
        {
            return getParametrosSistema(int.Parse(getInfoSistema("CodigoEntidade").ToString()), parametros);
        }

        public DataSet getListaTelasIniciais(int codigoUsuario, int codigoEntidade, string where)
        {
            string comandoSQL = string.Format(@"
	    SELECT [Iniciais], [NomeObjetoMenu_PT] 
        FROM {0}.{1}.f_GetPossiveisTelasIniciaisUsuario({2}, {3})
        WHERE 1=1
        ORDER BY 2
         {4} ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
            return getDataSet(comandoSQL);

        }

        public DataSet getLogoEntidade(int codigoEntidade, string where)
        {
            comandoSQL = string.Format(
               @"SELECT UN.CodigoUnidadeNegocio, 
                    UN.LogoUnidadeNegocio
               FROM {0}.{1}.UnidadeNegocio AS UN 
              WHERE UN.CodigoUnidadeNegocio = {2}
                {3}",
                    bancodb, Ownerdb, codigoEntidade, where);

            return getDataSet(comandoSQL);

        }

	    public DataSet getNomeEntidade(int codigoEntidade, string where)
	    {
	        comandoSQL = string.Format(
	           @"SELECT UN.CodigoUnidadeNegocio, 
	                    UN.NomeUnidadeNegocio
	               FROM {0}.{1}.UnidadeNegocio AS UN 
	              WHERE UN.CodigoUnidadeNegocio = {2}
	                {3}",
	                bancodb, Ownerdb, codigoEntidade, where);

	        return getDataSet(comandoSQL);

	    }
        public DataSet getLogoUnidade(int codigoUsuario, int codigoEntidade)
        {
            comandoSQL = string.Format(
               @"SELECT LogoUnidadeNegocio FROM {0}.{1}.f_getLogoUnidadeUsuario({2}, {3})",
                    bancodb, Ownerdb, codigoUsuario, codigoEntidade);

            return getDataSet(comandoSQL);

        }

        /// <summary>
        /// Devolve as URL's de acesso ao Brisk.
        /// </summary>
        /// <remarks>
        ///  Esta função devolve o link de acesso ao Brisk. O parâmetro de saída <paramref name="linkSistema"/> conterá o link padrão de acesso 
        ///  ao Brisk. Já o valro do parâmetro de saída <paramref name="URLAcessoItem"/> dependerá dos valores informados nos parâmetros conforme 
        ///  descrito em cada um.
        /// </remarks>
        /// <param name="CodigoEntidadeContexto">Código da entidade.</param>
        /// <param name="IndicaExigenciaURLBrisk1">O valor true para este parâmetro fará com que o parâmetro de saída <paramref name="URLAcessoItem"/> 
        ///  seja baseado forçosamente na URL de acesso ao Brisk1.</param>
        /// <param name="IndicaURLAcessoDireto">O valor true para este parâmetro fará com que o valor do parâmetro de saída <paramref name="URLAcessoItem"/> 
        ///  tenha, quando possível, uma URL de acesso direto ao item identificado nos valores passados nos parâmetros <paramref name="CodigoObjeto"/>
        ///  e os outros parâmetros relacionaods.</param>
        /// <param name= "CodigoObjeto">O valor passado neste parâmetro só será considerado se tiver true para o parâmetro <paramref name="IndicaURLAcessoDireto"/>.
        ///  Neste caso, deverá ser informado neste parâmetro um código que, em conjunto com os parâmetros <paramref name="CodigoTipoObjeto"/> ou 
        ///   < paramref name= "IniciaisTipoObjeto"/> identifique um objeto do sistema</param>
        /// <param name="CodigoTipoObjeto">Código do tipo do objeto para o qual será devolvido a URL acesso direto ao item. Pode ser nulo se 
        ///  o parâmetro <paramref name="IniciaisTipoObjeto"> for informado.O valor passado neste parâmetro também só será considerado se tiver
        ///  sido informado true para o parâmetro <paramref name="IndicaURLAcessoDireto"/>.</param>
        /// <param name="IniciaisTipoObjeto">Iniciais do tipo do objeto.Este parâmetro é ignorado se o parâmetro<paramref name="CodigoTipoObjeto"/>
        ///- tiver sido informado ou se tiver sido informado false para o parâmetro<paramref name="IndicaURLAcessoDireto"/>.</param>
        /// <param name="CodigoObjetoPai">O código do objeto pai do objeto informado no parâmetro <paramref name="CodigoObjeto"/>.
        ///  O valor passado neste parâmetro também só será considerado se tiver sido informado true para o parâmetro <paramref name="IndicaURLAcessoDireto"/>.</ param >
        ///  <param name="linkSistema">Parâmetro de saída que conterá o link de acesso ao sistema.</param>
        ///  <param name="URLAcessoItem">Parâmetro de saída que conterá a URL para acesso direto ao item caso os parâmetros associados a esta situação tenham sido informados.</param>
        public void getLinksAcessoSistema(int codigoEntidade, bool URLBrisk1, bool URLAcessoDireto, long? codigoObjetoAssociado, short? codigoTipoObjeto, string iniciaisTipoObjeto, long? codigoObjetoPai, out string linkSistema, out string URLAcessoItem)
        {
            linkSistema = URLAcessoItem = "";

            comandoSQL = string.Format(
               @"SELECT f.[LinkAcessoSistema], f.[UrlAcessoItem] FROM {0}.{1}.f_GetURLsAcessoSistema({2}, {3}, {4}, {5}, {6}, {7}, {8}) AS [f];",
                    bancodb, Ownerdb, codigoEntidade, URLBrisk1 ? 1 : 0, URLAcessoDireto ? 1 : 0, codigoObjetoAssociado.HasValue ? codigoObjetoAssociado.ToString() : "NULL"
                    , codigoTipoObjeto.HasValue ? codigoTipoObjeto.ToString() : "NULL", string.IsNullOrEmpty(iniciaisTipoObjeto) ? "NULL" : string.Format("'{0}'", iniciaisTipoObjeto)
                    , codigoObjetoPai.HasValue ? codigoObjetoPai.ToString() : "NULL");

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                linkSistema = ds.Tables[0].Rows[0]["LinkAcessoSistema"].ToString();
                URLAcessoItem = ds.Tables[0].Rows[0]["URLAcessoItem"].ToString();
            }
        }
        public DataSet getParametrosSistema(int codigoEntidade, params object[] parametros)
        {
            string comandoSQL = "";
            DataSet ds;

            if (codigoEntidade == -1)
            {
                comandoSQL = string.Format(@"DECLARE @codigoEntidadeMaster            Int

                                          --- identifica a entidade superior da instalação. Esta será a entidade em que os novos usuários serão incluídos
                                          SELECT TOP 1 @codigoEntidadeMaster = un.[CodigoUnidadeNegocio]
                                                FROM {0}.{1}.[UnidadeNegocio]        AS [un] 
                                                WHERE un.[DataExclusao] IS NULL AND un.[IndicaUnidadeNegocioAtiva] = 'S' 
                                                      AND un.[CodigoUnidadeNegocioSuperior] IS NULL AND un.[SiglaUF] = 'BR'
                                                ORDER BY un.[CodigoUnidadeNegocio];
                                                
                                          IF (@codigoEntidadeMaster IS NULL) BEGIN
                                                SELECT TOP 1 @codigoEntidadeMaster = un.[CodigoUnidadeNegocio]
                                                      FROM {0}.{1}.[UnidadeNegocio]  AS [un] 
                                                      WHERE un.[DataExclusao] IS NULL AND un.[IndicaUnidadeNegocioAtiva] = 'S' 
                                                            AND un.[CodigoUnidadeNegocioSuperior] IS NULL 
                                                      ORDER BY un.[CodigoUnidadeNegocio];
                                        
                                          END 
                                          SELECT @codigoEntidadeMaster AS codigoEntidade", bancodb, Ownerdb);


                DataSet dsEntidade = getDataSet(comandoSQL);

                if (DataSetOk(dsEntidade) && DataTableOk(dsEntidade.Tables[0]))
                    codigoEntidade = int.Parse(dsEntidade.Tables[0].Rows[0]["codigoEntidade"].ToString());

                comandoSQL = "";
            }

            // retorna todos os parametros
            if (parametros[0].ToString() == "-1")
            {
                // busca a lista de todos os parametros disponiveis na tabela parametrosSistema
                string comandoInterno = string.Format(
                    @"select Parametro FROM {0}.{1}.ParametroConfiguracaoSistema WHERE CodigoEntidade = {2}", bancodb, Ownerdb, codigoEntidade);

                ds = getDataSet(comandoInterno);
                if (ds != null)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt != null)
                    {
                        // monta o comandoSQL para retornar todos os parametros
                        foreach (DataRow row in dt.Rows)
                        {
                            comandoSQL += montaComandoGetParametro(codigoEntidade, row["Parametro"].ToString());
                        }
                    }
                }
            }
            else // retorna apenas os parametros indicados pelo usuário
            {
                foreach (object parametro in parametros)
                {
                    comandoSQL += montaComandoGetParametro(codigoEntidade, parametro.ToString());
                }
            }

            if (comandoSQL != "")
            {
                comandoSQL = "SELECT " + comandoSQL.Remove(comandoSQL.Length - 2);
            }

            ds = getDataSet(comandoSQL);
            return ds;

        }

        public DataSet getParametrosUsuario(int codigoUsuario, params object[] parametros)
        {
            string comandoSQL = "";
            DataSet ds;

            // retorna todos os parametros
            if (parametros[0].ToString() == "-1")
            {
                // busca a lista de todos os parametros disponiveis na tabela parametrosSistema
                string comandoInterno = string.Format(@"SELECT Parametro FROM {0}.{1}.TipoParametroUsuario", bancodb, Ownerdb);

                ds = getDataSet(comandoInterno);
                if (ds != null)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt != null)
                    {
                        // monta o comandoSQL para retornar todos os parametros
                        foreach (DataRow row in dt.Rows)
                        {
                            comandoSQL += montaComandoGetParametroUsuario(codigoUsuario, row["Parametro"].ToString());
                        }
                    }
                }
            }
            else // retorna apenas os parametros indicados pelo usuário
            {
                foreach (object parametro in parametros)
                {
                    comandoSQL += montaComandoGetParametroUsuario(codigoUsuario, parametro.ToString());
                }
            }

            if (comandoSQL != "")
            {
                comandoSQL = "SELECT " + comandoSQL.Remove(comandoSQL.Length - 2);
            }

            ds = getDataSet(comandoSQL);
            return ds;

        }


        public DataSet getParametrosIndicadores(string where)
        {
            string comandoSQL = string.Format(@"
              SELECT ParametroIndicadores.CodigoParametro, ParametroIndicadores.TipoIndicador, TipoStatusAnalise.TipoStatus, ParametroIndicadores.ValorInicial, 
                     ParametroIndicadores.ValorFinal
                FROM {0}.{1}.ParametroIndicadores AS ParametroIndicadores INNER JOIN
                     {0}.{1}.TipoStatusAnalise AS TipoStatusAnalise ON ParametroIndicadores.CodigoTipoStatus = TipoStatusAnalise.CodigoTipoStatus
              WHERE 1=1 {2}
              ORDER BY ParametroIndicadores.TipoIndicador, TipoStatusAnalise.TipoStatus", bancodb, Ownerdb, where);
            return getDataSet(comandoSQL);

        }

        public DataSet getConfiguracoesEntidade(int codigoEntidade, string where, bool indicaControladoSistema = false)
        {
            string str_IndicaControladoSistema = (indicaControladoSistema == false) ? " AND IndicaControladoSistema = 'N' " : "";
            string comandoSQL = string.Format(@"
                SELECT CodigoParametro
                      ,Parametro
                      ,Valor
                      ,CASE WHEN TipoDadoParametro = 'BOL' THEN CASE WHEN Valor = 'S' THEN '{4}' ELSE '{5}' END
                            WHEN TipoDadoParametro = 'LOG' THEN CASE WHEN Valor = '1' THEN '{4}' ELSE '{5}' END
                        ELSE Valor END AS DescricaoValor
                      ,DescricaoParametro_PT
                      ,DescricaoParametro_EN
                      ,DescricaoParametro_ES
                      ,TipoDadoParametro
                      ,ValorMinimo
                      ,ValorMaximo
                      ,CodigoConjuntoOpcaoParametro
                      ,GrupoParametro
                      ,IndicaControladoSistema
                  FROM {0}.{1}.ParametroConfiguracaoSistema
                 WHERE CodigoEntidade = {2} {6}                    
                   {3}", bancodb, Ownerdb, codigoEntidade, where, Resources.traducao.sim, Resources.traducao.nao, str_IndicaControladoSistema);
            return getDataSet(comandoSQL);

        }

        private string montaComandoGetParametro(int codigoEntidade, string nomeParametro)
        {
            return string.Format(
            @"(SELECT Valor 
                    FROM {0}.{1}.ParametroConfiguracaoSistema 
                   WHERE codigoEntidade = {3} AND Parametro = '{2}') AS {2}, ", bancodb, Ownerdb, nomeParametro, codigoEntidade);

        }
        private string montaComandoGetParametroUsuario(int codigoUsuario, string nomeParametro)
        {
            return string.Format(
                    @"(SELECT pu.[Valor]  
         FROM
          {0}.{1}.[ParametroUsuario]     AS [pu]

          INNER JOIN {0}.{1}.[TipoParametroUsuario]  AS [tpu]  ON 
           (       tpu.[CodigoTipoParametro] = pu.[CodigoTipoParametro] 
                  AND tpu.[Parametro]    = '{2}'       )   
         WHERE
          pu.[CodigoUsuario] = {3}) AS {2}, ", bancodb, Ownerdb, nomeParametro, codigoUsuario);

        }

        public DataSet f_getProjetosUsuario(int codigoUsuario, int codigoEntidade, int codigoCarteira)
        {
            comandoSQL = string.Format(
               @"SELECT CodigoProjeto FROM {0}.{1}.f_GetProjetosUsuario({2}, {3}, {4})",
                    bancodb, Ownerdb, codigoUsuario, codigoEntidade, codigoCarteira);

            return getDataSet(comandoSQL);

        }

        public string getTextoPadraoEntidade(int codigoEntidade, string iniciaisTexto)
        {
            string comandoSQL = string.Format(
                          @"SELECT TOP 1 tps.Texto 
                          FROM {0}.{1}.TextoPadraoSistema AS tps 
                         WHERE tps.IniciaisTexto = '{3}' 
                           AND tps.CodigoEntidade IN (0, {2})  -- pegando o texto da entidade em questão ou para todas as entidades (0)
                         ORDER BY tps.CodigoEntidade DESC
               ", bancodb, Ownerdb, codigoEntidade, iniciaisTexto);

            DataSet ds = getDataSet(comandoSQL);

            string textoPadrao = "";

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                textoPadrao = ds.Tables[0].Rows[0]["Texto"].ToString();

            return textoPadrao;
        }

        #endregion

        #region Anexos do projeto

        public DataSet getAnexoProjeto(int codEntidade, int codigoPastaSuperior, string where, string indicaPasta)
        {
            DataSet ds;
            string wherePasta = "";

            //SE NÃO FOR ESPECIFICADO UMA PASTA, É NECESSÁRIO LISTAR TODAS AS PASTAS E DOCUMENTOS
            // da RAIZ
            if (codigoPastaSuperior == -1)
            {
                wherePasta = string.Format(@" and A.CodigoAnexo  IN (SELECT CodigoAnexo  FROM {0}.{1}.[Anexo]  
                                                             WHERE ([CodigoPastaSuperior]=[CodigoAnexo] or [CodigoPastaSuperior]=-1))", bancodb, Ownerdb);

            }
            else
                wherePasta = string.Format(@" and CodigoPastaSuperior = " + codigoPastaSuperior.ToString() + " and A.[CodigoAnexo]!=A.[CodigoPastaSuperior]");
            string comandoSQL = string.Format(@"
        SELECT A.[CodigoAnexo]
              ,A.[Nome]             
              ,A.[DataInclusao]
              ,A.[CodigoUsuarioInclusao]             
              ,A.[CodigoEntidade]
              ,IsNull(A.[CodigoPastaSuperior],A.[CodigoAnexo]) AS CodigoPastaSuperior
              ,A.[IndicaPasta]
              ,A.[IndicaControladoSistema]
              ,A.[DescricaoAnexo]
              ,u.[NomeUsuario] as NomeUsuarioInclusao
        FROM {0}.{1}.[Anexo] A
        LEFT JOIN {0}.{1}.ConteudoAnexo CA ON (CA.CodigoAnexo = A.CodigoAnexo)
        INNER JOIN {0}.{1}.Usuario u ON (u.CodigoUsuario = A.CodigoUsuarioInclusao)
        WHERE A.CodigoEntidade = {2}
       {3}
     
        AND A.CodigoAnexo NOT IN
        (SELECT [CodigoAnexo]
           FROM {0}.{1}.[AnexoAssociacao]) {4} 
      ORDER BY A.IndicaPasta DESC, A.Nome

", bancodb, Ownerdb, codEntidade, wherePasta, where);
            ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getAnexo(int codigoAnexo)
        {
            DataSet ds;

            string comandoSQL = string.Format(@"
        SELECT A.[CodigoAnexo]
              ,A.[Nome]             
              ,A.[DataInclusao]
              ,A.[CodigoUsuarioInclusao]             
              ,A.[CodigoEntidade]
              ,IsNull(A.[CodigoPastaSuperior],A.[CodigoAnexo]) AS CodigoPastaSuperior
              ,A.[IndicaPasta]
              ,A.[IndicaControladoSistema]
              ,A.[DescricaoAnexo]
        FROM {0}.{1}.[Anexo] A
        WHERE A.CodigoAnexo = {2}

", bancodb, Ownerdb, codigoAnexo);
            ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getConteudoAnexoProjeto(int codigoAnexo, string where)
        {
            string comandoSQL = string.Format(@"
        SELECT ca.[CodigoAnexo],ca.[Anexo],A.[Nome],A.DescricaoAnexo
         FROM {0}.{1}.[Anexo] A  
        left join {0}.{1}.[ConteudoAnexo] as ca on (ca.[CodigoAnexo] = A.[CodigoAnexo])
        where ca.[CodigoAnexo] = {2}", bancodb, Ownerdb, codigoAnexo);
            return getDataSet(comandoSQL);

        }

        public bool incluiAnexoSistema(char indicaPasta, System.Nullable<int> codigoPastaSuperior, string nome, string descricao, System.Nullable<int> idRespAnexacao, string nomeRespAnexacao, byte[] arquivo, int codigoEntidade)
        {

            try
            {

                if (idRespAnexacao == null && nomeRespAnexacao == "")
                {
                    throw new Exception("O responsável pela operação não foi informado." + Delimitador_Erro);
                }

                if (nome == null || nome == "")
                {
                    throw new Exception("O nome do arquivo/pasta não foi informado." + Delimitador_Erro);
                }

                if (!nomeValido(nome))
                {
                    throw new Exception("O nome informado não é válido." + Delimitador_Erro);
                }

                if (indicaPasta != 'S' && indicaPasta != 'N')
                {
                    throw new Exception("A indicação (IndicaPasta) do tipo de objeto a ser inserido não é válida. Utilize 'S' ou 'N'." + Delimitador_Erro);
                }

                // busca o ID do responsável pelo lancamento

                // Verifica se o nome do arquivo ou pasta já existe
                int registrosAfetados = 0;
                string where = "";
                if (codigoPastaSuperior != null)
                    where = " AND CodigoPastaSuperior = " + codigoPastaSuperior.ToString();

                string ComandoSQL = string.Format(
                    @"SELECT 1 
                            FROM {0}.{1}.Anexo 
                           WHERE Nome = '{2}' {3}", bancodb, Ownerdb, nome.Replace("'", "''").Replace("*", "").Replace("|", "").Replace("\\", "").Replace(":", "").Replace("\"", "").Replace("&lt;", "").Replace("&gt;", "").Replace("?", "").Replace("/", "")
                         , where);
                DataSet ds = getDataSet(ComandoSQL);
                string mensagem = "Já existe uma pasta com o nome informado." + Delimitador_Erro;
                if (indicaPasta != 'S')
                    mensagem = "Já existe um arquivo com o nome informado." + Delimitador_Erro;
                if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                {
                    throw new Exception(mensagem);
                }

                ds = null;

                // insere o registro (pasta ou arquivo)
                ComandoSQL = string.Format(
                    @"BEGIN
                    declare @novoCodigo as bigint                    
                    DECLARE @indicaPasta as char(1)
                    SET @indicaPasta = '{2}'
                    INSERT INTO {0}.{1}.Anexo 
                                      ([IndicaPasta], [CodigoPastaSuperior], [Nome], [DescricaoAnexo], [DataInclusao],[CodigoUsuarioInclusao],[CodigoEntidade]) 
                               VALUES ('{2}',         {3},                   '{4}',  '{5}'           ,getdate()      ,{6}                    ,{7});
                    SELECT @novoCodigo= scope_identity();
                    SELECT @novoCodigo;
                    INSERT INTO {0}.{1}.[AcessoAnexo]
                                      ([CodigoAnexo] ,[CodigoUsuario], [TipoPermissao])
                                VALUES(@novoCodigo    ,{6}            ,'P')
                    if(@indicaPasta = 'S' and {3} = -1 )
				        BEGIN
							UPDATE {0}.{1}.[Anexo]
							 SET [CodigoPastaSuperior] = @novoCodigo
						 WHERE [CodigoAnexo] = @novoCodigo
				        END 
                   END 
            ", bancodb, Ownerdb, indicaPasta.ToString(), (codigoPastaSuperior == null) ? "-1" : codigoPastaSuperior.ToString(), nome.Replace("'", "''").Replace("*", "").Replace("|", "").Replace("\\", "").Replace(":", "").Replace("\"", "").Replace("&lt;", "").Replace("&gt;", "").Replace("?", "").Replace("/", "")
                         , descricao.Replace("'", "''"), idRespAnexacao, codigoEntidade);
                ds = getDataSet(ComandoSQL);
                int CodigoAnexo = int.Parse(ds.Tables[0].Rows[0][0].ToString());

                // se for arquivo, vamos inserir a imagem
                if (indicaPasta == 'N')
                {
                    SqlCommand Command = new SqlCommand();
                    SqlConnection Connection = new SqlConnection(strConn);

                    Command.Connection = Connection;
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = string.Format(
                        @"INSERT INTO {0}.{1}.ConteudoAnexo 
                                          ([CodigoAnexo], [Anexo]) 
                                   VALUES (@CodigoAnexo, @Anexo)", bancodb, Ownerdb);
                    Command.Parameters.Add(new SqlParameter("@CodigoAnexo", SqlDbType.Int, 0, ParameterDirection.Input, 0, 0, "CodigoAnexo", DataRowVersion.Current, false, null, "", "", ""));
                    Command.Parameters.Add(new SqlParameter("@Anexo", SqlDbType.Image, 0, ParameterDirection.Input, 0, 0, "Anexo", DataRowVersion.Current, false, null, "", "", ""));

                    Command.Parameters[0].Value = ((int)(CodigoAnexo));
                    Command.Parameters[1].Value = ((byte[])(arquivo));

                    Command.CommandTimeout = CDIS.ClasseDados.TimeOutSqlCommand;

                    ConnectionState previousConnectionState = Command.Connection.State;
                    if (previousConnectionState != ConnectionState.Open)
                    {
                        Command.Connection.Open();
                    }
                    try
                    {
                        registrosAfetados = Command.ExecuteNonQuery();
                    }
                    finally
                    {
                        Command.Connection.Close();
                    }
                }
                return true;


            }
            catch (Exception ex)
            {
                throw new Exception(getErroIncluiRegistro(ex.Message));
            }

        }

        public bool excluiAnexoSistema(char indicaPasta, int codigoAnexo, int codUsuario, int codigoEntidade, int codigoPastaSuperior)
        {

            try
            {
                if (indicaPasta != 'S' && indicaPasta != 'N')
                {
                    throw new Exception("A indicação (IndicaPasta) do tipo de objeto a ser inserido não é válida. Utilize 'S' ou 'N'." + Delimitador_Erro);
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
                             AND IndicaControladoSistema = 'N'
                             AND CodigoPastaSuperior <> CodigoAnexo                              
                            ", bancodb, Ownerdb, codigoAnexo, codigoAnexo, codigoEntidade);
                    DataSet ds = getDataSet(comandoSQL);
                    if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                    {
                        throw new Exception("Só é possível excluir pastas que estejam vazias." + Delimitador_Erro);
                    }
                    else
                    {
                        //primeeiro exclui registro da tabela AcessoAnexo
                        comandoSQL = string.Format(
                            @"DELETE FROM {0}.{1}.[AcessoAnexo]
                      WHERE CodigoAnexo = {2} and CodigoUsuario = {3}", bancodb, Ownerdb, codigoAnexo, codUsuario);
                        execSQL(comandoSQL, ref registrosAfetados);

                        // Exclui a pasta da tabela Anexo
                        comandoSQL = string.Format(
                            @"DELETE FROM {0}.{1}.Anexo
                           WHERE CodigoAnexo = {2} and CodigoEntidade = {3}", bancodb, Ownerdb, codigoAnexo, codigoEntidade);
                        execSQL(comandoSQL, ref registrosAfetados);
                        return true;
                    }

                }
                else // arquivo
                {

                    //primeeiro exclui registro da tabela AcessoAnexo

                    comandoSQL = string.Format(
                        @"DELETE FROM {0}.{1}.[AcessoAnexo]
                      WHERE CodigoAnexo = {2} AND CodigoUsuario = {3}", bancodb, Ownerdb, codigoAnexo, codUsuario);
                    execSQL(comandoSQL, ref registrosAfetados);

                    comandoSQL = string.Format(
                        @"DELETE FROM {0}.{1}.ConteudoAnexo
                           WHERE CodigoAnexo = {2}", bancodb, Ownerdb, codigoAnexo);
                    execSQL(comandoSQL, ref registrosAfetados);

                    // Exclui o registro da tabela AnexoProjeto
                    comandoSQL = string.Format(
                        @"DELETE FROM {0}.{1}.Anexo
                           WHERE CodigoAnexo = {2} AND CodigoEntidade = {3}", bancodb, Ownerdb, codigoAnexo, codigoEntidade);
                    execSQL(comandoSQL, ref registrosAfetados);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(getErroExcluiRegistro(ex.Message));
            }


        }

        private bool nomeValido(string nome)
        {
            char[] invalidos = System.IO.Path.GetInvalidFileNameChars();
            return (nome.IndexOfAny(invalidos) == -1);
        }

        public void atualizaAnexoSistema(char indicaPasta, int codigo, string nome, string descricao, string indicaLink, string palavraChave, string indicaPublica)
        {

            try
            {
                if (indicaPasta != 'S' && indicaPasta != 'N')
                {
                    throw new Exception("A indicação (IndicaPasta) do tipo de objeto a ser inserido não é válida. Utilize 'S' ou 'N'." + Delimitador_Erro);
                }

                if ((nome == null || nome.Trim() == "") && (descricao == null || descricao == ""))
                {
                    throw new Exception("O nome ou a descrição deve ser informado." + Delimitador_Erro);
                }

                if (nome != null && nome != "")
                {
                    if (!nomeValido(nome))
                    {
                        throw new Exception("O nome informado não é válido." + Delimitador_Erro);
                    }
                }

                if (nome.IndexOf('\'') >= 0 || descricao.IndexOf('\'') >= 0)
                {
                    throw new Exception("O caracter \"\'\" não pode ser utilizado para o nome ou descrição." + Delimitador_Erro);
                }



                // Verifica se o novo nome do arquivo ou pasta já existe
                int registrosAfetados = 0;
                comandoSQL = string.Format(
                    @"SELECT 1 
                            FROM {0}.{1}.Anexo
                           WHERE  CodigoAnexo <> {3}", bancodb, Ownerdb, nome.Replace("'", "''").Replace("*", "").Replace("|", "").Replace("\\", "").Replace(":", "").Replace("\"", "").Replace("&lt;", "").Replace("&gt;", "").Replace("?", "").Replace("/", "")
                               , codigo);
                DataSet ds = getDataSet(comandoSQL);
                string mensagem = "Já existe uma pasta com o nome informado." + Delimitador_Erro;
                if (indicaPasta != 'S')
                    mensagem = "Já existe um arquivo com o nome informado." + Delimitador_Erro;
                if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                {
                    //throw new Exception(mensagem);
                }
                // altera as informações 
                string set = "";
                set += " nome = '" + nome.Replace("'", "''").Replace("*", "").Replace("|", "").Replace("\\", "").Replace(":", "").Replace("\"", "").Replace("&lt;", "").Replace("&gt;", "").Replace("?", "").Replace("/", "")
                    + "', DescricaoAnexo = '" + descricao.Replace("'", "''") + "', PalavraChave = '" + palavraChave + "', IndicaAnexoPublicoExterno = '" + indicaPublica + "', IndicaLink = " + indicaLink;
                //set = set.Substring(2);

                comandoSQL = string.Format(
                    @"UPDATE {0}.{1}.Anexo
                         SET {3}
                       WHERE CodigoAnexo = {2} ", bancodb, Ownerdb, codigo, set);

                execSQL(comandoSQL, ref registrosAfetados);


            }
            catch (Exception ex)
            {
                throw new Exception(getErroAtualizaRegistro(ex.Message));
            }

        }
        #endregion

        #region avisos

        public bool atualizaAviso(string assunto, string aviso, string dataInicio, string dataTermino, int codigoEntidade, string tipoDestinatario, int[] codigoDestinatario, int codigoAviso, ref string msgErro)
        {
            int regAfetados = 0;
            string excluiAvisos = "";
            string incluiAvisosAtualizados = "";
            //DELETA  todos os destinatarios do aviso a ser atualizado
            excluiAvisos = string.Format(
            @"DELETE FROM {0}.{1}.[AvisoDestinatario] 
                    WHERE (CodigoAviso = {3})"
            , bancodb, Ownerdb, tipoDestinatario, codigoAviso);
            if (tipoDestinatario == "TD")
            {
                incluiAvisosAtualizados = string.Format(
                    @"INSERT INTO {0}.{1}.AvisoDestinatario(CodigoAviso,TipoDestinatario,CodigoDestinatario)
                                             SELECT @novoCodigo,           '{2}', CodigoUsuario from {0}.{1}.Usuario
                 ", bancodb, Ownerdb, tipoDestinatario);
            }
            else
            {
                //inclui a nova lista de destinatarios
                for (int i = 0; i < codigoDestinatario.Length; i++)
                {
                    incluiAvisosAtualizados += string.Format(
                    @"INSERT INTO {0}.{1}.[AvisoDestinatario] ([CodigoAviso],[TipoDestinatario],[CodigoDestinatario])
                                                VALUES({4}, '{2}' ,{3})", bancodb, Ownerdb, tipoDestinatario, codigoDestinatario[i], codigoAviso);
                }
            }
            string atualizaAviso = string.Format(
            @"BEGIN
            DECLARE @novoCodigo as int          
                UPDATE {0}.{1}.[Aviso]
                    SET [Assunto] = '{2}'
                        ,[Aviso] = '{3}'
                        ,[DataInicio] = convert(datetime,'{4}',103)
                        ,[DataTermino] = convert(datetime,'{5}',103)
                        ,[DataInclusao] = getdate()
                        ,[CodigoEntidade] = {6}
                    WHERE CodigoAviso = {7}
                
                SET @novoCodigo = {7}
                {8} --DELETA  todos os destinatarios do aviso a ser atualizado
                {9} --inclui a nova lista de destinatarios
            END"
            , bancodb, Ownerdb, assunto, aviso, dataInicio, dataTermino, codigoEntidade, codigoAviso, excluiAvisos, incluiAvisosAtualizados);
            try
            {
                classeDados.execSQL(atualizaAviso, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msgErro = ex.Message;
                return false;
            }
        }

        #endregion

        #region Fluxo Caixa

        public DataSet getFluxoCaixa(int codEntidade, int codProjeto)
        {
            DataSet ds;
            string comandoSQL = string.Format(@"EXEC {0}.{1}.p_GetFluxoCaixaProjeto {2}, {3} 
            ", bancodb, Ownerdb, codEntidade, codProjeto);
            ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getContasAnaliticasFluxoCaixaEntidade(int codigoEntidade, string where)
        {
            DataSet ds;
            string comandoSQL = string.Format(@"SELECT DISTINCT CodigoConta, CASE WHEN EntradaSaida = 'E' THEN 'Receitas' ELSE 'Despesas' END AS EntradaSaida
                                                           ,CodigoReservadoGrupoConta
                                              FROM {0}.{1}.PlanoContasFluxoCaixa
                                             WHERE CodigoEntidade = {2}
                                               AND IndicaContaAnalitica = 'S'
                                               AND CodigoReservadoGrupoConta IS NOT NULL
                                               {3}
            ", bancodb, Ownerdb, codigoEntidade, where);
            ds = getDataSet(comandoSQL);
            return ds;
        }



        public bool atualizaFluxoCaixa(int codigoProjeto, DataRow[] drs, string[] valores, int codigoPrevisao, ref string msg)
        {
            int regAfetados = 0;
            try
            {
                int i = 0;

                comandoSQL = "";

                foreach (DataRow dr in drs)
                {
                    comandoSQL += string.Format(@"                                             
                                         IF EXISTS(SELECT 1 FROM {0}.{1}.FluxoCaixaProjeto WHERE CodigoProjeto = {3}
                                                                                           AND CodigoConta = {4}
                                                                                           AND Ano = {5}
                                                                                           AND Mes = {6}
                                                                                           AND CodigoPrevisao = {7})
                                            BEGIN
                                                 UPDATE {0}.{1}.FluxoCaixaProjeto SET ValorPrevisto = {2}
                                                  WHERE CodigoProjeto = {3}
                                                    AND CodigoConta = {4}
                                                    AND Ano = {5}
                                                    AND Mes = {6}
                                                    AND CodigoPrevisao = {7}
                                            END
                                        ELSE
                                            BEGIN
                                                 INSERT INTO {0}.{1}.FluxoCaixaProjeto(CodigoProjeto, CodigoConta, Ano, Mes, ValorPrevisto, CodigoPrevisao) VALUES
                                                                              ({3}, {4}, {5}, {6}, {2}, {7})
                                                END
                                        "
                        , bancodb, Ownerdb, valores[i] == "" ? "NULL" : valores[i].ToString().Replace(",", "."), codigoProjeto, dr["_CodigoConta"], dr["_Ano"], dr["_Mes"], codigoPrevisao);

                    i++;
                }

                if (comandoSQL != "")
                    execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }


        public DataSet getPrevisoesOrcamentarias(int codigoEntidade, string where)
        {

            comandoSQL = string.Format(@"
            SELECT CodigoPrevisao, DescricaoPrevisao, Observacao, IndicaPrevisaoOficial
              FROM {0}.{1}.PrevisaoFluxoCaixaProjeto
             WHERE CodigoEntidade = {2}
               {3}", bancodb, Ownerdb, codigoEntidade, where);

            return getDataSet(comandoSQL);
        }


        public bool atualizaPrevisaoOrcamentaria(int codigoPrevisao, string descricao, string observacoes, int codigoUsuario)
        {
            bool retorno = false;
            try
            {
                comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoPrevisao int

                            SET @CodigoPrevisao = {2}
                            
                            UPDATE {0}.{1}.PrevisaoFluxoCaixaProjeto SET DescricaoPrevisao = '{3}'
                                                                        ,Observacao = '{4}'
                                                                        ,DataUltimaAlteracao = GetDate()
                                                                        ,CodigoUsuarioUltimaAlteracao = {5}
                             WHERE CodigoPrevisao = @CodigoPrevisao 
                        END
                        ", bancodb, Ownerdb
                             , codigoPrevisao
                             , descricao.Replace("'", "''")
                             , observacoes.Replace("'", "''")
                             , codigoUsuario);

                int regAf = 0;

                execSQL(comandoSQL, ref regAf);

                retorno = true;
            }
            catch
            {
                retorno = false;
            }
            return retorno;
        }

        public DataSet getDestinatariosMensagens(int codigoMensagem, string where)
        {
            DataSet ds;
            string comandoSQL = string.Format(@"
            SELECT u.NomeUsuario, u.CodigoUsuario, u.EMail, 'N' AS IndicaResponsavel  
              FROM {0}.{1}.MensagemDestinatario INNER JOIN
			       {0}.{1}.Usuario u ON u.CodigoUsuario = CodigoDestinatario
             WHERE CodigoMensagem = {2}
               AND u.DataExclusao IS NULL
               {3}
            UNION 
            SELECT u.NomeUsuario, u.CodigoUsuario, u.EMail, 'S' AS IndicaResponsavel
              FROM {0}.{1}.Mensagem m INNER JOIN
			       {0}.{1}.Usuario u ON u.CodigoUsuario = m.CodigoUsuarioInclusao
             WHERE CodigoMensagem = {2}
               AND u.DataExclusao IS NULL
               {3}
        ", bancodb, Ownerdb, codigoMensagem, where);

            ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getItensPainelMensagensUsuario(int codigoUsuarioLogado, string indicaEntradaSaida, string where)
        {
            string comandoSQL = "";

            if (indicaEntradaSaida == "E")
            {
                comandoSQL = string.Format(@"
            BEGIN
	            DECLARE @CodigoUsuario int,
			            @codigoTipoAssociacao Int

                SELECT  @codigoTipoAssociacao =  CodigoTipoAssociacao 
                FROM    {0}.{1}.TipoAssociacao 
                WHERE   IniciaisTipoAssociacao = 'MG'
            			
	            SET @CodigoUsuario = {2}

	            SELECT m.CodigoMensagem, m.Mensagem, m.DataInclusao, m.DataLimiteResposta, u.NomeUsuario, cm.IndicaTipoMensagem
                     , m.Assunto, m.Mensagem, CASE WHEN md.DataLeitura IS NULL THEN 'N' ELSE 'S' END AS Lida, m.Prioridade
                     , cat.DescricaoCategoria, m.CodigoObjetoAssociado, ta.IniciaisTipoAssociacao, m.CodigoCategoria
                     , {0}.{1}.f_GetDescricaoOrigemAssociacaoObjeto(m.CodigoEntidade, m.CodigoTipoAssociacao,null, m.CodigoObjetoAssociado,0,null) AS NomeObjeto
                     , CASE WHEN ta.IniciaisTipoAssociacao = 'DC' OR ta.IniciaisTipoAssociacao = 'DS' OR ta.IniciaisTipoAssociacao = 'PC' THEN 'PR' ELSE ta.IniciaisTipoAssociacao END AS TipoAssociacao
	              FROM {0}.{1}.Mensagem m INNER JOIN
		               {0}.{1}.MensagemDestinatario md ON md.CodigoMensagem = m.CodigoMensagem INNER JOIN
		               {0}.{1}.Usuario u ON u.CodigoUsuario = m.CodigoUsuarioInclusao INNER JOIN
	                   {0}.{1}.CaixaMensagem cm ON (cm.CodigoObjetoAssociado = m.CodigoMensagem AND cm.CodigoTipoAssociacao = @codigoTipoAssociacao) INNER JOIN
	                   {0}.{1}.TipoAssociacao ta ON ta.CodigoTipoAssociacao = m.CodigoTipoAssociacao LEFT JOIN
	                   {0}.{1}.CategoriaMensagem cat ON cat.CodigoCategoria = m.CodigoCategoria
	             WHERE (md.CodigoDestinatario = @CodigoUsuario OR md.CodigoDestinatario = -1)
                   AND cm.[CodigoUsuario]	= md.[CodigoDestinatario]
                   AND {0}.{1}.f_verificaObjetoMonitoravel(m.[CodigoMensagem], NULL, 'MG', 0, m.[CodigoEntidade], m.[CodigoEntidade], NULL, 'EN', 0) = 1
                   {3}  
            END
            ", bancodb, Ownerdb, codigoUsuarioLogado, where);
            }
            else
            {
                comandoSQL = string.Format(@"
            BEGIN
	            DECLARE @CodigoUsuario int,
			            @codigoTipoAssociacao Int

                SELECT  @codigoTipoAssociacao =  CodigoTipoAssociacao 
                FROM    {0}.{1}.TipoAssociacao 
                WHERE   IniciaisTipoAssociacao = 'MG'
            			
	            SET @CodigoUsuario = {2}

	            SELECT m.CodigoMensagem, m.Mensagem, m.DataInclusao, m.DataLimiteResposta, u.NomeUsuario, cm.IndicaTipoMensagem
                     , m.Assunto, m.Mensagem, 
                       ISNULL((SELECT CASE WHEN md.DataLeitura IS NULL THEN 'N' ELSE 'S' END 
                                 FROM {0}.{1}.MensagemDestinatario md 
                                WHERE md.CodigoMensagem = m.CodigoMensagem
                                  AND md.CodigoDestinatario = {2}), 'S') AS Lida, m.Prioridade
                     , ta.IniciaisTipoAssociacao, cat.DescricaoCategoria, m.CodigoObjetoAssociado, m.CodigoCategoria
                     , {0}.{1}.f_GetDescricaoOrigemAssociacaoObjeto(m.CodigoEntidade, m.CodigoTipoAssociacao,null, m.CodigoObjetoAssociado,0,null) AS NomeObjeto
                     , CASE WHEN ta.IniciaisTipoAssociacao = 'DC' OR ta.IniciaisTipoAssociacao = 'DS' OR ta.IniciaisTipoAssociacao = 'PC' THEN 'PR' ELSE ta.IniciaisTipoAssociacao END AS TipoAssociacao
	              FROM {0}.{1}.Mensagem m INNER JOIN 
		               {0}.{1}.Usuario u ON u.CodigoUsuario = m.CodigoUsuarioInclusao INNER JOIN
	                   {0}.{1}.CaixaMensagem cm ON (cm.CodigoObjetoAssociado = m.CodigoMensagem AND cm.CodigoTipoAssociacao = @codigoTipoAssociacao) INNER JOIN
	                   {0}.{1}.TipoAssociacao ta ON ta.CodigoTipoAssociacao = m.CodigoTipoAssociacao LEFT JOIN
	                   {0}.{1}.CategoriaMensagem cat ON cat.CodigoCategoria = m.CodigoCategoria
	             WHERE m.CodigoUsuarioInclusao = @CodigoUsuario  
                   {3}       	
            END
            ", bancodb, Ownerdb, codigoUsuarioLogado, where);
            }


            return getDataSet(comandoSQL);
        }



        public bool incluiMensagem(int codigoEntidade, int idProjeto, int codigoUsuarioInclusao, string assunto, string mensagem, DateTime dataLimiteResposta, bool respostaNecessaria, string prioridade, int categoria, int[] listaUsuariosSelecionados, string iniciaisObjeto, ref string mensagemErro)
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
                    , [CodigoCategoria]
                )
            VALUES
                ( '{2}', GETDATE(), '{3}' , {4}, {5} , {6} , @CodigoTipoAssociacao, {7}, {10}, '{11}', {12} )
         
             SELECT @CodigoMensagem = SCOPE_IDENTITY()
 
           {8}
        END
        ", bancodb, Ownerdb, mensagem.Replace("'", "''"), (respostaNecessaria) ? "S" : "N", codigoUsuarioInclusao, codigoEntidade
             , idProjeto, dateAsString, insereDestinatarios, iniciaisObjeto, prioridade == "" ? "''" : prioridade, assunto.Replace("'", "''"), categoria == -1 ? "NULL" : categoria.ToString());
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

        public bool incluiResposta(int codigoMensagem, int codigoUsuarioInclusao, string assunto, string mensagem, string prioridade, int categoria, string[] listaUsuariosSelecionados, ref string mensagemErro)
        {
            bool retorno = false;

            string insereDestinatarios = "";
            for (int j = 0; j < listaUsuariosSelecionados.Length; j++)
            {
                if (listaUsuariosSelecionados[j] != "")
                {
                    insereDestinatarios += string.Format(@"
                    INSERT INTO {0}.{1}.[MensagemDestinatario]
                        ([CodigoMensagem]
                        ,[CodigoDestinatario])
                    VALUES
                        (@CodigoMensagemNova, {2});"
                        , bancodb, Ownerdb, listaUsuariosSelecionados[j]);
                }
            }

            string comandoSQL = string.Format(@"
        BEGIN
            DECLARE @CodigoMensagemNova INT            

            INSERT INTO {0}.{1}.[Mensagem]
                (     [Mensagem]
                    , [DataInclusao]
                    , [IndicaRespostaNecessaria]
                    , [CodigoUsuarioInclusao]
                    , [CodigoEntidade]
                    , [CodigoObjetoAssociado]
                    , [CodigoTipoAssociacao]
                    , [Prioridade]
                    , [Assunto]
                    , [CodigoCategoria]
                )
            SELECT '{2}'
                        , GETDATE()
                        , [IndicaRespostaNecessaria]
                        , {3}
                        , [CodigoEntidade]
                        , [CodigoObjetoAssociado]
                        , [CodigoTipoAssociacao]
                        , {6}
                        , '{7}'
                        , {8}
                   FROM {0}.{1}.Mensagem
                  WHERE CodigoMensagem = {4}                    
         
            SELECT @CodigoMensagemNova = SCOPE_IDENTITY()
 
            {5}
        END
        ", bancodb, Ownerdb, mensagem.Replace("'", "''"), codigoUsuarioInclusao, codigoMensagem, insereDestinatarios, prioridade, assunto, categoria == -1 ? "NULL" : categoria.ToString());
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

        public bool atualizaCategoriaMensagem(int codigoMensagem, int codigoCategoria)
        {
            try
            {
                string comandoSQL = string.Format(@"
                                UPDATE {0}.{1}.Mensagem SET CodigoCategoria = {3}
                                 WHERE CodigoMensagem = {2}
                                ", bancodb, Ownerdb, codigoMensagem, codigoCategoria == -1 ? "NULL" : codigoCategoria.ToString());

                int registrosAfetados = 0;
                execSQL(comandoSQL, ref registrosAfetados);
                return true;

            }
            catch
            {
                return false;
            }
        }




        public bool atualizaPastaMensagensUsuario(int codigoUsuario, string nomePasta, string codigoPasta)
        {
            try
            {
                string comandoSQL = string.Format(@"
                                UPDATE {0}.{1}.PastaMensagemUsuario SET NomePasta = '{2}'
                                 WHERE CodigoPasta = {3}
                                   AND CodigoUsuario = {4}
                                ", bancodb, Ownerdb, nomePasta, codigoPasta, codigoUsuario);

                int registrosAfetados = 0;
                execSQL(comandoSQL, ref registrosAfetados);
                return true;

            }
            catch
            {
                return false;
            }
        }

        public bool excluiPastaMensagensUsuario(int codigoUsuario, string codigoPasta)
        {
            try
            {
                string comandoSQL = string.Format(@"
                                DELETE FROM {0}.{1}.PastaMensagemUsuario 
                                 WHERE CodigoPasta = {2}
                                   AND CodigoUsuario = {3}
                                ", bancodb, Ownerdb, codigoPasta, codigoUsuario);

                int registrosAfetados = 0;
                execSQL(comandoSQL, ref registrosAfetados);
                return true;

            }
            catch
            {
                return false;
            }
        }

        public bool atualizaMensagemLida(int codigoCaixaMensagem, int codigoObjetoMensagem, int codigoDestinatario)
        {
            try
            {
                string comandoSQL = string.Format(@"
                                UPDATE  {0}.{1}.CaixaMensagem
                                SET     DataPrimeiroAcessoMensagem = GETDATE()
                                WHERE   CodigoCaixaMensagem         = {2}
                                  AND   DataPrimeiroAcessoMensagem  IS NULL

                                UPDATE {0}.{1}.[MensagemDestinatario]
                                   SET   [DataLeitura] = GETDATE()
                                 WHERE   [CodigoMensagem]    = {3} 
                                   AND   CodigoDestinatario  = {4}
                                ", bancodb, Ownerdb, codigoCaixaMensagem
                                     , codigoObjetoMensagem, codigoDestinatario);

                int registrosAfetados = 0;
                execSQL(comandoSQL, ref registrosAfetados);
                return true;

            }
            catch
            {
                return false;
            }
        }

        public bool atualizaMensagem(int codigoObjetoMensagem, DateTime dataLimiteResposta, string mensagem)
        {
            try
            {
                string dateAsString = "";

                if (dataLimiteResposta != DateTime.MinValue)
                    dateAsString = "CONVERT(DATETIME, '" + dataLimiteResposta.Day + "/" + dataLimiteResposta.Month + "/" + dataLimiteResposta.Year + "', 103)";
                else
                    dateAsString = "NULL";

                string comandoSQL = string.Format(@" 
                                UPDATE {0}.{1}.[Mensagem]
                                   SET   [Mensagem] = '{2}',
                                         [DataLimiteResposta] = {4}
                                 WHERE   [CodigoMensagem]    = {3} 
                                ", bancodb, Ownerdb, mensagem.Replace("'", "''")
                                     , codigoObjetoMensagem
                                     , dateAsString);

                int registrosAfetados = 0;
                execSQL(comandoSQL, ref registrosAfetados);
                return true;

            }
            catch
            {
                return false;
            }
        }

        public bool atualizaRespostaMensagem(int codigoMensagem, int codigoUsuarioResposta, string resposta)
        {
            try
            {
                string comandoSQL = string.Format(@"UPDATE {0}.{1}.Mensagem
                                                   SET DataResposta = GETDATE()
                                                      ,Resposta = '{4}'
                                                      ,CodigoUsuarioResposta = {3}
                                                 WHERE CodigoMensagem = {2}

                                                UPDATE {0}.{1}.[MensagemDestinatario]
                                                   SET   [DataLeitura] = GETDATE()
                                                 WHERE   [CodigoMensagem]    = {2} 
                                                   AND   CodigoDestinatario  = {3}"
                                        , bancodb, Ownerdb, codigoMensagem, codigoUsuarioResposta, resposta.Replace("'", "''"));

                int registrosAfetados = 0;
                execSQL(comandoSQL, ref registrosAfetados);
                return true;

            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region RH

        public DataSet getRHProposta(int codEntidade, int codProjeto, string where)
        {
            DataSet ds;
            string comandoSQL = string.Format(@"EXEC {0}.{1}.[p_newproj_getValoresRH] {2}, {3};", bancodb, Ownerdb, codEntidade, codProjeto, where);
            ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getRecursosSelecaoBalanceamento(int codEntidade, string where, string whereGeral)
        {
            DataSet ds;
            string comandoSQL = string.Format(@"SELECT t.Codigo AS CodigoGrupoRecurso,
                       t.Grupo AS DescricaoGrupo
                 FROM
                (
                SELECT g.CodigoGrupoRecurso AS Codigo,
                       g.DescricaoGrupo AS Grupo,
                       p.CodigoProjeto,
                      Year(vi.Data) Ano,
                       Month(vi.Data) AS Mes
                  FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
                       {0}.{1}.vi_RecursoCorporativo AS rc ON (rc.CodigoGrupoRecurso = g.CodigoGrupoRecurso) INNER JOIN
                       {0}.{1}.vi_AtribuicaoDiariaRecurso AS vi ON (vi.CodigoRecursoMSProject = rc.CodigoRecursoMSProject) INNER JOIN
                       {0}.{1}.Projeto AS p ON (p.CodigoProjeto = vi.CodigoProjeto  {3}) 
                 WHERE p.CodigoProjeto IN (SELECT CodigoProjeto FROM {0}.{1}.f_GetProjetosSituacao({2},'Execucao'))
                   
                 GROUP BY g.DescricaoGrupo,
                          g.CodigoGrupoRecurso,
                          Year(vi.Data),
						  Month(vi.Data),
                          p.CodigoProjeto
                UNION
                SELECT g.CodigoGrupoRecurso AS Codigo,
                       g.DescricaoGrupo AS Grupo,
                       p.CodigoProjeto,
                       vi.Ano AS Ano,
                       vi.Mes AS Mes
                  FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
                       {0}.{1}.SelecaoGrupoRecurso AS vi ON (vi.CodigoGrupoRecurso = g.CodigoGrupoRecurso) INNER JOIN
                       {0}.{1}.Projeto AS p ON (p.CodigoProjeto = vi.CodigoProjeto  {3}) 
                 WHERE p.CodigoProjeto IN (SELECT CodigoProjeto FROM {0}.{1}.f_GetProjetosSituacao({2},'Novo'))
                   
                 GROUP BY g.DescricaoGrupo,
                          g.CodigoGrupoRecurso,
                          vi.Ano,
                          vi.Mes,
						  p.CodigoProjeto       
                UNION
                  SELECT g.CodigoGrupoRecurso,
                         g.DescricaoGrupo,
                         null,
                         YEAR(vd.Data),
                         MONTH(vd.Data)
                   FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
		                {0}.{1}.vi_RecursoCorporativo AS rc ON (rc.CodigoGrupoRecurso = g.CodigoGrupoRecurso) INNER JOIN
                        {0}.{1}.vi_DisponibilidadeDiariaRecurso AS vd ON (vd.CodigoRecursoMSProject = rc.CodigoRecursoMSProject)
                 WHERE rc.CodigoEntidade = {2}
                 GROUP BY  g.DescricaoGrupo,
                           g.CodigoGrupoRecurso,
                           YEAR(vd.Data),
                           MONTH(vd.Data)) AS t LEFT JOIN
                                              {0}.{1}.Projeto AS prj ON (t.CodigoProjeto = prj.CodigoProjeto
                                                                          {3})
                 WHERE t.Ano IN (SELECT Ano         
                                   FROM {0}.{1}.PeriodoAnalisePortfolio
                                  WHERE CodigoEntidade = {2}
                                    AND IndicaAnoPeriodoEditavel = 'S') 
                   {4}
                 GROUP BY t.Codigo,
                          t.Grupo", bancodb, Ownerdb, codEntidade, where, whereGeral);
            ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getDisponibilidadeRHEntidade(int codEntidade, string where)
        {
            DataSet ds;
            string comandoSQL = string.Format(@"SELECT Codigo,
                                                   Grupo,
                                                   Ano,
                                                   {0}.{1}.f_GetPeriodoExtenso({2},Ano,Mes) AS Periodo,
                                                   CASE WHEN (SUM(Capacidade) - SUM(Alocacao) - SUM(Previsao)) < 0 THEN (SUM(Alocacao) + SUM(Previsao)) + (SUM(Capacidade) - SUM(Alocacao) - SUM(Previsao)) 
                                                        ELSE SUM(Alocacao) + SUM(Previsao) END AS Alocacao,
                                                   SUM(Capacidade) AS Capacidade,
                                                   CASE WHEN (SUM(Capacidade) - SUM(Alocacao) - SUM(Previsao)) > 0 THEN SUM(Capacidade) - SUM(Alocacao) - SUM(Previsao) ELSE 0 END AS Disponibilidade,
                                                   CASE WHEN (SUM(Capacidade) - SUM(Alocacao) - SUM(Previsao)) < 0 THEN SUM(Capacidade) - SUM(Alocacao) - SUM(Previsao) ELSE 0 END AS SuperAlocacao
                                             FROM
                                            (
                                            SELECT g.CodigoGrupoRecurso AS Codigo,
                                                   g.DescricaoGrupo AS Grupo,
                                                   YEAR(vi.Data) AS Ano,
                                                   MONTH(vi.Data) AS Mes,
                                                   SUM(vi.Trabalho) AS Alocacao,
                                                   0 AS Previsao,
                                                   0 AS Capacidade
                                              FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
												   {0}.{1}.vi_RecursoCorporativo AS rc ON (rc.CodigoGrupoRecurso = g.CodigoGrupoRecurso) INNER JOIN	
                                                   {0}.{1}.vi_AtribuicaoDiariaRecurso AS vi ON (vi.CodigoRecursoMSProject = rc.CodigoRecursoMSProject) INNER JOIN
                                                   {0}.{1}.Projeto p ON (p.CodigoProjeto = vi.CodigoProjeto)
                                             WHERE rc.CodigoEntidade = {2}   
                                             GROUP BY g.CodigoGrupoRecurso,
                                                      g.DescricaoGrupo,
                                                      YEAR(vi.Data),
                                                          MONTH(vi.Data)
                                            UNION
                                            SELECT g.CodigoGrupoRecurso,
                                                   g.DescricaoGrupo,
                                                   pgr.Ano,
                                                   pgr.Mes,
                                                   0,
                                                   SUM(pgr.TrabalhoPrevisto),
                                                   0 
                                              FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
                                                   {0}.{1}.PrevisaoGrupoRH AS pgr ON (pgr.CodigoGrupoRH = g.CodigoGrupoRecurso) INNER JOIN
                                                   {0}.{1}.Projeto AS p ON (p.CodigoProjeto = pgr.CodigoProjeto)  
                                             WHERE p.CodigoEntidade = {2}   
                                               AND pgr.CodigoProjeto NOT IN (SELECT CodigoProjeto
                                                                               FROM {0}.{1}.ResumoProjeto
                                                                              WHERE InicioReprogramado IS NOT NULL)
                                             GROUP BY g.DescricaoGrupo,
                                                      g.CodigoGrupoRecurso,
                                                      pgr.Ano,
                                                      pgr.Mes         
                                            UNION
                                              SELECT g.CodigoGrupoRecurso,
                                                     g.DescricaoGrupo,
                                                     YEAR(vd.Data),
                                                     MONTH(vd.Data),
                                                       0,
                                                       0,
                                                       Sum(vd.Disponibilidade)
                                               FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
												    {0}.{1}.vi_RecursoCorporativo AS rc ON (rc.CodigoGrupoRecurso = g.CodigoGrupoRecurso) INNER JOIN
                                                    {0}.{1}.vi_DisponibilidadeDiariaRecurso AS vd ON (vd.CodigoRecursoMSProject = rc.CodigoRecursoMSProject) 
                                             WHERE rc.CodigoEntidade = {2} 
                                             GROUP BY  g.DescricaoGrupo,
                                                       g.CodigoGrupoRecurso,
                                                       YEAR(vd.Data),
                                                       MONTH(vd.Data)) AS t
                                             WHERE t.Ano IN (SELECT Ano         
                                                               FROM {0}.{1}.PeriodoAnalisePortfolio
                                                              WHERE CodigoEntidade = {2}
                                                                AND IndicaAnoPeriodoEditavel = 'S') 
                                               {3}                                                        
                                             GROUP BY Codigo,
                                                      Grupo,
                                                      Ano,
                                                      {0}.{1}.f_GetPeriodoExtenso({2},Ano,Mes)", bancodb, Ownerdb, codEntidade, where);
            ds = getDataSet(comandoSQL);
            return ds;
        }

        public string getGraficoRHProposta(DataTable dt, string titulo, int fonte)
        {
            //Cria as variáveis para a formação do XML
            StringBuilder xml = new StringBuilder();
            int i = 0;

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
                xml.Append(string.Format(@"<chart caption=""{0}"" adjustDiv=""1"" numVisiblePlot=""6""  scrollHeight=""12"" showLegend=""1""
                      BgColor=""F7F7F7"" canvasBgColor=""F7F7F7"" showvalues=""0"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" slantLabels=""1"" labelDisplay=""ROTATE""
                      chartTopMargin=""5"" decimals=""2"" chartRightMargin=""8"" chartBottomMargin=""2"" chartLeftMargin=""4"" showShadow=""0"" {1} {2} showBorder=""0"" showSum=""0"" 
                      baseFontSize=""{3}""  inThousandSeparator=""."" thousandSeparator=""."" inDecimalSeparator="","" decimalSeparator="","" >"
                            , titulo
                            , usarGradiente + usarBordasArredondadas
                            , exportar
                            , fonte));

                xml.Append("<categories>");

                for (i = 0; i < dt.Rows.Count; i++)
                {
                    xml.Append(string.Format(@"<category label=""{0}""/>", dt.Rows[i]["Periodo"].ToString()));

                }

                xml.Append("</categories>");

                //gera as colunas de projetos satisfatórios para cada entidade
                xml.Append(@"<dataset seriesName=""Alocação/Proposta"" color=""95C5FF"">");

                //percorre todo o DataTable
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    xml.Append(string.Format(@"<set label=""Alocação/Proposta"" value=""{0}""/>", dt.Rows[i]["Alocacao"].ToString()));
                }

                xml.Append("</dataset>");

                //gera as colunas de projetos satisfatórios para cada entidade
                xml.Append(string.Format(@"<dataset seriesName=""Disponibilidade"" color=""{0}"">", corSatisfatorio));

                //percorre todo o DataTable
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    xml.Append(string.Format(@"<set label=""Disponibilidade"" value=""{0}""/>", double.Parse(dt.Rows[i]["Disponibilidade"].ToString()),
                        corSatisfatorio));
                }

                xml.Append("</dataset>");

                //gera as colunas de projetos satisfatórios para cada entidade
                xml.Append(string.Format(@"<dataset seriesName=""Super Alocação"" color=""{0}"">", corCritico));

                //percorre todo o DataTable
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    xml.Append(string.Format(@"<set label=""Super Alocação"" value=""{0}""/>", double.Parse(dt.Rows[i]["SuperAlocacao"].ToString()) * -1,
                       corCritico));
                }

                xml.Append("</dataset>");

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
        #endregion

        #region Balanceamento Portfolio

        public DataSet getOlapFluxoCaixa(int codigoEntidade, int codigoPortfolio, int cenario, string where)
        {
            comandoSQL = string.Format(
                @"BEGIN
                  /* Apresenta informações sobre fluxos de caixa de projetos */
                  DECLARE @CodigoPortfolio Int,
                          @CodigoEntidade Int,
                          @Cenario SmallInt
                  
                  
                  SET @CodigoPortfolio = {2} -- Parâmetro
		          SET @CodigoEntidade = {3}
                  SET @Cenario = {4}
                  
                  DECLARE @tblOLAP TABLE
                    (_CodigoProjeto Int,
                     _Ano SmallInt,
                     _Mes SmallInt,
                     IncluidoPortfolio Char(3) DEFAULT 'NÃO',
                     Despesa Decimal(18,4),
                     Receita Decimal(18,4))
                 
                 /* Insere os projetos novos */    
                 INSERT INTO @tblOLAP
                  (_CodigoProjeto, _Ano, _Mes, Despesa,Receita)
                 SELECT sfc.CodigoProjeto,                        
                        sfc.Ano,
                        sfc.Mes,                       
                        Sum(sfc.ValorCusto + sfc.ValorInvestimento),
                        Sum(sfc.ValorReceita)
                   FROM {0}.{1}.SelecaoFluxoCaixa AS sfc                                                                                         
                  WHERE sfc.CodigoProjeto IN (SELECT CodigoProjeto FROM {0}.{1}.f_GetProjetosSituacao(@CodigoEntidade,'Todos')) 
                    AND sfc.CodigoProjeto IN (SELECT CodigoProjeto FROM {0}.{1}.f_GetProjetosCenario(@CodigoEntidade,@Cenario))                 
                    AND sfc.Ano IN (SELECT Ano 
                                      FROM {0}.{1}.PeriodoAnalisePortfolio
                                     WHERE IndicaAnoPeriodoEditavel = 'S')                   
                 GROUP BY sfc.CodigoProjeto,                         
                          sfc.Ano,
                          sfc.Mes 
                  
                  /* Retorna os projetos com suas categorias */       
                  SELECT NomeProjeto, 
                         c.DescricaoCategoria,
                         IncluidoPortfolio,
                         _Ano AS Ano,
                         {0}.{1}.f_GetPeriodoExtenso(@CodigoEntidade, _Ano,_Mes) AS Periodo,
                         Sum(Despesa) AS Despesa,
                         Sum(Receita) AS Receita,
                         s.DescricaoStatus, 
                         _CodigoProjeto AS CodigoProjeto
                    FROM @tblOLAP INNER JOIN
                         Projeto AS p ON (p.CodigoProjeto = _CodigoProjeto) INNER JOIN
                         Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria
                                        AND c.DataExclusao IS NULL) INNER JOIN
                         Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto)
                   WHERE 1 = 1 
                     {5}
                   GROUP BY NomeProjeto, 
                         c.DescricaoCategoria,
                         IncluidoPortfolio,
                         _Ano,
                         {0}.{1}.f_GetPeriodoExtenso(@CodigoEntidade, _Ano,_Mes),
                         s.DescricaoStatus, 
                         _CodigoProjeto

                END", bancodb, Ownerdb, codigoPortfolio, codigoEntidade, cenario, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getOlapCriterios(int codigoEntidade, int codigoPortfolio, char tipoFator, string where)
        {
            string clausulaTipoFator = "";

            if (tipoFator == 'I')
                clausulaTipoFator = "AND fp.[ValorSinalFator] > 0";
            else
                clausulaTipoFator = "AND fp.[ValorSinalFator] < 0";

            comandoSQL = string.Format(
                @"BEGIN
  
                  DECLARE @CodigoPortfolio Int,
                          @CodigoEntidade Int
                  
                  SET @CodigoPortfolio = {2}
                  SET @CodigoEntidade = {3}
                  
                  DECLARE @tblOLAP TABLE
                    (_CodigoProjeto Int,
                     NomeProjeto Varchar(255),
                     Categoria Varchar(255),
                     IncluidoPortfolio Char(3) DEFAULT 'NÃO',
                     Criterio Varchar(250),
                     OpcaoCriterio Varchar(250),
                     ValorCriterio Decimal(15,2),
                     Quantidade Int,
					 Cenario1 Char(1),
					 Cenario2 Char(1),
					 Cenario3 Char(1),
					 Cenario4 Char(1),
					 Cenario5 Char(1),
					 Cenario6 Char(1),
					 Cenario7 Char(1),
					 Cenario8 Char(1),
					 Cenario9 Char(1))
                 
                 /* Insere os projetos e suas categorias */    
                 INSERT INTO @tblOLAP
                  (_CodigoProjeto,NomeProjeto, Categoria, Criterio, OpcaoCriterio, ValorCriterio, Quantidade, Cenario1, Cenario2, Cenario3, Cenario4, Cenario5, Cenario6, Cenario7, Cenario8, Cenario9)
                 SELECT p.CodigoProjeto,
                        p.NomeProjeto,
                        c.DescricaoCategoria,
                        cs.DescricaoCriterioSelecao, 
                        ocs.DescricaoOpcaoCriterioSelecao,
                        moc.[PesoObjetoMatriz]*100.0,
                        1,
                        p.IndicaCenario1, p.IndicaCenario2, p.IndicaCenario3,
                        p.IndicaCenario4, p.IndicaCenario5, p.IndicaCenario6,
                        p.IndicaCenario7, p.IndicaCenario8, p.IndicaCenario9
                   FROM {0}.{1}.Projeto AS p INNER JOIN
                        {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria
                                               AND c.DataExclusao IS NULL) LEFT JOIN
                        {0}.{1}.ProjetoCriterioSelecao AS pcs ON (pcs.CodigoProjeto = p.CodigoProjeto AND pcs.EtapaPreenchimento=0) LEFT JOIN
                        {0}.{1}.CriterioSelecao AS cs ON (cs.CodigoCriterioSelecao = pcs.CodigoCriterioSelecao
                                              AND cs.CodigoEntidade = @CodigoEntidade) LEFT JOIN
                        {0}.{1}.OpcaoCriterioSelecao AS ocs ON (ocs.CodigoCriterioSelecao = pcs.CodigoCriterioSelecao
                                              AND ocs.CodigoOpcaoCriterioSelecao = pcs.CodigoOpcaoCriterioSelecao) INNER JOIN
                        {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                            AND s.IndicaSelecaoPortfolio = 'S') INNER JOIN 
                        {0}.{1}.[MatrizObjetoCriterio]	AS [moc] ON (moc.[CodigoCategoria] = p.[CodigoCategoria]
						    				AND moc.IniciaisTipoObjetoCriterioPai = 'CR'
											AND moc.[CodigoObjetoCriterioPai] = pcs.CodigoCriterioSelecao
											AND moc.CodigoObjetoCriterio = pcs.CodigoCriterioSelecao*1000+ASCII(pcs.CodigoOpcaoCriterioSelecao) )  INNER JOIN 
                        {0}.{1}.[MatrizObjetoCriterio] AS [mo2] ON (mo2.CodigoCategoria = moc.[CodigoCategoria]
											AND mo2.[IniciaisTipoObjetoCriterioPai] = 'GP' AND mo2.[CodigoObjetoCriterio] = pcs.[CodigoCriterioSelecao]) INNER JOIN 
                        {0}.{1}.[GrupoCriterioSelecao] AS [gcs] ON (gcs.[CodigoGrupoCriterio] = mo2.[CodigoObjetoCriterioPai]) INNER JOIN 
                        {0}.{1}.[FatorPortfolio] AS [fp] ON (fp.[CodigoFatorPortfolio] = gcs.[CodigoFatorPortfolio] {5})
                  WHERE p.DataExclusao IS NULL
                    AND p.CodigoEntidade = @CodigoEntidade
                  
                  /* Verifica quais projetos já fazem parte do portfólio */
                  UPDATE @tblOLAP
                     SET IncluidoPortfolio = 'SIM'
                   WHERE EXISTS (SELECT 1
                                   FROM {0}.{1}.PortfolioProjeto AS pp
                                  WHERE CodigoPortfolio = @CodigoPortfolio
                                    AND CodigoProjeto = _CodigoProjeto)
                                   
                  
                  /* Retorna os projetos com suas categorias */       
                  SELECT NomeProjeto, 
                         Categoria,
                         IncluidoPortfolio,
                         Criterio,
                         OpcaoCriterio,
                         ValorCriterio,
                         Quantidade,
                         CASE WHEN Cenario1 = 'S' THEN 'SIM' ELSE 'NÃO' END AS Cenario1,
                         CASE WHEN Cenario2 = 'S' THEN 'SIM' ELSE 'NÃO' END AS Cenario2,
                         CASE WHEN Cenario3 = 'S' THEN 'SIM' ELSE 'NÃO' END AS Cenario3,
                         CASE WHEN Cenario4 = 'S' THEN 'SIM' ELSE 'NÃO' END AS Cenario4,
                         CASE WHEN Cenario5 = 'S' THEN 'SIM' ELSE 'NÃO' END AS Cenario5,
                         CASE WHEN Cenario6 = 'S' THEN 'SIM' ELSE 'NÃO' END AS Cenario6,
                         CASE WHEN Cenario7 = 'S' THEN 'SIM' ELSE 'NÃO' END AS Cenario7,
                         CASE WHEN Cenario8 = 'S' THEN 'SIM' ELSE 'NÃO' END AS Cenario8,
                         CASE WHEN Cenario9 = 'S' THEN 'SIM' ELSE 'NÃO' END AS Cenario9
                    FROM @tblOLAP
                   WHERE 1 = 1 {4}
                END

                ", bancodb, Ownerdb, codigoPortfolio, codigoEntidade, where, clausulaTipoFator);

            return getDataSet(comandoSQL);
        }

        public DataSet getOlapRecursos(int codigoEntidade, string where)
        {
            comandoSQL = string.Format(
                  @"SELECT t.Codigo,
                       t.Grupo,
                       t.NomeProjeto,
                       t.Ano,
                       t.Mes,
                       {0}.{1}.f_GetPeriodoExtenso({2},t.Ano,t.Mes) AS Periodo,
                       SUM(t.Alocacao) AS Alocacao,
                       SUM(t.Capacidade) AS Capacidade,
                       SUM(t.Previsao) AS PropostaAlocacao,
                       SUM(t.Capacidade) - SUM(t.Alocacao) - SUM(t.Previsao) AS Disponibilidade
                 FROM
                (
                SELECT g.CodigoGrupoRecurso AS Codigo,
                       g.DescricaoGrupo AS Grupo,
                       p.CodigoProjeto,
                       p.NomeProjeto,
                      Year(vi.Data) Ano,
                       Month(vi.Data) AS Mes,
                       SUM(vi.Trabalho) AS Alocacao,
                       0 AS Previsao,
                       0 AS Capacidade
                  FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
                       {0}.{1}.vi_RecursoCorporativo AS rc ON (rc.CodigoGrupoRecurso = g.CodigoGrupoRecurso) INNER JOIN
                       {0}.{1}.vi_AtribuicaoDiariaRecurso AS vi ON (vi.CodigoRecursoMSProject = rc.CodigoRecursoMSProject) INNER JOIN
                       {0}.{1}.Projeto AS p ON (p.CodigoProjeto = vi.CodigoProjeto {3}) 
                 WHERE p.CodigoProjeto IN (SELECT CodigoProjeto FROM {0}.{1}.f_GetProjetosSituacao({2},'Execucao'))
                   
                 GROUP BY g.DescricaoGrupo,
                          g.CodigoGrupoRecurso,
                          p.CodigoProjeto,
                          p.NomeProjeto,
                          Year(vi.Data),
						  Month(vi.Data)
                UNION
                SELECT g.CodigoGrupoRecurso AS Codigo,
                       g.DescricaoGrupo AS Grupo,
                       p.CodigoProjeto,
                       p.NomeProjeto,
                       vi.Ano AS Ano,
                       vi.Mes AS Mes,
                       0,
                       SUM(vi.Trabalho),
                       0 AS Capacidade
                  FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
                       {0}.{1}.SelecaoGrupoRecurso AS vi ON (vi.CodigoGrupoRecurso = g.CodigoGrupoRecurso) INNER JOIN
                       {0}.{1}.Projeto AS p ON (p.CodigoProjeto = vi.CodigoProjeto {3}) 
                 WHERE p.CodigoProjeto IN (SELECT CodigoProjeto FROM {0}.{1}.f_GetProjetosSituacao({2},'Novo'))
                   
                 GROUP BY g.DescricaoGrupo,
                          g.CodigoGrupoRecurso,
                          p.CodigoProjeto,
                          p.NomeProjeto,
                          vi.Ano,
                          vi.Mes        
                UNION
                  SELECT g.CodigoGrupoRecurso,
                         g.DescricaoGrupo,
                         null,
                         'Capacidade',
                         YEAR(vd.Data),
                         MONTH(vd.Data),
                           0,
                           0,
                           Sum(vd.Disponibilidade)
                   FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
		                {0}.{1}.vi_RecursoCorporativo AS rc ON (rc.CodigoGrupoRecurso = g.CodigoGrupoRecurso) INNER JOIN
                        {0}.{1}.vi_DisponibilidadeDiariaRecurso AS vd ON (vd.CodigoRecursoMSProject = rc.CodigoRecursoMSProject)
                 WHERE rc.CodigoEntidade = {2}
                 GROUP BY  g.DescricaoGrupo,
                           g.CodigoGrupoRecurso,
                           YEAR(vd.Data),
                           MONTH(vd.Data)) AS t LEFT JOIN
                                              {0}.{1}.Projeto AS prj ON (t.CodigoProjeto = prj.CodigoProjeto
                                                                         {3})
                 WHERE t.Ano IN (SELECT Ano         
                                   FROM {0}.{1}.PeriodoAnalisePortfolio
                                  WHERE CodigoEntidade = {2}
                                    AND IndicaAnoPeriodoEditavel = 'S') 
                 GROUP BY t.Codigo,
                          t.Grupo,
                          t.NomeProjeto,
                          t.Ano,
                          t.Mes", bancodb, Ownerdb, codigoEntidade, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getOlapRecursosPorPortifolioESenario(int codigoEntidade, string where, int codigoPortfolio, string cenario)
        {
            comandoSQL = string.Format(
                  @"SELECT t.Codigo,
                       t.Grupo,
                       t.NomeProjeto,
                       t.Ano,
                       t.Mes,
                       {0}.{1}.f_GetPeriodoExtenso({2},t.Ano,t.Mes) AS Periodo,
                       SUM(t.Alocacao) AS Alocacao,
                       SUM(t.Capacidade) AS Capacidade,
                       SUM(t.Previsao) AS PropostaAlocacao,
                       SUM(t.Capacidade) - SUM(t.Alocacao) - SUM(t.Previsao) AS Disponibilidade
                 FROM
                (
                SELECT g.CodigoGrupoRecurso AS Codigo,
                       g.DescricaoGrupo AS Grupo,
                       p.CodigoProjeto,
                       p.NomeProjeto,
                      Year(vi.Data) Ano,
                       Month(vi.Data) AS Mes,
                       SUM(vi.Trabalho) AS Alocacao,
                       0 AS Previsao,
                       0 AS Capacidade
                  FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
                       {0}.{1}.vi_RecursoCorporativo AS rc ON (rc.CodigoGrupoRecurso = g.CodigoGrupoRecurso) INNER JOIN
                       {0}.{1}.vi_AtribuicaoDiariaRecurso AS vi ON (vi.CodigoRecursoMSProject = rc.CodigoRecursoMSProject) INNER JOIN
                       {0}.{1}.Projeto AS p ON (p.CodigoProjeto = vi.CodigoProjeto {3}) 

INNER JOIN f_GetProjetosSelecaoBalanceamento({4}, {5}, {2}) AS GPSB  ON (GPSB._CodigoProjeto = p.CodigoProjeto) 

                 WHERE p.CodigoProjeto IN (SELECT CodigoProjeto FROM {0}.{1}.f_GetProjetosSituacao({2},'Execucao'))
                   
                 GROUP BY g.DescricaoGrupo,
                          g.CodigoGrupoRecurso,
                          p.CodigoProjeto,
                          p.NomeProjeto,
                          Year(vi.Data),
						  Month(vi.Data)
                UNION
                SELECT g.CodigoGrupoRecurso AS Codigo,
                       g.DescricaoGrupo AS Grupo,
                       p.CodigoProjeto,
                       p.NomeProjeto,
                       vi.Ano AS Ano,
                       vi.Mes AS Mes,
                       0,
                       SUM(vi.Trabalho),
                       0 AS Capacidade
                  FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
                       {0}.{1}.SelecaoGrupoRecurso AS vi ON (vi.CodigoGrupoRecurso = g.CodigoGrupoRecurso) INNER JOIN
                       {0}.{1}.Projeto AS p ON (p.CodigoProjeto = vi.CodigoProjeto {3}) 

INNER JOIN f_GetProjetosSelecaoBalanceamento({4}, {5}, {2}) AS GPSB  ON (GPSB._CodigoProjeto = p.CodigoProjeto) 

                 WHERE p.CodigoProjeto IN (SELECT CodigoProjeto FROM {0}.{1}.f_GetProjetosSituacao({2},'Novo'))
                   
                 GROUP BY g.DescricaoGrupo,
                          g.CodigoGrupoRecurso,
                          p.CodigoProjeto,
                          p.NomeProjeto,
                          vi.Ano,
                          vi.Mes        
                UNION
                  SELECT g.CodigoGrupoRecurso,
                         g.DescricaoGrupo,
                         null,
                         'Capacidade',
                         YEAR(vd.Data),
                         MONTH(vd.Data),
                           0,
                           0,
                           Sum(vd.Disponibilidade)
                   FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
		                {0}.{1}.vi_RecursoCorporativo AS rc ON (rc.CodigoGrupoRecurso = g.CodigoGrupoRecurso) INNER JOIN
                        {0}.{1}.vi_DisponibilidadeDiariaRecurso AS vd ON (vd.CodigoRecursoMSProject = rc.CodigoRecursoMSProject)

                 WHERE rc.CodigoEntidade = {2}
                        AND g.CodigoGrupoRecurso IN
                        (
                         SELECT vi.CodigoGrupoRecurso
                           FROM dbo.SelecaoGrupoRecurso AS vi
                              INNER JOIN dbo.f_GetProjetosSelecaoBalanceamento({4}, {5}, {2}) AS GPSB ON (GPSB._CodigoProjeto = vi.CodigoProjeto)
                              INNER JOIN dbo.Projeto AS p ON (GPSB._CodigoProjeto = p.CodigoProjeto AND p.IndicaCenario{5} = 'S')
                           WHERE (vi.CodigoGrupoRecurso = g.CodigoGrupoRecurso)
                        )
                 GROUP BY  g.DescricaoGrupo,
                           g.CodigoGrupoRecurso,
                           YEAR(vd.Data),
                           MONTH(vd.Data)) AS t LEFT JOIN
                                              {0}.{1}.Projeto AS prj ON (t.CodigoProjeto = prj.CodigoProjeto
                                                                         {3})
                 WHERE t.Ano IN (SELECT Ano         
                                   FROM {0}.{1}.PeriodoAnalisePortfolio
                                  WHERE CodigoEntidade = {2}
                                    AND IndicaAnoPeriodoEditavel = 'S') 
                 GROUP BY t.Codigo,
                          t.Grupo,
                          t.NomeProjeto,
                          t.Ano,
                          t.Mes", bancodb, Ownerdb, codigoEntidade, where, codigoPortfolio, cenario);

            return getDataSet(comandoSQL);
        }

        public DataSet getOlapAnaliseGeral(int codigoEntidade, int codigoPortfolio, string where)
        {
            comandoSQL = string.Format(
                  @"SELECT p.NomeProjeto AS NomeProjeto,                  
                   un.SiglaUnidadeNegocio AS SiglaUnidade,
                   gu.NomeUsuario AS NomeGerenteUnidade,
                   IsNull(u.NomeUsuario,'Sem Responsável pelo Projeto') AS NomeGerenteProjeto,
                   c.DescricaoCategoria AS Categoria,
                   s.DescricaoStatus AS StatusProjeto,                  
                   rp.CustoRealTotal AS CustoReal,
                   rp.CustoLBTotal AS CustoPrevistoTotal,
                   rp.CustoLBData AS CustoPrevistoData,
                   rp.CustoTendenciaTotal AS Custo,
                   rp.CustoRealHE AS CustoHE,
                   rp.TrabalhoRealTotal AS TRabalhoReal,
                   rp.TrabalhoLBTotal AS TrabalhoPrevistoTotal,
                   rp.TrabalhoLBData AS TrabalhoPrevistoData,
                   rp.TrabalhoTendenciaTotal AS Trabalho,
                   rp.TrabalhoRealHE AS TrabalhoHE,
                   rp.COTA AS ValorPlanejado,
                   rp.COTR AS ValorAgregado,
                   rp.ReceitaRealTotal AS ReceitaRealTotal,
                   rp.ReceitaLBTotal AS ReceitaPrevistaTotal,
                   rp.ReceitaLBData AS ReceitaPrevistaData,
                   rp.ReceitaTendenciaTotal AS Receita,
                   rp.ScoreCriterios AS EscoreCriterios,
                   rp.ScoreRiscos AS EscoreRiscos,
                   rp.TrabalhoTendenciaTotal - rp.TrabalhoLBTotal AS VariacaoTrabalho,
                   rp.CustoTendenciaTotal - rp.CustoLBTotal AS VariacaoCusto,
                   rp.ReceitaTendenciaTotal - rp.ReceitaLBTotal AS VariacaoReceita,
                   rp.EAC AS EstimativaCustoConcluir,
                   CASE WHEN pp.CodigoProjeto IS NULL THEN 'Não' ELSE 'Sim' END AS ProjetoAssociadoPortfolio,
                   CASE WHEN pp.CodigoProjeto IS NULL THEN 'Não se Aplica' WHEN rp.CorGeral = 'Verde' THEN 'Satisfatório' WHEN rp.CorGeral = 'Amarelo' THEN 'Atenção' WHEN rp.CorGeral = 'Vermelho' THEN 'Crítico' ELSE 'Satisfatório' END AS Desempenho,
                   rp.PercentualRealizacao                                
              FROM {0}.{1}.ResumoProjeto AS rp INNER JOIN
                   {0}.{1}.Projeto AS p ON (rp.CodigoProjeto = p.CodigoProjeto) INNER JOIN
                   {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio) INNER JOIN
                   {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria) LEFT JOIN
                   {0}.{1}.PortfolioProjeto AS pp ON (pp.CodigoProjeto = p.CodigoProjeto) LEFT JOIN
                   {0}.{1}.Portfolio AS Portf ON (Portf.CodigoPortfolio = pp.CodigoPortfolio
                                      AND Portf.CodigoPortfolio = {3}) LEFT JOIN
                   {0}.{1}.Usuario AS u ON (u.CodigoUsuario = p.CodigoGerenteProjeto) INNER JOIN
                   {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto) LEFT JOIN
                   {0}.{1}.Status AS spp ON (spp.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                 AND spp.IndicaAcompanhamentoPortfolio = 'S') INNER JOIN
                   {0}.{1}.Usuario AS gu ON (gu.CodigoUsuario = un.CodigoUsuarioGerente)  
             WHERE p.DataExclusao IS NULL 
               AND p.CodigoEntidade = {2}
               AND p.CodigoProjeto  IN ( SELECT CodigoProjeto FROM {0}.{1}.f_GetProjetosSituacao({2}, 'TODOS') )
", bancodb, Ownerdb, codigoEntidade, codigoPortfolio, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getDisponibilidadeRHCenario(int codEntidade, int cenario, string where, string whereProjetos)
        {
            DataSet ds;
            string comandoSQL = string.Format(@"
              BEGIN
                    DECLARE @CodigoEntidade int
                    DECLARE @CodigoCenario int
                	
                    SET @CodigoEntidade = {2}
                    SET @CodigoCenario = {3}

                SELECT     Ano,
                           {0}.{1}.f_GetPeriodoExtenso(@CodigoEntidade,Ano,Mes) AS Periodo,
                           CASE WHEN (SUM(Capacidade) - SUM(Alocacao) - SUM(Previsao)) < 0 THEN (SUM(Alocacao) + SUM(Previsao)) + (SUM(Capacidade) - SUM(Alocacao) - SUM(Previsao)) 
                                ELSE SUM(Alocacao) + SUM(Previsao) END AS Alocacao,
                           SUM(Capacidade) AS Capacidade,
                           CASE WHEN (SUM(Capacidade) - SUM(Alocacao) - SUM(Previsao)) > 0 THEN SUM(Capacidade) - SUM(Alocacao) - SUM(Previsao) ELSE 0 END AS Disponibilidade,
                           CASE WHEN (SUM(Capacidade) - SUM(Alocacao) - SUM(Previsao)) < 0 THEN SUM(Capacidade) - SUM(Alocacao) - SUM(Previsao) ELSE 0 END AS SuperAlocacao
                     FROM
                    (
                    SELECT g.CodigoGrupoRecurso AS Codigo,
                           g.DescricaoGrupo AS Grupo,
                           Year(vi.Data) Ano,
						   Month(vi.Data) AS Mes,
                           SUM(Trabalho) AS Alocacao,
                           0 AS Previsao,
                           0 AS Capacidade
                      FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
                           {0}.{1}.vi_RecursoCorporativo AS rc ON (rc.CodigoGrupoRecurso = g.CodigoGrupoRecurso) INNER JOIN
                           {0}.{1}.vi_AtribuicaoDiariaRecurso AS vi ON (vi.CodigoRecursoMSProject = rc.CodigoRecursoMSProject)
                     WHERE vi.CodigoProjeto IN (SELECT CodigoProjeto FROM {0}.{1}.f_GetProjetosSituacao(@CodigoEntidade,'Execucao'))   
                       AND vi.CodigoProjeto IN (SELECT pc.CodigoProjeto FROM {0}.{1}.f_GetProjetosCenario(@CodigoEntidade,@CodigoCenario) pc INNER JOIN
                                                                           {0}.{1}.Projeto p ON p.CodigoProjeto = pc.CodigoProjeto
                                                                     WHERE 1 = 1 {5})
                     GROUP BY g.DescricaoGrupo,
                              g.CodigoGrupoRecurso,
                              Year(vi.Data),
							  Month(vi.Data)
                    UNION
                    SELECT g.CodigoGrupoRecurso AS Codigo,
                           g.DescricaoGrupo AS Grupo,
                           sgr.Ano AS Ano,
                           sgr.Mes AS Mes,
                           0,
                           SUM(sgr.Trabalho) AS Previsao,
                           0 AS Capacidade
                      FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
                           {0}.{1}.SelecaoGrupoRecurso AS sgr ON (sgr.CodigoGrupoRecurso = g.CodigoGrupoRecurso) 
                     WHERE sgr.CodigoProjeto IN (SELECT CodigoProjeto FROM {0}.{1}.f_GetProjetosSituacao(@CodigoEntidade,'Novo')) 
                       AND sgr.CodigoProjeto IN (SELECT pc.CodigoProjeto FROM {0}.{1}.f_GetProjetosCenario(@CodigoEntidade,@CodigoCenario) pc INNER JOIN
                                                                           {0}.{1}.Projeto p ON p.CodigoProjeto = pc.CodigoProjeto
                                                                     WHERE 1 = 1 {5})   
                     GROUP BY g.DescricaoGrupo,
                              g.CodigoGrupoRecurso,
                              sgr.Ano,
                              sgr.Mes         
                    UNION
                      SELECT g.CodigoGrupoRecurso,
                             g.DescricaoGrupo,
                             YEAR(vd.Data),
                             MONTH(vd.Data),
                               0,
                               0,
                               Sum(vd.Disponibilidade)
                       FROM {0}.{1}.vi_GrupoRecurso AS g INNER JOIN
                            {0}.{1}.vi_RecursoCorporativo AS rc ON (rc.CodigoGrupoRecurso = g.CodigoGrupoRecurso) INNER JOIN 
                            {0}.{1}.vi_DisponibilidadeDiariaRecurso AS vd ON (vd.CodigoRecursoMSProject = rc.CodigoRecursoMSProject) 
                     WHERE rc.CodigoEntidade = @CodigoEntidade 
                     GROUP BY  g.DescricaoGrupo,
                               g.CodigoGrupoRecurso,
                               YEAR(vd.Data),
                               MONTH(vd.Data)) AS t
                     WHERE t.Ano IN (SELECT Ano         
                                       FROM {0}.{1}.PeriodoAnalisePortfolio
                                      WHERE CodigoEntidade = @CodigoEntidade
                                        AND IndicaAnoPeriodoEditavel = 'S') 
                       {4}
                                
                     GROUP BY Ano,
                              {0}.{1}.f_GetPeriodoExtenso(@CodigoEntidade,Ano,Mes)
                              
                   END", bancodb, Ownerdb, codEntidade, cenario, where, whereProjetos);
            ds = getDataSet(comandoSQL);
            return ds;
        }

        public string geraAnaliseGraficaPortfolio(DataTable dt, string colunaTamanhoBolha, string tituloEixoX, string tituloEixoY,
            string nomeTamanhoBolha, string urlImage, int fonte)
        {
            StringBuilder xml = new StringBuilder();
            StringBuilder xmlAUX = new StringBuilder();
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

            double valorMáximoX = 0;
            //double valorMáximoY = 0;

            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valorX, valorY;
                string valorZ, nome;

                valorX = dt.Rows[i]["EixoX"].ToString(); // valorX = "0,54";
                valorY = dt.Rows[i]["EixoY"].ToString();
                valorZ = dt.Rows[i][colunaTamanhoBolha].ToString();
                nome = dt.Rows[i]["_NomeProjeto"].ToString().TrimEnd();

                if (double.Parse(valorZ) != 0)
                {
                    valorMáximoX = (double.Parse(valorX) > valorMáximoX) ? double.Parse(valorX) : valorMáximoX;
                    //valorMáximoY = (double.Parse(valorY) > valorMáximoY) ? double.Parse(valorY) : valorMáximoY;

                    nome = string.Format(@"{0}{8}{4}: {1:n2}{8}{5}: {2:n2}{8}Tamanho da Bolha({6}): {3:n2}{8}Categoria: {7}",
                                nome, double.Parse(valorX), double.Parse(valorY), double.Parse(valorZ), tituloEixoX, tituloEixoY, nomeTamanhoBolha, dt.Rows[i]["DescricaoCategoria"].ToString(), "{br}");

                    xmlAUX.Append(@"\n<dataSet showValues=""0"">");
                    xmlAUX.Append(string.Format(@"\n<set x=""{0}"" y=""{1}"" z=""{2}"" toolText=""{3}""/>", double.Parse(valorX) * 1000, valorY, valorZ, nome));
                    xmlAUX.Append("\n</dataSet>");
                }
            }

            valorMáximoX = valorMáximoX * 1000;
            //valorMáximoY = valorMáximoY * 1000;


            float percentualEscala = 50;

            DataSet dsParametrosSis = getParametrosSistema("PercentualEscalaBolhas");

            if (DataSetOk(dsParametrosSis) && DataTableOk(dsParametrosSis.Tables[0]) && dsParametrosSis.Tables[0].Rows[0]["PercentualEscalaBolhas"] + "" != "")
                percentualEscala = float.Parse(dsParametrosSis.Tables[0].Rows[0]["PercentualEscalaBolhas"] + "");

            valorMáximoX = (valorMáximoX + ((valorMáximoX * percentualEscala) / 100));


            string minimoX = "";
            string maximoX = "";

            if (valorMáximoX == 0)
            {
                maximoX = "1000";
                minimoX = "-200";
                valorMáximoX = 1000;
            }
            else
            {
                minimoX = string.Format("{0:n0}", (valorMáximoX / 5)).Replace(".", "").Replace(",", "");
                maximoX = string.Format("{0:n0}", (valorMáximoX)).Replace(".", "").Replace(",", "");
            }

            xml.Append(string.Format(@"<chart {4} setAdaptiveYMin=""1"" adjustDiv=""1"" chartTopMargin=""25"" chartLeftMargin=""0"" chartBottomMargin=""0""
            numberSuffix="""" decimalSeparator="","" numDivLines=""2"" xAxisMaxValue=""{5:n0}"" xAxisMinValue=""{6}"" 
            thousandSeparator=""."" inThousandSeparator=""."" bgColor=""FFFFFF"" canvasBgColor=""F7F7F7""  clipBubbles=""0""
            showBorder=""0"" outCnvBaseFontColor=""000000"" showPlotBorder=""0"" inDecimalSeparator="","" baseFontSize=""{3}""
            yAxisName=""{1}"" xAxisName=""{2}"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" >",
                                                                                           urlImage, tituloEixoY, tituloEixoX, fonte, exportar, maximoX, minimoX));



            double valorRealX = valorMáximoX / 1000;
            string valor1 = "", valor2 = "", valor3 = "";

            valor1 = string.Format("{0:n0}", valorMáximoX / 3).Replace(".", "").Replace(",", "");
            valor2 = string.Format("{0:n0}", valorMáximoX).Replace(".", "").Replace(",", "");
            valor3 = string.Format("{0:n0}", (valorMáximoX * 2) / 3).Replace(".", "").Replace(",", "");

            xml.Append(string.Format(@"\n<categories>
            \n<category label=""0"" x=""0"" showLabel=""1"" showVerticalLine=""1""/>
            \n<category label=""{3:n1}"" x=""{0}"" showVerticalLine=""1"" showLabel=""1""/>     
            \n<category label=""{5:n1}"" x=""{2}"" showVerticalLine=""1"" showLabel=""1""/>      
            \n</categories>", valor1, valor2, valor3, valorRealX / 3, valorRealX, (valorRealX * 2) / 3));


            xml.Append(xmlAUX.ToString());

            xml.Append(@"\n<styles>
                    \n<definition>
                    \n<style name=""myHTMLFont"" type=""font"" isHTML=""1"" />
                    \n</definition>
                    \n<application>
                    \n<apply toObject=""TOOLTIP"" styles=""myHTMLFont"" />
                    \n<apply toObject=""DATALABELS"" styles=""myHTMLFont"" />
                    \n</application>
                    \n</styles>
                    \n</chart>");
            return xml.ToString();
        }

        public string geraAnaliseGraficaCriterios(DataTable dt, string colunaTamanhoBolha, string tituloEixoX, string tituloEixoY,
            string nomeTamanhoBolha, string urlImage, int fonte)
        {
            StringBuilder xml = new StringBuilder();
            StringBuilder xmlAUX = new StringBuilder();
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

            double valorMáximoX = 0;
            //double valorMáximoY = 0;

            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valorX, valorY;
                string valorZ, nome;

                valorX = dt.Rows[i]["EixoX"].ToString(); // valorX = "0,54";
                valorY = dt.Rows[i]["EixoY"].ToString();
                valorZ = dt.Rows[i][colunaTamanhoBolha].ToString();
                nome = dt.Rows[i]["Cenario"].ToString().TrimEnd();

                if (double.Parse(valorZ) != 0)
                {
                    valorMáximoX = (double.Parse(valorX) > valorMáximoX) ? double.Parse(valorX) : valorMáximoX;
                    //valorMáximoY = (double.Parse(valorY) > valorMáximoY) ? double.Parse(valorY) : valorMáximoY;

                    nome = string.Format(@"{0}{7}{4}: {1:n2}{7}{5}: {2:n2}{7}{6}: {3:n2}",
                        nome, double.Parse(valorX), double.Parse(valorY), double.Parse(valorZ), tituloEixoX, tituloEixoY, nomeTamanhoBolha, "{br}");

                    xmlAUX.Append(@"\n<dataSet showValues=""0"">");
                    xmlAUX.Append(string.Format(@"\n<set x=""{0}"" y=""{1}"" z=""{2}"" toolText=""{3}""/>", double.Parse(valorX) * 1000, valorY, valorZ, nome));
                    xmlAUX.Append("\n</dataSet>");
                }
            }

            valorMáximoX = valorMáximoX * 1000;
            //valorMáximoY = valorMáximoY * 1000;


            float percentualEscala = 50;

            DataSet dsParametrosSis = getParametrosSistema("PercentualEscalaBolhas");

            if (DataSetOk(dsParametrosSis) && DataTableOk(dsParametrosSis.Tables[0]) && dsParametrosSis.Tables[0].Rows[0]["PercentualEscalaBolhas"] + "" != "")
                percentualEscala = float.Parse(dsParametrosSis.Tables[0].Rows[0]["PercentualEscalaBolhas"] + "");

            valorMáximoX = (valorMáximoX + ((valorMáximoX * percentualEscala) / 100));


            string minimoX = "";
            string maximoX = "";

            if (valorMáximoX == 0)
            {
                maximoX = "1000";
                minimoX = "-200";
                valorMáximoX = 1000;
            }
            else
            {
                minimoX = string.Format("{0:n0}", (valorMáximoX / 5)).Replace(".", "").Replace(",", "");
                maximoX = string.Format("{0:n0}", (valorMáximoX)).Replace(".", "").Replace(",", "");
            }

            xml.Append(string.Format(@"<chart {4} setAdaptiveYMin=""1"" adjustDiv=""1""
            decimalSeparator="","" numDivLines=""2"" xAxisMaxValue=""{5:n0}"" xAxisMinValue=""{6}"" 
            thousandSeparator=""."" inThousandSeparator=""."" bgColor=""FFFFFF"" canvasBgColor=""F7F7F7""  clipBubbles=""0""
            showBorder=""0"" outCnvBaseFontColor=""000000"" showPlotBorder=""0"" inDecimalSeparator="","" baseFontSize=""{3}""
            yAxisName=""{1}"" chartLeftMargin=""0"" chartTopMargin=""25"" chartBottomMargin=""0"" xAxisName=""{2}"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" >",
                                                                                           urlImage, tituloEixoY, tituloEixoX, fonte, exportar, maximoX, minimoX));



            double valorRealX = valorMáximoX / 1000;
            string valor1 = "", valor2 = "", valor3 = "";

            valor1 = string.Format("{0:n0}", valorMáximoX / 3).Replace(".", "").Replace(",", "");
            valor2 = string.Format("{0:n0}", valorMáximoX).Replace(".", "").Replace(",", "");
            valor3 = string.Format("{0:n0}", (valorMáximoX * 2) / 3).Replace(".", "").Replace(",", "");

            xml.Append(string.Format(@"\n<categories>
            \n<category label=""0"" x=""0"" showLabel=""1"" showVerticalLine=""1""/>
            \n<category label=""{3:n1}"" x=""{0}"" showVerticalLine=""1"" showLabel=""1""/>     
            \n<category label=""{5:n1}"" x=""{2}"" showVerticalLine=""1"" showLabel=""1""/>      
            \n</categories>", valor1, valor2, valor3, valorRealX / 3, valorRealX, (valorRealX * 2) / 3));


            xml.Append(xmlAUX.ToString());

            xml.Append(@"\n<styles>
                    \n<definition>
                    \n<style name=""myHTMLFont"" type=""font"" isHTML=""1"" />
                    \n</definition>
                    \n<application>
                    \n<apply toObject=""TOOLTIP"" styles=""myHTMLFont"" />
                    \n<apply toObject=""DATALABELS"" styles=""myHTMLFont"" />
                    \n</application>
                    \n</styles>
                    \n</chart>");
            return xml.ToString();
        }

        public DataSet getOpcoesCombosEixosAnaliseGrafica(int codigoEntidade, string where)
        {
            comandoSQL = string.Format(
                @"BEGIN
                  /* Select para preencher os combos relativos aos eixos X e Y dos gráficos de bolhas */
                  DECLARE @CodigoEntidade Int
                  
                  SET @CodigoEntidade = {2}
                  
                  SELECT CodigoCriterioSelecao,
                         DescricaoCriterioSelecao,
                         'CRITERIO' AS TipoEscolha
                    FROM {0}.{1}.CriterioSelecao
                   WHERE DataExclusao IS NULL
                     AND CodigoEntidade = @CodigoEntidade
                  UNION  
                  SELECT -1,
                         '" + Resources.traducao.import_ncia + @"',
                         'SOMACRITERIO' AS TipoEscolha
                  UNION
                  SELECT -2,
                         '" + Resources.traducao.complexidade + @"',
                         'SOMARISCO' AS TipoEscolha
                END", bancodb, Ownerdb, codigoEntidade, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getAnaliseGrafica(int codigoEntidade, int codigoEscolhaX, int codigoEscolhaY, string tipoEscolhaX, string tipoEscolhaY, char opcao, int codigoCategoria, int ano, bool indicaAcompanhamentoPortfolio, string where)
        {
            string colunaIndicaAcompanhamento = "IndicaSelecaoPortfolio";

            if (indicaAcompanhamentoPortfolio)
            {
                colunaIndicaAcompanhamento = "IndicaAcompanhamentoPortfolio";
            }

            comandoSQL = string.Format(
                @"BEGIN  
          DECLARE @TipoEscolhaX Varchar(15),
                  @EscolhaX Int,
                  @TipoEscolhaY Varchar(15),
                  @EscolhaY Int,
                  @CodigoEntidade Int,
                  @Opcao Char(1),
                  @Ano SmallInt,
                  @CodigoCategoria Int
                 
                  
          SET @TipoEscolhaX = '{5}' --> Colocar aqui qual o tipo de escolha para o eixo X
	  		  SET @EscolhaX = {3}             --> Colocar aqui o valor da escolha do usuário (só terá efeito se for um critério) para o Eixo X
	  		  SET @TipoEscolhaY = '{6}' --> Colocar aqui qual o tipo de escolha para o eixo Y
	  		  SET @EscolhaY = {4}             --> Colocar aqui o valor da escolha do usuário (só terá efeito se for um critério) para o Eixo Y
	  		  SET @CodigoEntidade = {2}
                  SET @Opcao = '{7}' --> Parâmetro. Se o valor for igual a 'C', traz o resultado agrupado por cenário. Se for 'P', agrupa por projeto
          
          --=-=-=-=-=-=> PARÂMETROS NOVOS INCLUIDOS EM 06/02/2010 <--=-=-=-=-=-=-=
	           SET @CodigoCategoria = {8} --> Parâmetro para indicar a categoria escolhida pelo usuário.        
	           SET @Ano = {9} --> Parâmetro para indicar o ano de análise para trazer os valores financeiros.
                  
          DECLARE @tblGraficoBolhas TABLE
            (_CodigoProjeto Int,
             _NomeProjeto Varchar(255),
             EixoX Decimal(18,4),
             EixoY Decimal(18,4),
             Despesa Decimal(18,4) DEFAULT 0,
             Receita Decimal(18,4) DEFAULT 0,
             ValorRisco Decimal(18,4) DEFAULT 0,
             ValorCriterio Decimal(18,4) DEFAULT 0,
             IndicaCenario1 Char(1),
             IndicaCenario2 Char(1),
             IndicaCenario3 Char(1),
             IndicaCenario4 Char(1),
             IndicaCenario5 Char(1),
             IndicaCenario6 Char(1),
             IndicaCenario7 Char(1),
             IndicaCenario8 Char(1),
             IndicaCenario9 Char(1),
             _CodigoCategoria SmallInt,
             DescricaoCategoria Varchar(255),
             Trabalho Decimal(18,1) DEFAULT 0) --> Incluída a variável para tratamento de RH.
             
              /* Insere os projetos na tabela temporária de retorno */
              IF @CodigoCategoria = -1
				  INSERT INTO @tblGraficoBolhas
				  (_CodigoProjeto,
				   _NomeProjeto,
				   EixoX,
				   EixoY,
				   _CodigoCategoria,
				   DescricaoCategoria,
				   IndicaCenario1,
				   IndicaCenario2,
				   IndicaCenario3,
				   IndicaCenario4,
				   IndicaCenario5,
				   IndicaCenario6,
				   IndicaCenario7,
				   IndicaCenario8,
				   IndicaCenario9)
				 SELECT p.CodigoProjeto,
						p.NomeProjeto,0,0,
						p.CodigoCategoria,
						c.DescricaoCategoria,
						p.IndicaCenario1,
						p.IndicaCenario2,
						p.IndicaCenario3,
						p.IndicaCenario4,
						p.IndicaCenario5,
						p.IndicaCenario6,
						p.IndicaCenario7,
						p.IndicaCenario8,
						p.IndicaCenario9
				   FROM {0}.{1}.Projeto AS p INNER JOIN
						{0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria
                                               AND c.DataExclusao IS NULL) INNER JOIN
						{0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
														 AND s.{11} = 'S')
				  WHERE p.DataExclusao IS NULL
					AND p.CodigoEntidade = @CodigoEntidade					
			  ELSE
			     INSERT INTO @tblGraficoBolhas
				  (_CodigoProjeto,
				   _NomeProjeto,
				   EixoX,
				   EixoY,
				   _CodigoCategoria,
				   DescricaoCategoria,
				   IndicaCenario1,
				   IndicaCenario2,
				   IndicaCenario3,
				   IndicaCenario4,
				   IndicaCenario5,
				   IndicaCenario6,
				   IndicaCenario7,
				   IndicaCenario8,
				   IndicaCenario9)
				 SELECT p.CodigoProjeto,
						p.NomeProjeto,0,0,
						p.CodigoCategoria,
						c.DescricaoCategoria,
						p.IndicaCenario1,
						p.IndicaCenario2,
						p.IndicaCenario3,
						p.IndicaCenario4,
						p.IndicaCenario5,
						p.IndicaCenario6,
						p.IndicaCenario7,
						p.IndicaCenario8,
						p.IndicaCenario9
				   FROM {0}.{1}.Projeto AS p INNER JOIN
						{0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria
                                               AND c.DataExclusao IS NULL) INNER JOIN
						{0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
														 AND s.{11} = 'S')
				  WHERE p.DataExclusao IS NULL
					AND p.CodigoEntidade = @CodigoEntidade
					AND p.CodigoCategoria = @CodigoCategoria
					{10}
              
              /* Atualiza o resultado para tamanho da bolha */    
              UPDATE @tblGraficoBolhas
                 SET Despesa = {0}.{1}.f_GetFinanceiroProposta(_CodigoProjeto,@Ano,'D'),
                     Receita = {0}.{1}.f_GetFinanceiroProposta(_CodigoProjeto,@Ano,'R'),
                     Trabalho = {0}.{1}.f_GetTrabalhoProposta(_CodigoProjeto,@Ano),
                     ValorCriterio = 100.00*IsNull((SELECT Sum(moc.[PesoObjetoMatriz])
									    FROM {0}.{1}.Projeto AS p INNER JOIN
										    {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria AND c.DataExclusao IS NULL) INNER JOIN
											{0}.{1}.ProjetoCriterioSelecao AS pcs ON (pcs.CodigoProjeto = p.CodigoProjeto AND pcs.EtapaPreenchimento=0) INNER JOIN
											{0}.{1}.[MatrizObjetoCriterio]	AS [moc] ON (moc.[CodigoCategoria] = p.[CodigoCategoria]
												AND moc.IniciaisTipoObjetoCriterioPai = 'CR'
												AND moc.[CodigoObjetoCriterioPai] = pcs.CodigoCriterioSelecao
												AND moc.CodigoObjetoCriterio = pcs.CodigoCriterioSelecao*1000+ASCII(pcs.CodigoOpcaoCriterioSelecao) ) 
											INNER JOIN {0}.{1}.[MatrizObjetoCriterio] AS [mo2] ON (mo2.CodigoCategoria = moc.[CodigoCategoria]
											    AND mo2.[IniciaisTipoObjetoCriterioPai] = 'GP' AND mo2.[CodigoObjetoCriterio] = pcs.[CodigoCriterioSelecao])
											INNER JOIN {0}.{1}.[GrupoCriterioSelecao] AS [gcs] ON (gcs.[CodigoGrupoCriterio] = mo2.[CodigoObjetoCriterioPai])
											INNER JOIN {0}.{1}.[FatorPortfolio]	AS [fp] ON (fp.[CodigoFatorPortfolio] = gcs.[CodigoFatorPortfolio]
											    AND fp.[ValorSinalFator]>0)
                                        WHERE p.CodigoProjeto = _CodigoProjeto {10}),0),
                      ValorRisco = 100.00*IsNull((SELECT Sum(moc.[PesoObjetoMatriz])
									    FROM {0}.{1}.Projeto AS p INNER JOIN
										    {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria AND c.DataExclusao IS NULL) INNER JOIN
											{0}.{1}.ProjetoCriterioSelecao AS pcs ON (pcs.CodigoProjeto = p.CodigoProjeto AND pcs.EtapaPreenchimento=0) INNER JOIN
											{0}.{1}.[MatrizObjetoCriterio]	AS [moc] ON (moc.[CodigoCategoria] = p.[CodigoCategoria]
												AND moc.IniciaisTipoObjetoCriterioPai = 'CR'
												AND moc.[CodigoObjetoCriterioPai] = pcs.CodigoCriterioSelecao
												AND moc.CodigoObjetoCriterio = pcs.CodigoCriterioSelecao*1000+ASCII(pcs.CodigoOpcaoCriterioSelecao) ) 
											INNER JOIN {0}.{1}.[MatrizObjetoCriterio] AS [mo2] ON (mo2.CodigoCategoria = moc.[CodigoCategoria]
											    AND mo2.[IniciaisTipoObjetoCriterioPai] = 'GP' AND mo2.[CodigoObjetoCriterio] = pcs.[CodigoCriterioSelecao])
											INNER JOIN {0}.{1}.[GrupoCriterioSelecao] AS [gcs] ON (gcs.[CodigoGrupoCriterio] = mo2.[CodigoObjetoCriterioPai])
											INNER JOIN {0}.{1}.[FatorPortfolio]	AS [fp] ON (fp.[CodigoFatorPortfolio] = gcs.[CodigoFatorPortfolio]
											    AND fp.[ValorSinalFator]<0)
                                        WHERE p.CodigoProjeto = _CodigoProjeto {10}),0) 
                    WHERE _CodigoProjeto IN(SELECT p.CodigoProjeto FROM Projeto p WHERE p.DataExclusao IS NULL
					                                                            AND p.CodigoEntidade = @CodigoEntidade
					                                                            {10})
                    
               
               /* Atualiza as informações para os eixos X e Y, caso seja para somar critérios ou riscos */
               UPDATE @tblGraficoBolhas
                  SET EixoX = CASE @TipoEscolhaX WHEN 'SOMACRITERIO' THEN ValorCriterio                      
                                                 WHEN 'SOMARISCO' THEN ValorRisco 
                                                                  ELSE 0 END,
                      EixoY = CASE @TipoEscolhaY WHEN 'SOMACRITERIO' THEN ValorCriterio                      
                                                 WHEN 'SOMARISCO' THEN ValorRisco 
                                                                  ELSE 0 END 
               
               /* Se for para apresentar valores diferentes de Riscos e Critérios, faz a atualização do que foi escolhido */
               IF @TipoEscolhaX = 'CRITERIO'
                  UPDATE @tblGraficoBolhas
                     SET EixoX = 100.00*IsNull((SELECT Sum(moc.[PesoObjetoMatriz])
									    FROM {0}.{1}.Projeto AS p INNER JOIN
										    {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria AND c.DataExclusao IS NULL) INNER JOIN
											{0}.{1}.ProjetoCriterioSelecao AS pcs ON (pcs.CodigoProjeto = p.CodigoProjeto AND pcs.EtapaPreenchimento=0) INNER JOIN
											{0}.{1}.[MatrizObjetoCriterio]	AS [moc] ON (moc.[CodigoCategoria] = p.[CodigoCategoria]
												AND moc.IniciaisTipoObjetoCriterioPai = 'CR'
												AND moc.[CodigoObjetoCriterioPai] = pcs.CodigoCriterioSelecao
												AND moc.CodigoObjetoCriterio = pcs.CodigoCriterioSelecao*1000+ASCII(pcs.CodigoOpcaoCriterioSelecao) ) 
											INNER JOIN {0}.{1}.[MatrizObjetoCriterio] AS [mo2] ON (mo2.CodigoCategoria = moc.[CodigoCategoria]
											    AND mo2.[IniciaisTipoObjetoCriterioPai] = 'GP' AND mo2.[CodigoObjetoCriterio] = pcs.[CodigoCriterioSelecao])
											INNER JOIN {0}.{1}.[GrupoCriterioSelecao] AS [gcs] ON (gcs.[CodigoGrupoCriterio] = mo2.[CodigoObjetoCriterioPai])
											INNER JOIN {0}.{1}.[FatorPortfolio]	AS [fp] ON (fp.[CodigoFatorPortfolio] = gcs.[CodigoFatorPortfolio]
											    AND fp.[ValorSinalFator]>0)
                                        WHERE p.CodigoProjeto = _CodigoProjeto 
                                            {10} 
                                            AND pcs.CodigoCriterioSelecao = @EscolhaX),0)
               
               /* Se for para apresentar valores diferentes de Riscos e Critérios no Eixo Y, faz a atualização do que foi escolhido */
               IF @TipoEscolhaY = 'CRITERIO'
                  UPDATE @tblGraficoBolhas
                     SET EixoY = 100.00*IsNull((SELECT Sum(moc.[PesoObjetoMatriz])
									    FROM {0}.{1}.Projeto AS p INNER JOIN
										    {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria AND c.DataExclusao IS NULL) INNER JOIN
											{0}.{1}.ProjetoCriterioSelecao AS pcs ON (pcs.CodigoProjeto = p.CodigoProjeto AND pcs.EtapaPreenchimento=0) INNER JOIN
											{0}.{1}.[MatrizObjetoCriterio]	AS [moc] ON (moc.[CodigoCategoria] = p.[CodigoCategoria]
												AND moc.IniciaisTipoObjetoCriterioPai = 'CR'
												AND moc.[CodigoObjetoCriterioPai] = pcs.CodigoCriterioSelecao
												AND moc.CodigoObjetoCriterio = pcs.CodigoCriterioSelecao*1000+ASCII(pcs.CodigoOpcaoCriterioSelecao) ) 
											INNER JOIN {0}.{1}.[MatrizObjetoCriterio] AS [mo2] ON (mo2.CodigoCategoria = moc.[CodigoCategoria]
											    AND mo2.[IniciaisTipoObjetoCriterioPai] = 'GP' AND mo2.[CodigoObjetoCriterio] = pcs.[CodigoCriterioSelecao])
											INNER JOIN {0}.{1}.[GrupoCriterioSelecao] AS [gcs] ON (gcs.[CodigoGrupoCriterio] = mo2.[CodigoObjetoCriterioPai])
											INNER JOIN {0}.{1}.[FatorPortfolio]	AS [fp] ON (fp.[CodigoFatorPortfolio] = gcs.[CodigoFatorPortfolio]
											    AND fp.[ValorSinalFator]>0)
                                        WHERE p.CodigoProjeto = _CodigoProjeto 
                                            {10} 
                                            AND pcs.CodigoCriterioSelecao = @EscolhaY),0)   
              IF @Opcao = 'C'                                                                                                                
				  /* Select para trazer os valores agrupados por cenário */
				  SELECT '" + Resources.traducao.cen_rio_1 + @"' AS Cenario,
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario1 = 'S'
				UNION
				  SELECT '" + Resources.traducao.cen_rio_2 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario2 = 'S'
				UNION
				  SELECT '" + Resources.traducao.cen_rio_3 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario3 = 'S'
				UNION
				  SELECT '" + Resources.traducao.cen_rio_4 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario4 = 'S'
				UNION
				  SELECT '" + Resources.traducao.cen_rio_5 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario5 = 'S'    
				UNION
				  SELECT '" + Resources.traducao.cen_rio_6 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario6 = 'S'    
				UNION
				  SELECT '" + Resources.traducao.cen_rio_7 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario7 = 'S'    
				UNION
				  SELECT '" + Resources.traducao.cen_rio_8 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario8 = 'S'  
				UNION
				  SELECT '" + Resources.traducao.cen_rio_9 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario9 = 'S'            
			  ELSE
			     SELECT * FROM @tblGraficoBolhas 
                  WHERE 1 = 1 
                     
                                                               
          END", bancodb, Ownerdb, codigoEntidade, codigoEscolhaX, codigoEscolhaY, tipoEscolhaX, tipoEscolhaY, opcao, codigoCategoria, ano, where, colunaIndicaAcompanhamento);

            return getDataSet(comandoSQL);
        }


        //Retorna no Gráfico somente as bolhas da carteira dos seu respectivo portifólio.
        public DataSet getAnaliseGraficaPorCarteira(int codigoEntidade, int codigoEscolhaX, int codigoEscolhaY, string tipoEscolhaX, string tipoEscolhaY, char opcao, int codigoCategoria, int ano, bool indicaAcompanhamentoPortfolio, int codigoPortfolio, string where)
        {
            string colunaIndicaAcompanhamento = "IndicaSelecaoPortfolio";

            if (indicaAcompanhamentoPortfolio)
            {
                colunaIndicaAcompanhamento = "IndicaAcompanhamentoPortfolio";
            }

            comandoSQL = string.Format(
                @"BEGIN  
          DECLARE @TipoEscolhaX Varchar(15),
                  @EscolhaX Int,
                  @TipoEscolhaY Varchar(15),
                  @EscolhaY Int,
                  @CodigoEntidade Int,
                  @Opcao Char(1),
                  @Ano SmallInt,
                  @CodigoCategoria Int
                 
                  
          SET @TipoEscolhaX = '{5}' --> Colocar aqui qual o tipo de escolha para o eixo X
	  		  SET @EscolhaX = {3}             --> Colocar aqui o valor da escolha do usuário (só terá efeito se for um critério) para o Eixo X
	  		  SET @TipoEscolhaY = '{6}' --> Colocar aqui qual o tipo de escolha para o eixo Y
	  		  SET @EscolhaY = {4}             --> Colocar aqui o valor da escolha do usuário (só terá efeito se for um critério) para o Eixo Y
	  		  SET @CodigoEntidade = {2}
                  SET @Opcao = '{7}' --> Parâmetro. Se o valor for igual a 'C', traz o resultado agrupado por cenário. Se for 'P', agrupa por projeto
          
          --=-=-=-=-=-=> PARÂMETROS NOVOS INCLUIDOS EM 06/02/2010 <--=-=-=-=-=-=-=
	           SET @CodigoCategoria = {8} --> Parâmetro para indicar a categoria escolhida pelo usuário.        
	           SET @Ano = {9} --> Parâmetro para indicar o ano de análise para trazer os valores financeiros.
                  
          DECLARE @tblGraficoBolhas TABLE
            (_CodigoProjeto Int,
             _NomeProjeto Varchar(255),
             EixoX Decimal(18,4),
             EixoY Decimal(18,4),
             Despesa Decimal(18,4) DEFAULT 0,
             Receita Decimal(18,4) DEFAULT 0,
             ValorRisco Decimal(18,4) DEFAULT 0,
             ValorCriterio Decimal(18,4) DEFAULT 0,
             IndicaCenario1 Char(1),
             IndicaCenario2 Char(1),
             IndicaCenario3 Char(1),
             IndicaCenario4 Char(1),
             IndicaCenario5 Char(1),
             IndicaCenario6 Char(1),
             IndicaCenario7 Char(1),
             IndicaCenario8 Char(1),
             IndicaCenario9 Char(1),
             _CodigoCategoria SmallInt,
             DescricaoCategoria Varchar(255),
             Trabalho Decimal(18,1) DEFAULT 0) --> Incluída a variável para tratamento de RH.
             
              /* Insere os projetos na tabela temporária de retorno */
              IF @CodigoCategoria = -1
				  INSERT INTO @tblGraficoBolhas
				  (_CodigoProjeto,
				   _NomeProjeto,
				   EixoX,
				   EixoY,
				   _CodigoCategoria,
				   DescricaoCategoria,
				   IndicaCenario1,
				   IndicaCenario2,
				   IndicaCenario3,
				   IndicaCenario4,
				   IndicaCenario5,
				   IndicaCenario6,
				   IndicaCenario7,
				   IndicaCenario8,
				   IndicaCenario9)
				 SELECT p.CodigoProjeto,
						p.NomeProjeto,0,0,
						p.CodigoCategoria,
						c.DescricaoCategoria,
						p.IndicaCenario1,
						p.IndicaCenario2,
						p.IndicaCenario3,
						p.IndicaCenario4,
						p.IndicaCenario5,
						p.IndicaCenario6,
						p.IndicaCenario7,
						p.IndicaCenario8,
						p.IndicaCenario9
				   FROM {0}.{1}.Projeto AS p INNER JOIN
						{0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria
                                               AND c.DataExclusao IS NULL) INNER JOIN
                        f_GetProjetosSelecaoBalanceamento({12}, {8}, {2}) AS GPSB  ON (GPSB._CodigoProjeto = P.CodigoProjeto) INNER JOIN
						{0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
														 AND s.{11} = 'S')
				  WHERE p.DataExclusao IS NULL
					AND p.CodigoEntidade = @CodigoEntidade					
			  ELSE
			     INSERT INTO @tblGraficoBolhas
				  (_CodigoProjeto,
				   _NomeProjeto,
				   EixoX,
				   EixoY,
				   _CodigoCategoria,
				   DescricaoCategoria,
				   IndicaCenario1,
				   IndicaCenario2,
				   IndicaCenario3,
				   IndicaCenario4,
				   IndicaCenario5,
				   IndicaCenario6,
				   IndicaCenario7,
				   IndicaCenario8,
				   IndicaCenario9)
				 SELECT p.CodigoProjeto,
						p.NomeProjeto,0,0,
						p.CodigoCategoria,
						c.DescricaoCategoria,
						p.IndicaCenario1,
						p.IndicaCenario2,
						p.IndicaCenario3,
						p.IndicaCenario4,
						p.IndicaCenario5,
						p.IndicaCenario6,
						p.IndicaCenario7,
						p.IndicaCenario8,
						p.IndicaCenario9
				   FROM {0}.{1}.Projeto AS p INNER JOIN
						{0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria
                                               AND c.DataExclusao IS NULL) INNER JOIN
                        f_GetProjetosSelecaoBalanceamento({12}, {8}, {2}) AS GPSB  ON (GPSB._CodigoProjeto = P.CodigoProjeto) INNER JOIN
						{0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
														 AND s.{11} = 'S')
				  WHERE p.DataExclusao IS NULL
					AND p.CodigoEntidade = @CodigoEntidade
					AND p.CodigoCategoria = @CodigoCategoria
--					{10}
              
              /* Atualiza o resultado para tamanho da bolha */    
              UPDATE @tblGraficoBolhas
                 SET Despesa = {0}.{1}.f_GetFinanceiroProposta(_CodigoProjeto,@Ano,'D'),
                     Receita = {0}.{1}.f_GetFinanceiroProposta(_CodigoProjeto,@Ano,'R'),
                     Trabalho = {0}.{1}.f_GetTrabalhoProposta(_CodigoProjeto,@Ano),
                     ValorCriterio = 100.00*IsNull((SELECT Sum(moc.[PesoObjetoMatriz])
									    FROM {0}.{1}.Projeto AS p INNER JOIN
										    {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria AND c.DataExclusao IS NULL) INNER JOIN
											{0}.{1}.ProjetoCriterioSelecao AS pcs ON (pcs.CodigoProjeto = p.CodigoProjeto AND pcs.EtapaPreenchimento=0) INNER JOIN
											{0}.{1}.[MatrizObjetoCriterio]	AS [moc] ON (moc.[CodigoCategoria] = p.[CodigoCategoria]
												AND moc.IniciaisTipoObjetoCriterioPai = 'CR'
												AND moc.[CodigoObjetoCriterioPai] = pcs.CodigoCriterioSelecao
												AND moc.CodigoObjetoCriterio = pcs.CodigoCriterioSelecao*1000+ASCII(pcs.CodigoOpcaoCriterioSelecao) ) 
											INNER JOIN {0}.{1}.[MatrizObjetoCriterio] AS [mo2] ON (mo2.CodigoCategoria = moc.[CodigoCategoria]
											    AND mo2.[IniciaisTipoObjetoCriterioPai] = 'GP' AND mo2.[CodigoObjetoCriterio] = pcs.[CodigoCriterioSelecao])
											INNER JOIN {0}.{1}.[GrupoCriterioSelecao] AS [gcs] ON (gcs.[CodigoGrupoCriterio] = mo2.[CodigoObjetoCriterioPai])
											INNER JOIN {0}.{1}.[FatorPortfolio]	AS [fp] ON (fp.[CodigoFatorPortfolio] = gcs.[CodigoFatorPortfolio]
											    AND fp.[ValorSinalFator]>0)
                                        WHERE p.CodigoProjeto = _CodigoProjeto {10}),0),
                      ValorRisco = 100.00*IsNull((SELECT Sum(moc.[PesoObjetoMatriz])
									    FROM {0}.{1}.Projeto AS p INNER JOIN
										    {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria AND c.DataExclusao IS NULL) INNER JOIN
											{0}.{1}.ProjetoCriterioSelecao AS pcs ON (pcs.CodigoProjeto = p.CodigoProjeto AND pcs.EtapaPreenchimento=0) INNER JOIN
											{0}.{1}.[MatrizObjetoCriterio]	AS [moc] ON (moc.[CodigoCategoria] = p.[CodigoCategoria]
												AND moc.IniciaisTipoObjetoCriterioPai = 'CR'
												AND moc.[CodigoObjetoCriterioPai] = pcs.CodigoCriterioSelecao
												AND moc.CodigoObjetoCriterio = pcs.CodigoCriterioSelecao*1000+ASCII(pcs.CodigoOpcaoCriterioSelecao) ) 
											INNER JOIN {0}.{1}.[MatrizObjetoCriterio] AS [mo2] ON (mo2.CodigoCategoria = moc.[CodigoCategoria]
											    AND mo2.[IniciaisTipoObjetoCriterioPai] = 'GP' AND mo2.[CodigoObjetoCriterio] = pcs.[CodigoCriterioSelecao])
											INNER JOIN {0}.{1}.[GrupoCriterioSelecao] AS [gcs] ON (gcs.[CodigoGrupoCriterio] = mo2.[CodigoObjetoCriterioPai])
											INNER JOIN {0}.{1}.[FatorPortfolio]	AS [fp] ON (fp.[CodigoFatorPortfolio] = gcs.[CodigoFatorPortfolio]
											    AND fp.[ValorSinalFator]<0)
                                        WHERE p.CodigoProjeto = _CodigoProjeto {10}),0) 
                    WHERE _CodigoProjeto IN(SELECT p.CodigoProjeto FROM Projeto p WHERE p.DataExclusao IS NULL
					                                                            AND p.CodigoEntidade = @CodigoEntidade
					                                                            {10})
                    
               
               /* Atualiza as informações para os eixos X e Y, caso seja para somar critérios ou riscos */
               UPDATE @tblGraficoBolhas
                  SET EixoX = CASE @TipoEscolhaX WHEN 'SOMACRITERIO' THEN ValorCriterio                      
                                                 WHEN 'SOMARISCO' THEN ValorRisco 
                                                                  ELSE 0 END,
                      EixoY = CASE @TipoEscolhaY WHEN 'SOMACRITERIO' THEN ValorCriterio                      
                                                 WHEN 'SOMARISCO' THEN ValorRisco 
                                                                  ELSE 0 END 
               
               /* Se for para apresentar valores diferentes de Riscos e Critérios, faz a atualização do que foi escolhido */
               IF @TipoEscolhaX = 'CRITERIO'
                  UPDATE @tblGraficoBolhas
                     SET EixoX = 100.00*IsNull((SELECT Sum(moc.[PesoObjetoMatriz])
									    FROM {0}.{1}.Projeto AS p INNER JOIN
										    {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria AND c.DataExclusao IS NULL) INNER JOIN
											{0}.{1}.ProjetoCriterioSelecao AS pcs ON (pcs.CodigoProjeto = p.CodigoProjeto AND pcs.EtapaPreenchimento=0) INNER JOIN
											{0}.{1}.[MatrizObjetoCriterio]	AS [moc] ON (moc.[CodigoCategoria] = p.[CodigoCategoria]
												AND moc.IniciaisTipoObjetoCriterioPai = 'CR'
												AND moc.[CodigoObjetoCriterioPai] = pcs.CodigoCriterioSelecao
												AND moc.CodigoObjetoCriterio = pcs.CodigoCriterioSelecao*1000+ASCII(pcs.CodigoOpcaoCriterioSelecao) ) 
											INNER JOIN {0}.{1}.[MatrizObjetoCriterio] AS [mo2] ON (mo2.CodigoCategoria = moc.[CodigoCategoria]
											    AND mo2.[IniciaisTipoObjetoCriterioPai] = 'GP' AND mo2.[CodigoObjetoCriterio] = pcs.[CodigoCriterioSelecao])
											INNER JOIN {0}.{1}.[GrupoCriterioSelecao] AS [gcs] ON (gcs.[CodigoGrupoCriterio] = mo2.[CodigoObjetoCriterioPai])
											INNER JOIN {0}.{1}.[FatorPortfolio]	AS [fp] ON (fp.[CodigoFatorPortfolio] = gcs.[CodigoFatorPortfolio]
											    AND fp.[ValorSinalFator]>0)
                                        WHERE p.CodigoProjeto = _CodigoProjeto 
                                            {10} 
                                            AND pcs.CodigoCriterioSelecao = @EscolhaX),0)
               
               /* Se for para apresentar valores diferentes de Riscos e Critérios no Eixo Y, faz a atualização do que foi escolhido */
               IF @TipoEscolhaY = 'CRITERIO'
                  UPDATE @tblGraficoBolhas
                     SET EixoY = 100.00*IsNull((SELECT Sum(moc.[PesoObjetoMatriz])
									    FROM {0}.{1}.Projeto AS p INNER JOIN
										    {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria AND c.DataExclusao IS NULL) INNER JOIN
											{0}.{1}.ProjetoCriterioSelecao AS pcs ON (pcs.CodigoProjeto = p.CodigoProjeto AND pcs.EtapaPreenchimento=0) INNER JOIN
											{0}.{1}.[MatrizObjetoCriterio]	AS [moc] ON (moc.[CodigoCategoria] = p.[CodigoCategoria]
												AND moc.IniciaisTipoObjetoCriterioPai = 'CR'
												AND moc.[CodigoObjetoCriterioPai] = pcs.CodigoCriterioSelecao
												AND moc.CodigoObjetoCriterio = pcs.CodigoCriterioSelecao*1000+ASCII(pcs.CodigoOpcaoCriterioSelecao) ) 
											INNER JOIN {0}.{1}.[MatrizObjetoCriterio] AS [mo2] ON (mo2.CodigoCategoria = moc.[CodigoCategoria]
											    AND mo2.[IniciaisTipoObjetoCriterioPai] = 'GP' AND mo2.[CodigoObjetoCriterio] = pcs.[CodigoCriterioSelecao])
											INNER JOIN {0}.{1}.[GrupoCriterioSelecao] AS [gcs] ON (gcs.[CodigoGrupoCriterio] = mo2.[CodigoObjetoCriterioPai])
											INNER JOIN {0}.{1}.[FatorPortfolio]	AS [fp] ON (fp.[CodigoFatorPortfolio] = gcs.[CodigoFatorPortfolio]
											    AND fp.[ValorSinalFator]>0)
                                        WHERE p.CodigoProjeto = _CodigoProjeto 
                                            {10} 
                                            AND pcs.CodigoCriterioSelecao = @EscolhaY),0)   
              IF @Opcao = 'C'                                                                                                                
				  /* Select para trazer os valores agrupados por cenário */
				  SELECT '" + Resources.traducao.cen_rio_1 + @"' AS Cenario,
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario1 = 'S'
				UNION
				  SELECT '" + Resources.traducao.cen_rio_2 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario2 = 'S'
				UNION
				  SELECT '" + Resources.traducao.cen_rio_3 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario3 = 'S'
				UNION
				  SELECT '" + Resources.traducao.cen_rio_4 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario4 = 'S'
				UNION
				  SELECT '" + Resources.traducao.cen_rio_5 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario5 = 'S'    
				UNION
				  SELECT '" + Resources.traducao.cen_rio_6 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario6 = 'S'    
				UNION
				  SELECT '" + Resources.traducao.cen_rio_7 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario7 = 'S'    
				UNION
				  SELECT '" + Resources.traducao.cen_rio_8 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario8 = 'S'  
				UNION
				  SELECT '" + Resources.traducao.cen_rio_9 + @"',
						  CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                    ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                         CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						 ISNULL(Sum(Despesa), 0) AS Despesa,
						 ISNULL(Sum(Receita), 0) AS Receita,
						 ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						 ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						 ISNULL(Sum(Trabalho),0) AS Trabalho,
						 COUNT(1) AS QuantidadeProjetos
					FROM @tblGraficoBolhas
				   WHERE IndicaCenario9 = 'S'            
			  ELSE
			     SELECT * FROM @tblGraficoBolhas 
                  WHERE 1 = 1 
                     
                                                               
          END", bancodb, Ownerdb, codigoEntidade, codigoEscolhaX, codigoEscolhaY, tipoEscolhaX, tipoEscolhaY, opcao, codigoCategoria, ano, where, colunaIndicaAcompanhamento, codigoPortfolio);

            return getDataSet(comandoSQL);
        }

        public DataSet getProjetosPorCriterio(int ano, int codigoPortfolio, int codigoEntidade, string where)
        {
            comandoSQL = string.Format(
                @"select * from {0}.{1}.f_GetProjetosSelecaoBalanceamento({2}, {3}, {4})
                WHERE 1 = 1 {5}", bancodb, Ownerdb, codigoPortfolio, ano, codigoEntidade, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getProjetosPorCategoria(int codigoPortfolio, int codigoEntidade, int ano, string where)
        {
            comandoSQL = string.Format(
                   @"SELECT COUNT(1) AS Projetos, 
                       SiglaCategoria AS Sigla,
                       Categoria AS DescricaoCategoria, 
                       ISNULL(SUM(Custo), 0) AS Custo, 
                       ISNULL(SUM(Receita), 0) AS Receita, 
	                   ISNULL(SUM(RH), 0) AS Trabalho, 
	                   ISNULL(SUM(ScoreRiscos), 0) AS Riscos, 
	                   ISNULL(SUM(ScoreCriterios), 0) AS Criterios,
	                   Sum(CASE WHEN IndicaProjetoNovo = 'S' THEN 1 ELSE 0 END) AS ProjetosNovos,
	                   Sum(CASE WHEN IndicaProjetoNovo = 'N' THEN 1 ELSE 0 END) AS ProjetosExecucao
                   FROM {0}.{1}.f_GetProjetosSelecaoBalanceamento({2}, {3}, {4}) fpsb
                  WHERE 1 = 1 {5}
                  GROUP BY Categoria, SiglaCategoria, 
                       _CodigoCategoria", bancodb, Ownerdb, codigoPortfolio, ano, codigoEntidade, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getNumerosCenario(int codigoPortfolio, int codigoEntidade, int ano, string where)
        {
            comandoSQL = string.Format(
                    @"SELECT COUNT(1) AS Projetos, 
                       ISNULL(SUM(Custo), 0) AS Custo, 
                       ISNULL(SUM(Receita), 0) AS Receita, 
	                   ISNULL(SUM(RH), 0) AS Trabalho, 
	                   ISNULL(AVG(ScoreRiscos), 0) AS Riscos, 
	                   ISNULL(AVG(ScoreCriterios), 0) AS Criterios,	                   
	                   ISNULL(Sum(CASE WHEN IndicaProjetoNovo = 'S' THEN 1 ELSE 0 END), 0) AS ProjetosNovos,
	                   ISNULL(Sum(CASE WHEN IndicaProjetoNovo = 'N' THEN 1 ELSE 0 END), 0) AS ProjetosExecucao
                   FROM {0}.{1}.f_GetProjetosSelecaoBalanceamento({2}, {3}, {4}) fpsb
                  WHERE 1 = 1 {5}", bancodb, Ownerdb, codigoPortfolio, ano, codigoEntidade, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getFluxoCaixaCenario(int codigoPortfolio, int codigoEntidade, int cenario, string where)
        {
            comandoSQL = string.Format(
                   @"BEGIN
                  /* Apresenta informações sobre fluxos de caixa de projetos */
                  DECLARE @CodigoPortfolio Int,
		                  @CodigoEntidade Int,
		                  @Cenario SmallInt,
		                  @CodigoProjetoCursor Int,
						  @Mes SmallInt


                  SET @CodigoPortfolio = {2} -- Parâmetro
                  SET @CodigoEntidade = {3}
                  SET @Cenario = {4}

                  DECLARE @tblOLAP TABLE
                    (_CodigoProjeto Int, 
                     _Ano SmallInt,
                     _Mes SmallInt,
                     IncluidoPortfolio Char(3) DEFAULT 'NÃO',
                     Despesa Decimal(18,4),
                     Receita Decimal(18,4))
                     
                  DECLARE @tblAcum TABLE
                    (_Ano SmallInt,
                     Periodo Varchar(100),  
					 Mes SmallInt,   
                     Saldo Decimal(18,4) Default 0)     

                 /* Insere os projetos*/    
                 INSERT INTO @tblOLAP
                  (_CodigoProjeto,  _Ano, _Mes, Despesa, Receita)
                 SELECT DISTINCT
	                sfc.CodigoProjeto,  
	                sfc.Ano,
	                sfc.Mes,
	                SUM(sfc.ValorCusto + sfc.ValorInvestimento),
	                SUM(sfc.ValorReceita)
                   FROM {0}.{1}.SelecaoFluxoCaixa AS sfc 
                  WHERE sfc.CodigoProjeto IN (SELECT CodigoProjeto FROM {0}.{1}.f_GetProjetosSituacao(@CodigoEntidade,'Todos')) 
                    AND sfc.CodigoProjeto IN (SELECT CodigoProjeto FROM {0}.{1}.f_GetProjetosCenario(@CodigoEntidade, @Cenario)) 
                    AND sfc.Ano IN (SELECT Ano 
		                              FROM {0}.{1}.PeriodoAnalisePortfolio
		                             WHERE IndicaAnoPeriodoEditavel = 'S')
                  GROUP BY 	sfc.CodigoProjeto,  
		                    sfc.Ano,
	                        sfc.Mes	             

                   /* O código abaixo é para trazer os saldos acumulados por projeto */
                  DECLARE @SaldoAcum Decimal(18,4),
	                      @Ano SmallInt,
	                      @Periodo Varchar(100),
	                      @Saldo Decimal(18,4),
	                      @CodigoProjetoAtual Int

                   DECLARE cCursorSaldoAcumulado CURSOR LOCAL FOR
                   SELECT _Ano AS Ano,
	                      {0}.{1}.f_GetPeriodoExtenso(@CodigoEntidade, _Ano,_Mes) AS Periodo,
						  CASE WHEN Convert(Varchar,_Ano) = {0}.{1}.f_GetPeriodoExtenso(@CodigoEntidade, _Ano,_Mes) THEN 0 ELSE _Mes END,
	                      Sum(Receita - Despesa) AS Saldo                       
                    FROM @tblOLAP
                    GROUP BY _Ano,
	                         {0}.{1}.f_GetPeriodoExtenso(@CodigoEntidade, _Ano,_Mes),
							 CASE WHEN Convert(Varchar,_Ano) = {0}.{1}.f_GetPeriodoExtenso(@CodigoEntidade, _Ano,_Mes) THEN 0 ELSE _Mes END
                    ORDER BY _Ano,
	                         {0}.{1}.f_GetPeriodoExtenso(@CodigoEntidade, _Ano,_Mes),
							 CASE WHEN Convert(Varchar,_Ano) = {0}.{1}.f_GetPeriodoExtenso(@CodigoEntidade, _Ano,_Mes) THEN 0 ELSE _Mes END

                  OPEN cCursorSaldoAcumulado

                  FETCH NEXT FROM cCursorSaldoAcumulado 
                             INTO @Ano,
			                      @Periodo,    
								  @Mes,              
			                      @Saldo
                			      
                  SET @CodigoProjetoAtual = @CodigoProjetoCursor                                           
                  SET @SaldoAcum = 0                                           
                  WHILE @@FETCH_STATUS = 0 
                    BEGIN
                      SET @SaldoAcum = @SaldoAcum + IsNull(@Saldo,0)
                  
                     INSERT INTO @tblAcum
					   (_Ano, Periodo, Mes, Saldo)
                       VALUES (@Ano, @Periodo, @Mes, @SaldoAcum)

                      FETCH NEXT FROM cCursorSaldoAcumulado 
				                 INTO @Ano,
					                  @Periodo,
									  @Mes,
					                  @Saldo
                		                             
                    END                                                   

                  CLOSE cCursorSaldoAcumulado
                  DEALLOCATE cCursorSaldoAcumulado  

                  /* Retorna o fluxo de caixa */       
                  SELECT _Ano AS Ano,
		                 Periodo AS Periodo,
						 Mes,
		                 Sum(Saldo) AS Saldo                                      
	                FROM @tblAcum
				GROUP BY _Ano, Mes, Periodo
                  
                END", bancodb, Ownerdb, codigoPortfolio, codigoEntidade, cenario, where);

            return getDataSet(comandoSQL);
        }

        public void publicaPortfolio(int codigoPortfolio, int codigoEntidade, int cenario, ref int registrosAfetados)
        {

            comandoSQL = string.Format(@"EXEC {0}.{1}.p_PublicaPortfolio {2}, {3}, {4}
               ", bancodb, Ownerdb, codigoPortfolio, codigoEntidade, cenario);

            execSQL(comandoSQL, ref registrosAfetados);

        }

        public string getGraficoProjetosCategoria(DataTable dt, string colunaValor, string descricao, string formatacao, string titulo, int fonte, string urlImage)
        {
            //Cria as variáveis para a formação do XML
            StringBuilder xml = new StringBuilder();
            StringBuilder xmlAux = new StringBuilder();

            int i = 0;

            try
            {
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    xmlAux.Append(string.Format(@"<set toolText=""{0} - {2}: {3:" + formatacao + @"}"" label=""{4}"" value=""{1}""/>", dt.Rows[i]["DescricaoCategoria"].ToString(), dt.Rows[i][colunaValor].ToString(), descricao, double.Parse(dt.Rows[i][colunaValor].ToString()), dt.Rows[i]["Sigla"].ToString()));
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
            xml.Append("<chart imageSave=\"1\" imageSaveURL=\"" + urlImage + "\" caption=\"" + titulo + "\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\" labelDistance=\"1\" enablesmartlabels=\"0\"" +
                " chartTopMargin=\"5\" showZeroPies=\"0\" chartRightMargin=\"8\" inDecimalSeparator=\",\" inThousandSeparator=\".\" decimalSeparator=\",\" thousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
                " showValues=\"0\" " + usarGradiente + usarBordasArredondadas + exportar + " use3Dlighting=\"0\" legendBorderAlpha=\"0\" showShadow=\"0\"  showLabels=\"1\" decimals=\"0\" baseFontSize=\"" + fonte + "\">");
            xml.Append(xmlAux.ToString());
            xml.Append("<styles>");
            xml.Append("<definition>");
            xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Roboto\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
            xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Roboto\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
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

        public string getGraficoFluxoCaixaCategoria(DataTable dt, string titulo, int fonte)
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

            xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""1"" chartTopMargin=""5"" canvasBgColor=""F7F7F7"" 
                                    shownames=""1"" showvalues=""0""  legendBorderAlpha=""0"" numVisiblePlot=""12"" showBorder=""0"" BgColor=""F7F7F7""
                                    inDecimalSeparator="","" showLegend=""0"" inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" >", usarGradiente + usarBordasArredondadas + exportar
                                                                                                                                                              , titulo
                                                                                                                                                              , fonte));
            xml.Append(string.Format(@"<categories>"));

            try
            {
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    xml.Append(string.Format(@"<category label=""{0}"" />", dt.Rows[i]["Periodo"].ToString()));
                }

            }
            catch
            {
                return "";
            }

            xml.Append(string.Format(@"</categories>"));
            xml.Append(string.Format(@"<dataset seriesName=""Saldo"" showValues=""0"">"));

            try
            {
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    xml.Append(string.Format(@"<set value=""{0}"" /> ", dt.Rows[i]["Saldo"].ToString()));
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

        public string getGraficoMetaIndicadorProjeto(DataTable dt, string titulo, int fonte, string unidadeMedida)
        {
            //Cria as variáveis para a formação do XML
            StringBuilder xml = new StringBuilder();

            int i = 0;

            string exportar = "";

            if (unidadeMedida == "%")
                unidadeMedida = @" numberSuffix=""%"" ";
            else
            {
                if (unidadeMedida.Contains("%"))
                    unidadeMedida = string.Format(@" numberPrefix=""{0} "" ", unidadeMedida);
                else
                    unidadeMedida = "";
            }

            if (fonte > 12)
            {
                exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
            }
            else
            {
                exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
            }
            //rotateLabels=""1"" slantLabels=""1""
            xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""1"" chartTopMargin=""5"" canvasBgColor=""F7F7F7"" 
                                    shownames=""1"" showvalues=""0""  legendBorderAlpha=""0"" showBorder=""0"" BgColor=""F7F7F7"" {3} chartRightMargin=""28"" rotateLabels=""1"" slantLabels=""1""
                                    inDecimalSeparator="","" showLegend=""1"" inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" >", usarGradiente + usarBordasArredondadas + exportar
                                                                                                                                                              , titulo
                                                                                                                                                              , fonte
                                                                                                                                                              , unidadeMedida));
            xml.Append(string.Format(@"<categories>"));

            try
            {
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    xml.Append(string.Format(@"<category label=""{0}"" />", dt.Rows[i]["Periodo"].ToString()));
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
                    xml.Append(string.Format(@"<set value=""{0}"" /> ", dt.Rows[i]["MetaAcumuladaAno"].ToString()));
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
                    xml.Append(string.Format(@"<set value=""{0}"" /> ", dt.Rows[i]["ResultadoAcumuladoAno"].ToString()));
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

        public string getGraficoDesempenhoMetaIndicadorProjeto(DataTable dt, int fonte, string unidadeMedida, int codigoEntidade)
        {
            //Cria as variáveis para a formação do XML
            StringBuilder xml = new StringBuilder();

            bool inverterCores = false;


            string exportar = "";

            double meta = 0, resultado = 0, faixaVerde = 0, faixaVermelha = 0, faixaAmarela = 0, faixaAzul = 0;

            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["Polaridade"] + "" != "")
                    inverterCores = dt.Rows[0]["Polaridade"].ToString().Trim() == "NEG";


                if (dt.Rows[0]["MetaAcumuladaAno"] + "" != "")
                    meta = double.Parse(dt.Rows[0]["MetaAcumuladaAno"].ToString());

                if (dt.Rows[0]["ResultadoAcumuladoAno"] + "" != "")
                    resultado = double.Parse(dt.Rows[0]["ResultadoAcumuladoAno"].ToString());
            }

            DataSet dsParam = getParametrosSistema(codigoEntidade, "stSatisfatorio", "stAtencao", "stCritico");

            if (DataSetOk(dsParam) && DataTableOk(dsParam.Tables[0]))
            {
                double vermelho = double.Parse(dsParam.Tables[0].Rows[0]["stCritico"].ToString());
                double amarelo = double.Parse(dsParam.Tables[0].Rows[0]["stAtencao"].ToString());
                double verde = double.Parse(dsParam.Tables[0].Rows[0]["stSatisfatorio"].ToString());

                faixaVermelha = (meta == 0) ? vermelho : (vermelho * meta) / 100;
                faixaAmarela = (meta == 0) ? amarelo : (amarelo * meta) / 100;
                faixaVerde = (meta == 0) ? verde : (verde * meta) / 100;

                double difFaixas = (faixaVerde - faixaAmarela) + faixaVerde;

                faixaAzul = resultado > difFaixas ? resultado : difFaixas;
            }


            if (unidadeMedida == "%")
                unidadeMedida = @" numberSuffix=""%"" ";
            else
            {
                if (unidadeMedida.Contains("$"))
                    unidadeMedida = string.Format(@" numberPrefix=""{0} "" ", unidadeMedida);
                else
                    unidadeMedida = "";
            }

            if (fonte > 12)
            {
                exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
            }
            else
            {
                exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
            }

            xml.Append(string.Format(@"<chart lowerLimit=""0"" upperLimit=""{10}"" baseFontSize=""{2}"" canvasBgColor=""F7F7F7"" showBorder=""0"" BgColor=""F7F7F7""
                                    palette=""1"" {1} chartRightMargin=""5"" chartLeftMargin=""5"" ledSize=""1"" formatNumber=""1"" decimals=""0"" formatNumberScale=""1""
                                    ledGap=""0"" ticksBelowGauge=""0"" inDecimalSeparator="","" decimalSeparator="","" thousandSeparator=""."" inThousandSeparator=""."" {0} >
                                       <colorRange>
                                      <color minValue=""0"" maxValue=""{4}"" code=""{7}"" /> 
                                      <color minValue=""{4}"" maxValue=""{5}"" code=""{8}"" /> 
                                      <color minValue=""{5}"" maxValue=""{6}"" code=""{9}"" /> 
                                      <color minValue=""{6}"" maxValue=""{10}"" code=""{11}"" /> 
                                      </colorRange>
                                      <value>{3}</value> 
                                      </chart>", usarGradiente + usarBordasArredondadas + exportar
                                                   , unidadeMedida
                                                   , fonte
                                                   , resultado
                                                   , faixaVermelha.ToString().Replace(',', '.')
                                                   , faixaAmarela.ToString().Replace(',', '.')
                                                   , faixaVerde.ToString().Replace(',', '.')
                                                   , (inverterCores) ? corSatisfatorio : corCritico
                                                   , corAtencao
                                                   , (inverterCores) ? corCritico : corSatisfatorio
                                                   , faixaAzul.ToString().Replace(',', '.')
                                                   , corExcelente));


            return xml.ToString();
        }

        public DataSet getRelatorioBalanceamento(int codigoPortfolio, int ano, int codigoEntidade)
        {
            string comandoSQL = string.Format(@"
        BEGIN  
            DECLARE @TipoEscolhaX Varchar(15),
                    @EscolhaX Int,
                    @TipoEscolhaY Varchar(15),
                    @EscolhaY Int,
                    @CodigoEntidade Int,
                    @Opcao Char(1),
                    @Ano SmallInt,
                    @CodigoCategoria Int
                 
                  
                    SET @TipoEscolhaX = 'SOMARISCO' --> Colocar aqui qual o tipo de escolha para o eixo X
	  		        SET @EscolhaX = -2             --> Colocar aqui o valor da escolha do usuário (só terá efeito se for um critério) para o Eixo X
	  		        SET @TipoEscolhaY = 'SOMACRITERIO' --> Colocar aqui qual o tipo de escolha para o eixo Y
	  		        SET @EscolhaY = -1             --> Colocar aqui o valor da escolha do usuário (só terá efeito se for um critério) para o Eixo Y
	  		        SET @CodigoEntidade = {2}
                    SET @Opcao = 'C' --> Parâmetro. Se o valor for igual a 'C', traz o resultado agrupado por cenário. Se for 'P', agrupa por projeto
          
                    --=-=-=-=-=-=> PARÂMETROS NOVOS INCLUIDOS EM 06/02/2010 <--=-=-=-=-=-=-=
	                SET @CodigoCategoria = -1 --> Parâmetro para indicar a categoria escolhida pelo usuário.        
	                SET @Ano = {3} --> Parâmetro para indicar o ano de análise para trazer os valores financeiros.
                  
                    DECLARE @tblGraficoBolhas TABLE
                    (_CodigoProjeto Int,
                     _NomeProjeto Varchar(255),
                     EixoX Decimal(18,4),
                     EixoY Decimal(18,4),
                     Despesa Decimal(18,4) DEFAULT 0,
                     Receita Decimal(18,4) DEFAULT 0,
                     ValorRisco Decimal(18,4) DEFAULT 0,
                     ValorCriterio Decimal(18,4) DEFAULT 0,
                     IndicaCenario1 Char(1),
                     IndicaCenario2 Char(1),
                     IndicaCenario3 Char(1),
                     IndicaCenario4 Char(1),
                     IndicaCenario5 Char(1),
                     IndicaCenario6 Char(1),
                     IndicaCenario7 Char(1),
                     IndicaCenario8 Char(1),
                     IndicaCenario9 Char(1),
                     _CodigoCategoria SmallInt,
                     DescricaoCategoria Varchar(255),
                     Trabalho Decimal(18,1) DEFAULT 0) --> Incluída a variável para tratamento de RH.
                     
                      /* Insere os projetos na tabela temporária de retorno */
                      IF @CodigoCategoria = -1
				          INSERT INTO @tblGraficoBolhas
				          (_CodigoProjeto,
				           _NomeProjeto,
				           EixoX,
				           EixoY,
				           _CodigoCategoria,
				           DescricaoCategoria,
				           IndicaCenario1,
				           IndicaCenario2,
				           IndicaCenario3,
				           IndicaCenario4,
				           IndicaCenario5,
				           IndicaCenario6,
				           IndicaCenario7,
				           IndicaCenario8,
				           IndicaCenario9)
				         SELECT p.CodigoProjeto,
						        p.NomeProjeto,0,0,
						        p.CodigoCategoria,
						        c.DescricaoCategoria,
						        p.IndicaCenario1,
						        p.IndicaCenario2,
						        p.IndicaCenario3,
						        p.IndicaCenario4,
						        p.IndicaCenario5,
						        p.IndicaCenario6,
						        p.IndicaCenario7,
						        p.IndicaCenario8,
						        p.IndicaCenario9
				           FROM {0}.{1}.Projeto AS p INNER JOIN
						        {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria
                                                       AND c.DataExclusao IS NULL) INNER JOIN
						        {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
														         AND s.IndicaSelecaoPortfolio = 'S')
				          WHERE p.DataExclusao IS NULL
					        AND p.CodigoEntidade = @CodigoEntidade					
			          ELSE
			             INSERT INTO @tblGraficoBolhas
				          (_CodigoProjeto,
				           _NomeProjeto,
				           EixoX,
				           EixoY,
				           _CodigoCategoria,
				           DescricaoCategoria,
				           IndicaCenario1,
				           IndicaCenario2,
				           IndicaCenario3,
				           IndicaCenario4,
				           IndicaCenario5,
				           IndicaCenario6,
				           IndicaCenario7,
				           IndicaCenario8,
				           IndicaCenario9)
				         SELECT p.CodigoProjeto,
						        p.NomeProjeto,0,0,
						        p.CodigoCategoria,
						        c.DescricaoCategoria,
						        p.IndicaCenario1,
						        p.IndicaCenario2,
						        p.IndicaCenario3,
						        p.IndicaCenario4,
						        p.IndicaCenario5,
						        p.IndicaCenario6,
						        p.IndicaCenario7,
						        p.IndicaCenario8,
						        p.IndicaCenario9
				           FROM {0}.{1}.Projeto AS p INNER JOIN
						        {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria
                                                       AND c.DataExclusao IS NULL) INNER JOIN
						        {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
														         AND s.IndicaSelecaoPortfolio = 'S')
				          WHERE p.DataExclusao IS NULL
					        AND p.CodigoEntidade = @CodigoEntidade
					        AND p.CodigoCategoria = @CodigoCategoria
        					
                      
                      /* Atualiza o resultado para tamanho da bolha */    
                      UPDATE @tblGraficoBolhas
                         SET Despesa = {0}.{1}.f_GetFinanceiroProposta(_CodigoProjeto,@Ano,'D'),
                             Receita = {0}.{1}.f_GetFinanceiroProposta(_CodigoProjeto,@Ano,'R'),
                             Trabalho = {0}.{1}.f_GetTrabalhoProposta(_CodigoProjeto,@Ano),
                             ValorCriterio = IsNull((SELECT Sum(IsNull(ocs.ValorOpcaoCriterioSelecao * {0}.{1}.f_GetPesoCriterio (c.CodigoCategoria, cs.CodigoCriterioSelecao),0))
												        FROM {0}.{1}.Projeto AS p INNER JOIN
                                                             {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria
                                                                                    AND c.DataExclusao IS NULL) INNER JOIN
                                                             {0}.{1}.ProjetoCriterioSelecao AS pcs ON (pcs.CodigoProjeto = p.CodigoProjeto AND pcs.EtapaPreenchimento=0) INNER JOIN
                                                             {0}.{1}.CriterioSelecao AS cs ON (cs.CodigoCriterioSelecao = pcs.CodigoCriterioSelecao) INNER JOIN
                                                             {0}.{1}.OpcaoCriterioSelecao AS ocs ON (ocs.CodigoCriterioSelecao = pcs.CodigoCriterioSelecao
																									          AND ocs.CodigoOpcaoCriterioSelecao = pcs.CodigoOpcaoCriterioSelecao)
                                                       WHERE p.CodigoProjeto = _CodigoProjeto ),0) 
                          WHERE 1 = 1 
                            
                       
                       /* Atualiza as informações para os eixos X e Y, caso seja para somar critérios ou riscos */
                       UPDATE @tblGraficoBolhas
                          SET EixoX = CASE @TipoEscolhaX WHEN 'SOMACRITERIO' THEN ValorCriterio                      
                                                         WHEN 'SOMARISCO' THEN ValorRisco 
                                                                          ELSE 0 END,
                              EixoY = CASE @TipoEscolhaY WHEN 'SOMACRITERIO' THEN ValorCriterio                      
                                                         WHEN 'SOMARISCO' THEN ValorRisco 
                                                                          ELSE 0 END 
                       
                       /* Se for para apresentar valores diferentes de Riscos e Critérios, faz a atualização do que foi escolhido */
                       IF @TipoEscolhaX = 'CRITERIO'
                          UPDATE @tblGraficoBolhas
                             SET EixoX = IsNull((SELECT Sum(IsNull(ocs.ValorOpcaoCriterioSelecao * {0}.{1}.f_GetPesoCriterio (c.CodigoCategoria, cs.CodigoCriterioSelecao),0))
                                                   FROM {0}.{1}.Projeto AS p INNER JOIN
                                                        {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria
                                                                               AND c.DataExclusao IS NULL) INNER JOIN
                                                        {0}.{1}.ProjetoCriterioSelecao AS pcs ON (pcs.CodigoProjeto = p.CodigoProjeto AND pcs.EtapaPreenchimento=0) INNER JOIN
                                                        {0}.{1}.CriterioSelecao AS cs ON (cs.CodigoCriterioSelecao = pcs.CodigoCriterioSelecao) INNER JOIN
                                                        {0}.{1}.OpcaoCriterioSelecao AS ocs ON (ocs.CodigoCriterioSelecao = pcs.CodigoCriterioSelecao
                                                                                                         AND ocs.CodigoOpcaoCriterioSelecao = pcs.CodigoOpcaoCriterioSelecao)
                                                  WHERE p.CodigoProjeto = _CodigoProjeto 
                                                    
                                                    AND cs.CodigoCriterioSelecao = @EscolhaX),0)
                       
                       /* Se for para apresentar valores diferentes de Riscos e Critérios no Eixo Y, faz a atualização do que foi escolhido */
                       IF @TipoEscolhaY = 'CRITERIO'
                          UPDATE @tblGraficoBolhas
                             SET EixoY = IsNull((SELECT Sum(IsNull(ocs.ValorOpcaoCriterioSelecao * {0}.{1}.f_GetPesoCriterio (c.CodigoCategoria, cs.CodigoCriterioSelecao),0))
                                                   FROM {0}.{1}.Projeto AS p INNER JOIN
                                                        {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria
                                                                               AND c.DataExclusao IS NULL) INNER JOIN
                                                        {0}.{1}.ProjetoCriterioSelecao AS pcs ON (pcs.CodigoProjeto = p.CodigoProjeto AND pcs.EtapaPreenchimento=0) INNER JOIN
                                                        {0}.{1}.CriterioSelecao AS cs ON (cs.CodigoCriterioSelecao = pcs.CodigoCriterioSelecao) INNER JOIN
                                                        {0}.{1}.OpcaoCriterioSelecao AS ocs ON (ocs.CodigoCriterioSelecao = pcs.CodigoCriterioSelecao
																								         AND ocs.CodigoOpcaoCriterioSelecao = pcs.CodigoOpcaoCriterioSelecao)
                                                  WHERE p.CodigoProjeto = _CodigoProjeto 
                                                    
                                                    AND cs.CodigoCriterioSelecao = @EscolhaY),0)   
                         
                      IF @Opcao = 'C'                                                                                                                
				          /* Select para trazer os valores agrupados por cenário */
				          SELECT 'Cenario 1' AS Cenario,
						          CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                            ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                                 CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											        ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						         ISNULL(Sum(Despesa), 0) AS Despesa,
						         ISNULL(Sum(Receita), 0) AS Receita,
						         ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						         ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						         ISNULL(Sum(Trabalho),0) AS Trabalho,
						         COUNT(1) AS QuantidadeProjetos
					        FROM @tblGraficoBolhas
				           WHERE IndicaCenario1 = 'S'
				        UNION
				          SELECT 'Cenario 2',
						          CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                            ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                                 CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											        ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						         ISNULL(Sum(Despesa), 0) AS Despesa,
						         ISNULL(Sum(Receita), 0) AS Receita,
						         ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						         ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						         ISNULL(Sum(Trabalho),0) AS Trabalho,
						         COUNT(1) AS QuantidadeProjetos
					        FROM @tblGraficoBolhas
				           WHERE IndicaCenario2 = 'S'
				        UNION
				          SELECT 'Cenario 3',
						          CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                            ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                                 CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											        ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						         ISNULL(Sum(Despesa), 0) AS Despesa,
						         ISNULL(Sum(Receita), 0) AS Receita,
						         ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						         ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						         ISNULL(Sum(Trabalho),0) AS Trabalho,
						         COUNT(1) AS QuantidadeProjetos
					        FROM @tblGraficoBolhas
				           WHERE IndicaCenario3 = 'S'
				        UNION
				          SELECT 'Cenario 4',
						          CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                            ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                                 CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											        ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						         ISNULL(Sum(Despesa), 0) AS Despesa,
						         ISNULL(Sum(Receita), 0) AS Receita,
						         ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						         ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						         ISNULL(Sum(Trabalho),0) AS Trabalho,
						         COUNT(1) AS QuantidadeProjetos
					        FROM @tblGraficoBolhas
				           WHERE IndicaCenario4 = 'S'
				        UNION
				          SELECT 'Cenario 5',
						          CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                            ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                                 CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											        ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						         ISNULL(Sum(Despesa), 0) AS Despesa,
						         ISNULL(Sum(Receita), 0) AS Receita,
						         ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						         ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						         ISNULL(Sum(Trabalho),0) AS Trabalho,
						         COUNT(1) AS QuantidadeProjetos
					        FROM @tblGraficoBolhas
				           WHERE IndicaCenario5 = 'S'
				        UNION
				          SELECT 'Cenario 6',
						          CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                            ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                                 CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											        ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						         ISNULL(Sum(Despesa), 0) AS Despesa,
						         ISNULL(Sum(Receita), 0) AS Receita,
						         ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						         ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						         ISNULL(Sum(Trabalho),0) AS Trabalho,
						         COUNT(1) AS QuantidadeProjetos
					        FROM @tblGraficoBolhas
				           WHERE IndicaCenario6 = 'S'
				        UNION
				          SELECT 'Cenario 7',
						          CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                            ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                                 CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											        ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						         ISNULL(Sum(Despesa), 0) AS Despesa,
						         ISNULL(Sum(Receita), 0) AS Receita,
						         ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						         ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						         ISNULL(Sum(Trabalho),0) AS Trabalho,
						         COUNT(1) AS QuantidadeProjetos
					        FROM @tblGraficoBolhas
				           WHERE IndicaCenario7 = 'S'
				        UNION
				          SELECT 'Cenario 8',
						          CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                            ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                                 CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											        ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						         ISNULL(Sum(Despesa), 0) AS Despesa,
						         ISNULL(Sum(Receita), 0) AS Receita,
						         ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						         ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						         ISNULL(Sum(Trabalho),0) AS Trabalho,
						         COUNT(1) AS QuantidadeProjetos
					        FROM @tblGraficoBolhas
				           WHERE IndicaCenario8 = 'S'
				        UNION
				          SELECT 'Cenario 9',
						          CASE WHEN @TipoEscolhaX IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoX), 0) 
						                            ELSE ISNULL(Sum(EixoX), 0) END AS EixoX,
                                 CASE WHEN @TipoEscolhaY IN ('SOMACRITERIO', 'SOMARISCO') THEN ISNULL(AVG(EixoY), 0)
											        ELSE ISNULL(Sum(EixoY), 0) END AS EixoY,
						         ISNULL(Sum(Despesa), 0) AS Despesa,
						         ISNULL(Sum(Receita), 0) AS Receita,
						         ISNULL(Avg(ValorRisco), 0) AS ValorRisco,
						         ISNULL(Avg(ValorCriterio), 0) AS ValorCriterio,
						         ISNULL(Sum(Trabalho),0) AS Trabalho,
						         COUNT(1) AS QuantidadeProjetos
					        FROM @tblGraficoBolhas
				           WHERE IndicaCenario9 = 'S'      
			          ELSE
			             SELECT * FROM @tblGraficoBolhas 
                          WHERE 1 = 1 

                  END", bancodb, Ownerdb, codigoEntidade, ano);

            return getDataSet(comandoSQL);

        }

        public DataSet getRelatorioProjetosPorCenario(int codigoPortfolio, int ano, int codigoEntidade)
        {
            string comandoSQL = string.Format(@"
        SELECT 'cenario1' as cenario, Ranking, Categoria, DesempenhoProjeto, NomeProjeto, ScoreCriterios, ScoreRiscos, Custo,Receita, RH, DescricaoStatusProjeto 
          FROM {0}.{1}.f_GetProjetosSelecaoBalanceamento({2},{3},{4})
         WHERE IndicaCenario1 = 'S' 
         
         UNION 
        
        SELECT 'cenario2' as cenario, Ranking, Categoria, DesempenhoProjeto, NomeProjeto, ScoreCriterios, ScoreRiscos, Custo,Receita, RH, DescricaoStatusProjeto 
          FROM {0}.{1}.f_GetProjetosSelecaoBalanceamento({2},{3},{4})
         WHERE IndicaCenario2 = 'S' 
        
         UNION
        
        SELECT 'cenario3' as cenario, Ranking, Categoria, DesempenhoProjeto, NomeProjeto, ScoreCriterios, ScoreRiscos, Custo,Receita, RH, DescricaoStatusProjeto 
          FROM {0}.{1}.f_GetProjetosSelecaoBalanceamento({2},{3},{4})
         WHERE IndicaCenario3 = 'S' 
        
         UNION
        
        SELECT 'cenario4' as cenario, Ranking, Categoria, DesempenhoProjeto, NomeProjeto, ScoreCriterios, ScoreRiscos, Custo,Receita, RH, DescricaoStatusProjeto 
          FROM {0}.{1}.f_GetProjetosSelecaoBalanceamento({2},{3},{4})
         WHERE IndicaCenario4 = 'S' 
        
         UNION
        
        SELECT 'cenario5' as cenario, Ranking, Categoria, DesempenhoProjeto, NomeProjeto, ScoreCriterios, ScoreRiscos, Custo,Receita, RH, DescricaoStatusProjeto 
          FROM {0}.{1}.f_GetProjetosSelecaoBalanceamento({2},{3},{4})
         WHERE IndicaCenario5 = 'S' 
        
         UNION
        
        SELECT 'cenario6' as cenario, Ranking, Categoria, DesempenhoProjeto, NomeProjeto, ScoreCriterios, ScoreRiscos, Custo,Receita, RH, DescricaoStatusProjeto 
          FROM {0}.{1}.f_GetProjetosSelecaoBalanceamento({2},{3},{4})
         WHERE IndicaCenario6 = 'S' 
        
         UNION
        
        SELECT 'cenario7' as cenario, Ranking, Categoria, DesempenhoProjeto, NomeProjeto, ScoreCriterios, ScoreRiscos, Custo,Receita, RH, DescricaoStatusProjeto 
          FROM {0}.{1}.f_GetProjetosSelecaoBalanceamento({2},{3},{4})
         WHERE IndicaCenario7 = 'S' 
        
         UNION
        
        SELECT 'cenario8' as cenario, Ranking, Categoria, DesempenhoProjeto, NomeProjeto, ScoreCriterios, ScoreRiscos, Custo,Receita, RH, DescricaoStatusProjeto 
          FROM {0}.{1}.f_GetProjetosSelecaoBalanceamento({2},{3},{4})
         WHERE IndicaCenario8 = 'S' 
        
         UNION
        
        SELECT 'cenario9' as cenario, Ranking, Categoria, DesempenhoProjeto, NomeProjeto, ScoreCriterios, ScoreRiscos, Custo,Receita, RH, DescricaoStatusProjeto 
          FROM {0}.{1}.f_GetProjetosSelecaoBalanceamento({2},{3},{4})
         WHERE IndicaCenario9 = 'S' ", bancodb, Ownerdb, codigoPortfolio, ano, codigoEntidade);
            return getDataSet(comandoSQL);
        }
        #endregion
    }
}