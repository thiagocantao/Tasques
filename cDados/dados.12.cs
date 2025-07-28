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
    #region cronograma orçamento
    public DataSet getCronogramaOrcamentario(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
                  @"select * from (SELECT t1.CodigoAcao AS CodigoAtividade, t1.CodigoAcaoSuperior AS CodigoAcao
                          ,t1.NumeroAcao AS NumeroAtividade, tSup.NumeroAcao, tSup.NomeAcao AS NomeAcao
                          ,t1.NomeAcao AS NomeAtividade, t1.FonteRecurso, t1.IndicaSemRecurso
                          ,(SELECT SUM(ValorTotal) 
				                     FROM {0}.{1}.CronogramaOrcamentario co INNER JOIN
                                          {0}.{1}.orc_planoContas opc ON (opc.SeqPlanoContas = co.SeqPlanoContas)
				                    WHERE co.CodigoAcao = t1.CodigoAcao 
				                      AND co.CodigoProjeto = t1.CodigoProjeto) AS Valor
			              ,ISNULL((SELECT TOP 1 'S'
				                     FROM {0}.{1}.CronogramaOrcamentarioAcao co  INNER JOIN
                                          {0}.{1}.orc_planoContas opc ON (opc.SeqPlanoContas = co.SeqPlanoContas)
				                    WHERE co.CodigoAcao = t1.CodigoAcao 
				                      AND co.CodigoProjeto = t1.CodigoProjeto), 'N') AS PossuiContas
						  , CASE WHEN t1.CodigoAcao = t1.CodigoAcaoSuperior THEN right('0000'+CONVERT(VARCHAR, t1.NumeroAcao),4)
									ELSE right('0000'+CONVERT(VARCHAR, tSup.NumeroAcao),4) + '.' + right('0000'+CONVERT(VARCHAR, t1.NumeroAcao),4) 
							END AS ordem
                     FROM {0}.{1}.tai02_AcoesIniciativa t1 INNER JOIN
		                  {0}.{1}.tai02_AcoesIniciativa tSup ON tSup.CodigoAcao = t1.CodigoAcaoSuperior
                    WHERE t1.CodigoAcao = t1.CodigoAcaoSuperior 
                      AND t1.CodigoProjeto = {2}
		              AND t1.IndicaSemRecurso = 'N'
                      {3} ) x 
                    ORDER BY x.ordem
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getValoresAcoesProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT t1.CodigoAcaoSuperior AS CodigoAcao, t1.FonteRecurso, SUM(ValorProposto) AS Valor			             
	             FROM {0}.{1}.tai02_AcoesIniciativa t1 INNER JOIN
			          {0}.{1}.CronogramaOrcamentarioAcao co ON co.CodigoAcao = t1.CodigoAcao INNER JOIN
                      {0}.{1}.orc_planoContas opc ON (opc.SeqPlanoContas = co.SeqPlanoContas)
	            WHERE t1.CodigoAcao = t1.CodigoAcaoSuperior 
		          AND t1.CodigoProjeto = {2}
		          AND IndicaSemRecurso = 'N'
                  {3}
                GROUP BY t1.CodigoAcaoSuperior, t1.FonteRecurso
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getContasAcoesProjeto(int codigoAcao, int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT co.CodigoProjeto, co.CodigoAcao, co.SeqPlanoContas, co.Quantidade, co.ValorUnitario
                      ,co.ValorProposto, co.MemoriaCalculo ,co.[ValorSuplemento],co.[ValorTransposto]
                      ,co.[ValorRealizado],co.[DisponibilidadeAtual],co.[DisponibilidadeReformulada]
                      , opc.CONTA_DES, opc.CONTA_COD
                 FROM {0}.{1}.CronogramaOrcamentarioAcao co INNER JOIN
			          {0}.{1}.orc_planoContas opc ON opc.SeqPlanoContas = co.SeqPlanoContas
                WHERE co.CodigoAcao = {2}
                  AND co.CodigoProjeto = {3}
                  {4}
                ORDER BY opc.CONTA_DES
               ", bancodb, Ownerdb, codigoAcao, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getContaAtividade(int codigoContaOrcamentaria, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT co.CodigoProjeto, co.CodigoAcao, co.SeqPlanoContas, co.Quantidade, co.ValorUnitario
                      ,co.ValorProposto, co.MemoriaCalculo ,co.[ValorSuplemento],co.[ValorTransposto]
                      ,co.[ValorRealizado],co.[DisponibilidadeAtual],co.[DisponibilidadeReformulada]
                      , opc.CONTA_DES, opc.CONTA_COD
                 FROM {0}.{1}.CronogramaOrcamentarioAcao co INNER JOIN
			          {0}.{1}.orc_planoContas opc ON opc.SeqPlanoContas = co.SeqPlanoContas
                WHERE co.SeqPlanoContas = {2}
                  {3}
               ", bancodb, Ownerdb, codigoContaOrcamentaria, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getPlanoContasAnoAtual(int codigoProjeto, int codigoConta, int codigoAtividade, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT SeqPlanoContas, opc.Ano, CONTA_COD, CONTA_DES
			         ,CASE WHEN DespesaReceita = 'D' THEN 'Despesa' ELSE 'Receita' END AS DespesaReceita
                 FROM {0}.{1}.orc_planoContas opc inner join
                      {0}.{1}.TermoAbertura02 ta on ta.codigoprojeto = {5} inner join 
                      {0}.{1}.orc_movimentoOrcamento omo on (omo.CodigoMovimentoOrcamento = ta.CodigoMovimentoOrcamento and opc.ano = omo.ano)
                WHERE  
                   opc.SeqPlanoContas NOT IN(SELECT co.SeqPlanoContas 
                                                          FROM {0}.{1}.CronogramaOrcamentarioAcao co 
                                                         WHERE CodigoAcao = {3} 
                                                           AND co.SeqPlanoContas <> {2})
                 {4}
                ORDER BY DespesaReceita, CONTA_COD, CONTA_DES
               ", bancodb, Ownerdb, codigoConta, codigoAtividade, where, codigoProjeto);
        return getDataSet(comandoSQL);
    }

    public DataSet getAnoOrcamento(int codigoEntidade, string where)
    {

        string comandoSQL = string.Format(
             @"SELECT [CodigoMovimentoOrcamento]
                     ,[DescricaoMovimentoOrcamento]
        FROM {0}.{1}.[orc_movimentoOrcamento]
        WHERE CodigoEntidade = {2}
           {3}
        ORDER BY 
       CodigoMovimentoOrcamento
               ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }


    public bool verificaAcessoAnoOrcamento(int codigoProjeto)
    {

        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN
                        DECLARE @QTd int
                        SELECT @Qtd = count(1)
                        FROM {0}.{1}.CronogramaOrcamentarioAcao p 
                        WHERE p.CodigoProjeto = {2}
                     SELECT ISNULL(@Qtd, 0) As Qtd

                    END ", bancodb, Ownerdb, codigoProjeto);


            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                int qtd = int.Parse(ds.Tables[0].Rows[0]["Qtd"].ToString());

                if (qtd == 0)
                    retorno = true;
                else
                {
                    retorno = false;
                }
            }
            else
                retorno = true;

        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool incluiContaAtividade(int codigoProjeto, int codigoAtividade, int codigoConta, string quantidade, string valorUnitario, string valorTotal
        , string memoriaCalculo, string ValorSuplemento, string ValorTransposto, string ValorRealizado, string DisponibilidadeAtual, string DisponibilidadeReformulada)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN

                        INSERT INTO {0}.{1}.CronogramaOrcamentarioAcao
                                   ([CodigoProjeto]
                                   ,[CodigoAcao]
                                   ,[SeqPlanoContas]
                                   ,[MemoriaCalculo]
                                   ,[Quantidade]
                                   ,[ValorUnitario]
                                   ,[ValorProposto]
                                   ,[ValorSuplemento]
                                   ,[ValorTransposto]
                                   ,[ValorRealizado]
                                   ,[DisponibilidadeAtual]
                                   ,[DisponibilidadeReformulada])
                             VALUES
                                   ({2}
                                   ,{3}
                                   ,{4}
                                   ,{5}
                                   ,{6}
                                   ,{7}
                                   ,'{8}'
                                   ,{9}
                                   ,{10}
                                   ,{11}
                                   ,{12})

                   END
                        ", bancodb, Ownerdb
                         , codigoProjeto, codigoAtividade, codigoConta, quantidade, valorUnitario, valorTotal, memoriaCalculo.Replace("'", "''")
                         , ValorSuplemento, ValorTransposto, ValorRealizado, DisponibilidadeAtual, DisponibilidadeReformulada);

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

    public bool atualizaContaAtividade(int codigoContaAnterior, int codigoAtividade, int codigoConta, string quantidade, string valorUnitario, string valorTotal
             , string memoriaCalculo, string ValorSuplemento, string ValorTransposto, string ValorRealizado, string DisponibilidadeAtual, string DisponibilidadeReformulada)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN

                        UPDATE {0}.{1}.CronogramaOrcamentarioAcao
                           SET SeqPlanoContas = {4}
                              ,Quantidade = {5}
                              ,ValorUnitario = {6}
                              ,ValorTotal = {7}
                              ,MemoriaCalculo = '{8}'
                              ,ValorSuplemento = {9}
                              ,ValorTransposto = {10}
                              ,ValorRealizado = {11}
                              ,DisponibilidadeAtual = {12}
                              ,DisponibilidadeReformulada = {13}
                         WHERE SeqPlanoContas = {2}
	                       AND CodigoAcao = {3}

                   END
                        ", bancodb, Ownerdb
                         , codigoContaAnterior, codigoAtividade, codigoConta, quantidade, valorUnitario, valorTotal, memoriaCalculo.Replace("'", "''")
                         , ValorSuplemento, ValorTransposto, ValorRealizado, DisponibilidadeAtual, DisponibilidadeReformulada);

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

    public bool excluiContaAtividade(int codigoConta, int codigoAtividade, int codigoProjeto)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN

                        DELETE {0}.{1}.CronogramaOrcamentarioAcao
                         WHERE SeqPlanoContas = {2}
	                       AND CodigoAcao = {3}
                           AND CodigoProjeto = {4}

                   END
                        ", bancodb, Ownerdb
                         , codigoConta, codigoAtividade, codigoProjeto);

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

    #endregion

    #region Plano de Trabalho Processos

    public DataSet getPlanoTrabalhoProjetoProcesso(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
                @"SELECT t1.CodigoAcao AS Codigo, t1.CodigoAcaoSuperior AS CodigoPai
			            ,CASE WHEN t1.CodigoAcao = t1.CodigoAcaoSuperior THEN CONVERT(VARCHAR, t1.NumeroAcao)
						            ELSE CONVERT(VARCHAR, tSup.NumeroAcao) + '.' + CONVERT(VARCHAR, t1.NumeroAcao) END AS Numero
			            ,t1.NomeAcao AS Descricao, t1.Inicio, t1.Termino
			            ,CASE WHEN t1.CodigoAcao = t1.CodigoAcaoSuperior THEN ' - '
			             ELSE t1.IndicaEventoInstitucional END AS Institucional
                        ,t1.CodigoUsuarioResponsavel
			            ,t1.NomeUsuarioResponsavel AS Responsavel, t1.FonteRecurso
	               FROM {0}.{1}.tai06_AcoesIniciativa t1 INNER JOIN
			            {0}.{1}.tai06_AcoesIniciativa tSup ON tSup.CodigoAcao = t1.CodigoAcaoSuperior
                  WHERE t1.CodigoProjeto = {2}
                    {3}
                  ORDER BY  tsup.NumeroAcao, 
                  CASE WHEN t1.CodigoAcao = t1.CodigoAcaoSuperior THEN 0
						        ELSE (t1.NumeroAcao) END	
                               --tSup.NumeroAcao, CASE WHEN t1.CodigoAcao = t1.CodigoAcaoSuperior THEN CONVERT(VARCHAR, t1.NumeroAcao)
						           -- ELSE CONVERT(VARCHAR, tSup.NumeroAcao) + '.' + CONVERT(VARCHAR, t1.NumeroAcao) END
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getMetasPlanoTrabalhoProjetoProcesso(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
                      @" SELECT SequenciaRegistro AS CodigoMeta, CodigoAcao AS Codigo, DescricaoProduto AS Meta
                           FROM {0}.{1}.tai06_ProdutosAcoesIniciativa
                          WHERE CodigoProjeto = {2}
                            {3}
                          ORDER BY DescricaoProduto
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getParceriasPlanoTrabalhoProcesso(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
                  @"SELECT p.CodigoAcao AS Codigo, ai.CodigoAcaoSuperior AS CodigoObjetoPai, p.SequenciaRegistro AS CodigoParceria
			              ,p.CodigoParceiro, p.NomeParceiro AS Area, p.ProdutoSolicitado AS Elemento 
                      FROM {0}.{1}.tai06_ParceirosIniciativa p INNER JOIN
			               {0}.{1}.tai06_AcoesIniciativa ai ON ai.CodigoAcao = p.CodigoAcao
                     WHERE p.CodigoProjeto = {2}
                       {3}
                     ORDER BY p.NomeParceiro, p.ProdutoSolicitado
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getMarcosPlanoTrabalhoProcesso(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
               @"SELECT m.CodigoAcao AS Codigo, ai.CodigoAcaoSuperior AS CodigoObjetoPai, m.SequenciaRegistro AS CodigoMarco
			          , m.NomeMarco AS Marco, m.DataLimitePrevista AS Data
                  FROM {0}.{1}.tai06_MarcosAcoesIniciativa m INNER JOIN
			           {0}.{1}.tai06_AcoesIniciativa ai ON ai.CodigoAcao = m.CodigoAcao
                 WHERE m.CodigoProjeto = {2}
                   {3}
                 ORDER BY m.DataLimitePrevista, m.NomeMarco
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiAcaoIniciativaProcesso(int codigoProjeto, int numeroAcao, string nomeAcao, int codigoResponsavel, int codigoEntidade
        , string fonteRecursos, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @NomeEntidade VarChar(100)
                                   ,@NomeUsuario VarChar(60)
                                   ,@CodigoAcao int

                            SELECT @NomeEntidade = NomeUnidadeNegocio
                              FROM {0}.{1}.UnidadeNegocio un
                             WHERE un.CodigoUnidadeNegocio = {6}

                            SELECT @NomeUsuario = NomeUsuario
                              FROM {0}.{1}.Usuario us
                             WHERE us.CodigoUsuario = {5}

                            UPDATE {0}.{1}.tai06_AcoesIniciativa SET NumeroAcao = (NumeroAcao + 1)
                             WHERE NumeroAcao >= {3}
                               AND CodigoProjeto = {2}
                               AND CodigoAcao = CodigoAcaoSuperior
                            
                            INSERT INTO {0}.{1}.tai06_AcoesIniciativa
                                       (CodigoProjeto
                                       ,NumeroAcao
                                       ,NomeAcao
                                       ,CodigoUsuarioResponsavel
                                       ,NomeUsuarioResponsavel
                                       ,CodigoEntidadeResponsavel
                                       ,NomeEntidadeResponsavel
                                       ,FonteRecurso
                                       ,IndicaSemRecurso)
                                 VALUES
                                       ({2}
                                       ,{3}
                                       ,'{4}'
                                       ,{5}
                                       ,@NomeUsuario
                                       ,{6}
                                       ,@NomeEntidade
                                       ,'{7}'
                                       ,'{8}')

                            SELECT @CodigoAcao = SCOPE_IDENTITY()

                            UPDATE {0}.{1}.tai06_AcoesIniciativa SET CodigoAcaoSuperior = @CodigoAcao
                             WHERE CodigoAcao = @CodigoAcao

                            SELECT @CodigoAcao AS CodigoAcao

                        END
                        ", bancodb, Ownerdb
                         , codigoProjeto, numeroAcao, nomeAcao.Replace("'", "''")
                         , codigoResponsavel, codigoEntidade, fonteRecursos, fonteRecursos == "SR" ? "S" : "N");

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["CodigoAcao"].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaAcaoIniciativaProcesso(int codigoProjeto, int numeroAcao, string nomeAcao, int codigoResponsavel, string fonteRecursos, int codigoAcao)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @NumeroAtualAcao int
                                   ,@NomeUsuario VarChar(60)

                            SELECT @NomeUsuario = NomeUsuario
                              FROM {0}.{1}.Usuario us
                             WHERE us.CodigoUsuario = {5}

                            SELECT @NumeroAtualAcao = NumeroAcao
                              FROM {0}.{1}.tai06_AcoesIniciativa
                             WHERE CodigoAcao = {8}
                               AND CodigoProjeto = {2}

                            IF (@NumeroAtualAcao > {3})
                                UPDATE {0}.{1}.tai06_AcoesIniciativa SET NumeroAcao = (NumeroAcao + 1)
                                 WHERE NumeroAcao < @NumeroAtualAcao
                                   AND NumeroAcao >= {3}
                                   AND CodigoProjeto = {2}
                                   AND CodigoAcao = CodigoAcaoSuperior
                            ELSE IF (@NumeroAtualAcao < {3})
                                UPDATE {0}.{1}.tai06_AcoesIniciativa SET NumeroAcao = (NumeroAcao - 1)
                                 WHERE NumeroAcao > @NumeroAtualAcao
                                   AND NumeroAcao <= {3}
                                   AND CodigoProjeto = {2}
                                   AND CodigoAcao = CodigoAcaoSuperior

                            UPDATE {0}.{1}.tai06_AcoesIniciativa 
                               SET NumeroAcao = {3}
                                  ,NomeAcao = '{4}'
                                  ,CodigoUsuarioResponsavel = {5}                                  
                                  ,NomeUsuarioResponsavel = @NomeUsuario
                                  ,FonteRecurso = '{6}'
                                  ,IndicaSemRecurso = '{7}'
                            WHERE CodigoAcao  = {8} 
                              AND CodigoProjeto = {2}

                            -- alterando atividades da ação 
                                 UPDATE {0}.{1}.tai06_AcoesIniciativa 
                                   SET FonteRecurso = '{6}'
                                     , IndicaSemrecurso = '{7}'
                                 WHERE codigoAcaoSuperior = {8} 
                                   AND CodigoProjeto = {2}
                          
                            IF ('{6}' = 'SR')
                            BEGIN
                                DELETE FROM {0}.{1}.CronogramaOrcamentario  
                                WHERE CodigoProjeto = {2}  
                                  AND CodigoAcao in (select distinct CodigoAcao from {0}.{1}.tai06_AcoesIniciativa where CodigoAcaoSuperior =  {8}  )
                            END    


                        END
                        ", bancodb, Ownerdb
                         , codigoProjeto, numeroAcao, nomeAcao.Replace("'", "''")
                         , codigoResponsavel, fonteRecursos, fonteRecursos == "SR" ? "S" : "N"
                         , codigoAcao);

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

    public bool excluiAcaoIniciativaProcesso(int codigoProjeto, int codigoAcao)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                         BEGIN
                            DECLARE @NumeroAtualAcao int

                            SELECT @NumeroAtualAcao = NumeroAcao
                              FROM {0}.{1}.tai06_AcoesIniciativa
                             WHERE CodigoAcao = {3}
                               AND CodigoProjeto = {2}

                            DELETE FROM {0}.{1}.tai06_AcoesIniciativa                                
                                  WHERE CodigoAcao = {3}                              
                                    AND CodigoProjeto = {2}
                            
                            UPDATE {0}.{1}.tai06_AcoesIniciativa SET NumeroAcao = (NumeroAcao - 1)
                             WHERE NumeroAcao >= @NumeroAtualAcao
                               AND CodigoProjeto = {2}
                               AND CodigoAcao = CodigoAcaoSuperior
                        END
                        ", bancodb, Ownerdb
                         , codigoProjeto, codigoAcao);

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

    public bool verificaExclusaoPlanoTrabalhoProcesso(int codigoAcao)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      SELECT 1 
                        FROM {0}.{1}.tai06_ProdutosAcoesIniciativa
                       WHERE CodigoAcao = {2}
                       UNION
                       SELECT 1 
                        FROM {0}.{1}.tai06_AcoesIniciativa
                       WHERE CodigoAcaoSuperior = {2}
                         AND CodigoAcao <> {2}
                        ", bancodb, Ownerdb
                         , codigoAcao);


            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                retorno = false;
            else
                retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public DataSet getAtividadeAcaoIniciativaProcesso(int codigoAtividade, string where)
    {
        string comandoSQL = string.Format(
                @"SELECT t1.CodigoAcao AS CodigoAtividade, t1.CodigoAcaoSuperior AS CodigoAcao
			            ,t1.NumeroAcao AS NumeroAtividade, tSup.NumeroAcao AS NumeroAcao
                        ,t1.NomeAcao AS Descricao, t1.Inicio, t1.Termino
                        ,t1.IndicaEventoInstitucional AS Institucional, t1.CodigoUsuarioResponsavel
                        ,t1.LocalEvento, t1.IndicaSemRecurso, t1.DetalhesEvento, tSup.NomeAcao AS NomeAcao
                   FROM {0}.{1}.tai06_AcoesIniciativa t1 INNER JOIN
  		                {0}.{1}.tai06_AcoesIniciativa tSup ON tSup.CodigoAcao = t1.CodigoAcaoSuperior
                  WHERE t1.CodigoAcao = {2}
                    {3}
               ", bancodb, Ownerdb, codigoAtividade, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiAtividadeAcaoIniciativaProcesso(int codigoProjeto, int codigoAcao, int numeroAtividade, string nomeAtividade, string dataInicio, string dataTermino
                                            , int codigoResponsavel, int codigoEntidade, string indicaSemRecursos, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @NomeEntidade VarChar(100)
                                   ,@NomeUsuario VarChar(60)
                                   ,@CodigoAtividade int
                                   ,@FonteRecurso Char(2)

                            SELECT @NomeEntidade = NomeUnidadeNegocio
                              FROM {0}.{1}.UnidadeNegocio un
                             WHERE un.CodigoUnidadeNegocio = {7}

                            SELECT @NomeUsuario = NomeUsuario
                              FROM {0}.{1}.Usuario us
                             WHERE us.CodigoUsuario = {6}

                            SELECT @FonteRecurso = FonteRecurso
                              FROM {0}.{1}.tai06_AcoesIniciativa
                             WHERE codigoAcao = {3}

                            UPDATE {0}.{1}.tai06_AcoesIniciativa SET NumeroAcao = (NumeroAcao + 1)
                             WHERE NumeroAcao >= {4}
                               AND CodigoAcaoSuperior = {3}
                               AND CodigoProjeto = {2}
                               AND CodigoAcao <> {3}
                            
                            INSERT INTO {0}.{1}.tai06_AcoesIniciativa
                                       (CodigoProjeto
                                       ,CodigoAcaoSuperior
                                       ,NumeroAcao
                                       ,NomeAcao
                                       ,CodigoUsuarioResponsavel
                                       ,NomeUsuarioResponsavel
                                       ,CodigoEntidadeResponsavel
                                       ,NomeEntidadeResponsavel
                                       ,Inicio
                                       ,Termino
                                       ,FonteRecurso
                                       ,IndicaSemRecurso)
                                 VALUES
                                       ({2}
                                       ,{3}
                                       ,{4}
                                       ,'{5}'
                                       ,{6}
                                       ,@NomeUsuario
                                       ,{7}
                                       ,@NomeEntidade
                                       ,{8}
                                       ,{9}
                                       ,@FonteRecurso
                                       ,'{10}')

                            SELECT @CodigoAtividade = SCOPE_IDENTITY()

                            UPDATE {0}.{1}.tai06_AcoesIniciativa 
	                           SET Inicio = (SELECT MIN(t.Inicio) FROM {0}.{1}.tai06_AcoesIniciativa t WHERE t.CodigoAcaoSuperior = {3})
			                      ,Termino = (SELECT MAX(t.Termino) FROM {0}.{1}.tai06_AcoesIniciativa t WHERE t.CodigoAcaoSuperior = {3})
                             WHERE CodigoAcao = {3}

                            SELECT @CodigoAtividade AS CodigoAtividade

                        END
                        ", bancodb, Ownerdb
                         , codigoProjeto, codigoAcao, numeroAtividade, nomeAtividade.Replace("'", "''")
                         , codigoResponsavel, codigoEntidade, dataInicio, dataTermino, indicaSemRecursos);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["CodigoAtividade"].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaAtividadeAcaoIniciativaProcesso(int codigoAtividade, int codigoProjeto, int codigoAcao, int numeroAtividade, string nomeAtividade, string dataInicio, string dataTermino
                                            , int codigoResponsavel, int codigoEntidade, string indicaSemRecursos)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @NumeroAtualAcao int
                                   ,@NomeUsuario VarChar(60)
                                   ,@FonteRecurso Char(2)

                            SELECT @NomeUsuario = NomeUsuario
                              FROM {0}.{1}.Usuario us
                             WHERE us.CodigoUsuario = {7}

                            SELECT @FonteRecurso = FonteRecurso
                              FROM {0}.{1}.tai06_AcoesIniciativa
                             WHERE codigoAcao = {4}

                            SELECT @NumeroAtualAcao = NumeroAcao
                              FROM {0}.{1}.tai06_AcoesIniciativa
                             WHERE CodigoAcao = {2}
                               AND CodigoProjeto = {3}
                               AND CodigoAcaoSuperior = {4}

                            IF (@NumeroAtualAcao > {5})
                                UPDATE {0}.{1}.tai06_AcoesIniciativa SET NumeroAcao = (NumeroAcao + 1)
                                 WHERE NumeroAcao < @NumeroAtualAcao
                                   AND NumeroAcao >= {5}
                                   AND CodigoProjeto = {3}
                                   AND CodigoAcaoSuperior = {4}                                   
                                   AND CodigoAcao <> {4}
                            ELSE IF (@NumeroAtualAcao < {5})
                                UPDATE {0}.{1}.tai06_AcoesIniciativa SET NumeroAcao = (NumeroAcao - 1)
                                 WHERE NumeroAcao > @NumeroAtualAcao
                                   AND NumeroAcao >= {5}
                                   AND CodigoProjeto = {3}
                                   AND CodigoAcaoSuperior = {4}                                   
                                   AND CodigoAcao <> {4}

                            UPDATE {0}.{1}.tai06_AcoesIniciativa 
                               SET NumeroAcao = {5}
                                  ,NomeAcao = '{6}'
                                  ,CodigoUsuarioResponsavel = {7}
                                  ,NomeUsuarioResponsavel = @NomeUsuario
                                  ,Inicio = {8}
                                  ,Termino = {9}
                                  ,FonteRecurso = @FonteRecurso
                                  ,IndicaSemRecurso = '{10}'
                            WHERE CodigoAcao = {2}
                              AND CodigoProjeto = {3}
                              AND CodigoAcaoSuperior = {4}

                            UPDATE {0}.{1}.tai06_AcoesIniciativa 
	                           SET Inicio = (SELECT MIN(t.Inicio) FROM {0}.{1}.tai06_AcoesIniciativa t WHERE t.CodigoAcaoSuperior = {4})
			                      ,Termino = (SELECT MAX(t.Termino) FROM {0}.{1}.tai06_AcoesIniciativa t WHERE t.CodigoAcaoSuperior = {4})
                             WHERE CodigoAcao = {4}

                        END
                        ", bancodb, Ownerdb
                         , codigoAtividade, codigoProjeto, codigoAcao, numeroAtividade, nomeAtividade.Replace("'", "''")
                         , codigoResponsavel, dataInicio, dataTermino, indicaSemRecursos);

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

    public bool excluiAtividadeAcaoIniciativaProcesso(int codigoProjeto, int codigoAtividade)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                         BEGIN
                            DECLARE @NumeroAtualAcao int,
                                    @CodigoAcaoSuperior int

                            SELECT @CodigoAcaoSuperior = CodigoAcaoSuperior
                              FROM {0}.{1}.tai06_AcoesIniciativa
                             WHERE CodigoAcao = {3}
                               AND CodigoProjeto = {2}

                            SELECT @NumeroAtualAcao = NumeroAcao
                              FROM {0}.{1}.tai06_AcoesIniciativa
                             WHERE CodigoAcao = {3}
                               AND CodigoProjeto = {2}

                            DELETE {0}.{1}.tai06_ProdutosAcoesIniciativa
                            WHERE CodigoAcao = {3}
                                                        
                            DELETE  {0}.{1}.tai06_ParceirosIniciativa
                            WHERE CodigoAcao = {3}

                            DELETE FROM {0}.{1}.tai06_AcoesIniciativa                                
                                  WHERE CodigoAcao = {3}                              
                                    AND CodigoProjeto = {2}
                            
                            UPDATE {0}.{1}.tai06_AcoesIniciativa SET NumeroAcao = (NumeroAcao - 1)
                             WHERE NumeroAcao >= @NumeroAtualAcao
                               AND CodigoProjeto = {2}
                               AND CodigoAcaoSuperior = @CodigoAcaoSuperior
                               AND CodigoAcao <> @CodigoAcaoSuperior
                        END
                        ", bancodb, Ownerdb
                         , codigoProjeto, codigoAtividade);

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

    public bool verificaExclusaoAtividadeAcaoIniciativaProcesso(int codigoAtividade)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      SELECT 1 
                        FROM {0}.{1}.tai06_ProdutosAcoesIniciativa
                       WHERE CodigoAcao = {2}
                       UNION
                       SELECT 1 
                        FROM {0}.{1}.tai06_ParceirosIniciativa
                       WHERE CodigoAcao = {2}
                        ", bancodb, Ownerdb
                         , codigoAtividade);


            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                retorno = false;
            else
                retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool incluiMetaAcaoIniciativaProcesso(int codigoProjeto, int codigoAcao, string meta)
    {
        bool retorno = false;
        int regAf = 0;
        try
        {
            comandoSQL = string.Format(@"
                        INSERT INTO {0}.{1}.tai06_ProdutosAcoesIniciativa(CodigoProjeto, CodigoAcao, DescricaoProduto)
                                                                   VALUES({2}, {3}, '{4}')
                        ", bancodb, Ownerdb
                         , codigoProjeto, codigoAcao, meta.Replace("'", "''"));

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaMetaAcaoIniciativaProcesso(int codigoMeta, string meta)
    {
        bool retorno = false;
        int regAf = 0;
        try
        {
            comandoSQL = string.Format(@"
                        UPDATE {0}.{1}.tai06_ProdutosAcoesIniciativa SET DescricaoProduto = '{3}'
                         WHERE SequenciaRegistro = {2}
                        ", bancodb, Ownerdb
                         , codigoMeta, meta.Replace("'", "''"));

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool excluiMetaAcaoIniciativaProcesso(int codigoMeta)
    {
        bool retorno = false;
        int regAf = 0;
        try
        {
            comandoSQL = string.Format(@"
                        DELETE FROM {0}.{1}.tai06_ProdutosAcoesIniciativa 
                         WHERE SequenciaRegistro = {2}
                        ", bancodb, Ownerdb
                         , codigoMeta);

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    //    public bool incluiMarcoAcaoIniciativaProcesso(int codigoProjeto, int codigoAcao, string marco, string data)
    //    {
    //        bool retorno = false;
    //        int regAf = 0;
    //        try
    //        {
    //            comandoSQL = string.Format(@"
    //                        INSERT INTO {0}.{1}.tai06_MarcosAcoesIniciativa(CodigoProjeto, CodigoAcao, NomeMarco, DataLimitePrevista)
    //                                                                   VALUES({2}, {3}, '{4}', {5})
    //                        ", bancodb, Ownerdb
    //                         , codigoProjeto, codigoAcao, marco.Replace("'", "''")
    //                         , data);

    //            execSQL(comandoSQL, ref regAf);

    //            retorno = true;
    //        }
    //        catch (Exception ex)
    //        {
    //            retorno = false;
    //        }
    //        return retorno;
    //    }

    //    public bool atualizaMarcoAcaoIniciativaProcesso(int codigoMarco, string marco, string data)
    //    {
    //        bool retorno = false;
    //        int regAf = 0;
    //        try
    //        {
    //            comandoSQL = string.Format(@"
    //                        UPDATE {0}.{1}.tai06_MarcosAcoesIniciativa 
    //                           SET NomeMarco = '{3}'
    //                              ,DataLimitePrevista = {4}
    //                         WHERE SequenciaRegistro = {2}
    //                        ", bancodb, Ownerdb
    //                         , codigoMarco, marco.Replace("'", "''")
    //                         , data);

    //            execSQL(comandoSQL, ref regAf);

    //            retorno = true;
    //        }
    //        catch (Exception ex)
    //        {
    //            retorno = false;
    //        }
    //        return retorno;
    //    }

    //    public bool excluiMarcoAcaoIniciativaProcesso(int codigoMarco)
    //    {
    //        bool retorno = false;
    //        int regAf = 0;
    //        try
    //        {
    //            comandoSQL = string.Format(@"
    //                        DELETE FROM {0}.{1}.tai06_MarcosAcoesIniciativa 
    //                         WHERE SequenciaRegistro = {2}
    //                        ", bancodb, Ownerdb
    //                         , codigoMarco);

    //            execSQL(comandoSQL, ref regAf);

    //            retorno = true;
    //        }
    //        catch (Exception ex)
    //        {
    //            retorno = false;
    //        }
    //        return retorno;
    //    }

    public bool incluiParceriaAcaoIniciativaProcesso(int codigoProjeto, int codigoAcao, string codigoParceiro, string produto)
    {
        bool retorno = false;
        int regAf = 0;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN

                        DECLARE @NomeParceiro VarChar(100)

                        SELECT @NomeParceiro = SiglaUnidadeNegocio
                          FROM {0}.{1}.UnidadeNegocio
                         WHERE CodigoUnidadeNegocio = {4}

                        INSERT INTO {0}.{1}.tai06_ParceirosIniciativa(CodigoProjeto, CodigoAcao, CodigoParceiro, NomeParceiro, ProdutoSolicitado)
                                                                   VALUES({2}, {3}, {4}, @NomeParceiro, '{5}')

                       END
                        
                        ", bancodb, Ownerdb
                         , codigoProjeto, codigoAcao, codigoParceiro, produto.Replace("'", "''"));

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaParceriaAcaoIniciativaProcesso(int codigoParceria, string codigoParceiro, string produto)
    {
        bool retorno = false;
        int regAf = 0;
        try
        {
            comandoSQL = string.Format(@"
                       BEGIN

                        DECLARE @NomeParceiro VarChar(100)

 
                        SELECT @NomeParceiro = SiglaUnidadeNegocio
                          FROM {0}.{1}.UnidadeNegocio
                         WHERE CodigoUnidadeNegocio = {3}

                        UPDATE {0}.{1}.tai06_ParceirosIniciativa 
                           SET CodigoParceiro = {3}
                              ,NomeParceiro = @NomeParceiro
                              ,ProdutoSolicitado = '{4}'
                         WHERE SequenciaRegistro = {2}

                       END
                        ", bancodb, Ownerdb
                         , codigoParceria, codigoParceiro, produto.Replace("'", "''"));

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool excluiParceriaAcaoIniciativaProcesso(int codigoParceria)
    {
        bool retorno = false;
        int regAf = 0;
        try
        {
            comandoSQL = string.Format(@"
                        DELETE FROM {0}.{1}.tai06_ParceirosIniciativa 
                         WHERE SequenciaRegistro = {2}
                        ", bancodb, Ownerdb
                         , codigoParceria);

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public DataSet getCronogramaOrcamentarioProcesso(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
                  @"SELECT t1.CodigoAcao AS CodigoAtividade, t1.CodigoAcaoSuperior AS CodigoAcao
                          ,t1.NumeroAcao AS NumeroAtividade, tSup.NumeroAcao, tSup.NomeAcao AS NomeAcao
                          ,t1.NomeAcao AS NomeAtividade, t1.FonteRecurso, t1.IndicaSemRecurso
                          ,(SELECT SUM(ValorTotal) 
				                     FROM {0}.{1}.CronogramaOrcamentarioProcesso co INNER JOIN
                                          {0}.{1}.orc_planoContas opc ON (opc.SeqPlanoContas = co.SeqPlanoContas)
				                    WHERE co.CodigoAcao = t1.CodigoAcao 
				                      AND co.CodigoProjeto = t1.CodigoProjeto) AS Valor
			              ,ISNULL((SELECT TOP 1 'S'
				                     FROM {0}.{1}.CronogramaOrcamentarioProcesso co  INNER JOIN
                                          {0}.{1}.orc_planoContas opc ON (opc.SeqPlanoContas = co.SeqPlanoContas)
				                    WHERE co.CodigoAcao = t1.CodigoAcao 
				                      AND co.CodigoProjeto = t1.CodigoProjeto), 'N') AS PossuiContas
                     FROM {0}.{1}.tai06_AcoesIniciativa t1 INNER JOIN
		                  {0}.{1}.tai06_AcoesIniciativa tSup ON tSup.CodigoAcao = t1.CodigoAcaoSuperior
                    WHERE t1.CodigoAcao <> t1.CodigoAcaoSuperior 
                      AND t1.CodigoProjeto = {2}
                      {3}
                    ORDER BY tSup.NumeroAcao,  CASE WHEN t1.CodigoAcao = t1.CodigoAcaoSuperior THEN CONVERT(VARCHAR, t1.NumeroAcao)
                            ELSE CONVERT(VARCHAR, tSup.NumeroAcao) + '.' + CONVERT(VARCHAR, t1.NumeroAcao) END
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getValoresAcoesProjetoProcesso(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT t1.CodigoAcaoSuperior AS CodigoAcao, t1.FonteRecurso, SUM(ValorTotal) AS Valor			             
	             FROM {0}.{1}.tai06_AcoesIniciativa t1 INNER JOIN
			          {0}.{1}.CronogramaOrcamentarioProcesso co ON co.CodigoAcao = t1.CodigoAcao INNER JOIN
                      {0}.{1}.orc_planoContas opc ON (opc.SeqPlanoContas = co.SeqPlanoContas)
	            WHERE t1.CodigoAcao <> t1.CodigoAcaoSuperior 
		          AND t1.CodigoProjeto = {2}
		          AND IndicaSemRecurso = 'N'
                  {3}
                GROUP BY t1.CodigoAcaoSuperior, t1.FonteRecurso
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getContasAcoesProjetoProcesso(int codigoAcao, int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT co.CodigoProjeto, co.CodigoAcao, co.SeqPlanoContas, co.Quantidade, co.ValorUnitario
                      ,co.ValorTotal, co.MemoriaCalculo, co.Plan01, co.Plan02, co.Plan03, co.Plan04, co.Plan05
                      ,co.Plan06, co.Plan07, co.Plan08, co.Plan09, co.Plan10, co.Plan11, co.Plan12, opc.CONTA_DES, opc.CONTA_COD
                 FROM {0}.{1}.CronogramaOrcamentarioProcesso co INNER JOIN
			          {0}.{1}.orc_planoContas opc ON opc.SeqPlanoContas = co.SeqPlanoContas
                WHERE co.CodigoAcao = {2}
                  AND co.CodigoProjeto = {3}
                  {4}
                ORDER BY opc.CONTA_DES
               ", bancodb, Ownerdb, codigoAcao, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getContaAtividadeProcesso(int codigoContaOrcamentaria, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT co.CodigoProjeto, co.CodigoAcao, co.SeqPlanoContas, co.Quantidade, co.ValorUnitario
                      ,co.ValorTotal, co.MemoriaCalculo, co.Plan01, co.Plan02, co.Plan03, co.Plan04, co.Plan05
                      ,co.Plan06, co.Plan07, co.Plan08, co.Plan09, co.Plan10, co.Plan11, co.Plan12, opc.CONTA_DES, opc.CONTA_COD
                 FROM {0}.{1}.CronogramaOrcamentarioProcesso co INNER JOIN
			          {0}.{1}.orc_planoContas opc ON opc.SeqPlanoContas = co.SeqPlanoContas
                WHERE co.SeqPlanoContas = {2}
                  {3}
               ", bancodb, Ownerdb, codigoContaOrcamentaria, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getPlanoContasAnoAtualProcesso(int codigoConta, int codigoAtividade, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT SeqPlanoContas, Ano, CONTA_COD, CONTA_DES
			         ,CASE WHEN DespesaReceita = 'D' THEN 'Despesa' ELSE 'Receita' END AS DespesaReceita
                 FROM {0}.{1}.orc_planoContas opc
                WHERE opc.Ano = YEAR(GetDate()) -- SESCOOP TEM QUE ALTERAR ESTA CLAUSULA WHERE 
                  AND opc.SeqPlanoContas NOT IN(SELECT co.SeqPlanoContas 
                                                          FROM {0}.{1}.CronogramaOrcamentario co 
                                                         WHERE CodigoAcao = {3} 
                                                           AND co.SeqPlanoContas <> {2})
                 {4}
                ORDER BY DespesaReceita, CONTA_COD, CONTA_DES
               ", bancodb, Ownerdb, codigoConta, codigoAtividade, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiContaAtividadeProcesso(int codigoProjeto, int codigoAtividade, int codigoConta, string quantidade, string valorUnitario, string valorTotal
        , string memoriaCalculo, string plan01, string plan02, string plan03, string plan04, string plan05, string plan06, string plan07, string plan08
        , string plan09, string plan10, string plan11, string plan12)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN

                        INSERT INTO {0}.{1}.CronogramaOrcamentarioProcesso
                                   (CodigoProjeto
                                   ,CodigoAcao
                                   ,SeqPlanoContas
                                   ,Quantidade
                                   ,ValorUnitario
                                   ,ValorTotal
                                   ,MemoriaCalculo
                                   ,Plan01
                                   ,Plan02
                                   ,Plan03
                                   ,Plan04
                                   ,Plan05
                                   ,Plan06
                                   ,Plan07
                                   ,Plan08
                                   ,Plan09
                                   ,Plan10
                                   ,Plan11
                                   ,Plan12)
                             VALUES
                                   ({2}
                                   ,{3}
                                   ,{4}
                                   ,{5}
                                   ,{6}
                                   ,{7}
                                   ,'{8}'
                                   ,{9}
                                   ,{10}
                                   ,{11}
                                   ,{12}
                                   ,{13}
                                   ,{14}
                                   ,{15}
                                   ,{16}
                                   ,{17}
                                   ,{18}
                                   ,{19}
                                   ,{20})

                   END
                        ", bancodb, Ownerdb
                         , codigoProjeto, codigoAtividade, codigoConta, quantidade, valorUnitario, valorTotal, memoriaCalculo.Replace("'", "''"), plan01, plan02, plan03, plan04, plan05
                         , plan06, plan07, plan08, plan09, plan10, plan11, plan12);

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

    public bool atualizaContaAtividadeProcesso(int codigoContaAnterior, int codigoAtividade, int codigoConta, string quantidade, string valorUnitario, string valorTotal
        , string memoriaCalculo, string plan01, string plan02, string plan03, string plan04, string plan05, string plan06, string plan07, string plan08
        , string plan09, string plan10, string plan11, string plan12)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN

                        UPDATE {0}.{1}.CronogramaOrcamentarioProcesso
                           SET SeqPlanoContas = {4}
                              ,Quantidade = {5}
                              ,ValorUnitario = {6}
                              ,ValorTotal = {7}
                              ,MemoriaCalculo = '{8}'
                              ,Plan01 = {9}
                              ,Plan02 = {10}
                              ,Plan03 = {11}
                              ,Plan04 = {12}
                              ,Plan05 = {13}
                              ,Plan06 = {14}
                              ,Plan07 = {15}
                              ,Plan08 = {16}
                              ,Plan09 = {17}
                              ,Plan10 = {18}
                              ,Plan11 = {19}
                              ,Plan12 = {20}
                         WHERE SeqPlanoContas = {2}
	                       AND CodigoAcao = {3}

                   END
                        ", bancodb, Ownerdb
                         , codigoContaAnterior, codigoAtividade, codigoConta, quantidade, valorUnitario, valorTotal, memoriaCalculo.Replace("'", "''"), plan01, plan02, plan03, plan04, plan05
                         , plan06, plan07, plan08, plan09, plan10, plan11, plan12);

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

    public bool excluiContaAtividadeProcesso(int codigoConta, int codigoAtividade, int codigoProjeto)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN

                        DELETE {0}.{1}.CronogramaOrcamentarioProcesso
                         WHERE SeqPlanoContas = {2}
	                       AND CodigoAcao = {3}
                           AND CodigoProjeto = {4}

                   END
                        ", bancodb, Ownerdb
                         , codigoConta, codigoAtividade, codigoProjeto);

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

    #endregion

    #region Painel de Gerenciamento

    public int getDiasToleranciaUHE()
    {
        int diasTolerancia = 10;

        DataSet ds = getParametrosSistema("diaImportacaoDadosEPBM");

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]) && ds.Tables[0].Rows[0]["diaImportacaoDadosEPBM"].ToString().Trim() != "")
            diasTolerancia = int.Parse(ds.Tables[0].Rows[0]["diaImportacaoDadosEPBM"].ToString());

        return diasTolerancia;
    }

    public DataSet getAcessoObjetosPainelGerenciamento(int codigoUsuario, int codigoEntidade)
    {
        string comandoSQL = string.Format(
             @"BEGIN 
	            DECLARE @CodigoUsuario INT
				       ,@CodigoEntidade INT
				       ,@CodigoProjetoPimental INT
				       ,@CodigoProjetoBeloMonte INT
				       ,@CodigoProjetoInfraestrutura INT
				       ,@CodigoProjetoDerivacao INT
				       ,@CodigoProjetoReservatorios INT
				 
	            SELECT @CodigoProjetoPimental = CodigoProjeto
		          FROM {0}.{1}.Projeto 
	             WHERE CodigoReservado = 'St_Pimental'
	 
	            SELECT @CodigoProjetoBeloMonte = CodigoProjeto
		          FROM {0}.{1}.Projeto 
	             WHERE CodigoReservado = 'St_BeloMonte'
	 
	            SELECT @CodigoProjetoInfraestrutura = CodigoProjeto
		          FROM {0}.{1}.Projeto 
	             WHERE CodigoReservado = 'Infraestrutura'
	 
	            SELECT @CodigoProjetoDerivacao = CodigoProjeto
		          FROM {0}.{1}.Projeto 
	             WHERE CodigoReservado = 'Cnl_Derivacao'
	 
	            SELECT @CodigoProjetoReservatorios = CodigoProjeto
		          FROM {0}.{1}.Projeto 
	             WHERE CodigoReservado = 'Diques'
				 
	            SET @CodigoUsuario = {2}
	            SET @CodigoEntidade = {3}
	
	            SELECT {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoEntidade, null, 'EN', 0, null, 'EN_AcsGestEmp') AS GestaoEmpreendimento
				      ,{0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoEntidade, null, 'EN', 0, null, 'EN_AcsEapUhe') AS EAP
				      ,{0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoEntidade, null, 'EN', 0, null, 'EN_AcsVisGloUhe') AS VisaoGlobalUHE
				      ,{0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoEntidade, null, 'EN', 0, null, 'EN_AcsPnlDspUhe') AS PainelDesempenho
				      ,{0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoEntidade, null, 'EN', 0, null, 'EN_AcsVisCusUhe') AS VisaoCusto
				      ,{0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjetoPimental, null, 'PR', 0, null, 'PR_Acs') AS Pimental
				      ,{0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjetoBeloMonte, null, 'PR', 0, null, 'PR_Acs') AS BeloMonte
				      ,{0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjetoInfraestrutura, null, 'PR', 0, null, 'PR_Acs') AS Infra
				      ,{0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjetoDerivacao, null, 'PR', 0, null, 'PR_Acs') AS Derivacao
				      ,{0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoProjetoReservatorios, null, 'PR', 0, null, 'PR_Acs') AS Reservatorios
				      ,{0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoEntidade, null, 'EN', 0, null, 'EN_AcsIndPnlGer') AS Indicadores
	
	
            END                     
               ", bancodb, Ownerdb, codigoUsuario, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getAreasPainelGerenciamento(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
              @"  SELECT CodigoCategoria, DescricaoCategoria, SiglaCategoria
                    FROM Categoria c
                   WHERE CodigoEntidade = 1
                     AND DataExclusao IS NULL
                     AND IniciaisPermissaoRequerida IS NOT NULL
                     AND dbo.f_VerificaAcessoConcedido({2}, {3}, {3}, NULL, 'EN', 0, NULL, c.IniciaisPermissaoRequerida) > 0
                     {4}
                   ORDER BY DescricaoCategoria              
               ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDataGeracaoTurbina(int unidadeGeradora, int codigoEntidade)
    {
        string comandoSQL = string.Format(
              @"SELECT Min(TerminoLB) 
                  FROM {0}.{1}.f_uhe_GetMarcosObraPrincipal({2}) 
                 WHERE NumeroUnidadeGeradora = {3} 
                   AND TipoMarco = 'Unidade'              
               ", bancodb, Ownerdb, codigoEntidade, unidadeGeradora);
        return getDataSet(comandoSQL);
    }

    public DataSet getDataObtencaoLO(int codigoEntidade)
    {
        string comandoSQL = string.Format(
              @"SELECT {0}.{1}.f_uhe_DataObtencaoLO() AS DataLO             
               ", bancodb, Ownerdb, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getInformacoesPainelGerenciamento(int codigoEntidade, int codigoArea, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN
                  DECLARE @CodigoEntidade Int,
                          @UltimaAtualizacao DateTime,
                          @IndicaDesatualizado Char(1),
                          @CodigoCategoria Int

                    SET @CodigoEntidade = {2}
                    SET @CodigoCategoria = {3}
                  
                    SELECT f.CorFisico      AS CorFisicoUHE,
                           f.CorCusto  AS CorFinanceiroUHE,
                           f.PercFisicoReal AS DesempenhoUHE           
                      FROM {0}.{1}.f_uhe_BulletsProjeto('UHE_Principal',@CodigoEntidade, @CodigoCategoria) AS f             
    
                     SELECT f.CorFisico      AS CorFisicoPimental,          
                            f.CorCusto  AS CorFinanceiroPimental,
                            f.PercFisicoReal AS DesempenhoPimental           
                       FROM {0}.{1}.f_uhe_BulletsProjeto('St_Pimental',@CodigoEntidade, @CodigoCategoria) AS f       
                    
                     SELECT f.CorFisico      AS CorFisicoBeloMonte,
                            f.CorCusto  AS CorFinanceiroBeloMonte,
                            f.PercFisicoReal AS DesempenhoBeloMonte    
                       FROM {0}.{1}.f_uhe_BulletsProjeto('St_BeloMonte',@CodigoEntidade, @CodigoCategoria) AS f                   
    
                     SELECT f.CorFisico AS CorFisicoInfra,
                            f.CorCusto AS CorFinanceiroInfra,
                            f.PercFisicoReal AS DesempenhoInfra   
                       FROM {0}.{1}.f_uhe_BulletsProjeto('Infraestrutura',@CodigoEntidade, @CodigoCategoria) AS f           
    
                       SELECT f.CorFisico AS CorFisicoDerivacao,
                              f.CorCusto AS CorFinanceiroDerivacao,
                              f.PercFisicoReal AS DesempenhoDerivacao           
                         FROM {0}.{1}.f_uhe_BulletsProjeto('Cnl_Derivacao',@CodigoEntidade, @CodigoCategoria) AS f              
         
   
                       SELECT f.CorFisico AS CorFisicoDiques,
                              f.CorCusto AS CorFinanceiroDiques,
                              f.PercFisicoReal AS DesempenhoDiques 
                         FROM {0}.{1}.f_uhe_BulletsProjeto('Diques',@CodigoEntidade, @CodigoCategoria) AS f 
         
     
                    END                 
               ", bancodb, Ownerdb, codigoEntidade, codigoArea, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getNumerosContratosPainelGerenciamento(string codigoReservado, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"            
                      SELECT * FROM {0}.{1}.f_uhe_NumerosContratos('{2}', {3})
            ", bancodb, Ownerdb, codigoReservado, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getIndicadoresOperacionaisPainelGerenciamento(string codigoReservado, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"            
                      SELECT * FROM {0}.{1}.f_uhe_Indicadores('{2}', {3}, 'O')
            ", bancodb, Ownerdb, codigoReservado, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getIndicadoresEstrategicosPainelGerenciamento(string codigoReservado, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"            
                      SELECT * FROM {0}.{1}.f_uhe_Indicadores('{2}', {3}, 'D')
            ", bancodb, Ownerdb, codigoReservado, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getCurvaSPainelGerenciamento(string codigoReservado, int codigoEntidade, string codigoArea, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT * FROM {0}.{1}.f_uhe_getCurva_S({3}, '{2}', {4}) 
               ", bancodb, Ownerdb, codigoReservado, codigoEntidade, codigoArea, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getCurvaSEconomicaPainelGerenciamento(string codigoReservado, int codigoEntidade, string codigoArea, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT * FROM {0}.{1}.f_uhe_getCurva_S_Economica({3}, '{2}', {4}) 
               ", bancodb, Ownerdb, codigoReservado, codigoEntidade, codigoArea, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getCurvaSPainelPercentualCusto(string codigoArea, int codigoEntidade, int ano, string where)
    {
        string comandoSQL = string.Format(
            @"  SELECT DescricaoPeriodo, ValorRealContrato_I0 AS ValorRealizado
		              ,ValorPrevistoOrcamento AS ValorPrevisto
					  ,ValorRealOrcamento AS ValorRealizadoReajustado
					  ,ValorPrevistoContrato_I0 AS ValorPrevistoContrato
                      ,ValorPrevistoOrcamentoPeriodo AS ValorPrevistoPeriodo
                      ,ValorRealContrato_I0_Periodo AS ValorRealizadoPeriodo
					  ,ValorRealOrcamentoPeriodo AS ValorRealizadoReajustadoPeriodo
					  ,ValorPrevistoContrato_I0_Periodo AS ValorPrevistoContratoPeriodo
                      ,DataReferencia
                 FROM {0}.{1}.f_uhe_ComparativoOrcamentoContrato({2}, {4})
               
               ", bancodb, Ownerdb, codigoEntidade, ano, codigoArea, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getCurvaSPainelPercentualCapexOpex(string codigoArea, int codigoEntidade, int ano, string where)
    {
        string comandoSQL = string.Format(
            @"  SELECT DescricaoPeriodo, ValorPrevistoAcum AS ValorPrevisto, ValorRealAcum AS ValorRealizado, ValorPrevistoPeriodo, ValorRealPeriodo
		              ,PercentualRealAcum AS PercentualRealizado
		              ,PercentualPrevistoAcum AS PercentualPrevisto
                      ,DataReferencia
                 FROM {0}.{1}.f_uhe_OrcamentoCompetenciaPrevistoRealizado({2}, {3}, {4})
               
               ", bancodb, Ownerdb, codigoEntidade, ano, codigoArea, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getValoresRealizadosPorSitio(string codigoArea, int codigoEntidade, int ano, int mes, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT CodigoSitio AS Codigo, NomeSitio AS Descricao, ValorRealAcumReaj AS ValorRealizadoReajustado
                 FROM {0}.{1}.f_uhe_MedicaoPrevistaRealizadaPorSitio({2}, {3}, {4}, {5}, -1)
               ", bancodb, Ownerdb, codigoEntidade, ano, mes, codigoArea, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getValoresAcompanhamentoEscopo(string codigoArea, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT * FROM {0}.{1}.f_uhe_ResumoAcompanhamentoEscopo({2}, {3})
               ", bancodb, Ownerdb, codigoEntidade, codigoArea, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getValoresDesembolsoFinanceiro(string codigoArea, int codigoEntidade, string where)
    {
        int diasTolerancia = getDiasToleranciaUHE();
        DateTime data = DateTime.Now.Day < diasTolerancia ? DateTime.Now.AddMonths(-2) : DateTime.Now.AddMonths(-1);
        int ano = data.Year;
        double mesatual = data.Month;
        int trimestre = Convert.ToInt32(Math.Round(Convert.ToDouble(mesatual) / 3 + 0.25));

        string comandoSQL = string.Format(
              @"SELECT Descricao, ValorPrevisto, ValorReal, ValorPrevistoAcumulado, ValorRealAcumulado, PercPrevisto * 100 AS PercPrevisto, PercReal * 100 AS PercReal
                  FROM {0}.{1}.f_uhe_ResumoPrevisaoDesembolso({2}, {3}, {4}, {5})                
               ", bancodb, Ownerdb, codigoEntidade, codigoArea, ano, trimestre, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getAvancoFinanceiroSitio(string codigoArea, int codigoEntidade, int ano, int mes, string codigoSitio, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT NomeSitio AS Descricao, PercentualRealAcum * 100 AS PercentualRealizado, PercentualPrevistoAcum * 100 AS PercentualPrevisto
                      ,ValorRealAcumReaj AS ValorReal, ValorPrevistoAcum AS ValorPrevisto
                 FROM {0}.{1}.f_uhe_MedicaoPrevistaRealizadaPorSitio({2}, {3}, {4}, {5}, {6})
               ", bancodb, Ownerdb, codigoEntidade, ano, mes, codigoArea, codigoSitio);
        return getDataSet(comandoSQL);
    }

    public DataSet getAvancoFinanceiroPorSitio(string codigoArea, int codigoEntidade, int ano, int mes, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT NomeSitio AS Descricao, PercentualRealAcumReaj * 100 AS PercentualRealizado, PercentualPrevistoAcum * 100 AS PercentualPrevisto
                      ,ValorRealAcumReaj AS ValorReal, ValorPrevistoAcum AS ValorPrevisto, ValorPrevistoAcum - ValorRealAcumReaj AS ValorRestante
                      ,(1 - PercentualRealAcumReaj) * 100 AS PercentualRestante
                 FROM {0}.{1}.f_uhe_MedicaoPrevistaRealizadaPorSitio({2}, {3}, {4}, {5}, -1)
               ", bancodb, Ownerdb, codigoEntidade, ano, mes, codigoArea);

        //        string comandoSQL = string.Format(
        //              @"SELECT 'NomeSitio' AS Descricao, 0.3 * 100 AS PercentualRealizado
        //                       ,(1 - 0.3) * 100 AS PercentualRestante
        //                       ,300 AS ValorReal, 300 / 0.3 - 300 AS ValorRestante
        //               ", bancodb, Ownerdb, codigoEntidade, ano, mes, codigoArea);

        return getDataSet(comandoSQL);
    }

    public DataSet getValoresContratosPainelCustosNE(string where)
    {
        string comandoSQL = string.Format(
            @"SELECT DescricaoContrato AS Descricao, ValorContratado AS ValorContratado
			        ,ValorContratadoReaj AS ValorContratadoReajustado, ValorRealizado AS ValorRealizado
			        ,ValorRealizadoReaj AS ValorRealizadoReajustado 
               FROM {0}.{1}.f_uhe_ValoresContratosEspeciais()
               ", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getValoresSitiosPainelCustosNE(string codigoArea, int codigoEntidade, int ano, int mes, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT NomeSitio AS Descricao, ValorRealAcumReaj AS ValorRealizadoReajustado
                 FROM {0}.{1}.f_uhe_MedicaoPrevistaRealizadaPorSitio({2}, {3}, {4}, {5}, -1)
               ", bancodb, Ownerdb, codigoEntidade, ano, mes, codigoArea, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getFotosPainelGerenciamento(string codigoReservado, int codigoEntidade, int numeroFotos, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT * FROM {0}.{1}.f_uhe_Fotos('{2}', {3}, {4})
               WHERE 1 = 1
                 {5}
               ", bancodb, Ownerdb, codigoReservado, codigoEntidade, numeroFotos, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getCodigosFotosPainelGerenciamento(string codigoReservado, int codigoEntidade, int numeroFotos, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT * FROM {0}.{1}.f_uhe_CodigosFotos('{2}', {3}, {4})
               WHERE 1 = 1
                 {5}
               ", bancodb, Ownerdb, codigoReservado, codigoEntidade, numeroFotos, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getCodigosFotosPainelProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
            @"BEGIN
               DECLARE @CodigoProjeto Int
   
               DECLARE @tblRetorno TABLE
					            (CodigoFoto			Int,
					             DataVersao		DateTime,
					             DescricaoFoto	Varchar(500)) 
   
               /* Obtém o código do projeto relacionado ao código reservado da obra que foi informado */
               SET @CodigoProjeto = {2}
      
               /* 
                  Cursor para trazer as fotos relacionadas ao projeto em questão. Ele é necessário em virtude do 
                  parâmetro que filtra a quantidade de fotos a serem retornadas.
               */      
               INSERT INTO @tblRetorno (CodigoFoto, DataVersao)
                SELECT a.CodigoAnexo, Max(av.DataVersao)
	                FROM {0}.{1}.Anexo AS a INNER JOIN
					     {0}.{1}.AnexoAssociacao AS aa ON (aa.CodigoAnexo           = a.CodigoAnexo 
											                       AND aa.CodigoTipoAssociacao  = 4
											                       AND aa.CodigoObjetoAssociado = @CodigoProjeto) INNER JOIN
                       {0}.{1}.AnexoVersao AS av ON (av.codigoAnexo = a.CodigoAnexo) 
		             WHERE a.DataExclusao IS NULL
			             AND (av.nomeArquivo LIKE '%.JPG%' 
				            OR av.NomeArquivo LIKE '%.BMP%'
				            OR av.NomeArquivo LIKE '%.JPEG%'
				            OR av.NomeArquivo LIKE '%.PNG%')
		               AND a.IndicaPasta = 'N'	
		               AND NOT EXISTS(SELECT 1 
		                                FROM {0}.{1}.Anexo AS ai 
		                               WHERE ai.CodigoAnexo = a.CodigoPastaSuperior 
		                                 AND ai.Nome <> 'Evolução do Sítio'
		                                 AND ai.IndicaControladoSistema = 'S')
		              GROUP BY a.CodigoAnexo, av.DataVersao 				   
                 ORDER BY av.DataVersao DESC    
     
                 UPDATE @tblRetorno
                 SET  DescricaoFoto = a.descricaoAnexo
                 FROM {0}.{1}.Anexo a
                 WHERE a.codigoAnexo = codigoFoto;
     
                 SELECT * FROM @tblRetorno
     
            END   
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getFotoEscavacaoPainelGerenciamento(int codigoAnexo, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT a.DescricaoAnexo AS DescricaoFoto,
                     ca.Anexo AS Foto               
	            FROM {0}.{1}.Anexo AS a INNER JOIN					     
			         {0}.{1}.AnexoVersao AS av ON (av.codigoAnexo = a.CodigoAnexo) INNER JOIN                                                                     
			         {0}.{1}.ConteudoAnexo AS ca ON (ca.codigoSequencialAnexo = av.codigoSequencialAnexo)						 
               WHERE a.CodigoAnexo = {2}
	             AND av.dataVersao IN(SELECT MAX(av2.DataVersao) FROM AnexoVersao av2 WHERE av2.codigoAnexo = a.CodigoAnexo) 
                 {3}
               ORDER BY av.DataVersao DESC
               ", bancodb, Ownerdb, codigoAnexo, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getFotosCanaisDiques(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT * FROM {0}.{1}.f_uhe_CodigosFotos('Diques', {2}, 2) ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }


    public DataSet getFotosCanalDerivacao(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT * FROM {0}.{1}.f_uhe_CodigosFotos('Cnl_Derivacao', {2}, 2)
              UNION ALL
              SELECT * FROM {0}.{1}.f_uhe_CodigosFotos('Cnl_Transp', {2}, 2)
               ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }



    public DataSet getFotosGaleria(string iniciaisAssociacao, int codigoObjetoAssociado, int numeroFotos, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT TOP {4} ca.Anexo, a.DescricaoAnexo, ISNULL(a.dataCheckin, a.datainclusao) AS Data
				FROM {0}.{1}.Anexo AS a INNER JOIN
					 {0}.{1}.AnexoAssociacao AS aa ON (aa.CodigoAnexo = a.CodigoAnexo 
											  AND aa.CodigoTipoAssociacao = 14) INNER JOIN
					 {0}.{1}.AnexoVersao AS av ON (av.codigoAnexo = a.CodigoAnexo) INNER JOIN                                                                     
					 {0}.{1}.ConteudoAnexo AS ca ON (ca.codigoSequencialAnexo = av.codigoSequencialAnexo)						 
				WHERE a.DataExclusao IS NULL
					AND (av.nomeArquivo LIKE '%.JPG%' 
					OR av.NomeArquivo LIKE '%.BMP%'
					OR av.NomeArquivo LIKE '%.PNG%')
                ORDER BY ISNULL(a.dataCheckIn, DataInclusao) DESC
               ", bancodb, Ownerdb, iniciaisAssociacao, codigoObjetoAssociado, numeroFotos, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoFisicoPainelGerenciamento(string codigoReservado, int codigoEntidade, int codigoArea, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT PercFisicoReal AS PercentualReal, PercFisicoPrevisto / 100 AS PercentualPrevisto, CorFisico AS corDesempenho
                 FROM {0}.{1}.f_uhe_BulletsProjeto('{2}', {3}, {4})                                       
               ", bancodb, Ownerdb, codigoReservado, codigoEntidade, codigoArea, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoFinanceiroPainelGerenciamento(string codigoReservado, int codigoEntidade, int codigoArea, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT PercCustoReal AS PercentualReal, PercCustoPrevisto / 100 AS PercentualPrevisto, CorCusto AS corDesempenho
                 FROM {0}.{1}.f_uhe_BulletsProjeto('{2}', {3}, {4})               
               ", bancodb, Ownerdb, codigoReservado, codigoEntidade, codigoArea, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoFisicoFinanceiroPainelGerenciamento(string codigoReservado, int codigoEntidade, int codigoArea, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT * FROM {0}.{1}.f_uhe_BulletsProjeto('{2}', {3}, {4})                          
               ", bancodb, Ownerdb, codigoReservado, codigoEntidade, codigoArea, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getAlocacaoMaoObraEquipamentoPeriodoPainelGerenciamento(string codigoReservado, string parametroIndicador, int codigoEntidade, int codigoArea, string where)
    {
        string comandoSQL = string.Format(
             @"       
               BEGIN
		                DECLARE @CodigoIndicador int,
						        @CodigoEntidade int,
                                @CodigoCategoria int
						
		                SET @CodigoEntidade = {4}
                        SET @CodigoCategoria = {5}
		
		                SELECT @CodigoIndicador = Valor FROM ParametroConfiguracaoSistema
		                 WHERE Parametro = '{3}'	
			                 AND CodigoEntidade = @CodigoEntidade
		
		                SELECT MesAno AS Periodo, ValorPrevisto AS Previsto, ValorReal AS Real
                          FROM {0}.{1}.f_uhe_HistoricoIndicadorProjeto(@CodigoIndicador, '{2}', @CodigoEntidade, YEAR(GetDate()), MONTH(GetDate()), 5, 'S', @CodigoCategoria)

                END         
               ", bancodb, Ownerdb, codigoReservado, parametroIndicador, codigoEntidade, codigoArea, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoProjetosPainelGerenciamento(string codigoReservado, int codigoEntidade, int codigoArea, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT Satisfatorio, Atencao, Finalizando, Excelente, SemAcompanhamento AS Sem_Acompanhamento, Critico 
                FROM {0}.{1}.f_uhe_ProjetosDesempenho('{2}', {3}, {4})
            ", bancodb, Ownerdb, codigoReservado, codigoEntidade, codigoArea, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getAlocacaoMaoObraEquipamentosPainelGerenciamento(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT 'Mão de Obra' AS Descricao, 80 AS Previsto, 20 AS Real 
               UNION    
               SELECT 'Equipamentos' AS Descricao, 60 AS Previsto, 40 AS Real            
               ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getTabelasPainelPresidencia(string nomeSitio, int codigoEntidade, int unidadeGeradora, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT Concluidos, ConcluidosAmarelo, ConcluidosVerde, ConcluidosVermelho
			        ,AConcluir, AConcluirAmarelo, AConcluirVerde, AConcluirVermelho, CorStatusUnidade 
	           FROM {0}.{1}.f_getMarcosObraPrincipalPorDesempenho({2}, '{3}', {4})
               ", bancodb, Ownerdb, codigoEntidade, nomeSitio, unidadeGeradora);
        return getDataSet(comandoSQL);
    }

    public DataSet getTabelasPainelPresidenciaDS(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT Concluidos, ConcluidosAmarelo, ConcluidosVerde, ConcluidosVermelho
			        ,AConcluir, AConcluirAmarelo, AConcluirVerde, AConcluirVermelho, CorStatusUnidade 
	           FROM {0}.{1}.[f_uhe_getMarcosSocioambientalPorDesempenho]({2})
               ", bancodb, Ownerdb, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getTarefasStatusPainelPresidencia(int codigoEntidade, string nomeSitio, int unidadeGeradora)
    {
        string comandoSQL = string.Format(
            @"SELECT * 
	           FROM {0}.{1}.f_uhe_getMarcosUnidade({2}, '{3}', {4})
              ORDER BY PrevisaoTermino
               ", bancodb, Ownerdb, codigoEntidade, nomeSitio, unidadeGeradora);

        return getDataSet(comandoSQL);
    }

    public DataSet getTarefasStatusPainelPresidenciaDS(int codigoEntidade)
    {
        string comandoSQL = string.Format(
            @"SELECT * 
	           FROM {0}.{1}.f_uhe_CronogramaSocioAmbiental({2})
              ORDER BY PrevisaoTermino
               ", bancodb, Ownerdb, codigoEntidade);

        return getDataSet(comandoSQL);
    }

    public DataSet getListaPrevisaoDesembolso(int codigoEntidade, int area, int ano, int trimestre)
    {
        string comandoSQL = string.Format(
            @"SELECT * 
	           FROM {0}.{1}.f_uhe_ListaPrevisaoDesembolso({2}, {3}, {4}, {5})
              WHERE Mes IS NULL
              ORDER BY NomeConta
               ", bancodb, Ownerdb, codigoEntidade, area, ano, trimestre);

        return getDataSet(comandoSQL);
    }

    public DataSet getMapaIndicadoresPainelPresidencia(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
            @"select * from {0}.{1}.[f_uhe_ListaGestaoIndicadoresEstrategicos]({2})
               WHERE 1 = 1
                 {3}
                       ", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getListaAcompanhamentoCusto(int codigoEntidade, int categoria, int ano)
    {
        string comandoSQL = string.Format(
        @"SELECT * FROM {0}.{1}.f_uhe_DetalheOrcamentoCompetenciaPrevistoRealizado({2}, {4}, {3})
           ORDER BY Grupo, Conta 
        ", bancodb, Ownerdb, codigoEntidade, categoria, ano);

        return getDataSet(comandoSQL);
    }

    public DataSet getListaAcompanhamentoEscopo(int codigoEntidade, int categoria, string status)
    {
        string whereStatus = "";
        if (status != "")
        {
            whereStatus = string.Format(" where Status = '{0}'", status);
        }

        string comandoSQL = string.Format(
        @"SELECT * FROM {0}.{1}.f_uhe_ListaAcompanhamentoEscopo({2},{3})
              {4}
        order by dataprevista, Item 
        ", bancodb, Ownerdb, codigoEntidade, categoria, whereStatus);

        return getDataSet(comandoSQL);
    }

    public DataSet getArvoreAcompanhamentoEscopo(int codigoEntidade, int categoria, string status)
    {
        string comandoSQL = string.Format(
        @"SELECT * 
            FROM {0}.{1}.f_uhe_AcompanhamentoEscopoPorPercentual({2}, {3}, '{4}')
           ORDER BY dataprevista, Item 
        ", bancodb, Ownerdb, codigoEntidade, categoria, status);

        return getDataSet(comandoSQL);
    }

    public DataSet getTabelaIndicadoresPainelPresidencia(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
             @"select * from f_uhe_GetCorIndicadoresGerais({2})
               ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public string getUnidadeMedidaMaoObraEquipamento(int codigoEntidade, string nomeParametro)
    {
        string unidadeMedida = "";

        string comandoSQL = string.Format(
             @"BEGIN
	                DECLARE @CodigoIndicador int
	
	                SELECT @CodigoIndicador = ISNULL(Valor, -1)
                      FROM {0}.{1}.ParametroConfiguracaoSistema 
                     WHERE Parametro = '{3}'
	                   AND CodigoEntidade = {2} 
	
	                SELECT tum.SiglaUnidadeMedida 
  	                  FROM {0}.{1}.IndicadorOperacional i INNER JOIN
	 	                   {0}.{1}.TipoUnidadeMedida tum ON tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida
	                 WHERE CodigoIndicador = @CodigoIndicador 

                END
               ", bancodb, Ownerdb, codigoEntidade, nomeParametro);
        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            unidadeMedida = ds.Tables[0].Rows[0]["SiglaUnidadeMedida"].ToString();

        return unidadeMedida;
    }

    public DataSet getTarefasGanttPainelGerenciamento(int codigoEntidade, string sitio, int unidadeGeradora, string palavraChave, string apenasConcluidos, string dataTermino, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT * FROM {0}.{1}.f_uhe_ganttUnidadeGeradora({2}, '{3}', {4}, {5}, {6}, {7}) 
                 WHERE 1 = 1
                   {8}
               ", bancodb, Ownerdb, codigoEntidade, sitio, unidadeGeradora, palavraChave, apenasConcluidos, dataTermino, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoDesempenhoPaineisUHE(DataTable dt, DataTable dtIndicadores, int fonte, string urlImage, string colPercPrevisto, string colPercReal, string titulo, bool mostraCorLaranja)
    {
        StringBuilder xml = new StringBuilder();

        float percentualFisico = 0;
        float percentualPrevisto = 0;
        string real = "";
        string previsto = "";
        string desempenhoProjeto = "";
        string corNivel = corSatisfatorio;

        try
        {
            desempenhoProjeto = mostraCorLaranja ? dt.Rows[0]["corDesempenho"].ToString() : "";
            real = dt.Rows[0][colPercReal].ToString();
            previsto = dt.Rows[0][colPercPrevisto].ToString();
            if ((real != "") && (previsto != ""))
            {

                percentualFisico = float.Parse(real);
                percentualPrevisto = float.Parse(previsto);

            }
        }
        catch
        {

        }

        string exportar = "";

        if (desempenhoProjeto == "Laranja" && mostraLaranja == "S")
            corNivel = "FF6600";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1"" ";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        float valorPercentualPrevisto = percentualPrevisto;

        percentualPrevisto = percentualPrevisto == -1 ? 1 : percentualPrevisto;

        //configuração geral do gráfico
        xml.Append("<chart imageSave=\"1\" imageSaveURL=\"" + urlImage + "\" upperLimit=\"100\" showValue=\"1\" decimals=\"2\" decimalSeparator=\",\" " +
                            "inDecimalSeparator=\",\" baseFontSize=\"" + fonte + "\"  " + exportar +
                            "bgColor=\"FFFFFF\" showBorder=\"0\" chartTopMargin=\"5\" " +
                            "chartBottomMargin=\"20\" lowerLimit=\"0\" numberSuffix=\"%\" " +
                            "gaugeFillRatio=\"\" >");
        xml.Append(" <colorRange>");

        if (percentualFisico == 100)
        {
            try
            {
                //define a escala do gráfico
                xml.Append(" <color minValue=\"0\" maxValue=\"" + (int)(float.Parse(dtIndicadores.Rows[0][1].ToString())) + "\" name=\" \" code=\"" + corCritico + "\" />");
                xml.Append(" <color minValue=\"" + (int)(float.Parse(dtIndicadores.Rows[0][1].ToString())) +
                    "\" maxValue=\"" + (int)(float.Parse(dtIndicadores.Rows[1][1].ToString())) + "\" name=\" \" code=\"" + corAtencao + "\" />");
                xml.Append(" <color minValue=\"" + (int)(float.Parse(dtIndicadores.Rows[1][1].ToString())) + "\" maxValue=\"100\" name=\" \" code=\"" + corNivel + "\" />");
            }
            catch { }
        }
        else
        {
            if ((percentualPrevisto * 100) == 1)
            {
                //define a escala do gráfico
                xml.Append(" <color minValue=\"0\" maxValue=\"1\" name=\" \" code=\"" + corCritico + "\" />");
                xml.Append(" <color minValue=\"1\" maxValue=\"100\" name=\" \" code=\"" + corNivel + "\" />");
            }
            else if ((percentualPrevisto * 100) <= 1 && valorPercentualPrevisto > 0)
            {
                //define a escala do gráfico
                xml.Append(" <color minValue=\"0\" maxValue=\"100\" name=\" \" code=\"" + corNivel + "\" />");
            }
            else
            {
                if ((percentualPrevisto * 100) <= 2 && valorPercentualPrevisto > 0)
                {
                    //define a escala do gráfico
                    xml.Append(" <color minValue=\"0\" maxValue=\"1\" name=\" \" code=\"" + corCritico + "\" />");
                    xml.Append(" <color minValue=\"1\" maxValue=\"100\" name=\" \" code=\"" + corNivel + "\" />");
                }
                else
                {
                    if ((percentualPrevisto * 100) == 100 && valorPercentualPrevisto > 0 && (desempenhoProjeto != "Laranja" || mostraLaranja == "N"))
                    {
                        //define a escala do gráfico
                        xml.Append(" <color minValue=\"0\" maxValue=\"99\" name=\" \" code=\"" + corCritico + "\" />");
                        xml.Append(" <color minValue=\"99\" maxValue=\"100\" name=\" \" code=\"" + corNivel + "\" />");
                    }
                    else
                    {
                        try
                        {
                            //define a escala do gráfico
                            xml.Append(" <color minValue=\"0\" maxValue=\"" + (int)(percentualPrevisto * float.Parse(dtIndicadores.Rows[0][1].ToString())) + "\" name=\" \" code=\"" + corCritico + "\" />");
                            xml.Append(" <color minValue=\"" + (int)(percentualPrevisto * float.Parse(dtIndicadores.Rows[0][1].ToString())) +
                                "\" maxValue=\"" + (int)(percentualPrevisto * float.Parse(dtIndicadores.Rows[1][1].ToString())) + "\" name=\" \" code=\"" + corAtencao + "\" />");
                            xml.Append(" <color minValue=\"" + (int)(percentualPrevisto * float.Parse(dtIndicadores.Rows[1][1].ToString())) + "\" maxValue=\"100\" name=\" \" code=\"" + corNivel + "\" />");
                        }
                        catch { }
                    }
                }
            }
        }

        string percentual = "" + percentualPrevisto * 100;

        xml.Append(" </colorRange>");
        xml.Append(" <dials>");
        xml.Append(" <dial value=\"" + (percentualFisico) + "\"/>");
        xml.Append(" </dials>");
        xml.Append(" <trendpoints>");
        xml.Append(" <point startValue=\"" + percentual + "\" markerTooltext=\"Meta = " + percentual + "%\" useMarker=\"1\" dashed=\"1\" dashLen=\"2\" dashGap=\"2\" valueInside=\"1\"/>");
        xml.Append(" </trendpoints>");
        xml.Append("</chart>");

        return xml.ToString();
    }

    public string getGraficoBulletsSitio(DataTable dt, string tipoDados,
        string colunaPrevistoData, string colunaReal, string colunaCor, string urlImage, string subTitulo, int fonte)
    {
        StringBuilder xml = new StringBuilder();
        string cor = "", corDesempenho = "";
        double previstoData = 0, realGeral = 0;

        if (DataTableOk(dt))
        {
            previstoData = dt.Rows[0][colunaPrevistoData].ToString() == "" ? 0 : double.Parse(dt.Rows[0][colunaPrevistoData].ToString());
            realGeral = dt.Rows[0][colunaReal].ToString() == "" ? 0 : double.Parse(dt.Rows[0][colunaReal].ToString());
            corDesempenho = dt.Rows[0][colunaCor].ToString();
        }

        if (tipoDados == "Físico")
        {
            if (corDesempenho == "Verde")
            {
                cor = corSatisfatorio;
            }
            else if (corDesempenho == "Amarelo")
            {
                cor = corAtencao;
            }
            else
            {
                cor = corCritico;
            }

        }
        else
        {
            if (corDesempenho == "Verde")
            {
                cor = corSatisfatorio;
            }
            else
            {
                if (corDesempenho == "Amarelo")
                {
                    cor = corAtencao;
                }
                else
                {
                    cor = corCritico;
                }
            }
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

        xml.Append("<chart imageSave=\"1\" imageSaveURL=\"" + urlImage + "\" palette=\"1\" majorTMNumber=\"3\" minorTMNumber=\"3\" adjustTM=\"0\" caption=\"" + tipoDados + "\" plotFillColor=\"" + cor + "\" animation=\"1\" lowerLimit=\"0\"" +
            " chartTopMargin=\"5\" chartRightMargin=\"30\" valuePadding=\"0\" decimals=\"2\" forceDecimals=\"1\" inThousandSeparator=\".\" inDecimalSeparator=\",\" decimalSeparator=\",\" chartBottomMargin=\"2\" chartLeftMargin=\"1\" showPlotBorder=\"1\" plotBorderColor='333333' plotBorderThickness='1' colorRangeFillRatio=\"0,10,80,10\" plotBorderAlpha='35'  " +
            " showShadow=\"1\" baseFontSize=\"" + fonte + "\" subcaption=\"" + subTitulo + "\"" +
            " showColorRangeBorder=\"1\" plotFillPercent=\"55\" targetFillPercent=\"65\"  " + usarGradiente + usarBordasArredondadas + exportar + " roundRadius=\"0\" upperLimit=\"100\" formatNumberScale=\"1\" thousandSeparator=\".\" canvasLeftMargin=\"" + (7 * fonte) + "\">");

        xml.Append("<colorRange>");
        xml.Append("<color minValue=\"0\" maxValue=\"100\" code=\"" + corFundoBullets + "\"/>");
        xml.Append("</colorRange>");
        xml.Append("<value>" + string.Format("{0:n0}", realGeral).Replace(".", "") + "</value>");
        xml.Append("<target>" + string.Format("{0:n0}", previstoData).Replace(".", "") + "</target>");
        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte + 1) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontSubTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("</definition>");
        xml.Append("<application>");
        xml.Append("<apply toObject=\"Caption\" styles=\"fontTitulo\" />");
        xml.Append("<apply toObject=\"SubCaption\" styles=\"fontSubTitulo\" />");
        xml.Append("<apply toObject=\"LIMITVALUES\" styles=\"fontSubTitulo\" />");
        xml.Append("<apply toObject=\"TICKVALUES\" styles=\"fontSubTitulo\" />");
        xml.Append("</application>");
        xml.Append("</styles>");
        xml.Append("</chart>");

        return xml.ToString();
    }

    public string getGraficoMaoDeObraPainelGerenciamento(DataTable dt, string titulo, int fonte)
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

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""2"" chartLeftMargin=""5"" chartRightMargin=""1"" 
                                    chartTopMargin=""5"" canvasBgColor=""F7F7F7"" canvasLeftMargin=""15"" canvasRightMargin=""20"" canvasBottomMargin=""2""
                                    showvalues=""1""  showBorder=""0"" BgColor=""F7F7F7"" labelPadding=""0"" decimals=""1""
                                    inDecimalSeparator="","" showLegend=""1""  inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" >", usarGradiente + usarBordasArredondadas + exportar
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
        //xml.Append(string.Format(@"<dataset seriesName=""Previsto"">"));

        //try
        //{
        //    for (i = 0; i < dt.Rows.Count; i++)
        //    {
        //        string valor = dt.Rows[i]["Previsto"].ToString();

        //        string displayValue = valor == "" ? "-" : string.Format("{0:n2}", double.Parse(valor));

        //        xml.Append(string.Format(@"<set value=""{0}"" toolText=""Previsto: {1}"" color=""{2}""/> ", valor == "" ? "0" : valor, displayValue, corReal));
        //    }

        //}
        //catch
        //{
        //    return "";
        //}

        //xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Realizado"" color=""{0}"" >", corReal));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["Real"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n2}", double.Parse(valor));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Realizado: {1}"" color=""{2}"" /> ", valor == "" ? "0" : valor, displayValue, corReal));
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

    public string getGraficoCurvaSCustoGerenciamento(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot)
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

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""2"" chartLeftMargin=""5"" chartRightMargin=""1"" chartTopMargin=""5"" canvasBgColor=""F7F7F7"" canvasLeftMargin=""15"" canvasRightMargin=""20"" canvasBottomMargin=""2""
                                    numDivLines=""{4}"" showvalues=""0""  numVisiblePlot=""{5}""  showBorder=""0"" BgColor=""F7F7F7"" labelDisplay=""NONE"" labelPadding=""0"" 
                                    inDecimalSeparator="","" scrollHeight=""{3}"" showLegend=""1""  inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" >", usarGradiente + usarBordasArredondadas + exportar
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
                xml.Append(string.Format(@"<category label=""{0}"" />", dt.Rows[i]["DescricaoPeriodo"].ToString()));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));
        /*PercentualRealizadoReajustado
					  ,1000 AS PercentualPrevistoContrato*/

        xml.Append(string.Format(@"<dataset seriesName=""Orçamento"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorPrevisto"].ToString();
                string valorPeriodo = dt.Rows[i]["ValorPrevistoPeriodo"].ToString();
                string dataRef = dt.Rows[i]["DataReferencia"].ToString();

                int anoRef = DateTime.Parse(dataRef).Year;
                int mesRef = DateTime.Parse(dataRef).Month;
                int anoAtual = DateTime.Now.Year;
                int mesAtual = DateTime.Now.Month;

                char dashed = '0';

                if (anoAtual > anoRef)
                    dashed = '0';
                else
                {
                    if (anoAtual == anoRef && mesAtual > mesRef)
                        dashed = '0';
                    else
                        dashed = '1';
                }

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));
                string displayValuePeriodo = valorPeriodo == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorPeriodo));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Orçamento(Acumulado): {1}{4}Orçamento(No Período): {3}"" dashed=""{2}""/> ", valor == "" ? "0" : valor, displayValue, dashed, displayValuePeriodo, "{br}"));
            }
        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Contrato I0"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorPrevistoContrato"].ToString();
                string valorPeriodo = dt.Rows[i]["ValorPrevistoContratoPeriodo"].ToString();
                string dataRef = dt.Rows[i]["DataReferencia"].ToString();

                int anoRef = DateTime.Parse(dataRef).Year;
                int mesRef = DateTime.Parse(dataRef).Month;
                int anoAtual = DateTime.Now.Year;
                int mesAtual = DateTime.Now.Month;

                char dashed = '0';

                if (anoAtual > anoRef)
                    dashed = '0';
                else
                {
                    if (anoAtual == anoRef && mesAtual > mesRef)
                        dashed = '0';
                    else
                        dashed = '1';
                }

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));
                string displayValuePeriodo = valorPeriodo == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorPeriodo));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Contrato I0 (Acumulado): {1}{3}Contrato I0 (No Período): {4}"" dashed=""{2}""/> ", valor == "" ? "0" : valor, displayValue, dashed, "{br}", displayValuePeriodo));
            }
        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Realizado I0"" showValues=""0"">"));
        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorRealizado"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                string valorPeriodo = dt.Rows[i]["ValorRealizadoPeriodo"].ToString();

                string displayValuePeriodo = valorPeriodo == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorPeriodo));

                string dataRef = dt.Rows[i]["DataReferencia"].ToString();

                if (dataRef != "")
                {
                    int anoRef = DateTime.Parse(dataRef).Year;
                    int mesRef = DateTime.Parse(dataRef).Month;
                    int anoAtual = DateTime.Now.Year;
                    int mesAtual = DateTime.Now.Month;

                    bool mostraReg = false;


                    if (anoAtual > anoRef)
                        mostraReg = true;
                    else
                    {
                        if (anoAtual == anoRef && mesAtual >= mesRef)
                            mostraReg = true;
                        else
                            mostraReg = false;
                    }


                    if (mostraReg)
                        xml.Append(string.Format(@"<set value=""{0}"" toolText=""Realizado I0(Acumulado): {1}{2}Realizado I0(No Período): {3}""/> ", valor, displayValue, "{br}", displayValuePeriodo));
                }
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Realizado Reajustado"" showValues=""0"">"));
        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorRealizadoReajustado"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                string valorPeriodo = dt.Rows[i]["ValorRealizadoReajustadoPeriodo"].ToString();

                string displayValuePeriodo = valorPeriodo == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorPeriodo));

                string dataRef = dt.Rows[i]["DataReferencia"].ToString();

                if (dataRef != "")
                {
                    int anoRef = DateTime.Parse(dataRef).Year;
                    int mesRef = DateTime.Parse(dataRef).Month;
                    int anoAtual = DateTime.Now.Year;
                    int mesAtual = DateTime.Now.Month;

                    bool mostraReg = false;


                    if (anoAtual > anoRef)
                        mostraReg = true;
                    else
                    {
                        if (anoAtual == anoRef && mesAtual >= mesRef)
                            mostraReg = true;
                        else
                            mostraReg = false;
                    }


                    if (mostraReg)
                        xml.Append(string.Format(@"<set value=""{0}"" toolText=""Realizado Reajustado(Acumulado): {1}{2}Realizado Reajustado(No Período): {3}""/> ", valor, displayValue, "{br}", displayValuePeriodo));
                }
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

    public string getGraficoCurvaSCapexOpex(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot)
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

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""2"" chartLeftMargin=""5"" chartRightMargin=""10"" chartTopMargin=""5"" canvasBgColor=""F7F7F7"" canvasLeftMargin=""15"" canvasRightMargin=""20"" canvasBottomMargin=""2""
                                    numDivLines=""{4}"" showvalues=""0""  numVisiblePlot=""{5}""  showBorder=""0"" BgColor=""F7F7F7"" labelDisplay=""NONE"" 
                                    inDecimalSeparator="",""  showLegend=""1""  inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" >", usarGradiente + usarBordasArredondadas + exportar
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
                xml.Append(string.Format(@"<category label=""{0}"" />", dt.Rows[i]["DescricaoPeriodo"].ToString()));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));

        xml.Append(string.Format(@"<dataset seriesName=""Orçamento"" showValues=""0"">"));

        int anoAtual = DateTime.Now.Year;
        int mesAtual = DateTime.Now.Month;

        string comandoSQL = string.Format(@"select top 1 Mes, Ano
                                                        from uhe_PlanilhaOrcamento
                                                        where Mes is not null
                                                        order by dataImportacao desc");

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            anoAtual = int.Parse(ds.Tables[0].Rows[0]["Ano"].ToString());
            mesAtual = int.Parse(ds.Tables[0].Rows[0]["Mes"].ToString());
        }

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorPrevisto"].ToString();
                string valorPeriodo = dt.Rows[i]["ValorPrevistoPeriodo"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));
                string displayValuePeriodo = valorPeriodo == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorPeriodo));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Orçamento(Acumulado): {1}{2}Orçamento(No Período):{3}"" /> ", valor == "" ? "0" : valor, displayValue, "{BR}", displayValuePeriodo));

            }
        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Realizado"" showValues=""0"" >"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorRealizado"].ToString();
                string valorPeriodo = dt.Rows[i]["ValorRealPeriodo"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));
                string displayValuePeriodo = valorPeriodo == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorPeriodo));

                string dataRef = dt.Rows[i]["DataReferencia"].ToString();

                if (dataRef != "")
                {
                    int anoRef = DateTime.Parse(dataRef).Year;
                    int mesRef = DateTime.Parse(dataRef).Month;

                    char dashed = '0';
                    string labelRealizado = "Realizado";

                    if (anoAtual > anoRef)
                        dashed = '0';
                    else
                    {
                        if (anoAtual == anoRef && mesAtual >= mesRef)
                            dashed = '0';
                        else
                        {
                            valor = "";
                        }
                    }

                    xml.Append(string.Format(@"<set value=""{0}"" toolText=""{5}(Acumulado): {1}{2}{5}(No Período):{3}"" dashed=""0""/> ", valor, displayValue, "{BR}", displayValuePeriodo, dashed, labelRealizado));
                }
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Melhor Estimativa"" showValues=""0"" >"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorRealizado"].ToString();
                string valorPeriodo = dt.Rows[i]["ValorRealPeriodo"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));
                string displayValuePeriodo = valorPeriodo == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorPeriodo));

                string dataRef = dt.Rows[i]["DataReferencia"].ToString();

                if (dataRef != "")
                {
                    int anoRef = DateTime.Parse(dataRef).Year;
                    int mesRef = DateTime.Parse(dataRef).Month;

                    char dashed = '0';
                    string labelRealizado = "Melhor Estimativa";

                    if (anoAtual > anoRef)
                        valor = "";
                    else
                    {
                        if (anoAtual == anoRef && mesAtual > mesRef)
                            valor = "";
                    }

                    if (anoAtual == anoRef && mesAtual == mesRef)
                        xml.Append(string.Format(@"<set value=""{0}"" anchorAlpha='0' anchorRadius='1' showToolTip='0' toolText="" - "" dashed=""1""/> ", valor, displayValue, "{BR}", displayValuePeriodo, dashed, labelRealizado));
                    else
                        xml.Append(string.Format(@"<set value=""{0}"" toolText=""{5}(Acumulado): {1}{2}{5}(No Período):{3}"" dashed=""1""/> ", valor, displayValue, "{BR}", displayValuePeriodo, dashed, labelRealizado));
                }
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

    public string getGraficoValoresRealizadosPorSitio(DataTable dt, string codigoArea, string titulo, int fonte, bool linkGrafico)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        string link = ""; ;
        int i = 0;

        try
        {
            try
            {
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    string tituloPopup = Server.UrlEncode("Resumo dos Avanços Financeiros nas Obras - " + dt.Rows[i]["Descricao"].ToString());

                    if (linkGrafico)
                        link = string.Format(@"link=""JavaScript: isJavaScriptCall=true; abreGrafico({0}, {1}, '{2}');"" ", dt.Rows[i]["Codigo"].ToString()
                                                                                                                                    , codigoArea
                                                                                                                                    , tituloPopup);

                    xmlAux.Append(string.Format(@"<set label=""{0}"" value=""{1}"" {2}/>", dt.Rows[i]["Descricao"].ToString(),
                                                                                           dt.Rows[i]["ValorRealizadoReajustado"],
                                                                                           link));
                }

            }
            catch
            {
                return "";
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
        xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\" showPercentValues=\"1\" showPercentInToolTip=\"0\" formatNumberScale=\"0\"" +
            " chartTopMargin=\"5\" showZeroPies=\"0\" chartRightMargin=\"8\" inThousandSeparator=\".\" thousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"4\" numberPrefix=\"R$\"" +
            " adjustDiv=\"1\"  slantLabels=\"1\" labelDisplay=\"ROTATE\" labelDistance=\"1\" enablesmartlabels=\"0\" showLegend=\"1\" minimiseWrappingInLegend=\"1\"  legendNumColumns=\"3\" inDecimalSeparator=\",\" decimalSeparator=\",\"" +
            usarGradiente + usarBordasArredondadas + exportar + " showShadow=\"1\" showLabels=\"0\" baseFontSize=\"" + fonte + "\">");
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

    public string getGraficoAvancoFinanceiroSitio(DataTable dt, string coluna1, string coluna2, string colunaApresentacao1, string colunaApresentacao2, string legenda1, string legenda2, string titulo, int fonte, int mostrarValores)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();
        int i = 0;

        try
        {
            xmlAux.Append("<categories>");

            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append("<category label=\"" + dt.Rows[i]["Descricao"] + "\"/>");

            }

            xmlAux.Append("</categories>");

            if (coluna1 != "")
            {
                //gera as colunas de projetos satisfatórios para cada entidade
                xmlAux.Append("<dataset seriesName=\"" + legenda1 + "\" color=\"" + corReal + "\">");

                //percorre todo o DataTable
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    xmlAux.Append(string.Format(@"<set label=""{1}"" toolText=""{1}: {2:n2}""  value=""{0}""/>", dt.Rows[i][coluna1].ToString(), legenda1, dt.Rows[i][colunaApresentacao1]));
                }

                xmlAux.Append("</dataset>");
            }

            if (coluna2 != "")
            {
                //gera as colunas de projetos em atenção para cada entidade
                xmlAux.Append("<dataset seriesName=\"" + legenda2 + "\" color=\"" + corPrevisto + "\">");

                //percorre todo o DataTable
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    xmlAux.Append(string.Format(@"<set label=""{1}"" toolText=""{1}: {2:n2}"" value=""{0}""/>", dt.Rows[i][coluna2].ToString(), legenda2, dt.Rows[i][colunaApresentacao2]));
                }

                xmlAux.Append("</dataset>");
            }
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
        xml.Append("<chart use3DLighting=\"0\" showShadow=\"0\" caption=\"" + titulo + "\" palette=\"1\" inDecimalSeparator=\",\" chartLeftMargin=\"4\"" +
            " legendPadding=\"0\" canvasBottomMargin=\"0\" showBorder=\"0\" yAxisMaxValue=\"100\" numberSuffix=\"%\" baseFontSize=\"" + fonte + "\" " + //numVisiblePlot=\"10\"" +
            " chartRightMargin=\"20\" showLegend=\"1\" BgColor=\"F7F7F7\" showLimits=\"1\" labelDisplay=\"WRAP\"" +
            " canvasBgColor=\"F7F7F7\" chartTopMargin=\"5\" canvasPadding=\"0\" canvasBorderThickness=\"1\" canvasBorderColor=\"A1A1A1\"" +
            " decimalSeparator=\".\" " + usarGradiente + usarBordasArredondadas + exportar + " shownames=\"1\" adjustDiv=\"1\" showvalues=\"" + mostrarValores + "\" decimals=\"2\" chartBottomMargin=\"2\">");

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

    public string getGraficoAcompanhamentoEscopo(DataTable dt, string titulo, int fonte, string codigoArea, bool mostrarLink)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        try
        {
            string link = "";

            if (mostrarLink)
                link = string.Format(@"link=""JavaScript: isJavaScriptCall=true; abreTabela({0}, 'S', 'Contratados');"" ", codigoArea);

            xmlAux.Append(string.Format(@"<set label=""Contratado"" value=""{0}"" {1}/>", dt.Rows[0]["Contratados"].ToString(), link));

            if (mostrarLink)
                link = string.Format(@"link=""JavaScript: isJavaScriptCall=true; abreTabela({0}, 'N', '{1}');"" ", codigoArea, Server.UrlEncode("Não Contratados"));

            xmlAux.Append(string.Format(@"<set label=""Não Contratado"" value=""{0}"" {1}/>", dt.Rows[0]["NaoContratados"].ToString(), link));

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
        xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\" showPercentValues=\"1\"" +
            " chartTopMargin=\"1\" showZeroPies=\"0\" chartRightMargin=\"1\" inThousandSeparator=\".\" chartBottomMargin=\"1\" chartLeftMargin=\"1\"" +
            " adjustDiv=\"1\"  showLegend=\"1\" minimiseWrappingInLegend=\"1\" labelDistance=\"1\" enablesmartlabels=\"0\" legendNumColumns=\"3\" inDecimalSeparator=\",\" decimalSeparator=\",\"" +
            usarGradiente + usarBordasArredondadas + exportar + " showShadow=\"1\" showLabels=\"0\" showValues=\"1\" baseFontSize=\"" + fonte + "\">");
        xml.Append(xmlAux.ToString());
        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Verdana\" size=\"" + (fonte - 1) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte + 1) + "\" color=\"000000\" bold=\"0\"/>");
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

    public string getGraficoDesembolsoFinanceiro(DataTable dt, string titulo, int fonte, string codigoArea, bool mostrarLink)
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

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""2"" chartLeftMargin=""5"" chartRightMargin=""1"" 
                                    chartTopMargin=""5"" canvasBgColor=""F7F7F7"" canvasLeftMargin=""15"" canvasRightMargin=""20"" canvasBottomMargin=""2""
                                    showvalues=""0""  showBorder=""0"" BgColor=""F7F7F7"" labelPadding=""0"" decimals=""0""  numDivLines=""4""  SYAxisMaxValue=""100"" 
                                    inDecimalSeparator="","" minimiseWrappingInLegend=""1"" legendPadding=""0"" showLegend=""1"" legendNumColumns=""4"" inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" sNumberSuffix=""%"">", usarGradiente + usarBordasArredondadas + exportar
                                                                                                                                                          , titulo
                                                                                                                                                          , fonte));
        xml.Append(string.Format(@"<categories>"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<category label=""{0}"" />", dt.Rows[i]["Descricao"].ToString()));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));

        xml.Append(string.Format(@"<dataset seriesName=""Previsto"" color=""{0}"">", corPrevisto));

        int indexLinha = 0;

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string link = "";

                if (mostrarLink && indexLinha == 0)
                    link = string.Format(@"link=""JavaScript: isJavaScriptCall=true; abreTabela({0});"" ", codigoArea);

                string valor = dt.Rows[i]["ValorPrevisto"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n2}", double.Parse(valor));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Previsto: {1}"" color=""{2}"" {3}/> ", valor, displayValue, corPrevisto, link));

                indexLinha++;
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Realizado"" color=""{0}"">", corReal));

        indexLinha = 0;

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string link = "";

                //if (mostrarLink && indexLinha == 0)
                //    link = string.Format(@"link=""JavaScript: isJavaScriptCall=true; abreTabela({0});"" ", codigoArea);

                string valor = dt.Rows[i]["ValorReal"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n2}", double.Parse(valor));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Realizado: {1}"" color=""{2}"" {3}/> ", valor == "" ? "0" : valor, displayValue, corReal, link));
            }
        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Previsto Acum."" showValues= ""0""  color=""{0}"" parentYAxis=""S"">", corPrevisto));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["PercPrevisto"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n2}", double.Parse(valor));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Previsto Acumulado: {1}%"" color=""{2}""/> ", valor, displayValue, corPrevisto));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Realizado Acum."" showValues= ""0"" color=""{0}"" parentYAxis=""S"">", corReal));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["PercReal"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n2}", double.Parse(valor));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Realizado Acumulado: {1}%""  color=""{2}""/> ", valor, displayValue, corReal));
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

    public string getGraficoGanttPainelPresidencia(DataTable dt)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        #region Configurações do Gantt

        xml.Append(@"<anygantt>
                        <datagrid width=""610"">
                        <columns>                                            
                            <column width=""280"" cell_align=""LeftLevelPadding"">
                            <header>
                                <text>Descrição</text>
                            </header>
                            <format>{%Name}</format>
                            </column>
                            <column width=""90"" cell_align=""Center"">
                            <header>
                                <text>Término</text>
                            </header>
                            <format>{%PeriodEnd}{dateTimeFormat:%dd/%MM/%yyyy}</format>
                            </column>
                          <column width=""180"" cell_align=""Left"">
                            <header>
                                <text>Recursos</text>
                            </header>
                            <format>{%Recursos}</format>
                            </column>
				          <column width=""75"" cell_align=""Center"">
                            <header>
                                <text>Concluído</text>
                            </header>
                            <format>{%IndicaConcluido}</format>
                            </column>
                        </columns>
                        <style interlaced=""true""> 
                            <cell> 
                            <even>
                            <fill enabled=""true"" type=""Solid"" color=""#3D34B1"" >                                              
                            </fill>                                            
                            </even>
                            <odd>
                            <fill enabled=""true"" type=""Solid"" color=""White"">                                             
                            </fill>
                            </odd>
                            </cell>
                        </style>
                        </datagrid>
                        <settings> 
                          <background enabled=""false"" />                                    
                        <image_export url=""../../AnyChartPNGSaver.aspx"" mode=""FitContent"" height=""100%""/>
                        <context_menu save_as_image=""false"" version_info=""false"" print_chart=""false"" about_anygantt=""false"" >
                        <save_as_image_item_text>Salvar Gráfico como Imagem</save_as_image_item_text>
                        <print_chart_item_text>Imprimir Gráfico</print_chart_item_text>
                    </context_menu>
                            <print columns_on_every_page=""1"" mode=""SinglePage"">
                            </print>                                        
                        <locale>
                        <date_time_format week_starts_from_monday=""True"">
                            <months>
                            <names>Janeiro,Fevereiro,Março,Abril,Maio,Junho,Jullho,Agosto,Setembro,Outubro,Novembro,Dezembro</names>
                            <short_names>Jan,Fev,Mar,Abr,Mai,Jun,Jul,Ago,Set,Out,Nov,Dez</short_names>
                            </months>
                            <week_days>
                            <names>Domingo,Segunda,Terça,Quarta,Quinta,Sexta,Sábado</names>
                            <short_names>Dom,Seg,Ter,Qua,Qui,Sex,Sab</short_names>
                            </week_days>
                            <format>
                                <full>%dd/%MM/%yyyy %HH:%mm:%ss</full>
                                <date>%dd/%MM/%yyyy</date>
                                <time>%HH:%mm:%ss</time>
                            </format>
                        </date_time_format>
                        </locale>
                        <navigation enabled=""True"" position=""Top"" size=""30"">
                        <buttons collapse_expand_button=""true"" align=""Far"" /> 
                        <text>Gráfico de Gantt</text> 
                        <font face=""Verdana"" size=""10"" bold=""true"" color=""White"" /> 
                        <background>
                        <fill type=""Gradient"">
                        <gradient>
                        <key color=""#B0B0B0"" position=""0"" /> 
                        <key color=""#A0A0A0"" position=""0.3"" /> 
                        <key color=""#999999"" position=""0.5"" /> 
                        <key color=""#A0A0A0"" position=""0.7"" /> 
                        <key color=""#B0B0B0"" position=""1"" /> 
                        </gradient>
                        </fill>
                        <border type=""Solid"" color=""#494949"" /> 
                        </background>
                        </navigation>
                        <background enabled=""false"" /> 
                        </settings>
                        <timeline>
                        <plot line_height=""25"" item_height=""20"" >
                            <grid>
                            <horizontal>
                            <line enabled=""true"" color=""DarkSeaGreen"" thickness=""1"" /> 
                            <even>
                            <fill enabled=""true"" color=""DarkSeaGreen"" opacity=""0.2"" /> 
                            </even>
                            <odd>
                            <fill enabled=""true"" color=""White"" opacity=""1"" /> 
                            </odd>
                            </horizontal>
                            </grid>
                            </plot>
                        <scale>
                        <patterns>
                        <weeks>
                            <pattern>%dd/%MM/%yy</pattern>
                            <pattern>%dd/%MMM/%yy</pattern>
                            <pattern>%dd/%MMMM/%yyyy</pattern>
                        </weeks>
                        <days>
                            <pattern>%dd/%MM/%yy</pattern>
                            <pattern>%dd/%MMM/%yy</pattern>
                            <pattern>%dd/%MMMM/%yyyy</pattern>
                            <pattern is_lower=""true"">%dd</pattern>
                        </days>
                        <months>
                            <pattern is_lower=""true"">%MM</pattern>
                            <pattern is_lower=""true"">%MMM</pattern>
                            <pattern is_lower=""true"">%MMMM</pattern>
                            <pattern>%MMM/%yy</pattern>
                            <pattern>%MMMM/%yyyy</pattern>
                        </months>
                        <quarters>
                            <pattern>T %q/%yyyy</pattern>
                            <pattern>Trim %q/%yyyy</pattern>
                            <pattern>Trimestre %q/%yyyy</pattern>
                        </quarters>
                        <semesters>
                            <pattern is_lower=""true"">S %r</pattern>
                        </semesters>
                        </patterns>
                    </scale>
                    </timeline>");
        #endregion

        #region Estilos

        xml.Append(getEstilosPainelGerenciamento());

        #endregion

        xml.Append(@"<project_chart>
                      <tasks>");

        foreach (DataRow dr in dt.Rows)
        { //            
            string nomeStyle = "";

            if (dr["CorTarefa"] + "" != "")
            {
                if (dr["IndicaTarefaResumo"] + "" == "S")
                {
                    nomeStyle = "style=\"sumario" + dr["corTarefa"] + "\"";
                }
                else if (dr["IndicaMarco"] + "" == "True" || dr["IndicaMarco"] + "" == "1")
                {
                    nomeStyle = "style=\"marco" + dr["corTarefa"] + "\"";
                }
                else
                {
                    nomeStyle = "style=\"" + dr["CorTarefa"] + "\"";//sumarioVerde
                }
            }

            float concluido = dr["Concluido"] + "" == "" ? 0 : float.Parse(dr["Concluido"].ToString());

            xml.Append(Environment.NewLine);
            xml.Append(string.Format(@" <task id=""{0}"" name=""{1}"" {2} actual_start=""{3}"" {6}=""{4}"" progress=""{5:n0}%"" {7} >
                                        <attributes>
                                          <attribute name=""IndicaConcluido"">{8}</attribute> 
                                          <attribute name=""Recursos"">{9}</attribute> 
                                        </attributes>
                                        </task>"
                , dr["CodigoTarefa"].ToString()
                , dr["NomeTarefa"].ToString().Replace("\"", "'").Replace("<", "&lt;").Replace(">", "&gt;").Replace("–", "-")
                , "parent=\"" + ((dr["TarefaSuperior"].ToString() == dr["CodigoTarefa"].ToString() || dr["TarefaSuperior"].ToString() == "0") ? "" : dr["TarefaSuperior"].ToString()) + "\""
                , dr["Inicio"] + "" == "" ? "" : string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Parse(dr["Inicio"].ToString()))
                , dr["Termino"] + "" == "" ? "" : string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Parse(dr["Termino"].ToString()))
                , concluido
                , dr["IndicaMarco"] + "" == "True" || dr["IndicaMarco"] + "" == "1" ? "actual_end1" : "actual_end"
                , nomeStyle
                , concluido < 100 ? "Não" : "Sim "
                , dr["Recursos"]));
        }

        xml.Append(@"</tasks>");
        xml.Append(@"</project_chart>
                      </anygantt>");

        return xml.ToString();
    }

    private string getEstilosPainelGerenciamento()
    {
        return string.Format(@"<styles>
                           <defaults>
                              <task>
                                <task_style>
                                    <row_datagrid>
                                        <cell>
                                          <font face=""Verdana"" size=""11"" />    
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                                        </cell>
                                        <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                                      </row_datagrid>
                                    <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                             <progress>
					          <bar_style>

					            <middle shape=""HalfCenter"" >
                                  <fill enabled=""true"" type=""Gradient"" color=""#CCDBFF"" />					              
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>		
                            <actual>
					          <bar_style>
    <labels>
        <label anchor=""Right"" valign=""Center"" halign=""Far"" vpadding=""2""> 
        <text><![CDATA[]]></text>
    </label> 
    </labels> 
</bar_style>
</actual>
                                <baseline>
<bar_style>
    <labels>
        <label anchor=""Right"" valign=""Center"" halign=""Far"" vpadding=""2""> 
        <text><![CDATA[]]></text> 
    </label> 
    </labels> 
	<middle shape=""HalfCenter""> 
	<fill enabled=""true"" type=""Gradient"">
	<gradient>
	<key color=""#E1E1E1"" />
	<key color=""#A1A1A1"" />
	</gradient>
	</fill>
	</middle>
	<labels />
	</bar_style>
	</baseline>
</task_style>
                              </task>

                              <milestone>                                
                                <task_style>
                                <row_datagrid>
                            <cell>
                              <font face=""Verdana"" size=""11"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                                                       
                            </cell>
                            <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                                <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                                    </task_style>
                              </milestone>
                              <summary>
                                <task_style>
                                  <row_datagrid>
                                    <cell>
                                      <font face=""Verdana"" size=""11"" />    
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                                                        
                                    </cell>
                                    <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                                  </row_datagrid>
                                  <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                                  <progress>
                                    <bar_style>
                                      <middle shape=""HalfTop"">
                                        <fill enabled=""true"" type=""Gradient"" color=""#CCDBFF"" opacity=""1"" />
                                      </middle>
                                    </bar_style>
                                  </progress>
                                </task_style>
                              </summary>
                            </defaults>
				          <task_styles>
<task_style name=""marcoVerde"">
                         <actual>
                                 <bar_style>
    <labels>
        <label anchor=""Right"" valign=""Center"" halign=""Far"" vpadding=""2""> 
        <text><![CDATA[ ]]></text> 
    </label> 
    </labels> 
                                  <middle shape=""Rhomb"" /> 
                                 <start>
                                 <marker type=""Rhomb"">
                                  <fill type=""Solid"" color=""#B9992B"" /> 
                                  <border color=""#454545"" /> 
                                  </marker>
                                  </start>
                                 <end>
                                 <marker type=""Rhomb"">
                                  <fill type=""Solid"" color=""#B9992B"" /> 
                                  <border color=""#454545"" /> 
                                  </marker>
                                  </end>
                                  </bar_style>
                                  </actual>
                          <row_datagrid>
                          <cell> 
	                                    <font face=""Verdana"" size=""11"" />  
	                                </cell>
<tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                        </task_style>
<task_style name=""marcoAmarelo"">
                         <actual>
                                 <bar_style>
    <labels>
        <label anchor=""Right"" valign=""Center"" halign=""Far"" vpadding=""2""> 
        <text><![CDATA[ ]]></text> 
    </label> 
    </labels> 
                                  <middle shape=""Rhomb"" /> 
                                 <start>
                                 <marker type=""Rhomb"">
                                  <fill type=""Solid"" color=""#B9992B"" /> 
                                  <border color=""#454545"" /> 
                                  </marker>
                                  </start>
                                 <end>
                                 <marker type=""Rhomb"">
                                  <fill type=""Solid"" color=""#B9992B"" /> 
                                  <border color=""#454545"" /> 
                                  </marker>
                                  </end>
                                  </bar_style>
                                  </actual>
                          <row_datagrid>
                             <cell> 
	                                    <font face=""Verdana"" size=""11"" bold=""true"" color=""gold"" />  
	                                </cell> 
                            

<tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                        </task_style>
<task_style name=""marcoVermelho"">
                         <actual>
                                 <bar_style>
    <labels>
        <label anchor=""Right"" valign=""Center"" halign=""Far"" vpadding=""2""> 
        <text><![CDATA[ ]]></text> 
    </label> 
    </labels> 
                                  <middle shape=""Rhomb"" /> 
                                 <start>
                                 <marker type=""Rhomb"">
                                  <fill type=""Solid"" color=""#B9992B"" /> 
                                  <border color=""#454545"" /> 
                                  </marker>
                                  </start>
                                 <end>
                                 <marker type=""Rhomb"">
                                  <fill type=""Solid"" color=""#B9992B"" /> 
                                  <border color=""#454545"" /> 
                                  </marker>
                                  </end>
                                  </bar_style>
                                  </actual>
                          <row_datagrid>
                             <cell> 
	                                    <font face=""Verdana"" size=""11"" bold=""true"" color=""red"" />  
	                                </cell> 
                            

<tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                        </task_style>                        
                        <task_style name=""sumarioVerde"">
                          <row_datagrid>
                            <cell> 
	                                    <font face=""Verdana"" size=""11"" />  
	                                </cell>
                            <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                          <progress>
					          <bar_style>
					            <middle shape=""HalfTop"" >
                                  <fill enabled=""true"" type=""Gradient"" color=""#CCDBFF"" />					              
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>		
                            <actual>		
                            <bar_style>
    <labels>
        <label anchor=""Right"" valign=""Center"" halign=""Far"" vpadding=""2""> 
        <text><![CDATA[ ]]></text> 
    </label> 
    </labels> 
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#4A4A4A"" />
						            </middle>
                                  <start>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </start>
                                  <end>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </end>
                                </bar_style>
                             </actual>
                        </task_style>
<task_style name=""sumarioAmarelo"">
                          <row_datagrid>
                            <cell> 
	                                    <font face=""Verdana"" size=""11"" bold=""true"" color=""gold""/>  
	                                </cell>
                            <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                          <progress>
					          <bar_style>
					            <middle shape=""HalfTop"" >
                                  <fill enabled=""true"" type=""Gradient"" color=""#CCDBFF"" />					              
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>		
                            <actual>		
                            <bar_style>
    <labels>
        <label anchor=""Right"" valign=""Center"" halign=""Far"" vpadding=""2""> 
        <text><![CDATA[ ]]></text> 
    </label> 
    </labels> 
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#4A4A4A"" />
						            </middle>
                                  <start>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </start>
                                  <end>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </end>
                                </bar_style>
                             </actual>
                        </task_style>
<task_style name=""sumarioVermelho"">
                          <row_datagrid>
                            <cell> 
	                                    <font face=""Verdana"" size=""11"" bold=""true"" color=""red""/>  
	                                </cell>
                            <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                          <progress>
					          <bar_style>
					            <middle shape=""HalfTop"" >
                                  <fill enabled=""true"" type=""Gradient"" color=""#CCDBFF"" />					              
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>		
                            <actual>		
                            <bar_style>
    <labels>
        <label anchor=""Right"" valign=""Center"" halign=""Far"" vpadding=""2""> 
        <text><![CDATA[ ]]></text> 
    </label> 
    </labels> 
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#4A4A4A"" />
						            </middle>
                                  <start>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </start>
                                  <end>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </end>
                                </bar_style>
                             </actual>
                        </task_style>
                        <task_style name=""Verde"">
                          <row_datagrid>
                                    <cell> 
	                                    <font face=""Verdana"" size=""11"" />  
	                                </cell> 
                            <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                          <progress>
					          <bar_style>
					            <middle shape=""HalfCenter"" >
                                  <fill enabled=""true"" type=""Gradient"" color=""#CCDBFF"" />					              
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>                           
                        </task_style>
<task_style name=""Amarelo"">
                          <row_datagrid>
                                    <cell> 
	                                    <font face=""Verdana"" size=""11"" bold=""true"" color=""gold"" />  
	                                </cell> 
                            <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                          <progress>
					          <bar_style>
					            <middle shape=""HalfCenter"" >
                                  <fill enabled=""true"" type=""Gradient"" color=""#CCDBFF"" />					              
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>                           
                        </task_style>
<task_style name=""Vermelho"">
                          <row_datagrid>
                                    <cell> 
	                                    <font face=""Verdana"" size=""11"" bold=""true"" color=""red"" />  
	                                </cell> 
                            <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Término: {1}
Concluído: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                          <progress>
					          <bar_style>
					            <middle shape=""HalfCenter"" >
                                  <fill enabled=""true"" type=""Gradient"" color=""#CCDBFF"" />					              
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>                           
                        </task_style>
                        </task_styles>
				        </styles>", "{%Name}", "{%ActualEnd}{dateTimeFormat:%dd/%MM/%yyyy}", "{%IndicaConcluido}");
    }

    #endregion

    #region Métodos Ágeis

    public string getGraficoMetodosAgeis(DataTable dt)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        int i = 0;

        string exportar = "";
        int fonte = 11;

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""2"" chartLeftMargin=""5"" chartRightMargin=""10"" chartTopMargin=""5"" canvasBgColor=""F7F7F7"" canvasLeftMargin=""15"" canvasRightMargin=""20"" canvasBottomMargin=""2""
                                    showvalues=""0""  showBorder=""0"" BgColor=""F7F7F7"" slantLabels=""1"" labelDisplay=""ROTATE"" numVisiblePlot=""30"" 
                                    inDecimalSeparator="",""  showLegend=""1""  inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" >", usarGradiente + usarBordasArredondadas + exportar
                                                                                                                                                          , ""
                                                                                                                                                          , fonte));
        xml.Append(string.Format(@"<categories>"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<category label=""{0:dd/MM/yyyy}"" />", dt.Rows[i]["Data"]));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));

        xml.Append(string.Format(@"<dataset seriesName=""Previsto"" showValues=""0"">"));


        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorPrevisto"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n0}", double.Parse(valor));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Previsto:{1}"" /> ", valor == "" ? "0" : valor, displayValue));

            }
        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""A Realizar"" showValues=""0"" >"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorReal"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n0}", double.Parse(valor));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""A Realizar:{1}"" dashed=""0""/> ", valor, displayValue));

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

    #endregion

    #region TAI01
    public bool verificaDuplicidadeResultadoEsperado(string codigoIndicador, int codigoProjeto, string Meta)
    {
        string comandoSQL = string.Format(@"            
               select * from dbo.tai02_ResultadosEsperados
                where CodigoProjeto = {2}
                  and DescricaoResultado = '{3}' 
                  and codigoIndicador = cast('{4}' AS Int)
            ", bancodb, Ownerdb, codigoProjeto, Meta, codigoIndicador);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            return true;

        return false;
    }
    #endregion

    #region Parecer Fluxo

    public DataSet getUsuariosParecerFluxo(int codigoWorflow, int codigoInstancia, int codigoEtapa, int sequencia, string where)
    {
        string comandoSQL = string.Format(
                    @"SELECT CodigoTramitacaoEtapaFluxo, CodigoWorkflow, CodigoInstancia, CodigoEtapa
			                ,SequenciaEtapa, CodigoUsuarioParecer, up.NomeUsuario AS UsuarioParecer
			                ,DataSolicitacaoParecer, ComentarioSolicitacaoParecer, DataPrevistaParecer, DataParecer, Parecer
			                ,CASE WHEN tef.DataParecer IS NULL AND (tef.CodigoEtapa <> {4} OR tef.SequenciaEtapa <> {5}) THEN 'Expirado'                                  
						          WHEN tef.DataSolicitacaoParecer IS NULL THEN 'Aguardando Envio'
						          WHEN tef.DataParecer IS NULL AND tef.DataPrevistaParecer + 1 < GETDATE() THEN 'Atrasado'
						          WHEN tef.DataParecer IS NULL AND tef.DataPrevistaParecer + 1 >= GETDATE() THEN 'Aguardando Tramitação'
						          WHEN tef.DataExclusao IS NOT NULL THEN 'Excluído'
						          WHEN tef.DataParecer IS NOT NULL THEN 'Concluído'
						          ELSE '' END AS StatusParecer
                  FROM {0}.{1}.TramitacaoEtapaFluxo tef INNER JOIN
			           {0}.{1}.Usuario up ON up.CodigoUsuario = tef.CodigoUsuarioParecer
                 WHERE tef.CodigoWorkflow = {2}
                   AND tef.CodigoInstancia = {3}
                   AND tef.DataExclusao IS NULL
                   {6}
                 ORDER BY up.NomeUsuario
               ", bancodb, Ownerdb, codigoWorflow, codigoInstancia, codigoEtapa, sequencia, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiUsuarioTramitacao(int codigoWF, int codigoInstancia, int codigoEtapa, int sequencia, int codigoUsuarioParecer, int codigoUsuarioLogado, string dataPrevista, string comentarios, ref int codigoTramitacaoEtapaFluxo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN
                        DECLARE @CodigoTramitacaoEtapaFluxo int

                        INSERT INTO {0}.{1}.TramitacaoEtapaFluxo
                                   ([CodigoWorkflow]
                                   ,[CodigoInstancia]
                                   ,[CodigoEtapa]
                                   ,[SequenciaEtapa]
                                   ,[CodigoUsuarioParecer]
                                   ,[CodigoUsuarioInclusao]
                                   ,[DataInclusao]
                                   ,[ComentarioSolicitacaoParecer]
                                   ,[DataPrevistaParecer])
                             VALUES
                                   ({2}
                                   ,{3}
                                   ,{4}
                                   ,{5}
                                   ,{6}
                                   ,{7}
                                   ,GetDate()
                                   ,'{8}'
                                   ,{9})
                            
                            SELECT @CodigoTramitacaoEtapaFluxo = SCOPE_IDENTITY()

                            SELECT @CodigoTramitacaoEtapaFluxo AS CodigoTramitacaoEtapaFluxo
                    END
                        ", bancodb, Ownerdb
                         , codigoWF, codigoInstancia, codigoEtapa, sequencia, codigoUsuarioParecer, codigoUsuarioLogado
                         , comentarios.Replace("'", "''"), dataPrevista);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                codigoTramitacaoEtapaFluxo = int.Parse(ds.Tables[0].Rows[0]["CodigoTramitacaoEtapaFluxo"].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaUsuarioTramitacao(int codigoTramitacao, int codigoUsuarioLogado, string dataPrevista, string comentarios)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      UPDATE {0}.{1}.TramitacaoEtapaFluxo SET ComentarioSolicitacaoParecer = '{3}'
                                                             ,DataPrevistaParecer = {4}
                                                             ,CodigoUsuarioUltimaAlteracao = {5}
                                                             ,DataUltimaAlteracao = GetDate()
                             WHERE CodigoTramitacaoEtapaFluxo = {2}
                        ", bancodb, Ownerdb
                         , codigoTramitacao
                         , comentarios.Replace("'", "''")
                         , dataPrevista
                         , codigoUsuarioLogado);

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

    public bool atualizaParecerUsuario(int codigoUsuario, int codigoWorkflow, int codigoInstancia, string comentarios)
    {
        bool retorno = false;

        try
        {
            comandoSQL = string.Format(@"
                      UPDATE {0}.{1}.TramitacaoEtapaFluxo SET Parecer = '{3}'
                             WHERE CodigoUsuarioParecer = {2}
                               AND DataParecer IS NULL
                               AND DataExclusao IS NULL
                               AND CodigoWorkflow = {4}
                               AND CodigoInstancia =  {5}
                        ", bancodb, Ownerdb
                         , codigoUsuario
                         , comentarios.Replace("'", "''")
                         , codigoWorkflow
                         , codigoInstancia);

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

    public bool enviaParecerUsuario(int codigoUsuario, int[] usuarios)
    {
        bool retorno = false;

        foreach (int codigo in usuarios)
        {
            comandoSQL += string.Format(@"
                    UPDATE {0}.{1}.TramitacaoEtapaFluxo SET DataSolicitacaoParecer = GetDate()
                                                           ,CodigoUsuarioUltimaAlteracao = {3}
                                                           ,DataUltimaAlteracao = GetDate()
                    WHERE CodigoTramitacaoEtapaFluxo = {2}
                ", bancodb, Ownerdb, codigo, codigoUsuario);
        }

        try
        {


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

    public bool excluiUsuarioTramitacao(int codigoTramitacao, int codigoUsuarioLogado)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      UPDATE {0}.{1}.TramitacaoEtapaFluxo SET CodigoUsuarioExclusao = {3}
                                                             ,DataExclusao = GetDate()
                             WHERE CodigoTramitacaoEtapaFluxo = {2}
                        ", bancodb, Ownerdb
                         , codigoTramitacao
                         , codigoUsuarioLogado);

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

    public bool getAcessoInteracaoFluxo(int codigoEntidade, int codigoWf, int codigoInstancia, int codigoEtapa, int codigoSequencia, int codigoUsuarioLogado)
    {
        bool retorno = false;

        string comandoSQL = string.Format(
                    @"BEGIN
	DECLARE 
				@in_codigoWorkFlow								int
			, @in_codigoInstanciaWf							bigint
			, @in_SequenciaOcorrenciaEtapaWf		int
			, @in_codigoEtapaWf									int
			, @in_identificadorUsuario					varchar(50)
			, @ou_nivelAcesso										int							
			, @ou_codigoRetorno									int							
			, @ou_mensagemErro									nvarchar(2048)	
			
			, @l_podeInteragirTelaTramitacao		bit
			, @l_nRet														int


	SET	@in_codigoWorkFlow				= {3}
	SET @in_codigoInstanciaWf			= {4}
	SET @in_SequenciaOcorrenciaEtapaWf = {6}
	SET @in_codigoEtapaWf					= {5}
	SET @in_identificadorUsuario	= '{7}'

	EXEC @l_nRet = dbo.p_wf_verificaAcessoEtapaWf 
				@in_codigoWorkFlow								= @in_codigoWorkFlow
			, @in_codigoInstanciaWf							= @in_codigoInstanciaWf
			, @in_SequenciaOcorrenciaEtapaWf		= @in_SequenciaOcorrenciaEtapaWf
			, @in_codigoEtapaWf									= @in_codigoEtapaWf
			, @in_identificadorUsuario					= @in_identificadorUsuario
			, @ou_nivelAcesso										= @ou_nivelAcesso										OUTPUT
			, @ou_codigoRetorno									= @ou_codigoRetorno									OUTPUT
			, @ou_mensagemErro									= @ou_mensagemErro									OUTPUT

		-- se não tiver acesso de ação na etapa ou se o acesso é em virtude tramitação de parecer, 
		-- não pode 'interagir' na tela de tramitação, apenas dará o seu parecer se for o caso.
		IF ( ((@ou_nivelAcesso & 2)=0) OR ( (@ou_codigoRetorno&16)>0 ) ) BEGIN
			SET @l_podeInteragirTelaTramitacao = 0
		END 
		ELSE BEGIN
			SET @l_podeInteragirTelaTramitacao = 1
		END
		-----------------------------------------------------------------------------------------

	SELECT @l_podeInteragirTelaTramitacao AS Acesso
END -- BEGIN inicial
               ", bancodb, Ownerdb, codigoEntidade, codigoWf, codigoInstancia, codigoEtapa, codigoSequencia, codigoUsuarioLogado);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            retorno = (bool)ds.Tables[0].Rows[0]["Acesso"];
        }

        return retorno;
    }

    #endregion

    #region Substituição de Recursos

    public DataSet getObjetosResponsabilidadeUsuario(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
                    @"SELECT * 
                        FROM {0}.{1}.f_GetObjetosResponsabilidadeUsuario({2}, {4})
                       WHERE 1 = 1
                         {3}
               ", bancodb, Ownerdb, codigoUsuario, where, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    #endregion


}
}