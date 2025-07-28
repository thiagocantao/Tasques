using DevExpress.Web;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

/// <summary>
/// Summary description for myDados
/// </summary>


namespace BriskPtf
{
public partial class dados
{
    #region _Projetos - DadosProjeto - Aquisições de Projeto
    public DataSet getAquisicoesProjeto(int codigoProjeto, string where)
    {
        comandoSQL = string.Format(@"
            SELECT ap.CodigoAquisicao
                  ,ap.Item
                  ,ap.DataPrevista
                  ,ap.ValorPrevisto
                  ,ap.DataReal
                  ,ap.PercentualContratado
                  ,ap.CodigoResponsavel
                  ,ap.DataAberturaRC
                  ,ap.ModalidadeRC
                  ,ap.NumeroRC
                  ,CASE WHEN ap.Status = 1 THEN 'Sim' WHEN ap.Status = 2 THEN 'Não' ELSE 'Parcial' END AS Status
                  ,u.NomeUsuario
                  ,ap.GrupoAquisicao
                  ,ap.CodigoConta
                  ,pcfc.DescricaoConta
              FROM {0}.{1}.AquisicaoProjeto ap
        INNER JOIN {0}.{1}.Usuario u ON u.CodigoUsuario = ap.CodigoResponsavel
         LEFT JOIN {0}.{1}.PlanoContasFluxoCaixa pcfc on (pcfc.CodigoConta = ap.CodigoConta)
             WHERE ap.DataExclusao IS NULL
               AND ap.CodigoProjeto = {2}
               {3}
            ORDER BY ap.Item", bancodb, Ownerdb, codigoProjeto, where);

        return getDataSet(comandoSQL);
    }

    public bool incluiAquisicaoProjeto(int codigoProjeto, string itemAquisicao, string dataPrevista, string valorPrevisto, string percentualContratado, int codigoResponsavel, int codigoUsuarioLogado, string tipoDeItem, int status, int codigoConta, ref int novoCodigo, ref string mensagemErro)
    {
        decimal percentualContratado1 = 0;
        if (percentualContratado != "NULL")
        {
            percentualContratado1 = decimal.Parse(percentualContratado) / 100;
        }
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @CodigoAquisicao int

                            INSERT INTO {0}.{1}.AquisicaoProjeto
                                   (CodigoProjeto,Item,DataPrevista
                                   ,ValorPrevisto,PercentualContratado,CodigoResponsavel
                                   ,DataInclusao,CodigoUsuarioInclusao,GrupoAquisicao, Status
                                   ,CodigoConta)
                             VALUES({2},'{3}',{4}
                                   ,{5},  {6},{7}
                                   ,getDate(),{8},'{9}',{10}
                                   ,{11})

                            SELECT @CodigoAquisicao = SCOPE_IDENTITY()

                            SELECT @CodigoAquisicao AS CodigoAquisicao

                          END
                        ", /*{0}*/bancodb
                         , /*{1}*/Ownerdb
                         , /*{2}*/codigoProjeto
                         , /*{3}*/itemAquisicao.Replace("'", "''")
                         , /*{4}*/dataPrevista
                         , /*{5}*/valorPrevisto.Replace(".", "").Replace(",", ".")
                         , /*{6}*/percentualContratado1.ToString().Replace(".", "").Replace(",", ".")
                         , /*{7}*/codigoResponsavel
                         , /*{8}*/codigoUsuarioLogado
                         , /*{9}*/tipoDeItem
                         , /*{10}*/status
                         , /*{11}*/(codigoConta == -1) ? "NULL" : codigoConta.ToString());

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["CodigoAquisicao"].ToString());

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;
    }

    public bool atualizaAquisicaoProjeto(int codigoAquisicao, string itemAquisicao, string dataPrevista, string valorPrevisto, string percentualContratado, string tipoDeItem, int codigoResponsavel, int codigoUsuarioLogado, int status, int codigoConta, ref string mensagemErro)
    {
        decimal percentualContratado1 = 0;
        if (percentualContratado != "NULL")
        {
            percentualContratado1 = decimal.Parse(percentualContratado) / 100;
        }
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                            BEGIN
                            UPDATE {0}.{1}.AquisicaoProjeto
                               SET Item = '{3}'
                                  ,DataPrevista = {4}
                                  ,ValorPrevisto = {5}
                                  ,PercentualContratado = {6}
                                  ,CodigoResponsavel = {7}
                                  ,DataUltimaAlteracao = GetDate()
                                  ,CodigoUsuarioUltimaAlteracao = {8}
                                  ,GrupoAquisicao = '{9}'
                                  ,Status = {10}
                                  ,CodigoConta = {11}
                             WHERE CodigoAquisicao = {2}
                            END
                        ",/*{0}*/  bancodb, /*{1}*/ Ownerdb
                         ,/*{2}*/  codigoAquisicao
                         ,/*'{3}'*/ itemAquisicao.Replace("'", "''")
                         ,/*'{4}'*/  dataPrevista
                         ,/*'{5}'*/  valorPrevisto.Replace(".", "").Replace(",", ".")
                         ,/*'{6}'*/  percentualContratado1.ToString().Replace(".", "").Replace(",", ".")
                         ,/*'{7}'*/  (codigoResponsavel == -1) ? "null" : codigoResponsavel.ToString()
                         ,/*'{8}'*/  codigoUsuarioLogado
                         ,/*'{9}'*/ tipoDeItem
                         ,/*'{10}'*/status
                         ,/*'{11}'*/(codigoConta == -1) ? "NULL" : codigoConta.ToString());

            int regAf = 0;

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;
    }

    public bool excluiAquisicaoProjeto(int codigoAquisicao, int codigoUsuario)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                       UPDATE {0}.{1}.[AquisicaoProjeto]
                               SET DataExclusao = GetDate()
                                  ,CodigoUsuarioExclusao = {3}
                        WHERE CodigoAquisicao = {2}
                        ", bancodb, Ownerdb
                         , codigoAquisicao
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
    #endregion

    #region _Projetos - getOLAP CONTRATOS E DEMANDAS

    public DataSet getOLAPContratos(int codigoEntidade, int codigoUsuario, int CodigoCarteira, string tipoPessoa)
    {
        string comandoSQL = string.Format(@" SELECT * FROM {0}.{1}.f_GetDadosOLAP_Contratos({3},{2}) where TipoPessoa = '{5}'", bancodb, Ownerdb, codigoEntidade, codigoUsuario, CodigoCarteira, tipoPessoa);
        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getOLAPContratos2(int codigoEntidade, int codigoUsuario)
    {

        string comandoSQL = string.Format(@"SELECT * FROM {0}.{1}.f_GetDadosOLAP_Contratos2({3},{2}) x WHERE x.TipoPessoa = 'F' ", bancodb, Ownerdb, codigoEntidade, codigoUsuario);
        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getOLAPProjetoseObras(int codigoEntidade, int codigoUsuario, int codigoCarteira)
    {
        string comandoSQL = string.Format(@" SELECT * FROM {0}.{1}.f_GetDadosOLAP_ProjetoseObras({3},{2}) x WHERE x.TipoPessoa = 'F' 
                                           AND x.[CodigoProjeto] IN (SELECT [CodigoProjeto] FROM {0}.{1}.f_GetProjetosUsuario( {3}, {2}, {4}))", bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoCarteira);
        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getOLAPTarefasAtribuicoes(int codigoEntidade, int codigoUsuario, int codigoCarteira, string dataInicial, string dataFinal)
    {

        string whereSelect1 = string.Format(" AND t.Termino >= CONVERT(SmallDateTime, '{0}', 103) AND t.Termino <= CONVERT(SmallDateTime, '{1}', 103)", dataInicial
           , dataFinal);
        //select utilizado para trazer somente as tarefas de ToDoList que foram CONCLUÍDAS
        string whereSelect2 = string.Format(" AND f.TerminoPrevisto >= CONVERT(SmallDateTime, '{0}', 103) AND f.TerminoPrevisto <= CONVERT(SmallDateTime, '{1}', 103)", dataInicial
           , dataFinal);


        string comandoSQL = string.Format(@"
        
        SELECT un.NomeUnidadeNegocio,
			   tp.TipoProjeto,
               s.DescricaoStatus             AS NomeStatus,
               IsNull(prg.NomeProjeto,'S/I') AS NomePrograma,
               p.NomeProjeto,
               IsNull(U.NomeUsuario,'S/I')   AS NomeGerente,
               IsNull(tsup.NomeTarefa,'S/I') AS NomeTarefaSuperior,
               cast(t.SequenciaTarefaCronograma as varchar(4))+' - '+ t.NomeTarefa NomeTarefa,       
               CAST( CAST(p.CodigoProjeto AS Bigint) * 2147483649 + CAST(t.CodigoTarefa AS Bigint) AS Bigint ) AS IDTarefa,
               t.Inicio,
               t.InicioReal,
               t.InicioLB,
               t.Termino,
               t.TerminoReal,
               t.TerminoLB,
               t.Anotacoes,
               IsNull(t.PercentualFisicoConcluido,0) AS PercentualRealTarefa,
               CASE WHEN t.IndicaMarco = 'S' THEN 'Sim' ELSE 'Não' END AS IndicaMarco,
               CASE WHEN t.IndicaTarefaCritica = 'S' THEN 'Sim' ELSE 'Não' END AS IndicaTarefaCritica,
               rcp.NomeRecurso,
               a.Trabalho AS Trabalho,
               a.TrabalhoReal AS TrabalhoReal,
               a.TrabalhoLB AS TrabalhoLB,
               a.Trabalho - a.TrabalhoLB AS VariacaoTrabalho,
               a.Trabalho - a.TrabalhoReal AS TrabalhoRestante,
               CASE WHEN t.IndicaTarefaAtrasadaLB = 'S' THEN 'Sim' ELSE 'Não' END AS IndicaAtraso,
               t.PercentualFisicoPrevistoLB AS PercentualPrevistoTarefa,
               dbo.f_GetStringTipoTarefa(t.CodigoCronogramaProjeto, t.CodigoTarefa) AS TipoTarefa
          FROM {0}.{1}.TarefaCronogramaProjeto AS t LEFT JOIN
               {0}.{1}.AtribuicaoRecursoTarefa AS a ON (t.CodigoCronogramaProjeto = a.CodigoCronogramaProjeto
                                                           AND t.CodigoTarefa = a.CodigoTarefa)  LEFT JOIN
               {0}.{1}.RecursoCronogramaProjeto AS rcp ON (rcp.CodigoCronogramaProjeto = a.CodigoCronogramaProjeto
                                                                           AND rcp.CodigoRecursoProjeto = a.CodigoRecursoProjeto
                                                                           AND rcp.CodigoTipoRecurso = 1) INNER JOIN                                            
               {0}.{1}.CronogramaProjeto AS cp ON (cp.CodigoCronogramaProjeto = t.CodigoCronogramaProjeto) INNER JOIN                                             
               {0}.{1}.Projeto AS p ON (p.CodigoProjeto = cp.CodigoProjeto) INNER JOIN
               {0}.{1}.f_GetProjetosUsuario({5}, {2}, {6}) fpr ON fpr.CodigoProjeto = p.CodigoProjeto INNER JOIN
               {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                       AND s.CodigoStatus NOT IN (4,5)) INNER JOIN --> Não traz cancelados e suspensos!
               {0}.{1}.TipoProjeto AS tp ON (tp.CodigoTipoProjeto = p.CodigoTipoProjeto) INNER JOIN           
               {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio) LEFT JOIN
               {0}.{1}.Usuario AS u ON (u.CodigoUsuario = p.CodigoGerenteProjeto) LEFT JOIN
               {0}.{1}.TarefaCronogramaProjeto AS tSup ON (tSup.CodigoCronogramaProjeto = t.CodigoCronogramaProjeto
                                                           AND tSup.CodigoTarefa = t.CodigoTarefaSuperior
                                                           AND tSup.SequenciaTarefaCronograma <> 0) LEFT JOIN
               {0}.{1}.LinkProjeto AS lp ON (lp.CodigoProjetoFilho = p.CodigoProjeto)  LEFT JOIN                
               {0}.{1}.Projeto AS prg ON (prg.CodigoProjeto = lp.CodigoProjetoPai) left join
			   {0}.{1}.TipoTarefaCronograma AS ttc ON (ttc.CodigoTipoTarefaCronograma = t.CodigoTipoTarefaCronograma)																					                    
         WHERE p.CodigoEntidade = {2} --> Parâmetro                    
           AND t.IndicaTarefaResumo = 'N'           
           AND t.DataExclusao IS NULL
           AND cp.DataExclusao IS NULL
           AND p.DataExclusao IS NULL
                 {3}
       UNION
          SELECT 'S/I',
			   'Tarefa Extra Cronograma',
               f.DescricaoStatusTarefa,
               'S/I',
               f.DescricaoOrigem,
               f.NomeUsuarioResponsavel,
               'S/I',
               f.DescricaoTarefa,       
               CAST( CAST(f.CodigoProjeto AS Bigint) * 2147483649 + CAST(f.CodigoTarefa AS Bigint) AS Bigint ) AS IDTarefa,
               f.InicioPrevisto,
               f.InicioReal,
               f.InicioPrevisto,
               f.TerminoPrevisto,
               f.TerminoReal,
               f.TerminoPrevisto,
               Convert(Varchar(4000),f.Anotacoes),
               f.PercentualConcluido,
               'Não',
               'Não',
               f.NomeUsuarioResponsavel,
               f.EsforcoPrevisto,
               f.EsforcoReal,
               f.EsforcoPrevisto,
               f.EsforcoPrevisto - f.EsforcoReal,
               f.EsforcoPrevisto - f.EsforcoReal,
               CASE WHEN  f.Estagio = 'Atrasada' THEN  'Sim' ELSE 'Não' END,               
               CASE WHEN  Convert(DateTime,f.TerminoPrevisto,102) <= Convert(DateTime,GETDATE()-1,102) THEN  100 ELSE 0 END,
               'S/I'
           FROM {0}.{1}.f_GetTarefasToDoListProjeto({2}, -1,-1) AS f    
          WHERE 1=1 
                {4}", bancodb, Ownerdb, codigoEntidade, whereSelect1, whereSelect2, codigoUsuario, codigoCarteira);
        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getOLAPDemandas(int codigoEntidade, string dataInicial, string dataFinal)
    {
        //dados forçados para configuração do relatorio de demandas
        string comandoSQL = string.Format(@"
BEGIN        
DECLARE @DataInicial    SmallDateTime
        DECLARE @DataFinal      SmallDateTime
        SET @DataInicial        = CONVERT(SmallDateTime, '{3}', 103)
        SET @DataFinal          = CONVERT(SmallDateTime, '{4}', 103)
        SELECT un.SiglaUnidadeNegocio AS Unidade,
               tp.TipoProjeto AS IndicaProjetoDemanda,
               s.DescricaoStatus AS Status,
               p.NomeProjeto AS NomeProjetoDemanda,
               u.NomeUsuario AS NomeRecurso,      
               tsp.Data        AS Dia,
               {0}.{1}.f_GetIdentificacaoSemana(tsp.Data) AS Semana,
               (CASE MONTH(tsp.Data)
                    WHEN  1 THEN 'Jan'  
                    WHEN  2 THEN 'Fev'  
                    WHEN  3 THEN 'Mar'  
                    WHEN  4 THEN 'Abr'  
                    WHEN  5 THEN 'Mai'  
                    WHEN  6 THEN 'Jun'  
                    WHEN  7 THEN 'Jul'  
                    WHEN  8 THEN 'Ago'  
                    WHEN  9 THEN 'Set'  
                    WHEN 10 THEN 'Out' 
                    WHEN 11 THEN 'Nov'
                    WHEN 12 THEN 'Dez' 
                    ELSE 'Nenhum' END)AS Mes,
                YEAR(tsp.Data) AS Ano,               
               IsNull(tsp.TrabalhoAprovado,0) AS Alocacao,
               ttt.DescricaoTipoTarefaTimeSheet AS TipoAtividade       
          FROM {0}.{1}.TimeSheetProjeto AS tsp        
    INNER JOIN {0}.{1}.Projeto AS p ON (tsp.CodigoProjeto = p.CodigoProjeto) 
    INNER JOIN {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio)  
    INNER JOIN {0}.{1}.TipoProjeto AS tp ON (tp.CodigoTipoProjeto = p.CodigoTipoProjeto) 
    INNER JOIN {0}.{1}.Usuario AS u ON (u.CodigoUsuario = tsp.CodigoUsuario) 
    INNER JOIN {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto) 
    INNER JOIN {0}.{1}.TipoTarefaTimeSheet AS ttt ON (ttt.CodigoTipoTarefaTimeSheet = tsp.CodigoTipoTarefaTimeSheet)
         WHERE p.DataExclusao IS NULL
           AND tsp.StatusTimesheet = 'AP'
           AND p.CodigoEntidade = {2}
           AND tsp.Data BETWEEN @DataInicial AND @DataFinal
   END", bancodb, Ownerdb, codigoEntidade, dataInicial, dataFinal);
        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    #endregion

    #region _Projetos - Fluxos

    public DataSet getFluxosAssociadosProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT fp.CodigoFluxo, fp.StatusRelacionamento, fp.TextoOpcaoFluxo
                      ,CASE WHEN fp.StatusRelacionamento = 'A' THEN '" + Resources.traducao.ativo + @"' ELSE '" + Resources.traducao.inativo + @"' END AS NomeStatusRelacionamento
		              ,CONVERT(VarChar(10), fp.DataAtivacao, 103) AS DataAtivacao
                      ,CONVERT(VarChar(10), fp.DataDesativacao, 103) AS DataDesativacao, fp.TipoOcorrenciaFluxo, f.NomeFluxo 
		              ,ua.NomeUsuario AS UsuarioAtivacao, ua.NomeUsuario AS UsuarioDesativacao
                  FROM {0}.{1}.FluxosProjeto fp INNER JOIN
			           {0}.{1}.Fluxos f ON f.CodigoFluxo = fp.CodigoFluxo LEFT JOIN		
			           {0}.{1}.Usuario ua ON ua.CodigoUsuario = fp.IdentificadorUsuarioAtivacao LEFT JOIN
			           {0}.{1}.Usuario ud ON ud.CodigoUsuario = fp.IdentificadorUsuarioDesativacao
                 WHERE fp.CodigoProjeto = {2}
                   {3}
                ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getFluxosDisponiveisAssociacaoProjeto(int codigoProjeto)
    {
        string comandoSQL = string.Format(@"
                BEGIN
		            DECLARE @CodigoTipoProjeto int,
						    @CodigoProjeto int,
		                    @CodigoEntidade int
		            SET @CodigoProjeto = {2}
		
		            SELECT @CodigoTipoProjeto = CodigoTipoProjeto, @CodigoEntidade = p.CodigoEntidade
			          FROM {0}.{1}.Projeto p 
		             WHERE p.CodigoProjeto = @CodigoProjeto
					
		            SELECT CodigoFluxo, NomeFluxo, Descricao
			          FROM {0}.{1}.Fluxos f
		             WHERE f.StatusFluxo = 'A'
                       AND f.CodigoEntidade = @CodigoEntidade
		               AND f.CodigoFluxo NOT IN(SELECT ftp.CodigoFluxo FROM {0}.{1}.FluxosTipoProjeto ftp WHERE ftp.CodigoTipoProjeto = @CodigoTipoProjeto
						                         UNION
						                        SELECT fp.CodigoFluxo FROM {0}.{1}.FluxosProjeto fp WHERE fp.CodigoProjeto = @CodigoProjeto)
                       AND EXISTS (SELECT w.CodigoFluxo FROM {0}.{1}.Workflows w WHERE f.CodigoFluxo = w.CodigoFluxo AND w.DataRevogacao IS NULL AND DataPublicacao  IS NOT NULL)
  
              END
                ", bancodb, Ownerdb, codigoProjeto);
        return getDataSet(comandoSQL);
    }

    public bool verificaExistenciaNomeFluxoAssociadoProjeto(int codigoProjeto, string nomeFluxo, int codigoFluxo)
    {
        bool retorno = false;

        string comandoSQL = string.Format(@"
                 BEGIN
		            DECLARE @CodigoTipoProjeto  int,
						    @CodigoProjeto      int,
                            @CodigoEntidade     int
		
		            SET @CodigoProjeto = {2}
		
		            SELECT @CodigoTipoProjeto = CodigoTipoProjeto
			          FROM {0}.{1}.Projeto p 
		             WHERE p.CodigoProjeto = @CodigoProjeto

		            SELECT @CodigoEntidade = f.CodigoEntidade
			          FROM {0}.{1}.Fluxos f 
		             WHERE f.CodigoFluxo = {4};
                    
					
		            SELECT ftp.CodigoFluxo 
                      FROM {0}.{1}.FluxosTipoProjeto ftp 
                     WHERE ftp.CodigoTipoProjeto = @CodigoTipoProjeto
                       AND ftp.TextoOpcaoFluxo = '{3}'
                       AND ftp.CodigoFluxo <> {4}
					UNION
					 SELECT fp.CodigoFluxo 
                       FROM {0}.{1}.FluxosProjeto fp 
                      WHERE fp.CodigoProjeto = @CodigoProjeto
                        AND fp.TextoOpcaoFluxo = '{3}'
                        AND fp.CodigoFluxo <> {4}
  
              END
                ", bancodb, Ownerdb, codigoProjeto, nomeFluxo, codigoFluxo);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            retorno = true;

        return retorno;
    }

    public bool incluiAssociacaoFluxoProjeto(int codigoProjeto, int codigoFluxo, string statusRelacionamento,
                                            string nomeOpcao, string ocorrencia, int[] arrayStatus, int codigoUsuarioInclusao, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            int regAf = 0;

            string insertStatus = "";

            foreach (int codigoStatus in arrayStatus)
            {
                insertStatus += string.Format(@"INSERT INTO {0}.{1}.[FluxosStatusProjeto]
                                                       ([CodigoFluxo]
                                                       ,[CodigoProjeto]
                                                       ,[CodigoStatus]
                                                       ,[StatusRelacionamento]
                                                       ,[DataAtivacao]
                                                       ,[IdentificadorUsuarioAtivacao]
                                                       ,[DataDesativacao]
                                                       ,[IdentificadorUsuarioDesativacao])
                                                 VALUES
                                                       ({2}
                                                       ,{3}
                                                       ,{4}
                                                       ,'{5}'
                                                       ,{6}
                                                       ,{7}
                                                       ,{8}
                                                       ,{9})", bancodb
                                                             , Ownerdb
                                                             , codigoFluxo
                                                             , codigoProjeto
                                                             , codigoStatus
                                                             , statusRelacionamento
                                                             , "GetDate()"
                                                             , codigoUsuarioInclusao.ToString()
                                                             , statusRelacionamento == "D" ? "GetDate()" : "NULL"
                                                             , statusRelacionamento == "D" ? codigoUsuarioInclusao.ToString() : "NULL");
            }

            comandoSQL = string.Format(@"
                    INSERT INTO {0}.{1}.[FluxosProjeto]
                           ([CodigoFluxo]
                           ,[CodigoProjeto]
                           ,[StatusRelacionamento]
                           ,[TextoOpcaoFluxo]
                           ,[DataAtivacao]
                           ,[IdentificadorUsuarioAtivacao]
                           ,[DataDesativacao]
                           ,[IdentificadorUsuarioDesativacao]
                           ,[TipoOcorrenciaFluxo])
                     VALUES
                           ({2}
                           ,{3}
                           ,'{4}'
                           ,'{5}'
                           ,{6}
                           ,{7}
                           ,{8}
                           ,{9}
                           ,'{10}')

                    {11}
                   ", bancodb
                    , Ownerdb
                    , codigoFluxo
                    , codigoProjeto
                    , statusRelacionamento
                    , nomeOpcao.Replace("'", "''")
                    , "GetDate()"
                    , codigoUsuarioInclusao.ToString()
                    , statusRelacionamento == "D" ? "GetDate()" : "NULL"
                    , statusRelacionamento == "D" ? codigoUsuarioInclusao.ToString() : "NULL"
                    , ocorrencia
                    , insertStatus);

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }


    public bool atualizaAssociacaoFluxoProjeto(int codigoProjeto, int codigoFluxo, string statusRelacionamento, string statusRelacionamentoAnterior,
                                                string nomeOpcao, string ocorrencia, int[] arrayStatus, int codigoUsuarioInclusao, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            int regAf = 0;

            string insertStatus = "";

            string novaDataAtivacao = "DataAtivacao";
            string novoUsuarioAtivacao = "IdentificadorUsuarioAtivacao";
            string novaDataDesativacao = "DataDesativacao";
            string novoUsuarioDesativacao = "IdentificadorUsuarioDesativacao";

            string codigosSelecionados = "";

            if (statusRelacionamento != statusRelacionamentoAnterior)
            {
                if (statusRelacionamento == "A")
                {
                    novaDataAtivacao = "GetDate()";
                    novoUsuarioAtivacao = codigoUsuarioInclusao.ToString();
                    novaDataDesativacao = "NULL";
                    novoUsuarioDesativacao = "NULL";
                }
                else
                {
                    novaDataDesativacao = "GetDate()";
                    novoUsuarioDesativacao = codigoUsuarioInclusao.ToString();
                }
            }

            foreach (int codigoStatus in arrayStatus)
            {
                codigosSelecionados += codigoStatus + ",";

                insertStatus += string.Format(@" IF EXISTS(SELECT 1 FROM {0}.{1}.FluxosStatusProjeto 
                                                            WHERE CodigoProjeto = {3} 
                                                            AND CodigoFluxo = {2} 
                                                            AND CodigoStatus = {4})
                                            BEGIN
                                                UPDATE {0}.{1}.FluxosStatusProjeto SET [StatusRelacionamento] = '{5}'
                                                       ,[DataAtivacao] = {10}
                                                       ,[IdentificadorUsuarioAtivacao] = {11}
                                                       ,[DataDesativacao] = {12}
                                                       ,[IdentificadorUsuarioDesativacao] = {13}
                                                 WHERE CodigoProjeto = {3} 
                                                   AND CodigoFluxo = {2} 
                                                   AND CodigoStatus = {4}
                                            END
                                        ELSE
                                            BEGIN
                                                INSERT INTO {0}.{1}.[FluxosStatusProjeto]
                                                       ([CodigoFluxo]
                                                       ,[CodigoProjeto]
                                                       ,[CodigoStatus]
                                                       ,[StatusRelacionamento]
                                                       ,[DataAtivacao]
                                                       ,[IdentificadorUsuarioAtivacao]
                                                       ,[DataDesativacao]
                                                       ,[IdentificadorUsuarioDesativacao])
                                                 VALUES
                                                       ({2}
                                                       ,{3}
                                                       ,{4}
                                                       ,'{5}'
                                                       ,{6}
                                                       ,{7}
                                                       ,{8}
                                                       ,{9})
                                             END", bancodb
                                                             , Ownerdb
                                                             , codigoFluxo
                                                             , codigoProjeto
                                                             , codigoStatus
                                                             , statusRelacionamento
                                                             , "GetDate()"
                                                             , codigoUsuarioInclusao.ToString()
                                                             , statusRelacionamento == "D" ? "GetDate()" : "NULL"
                                                             , statusRelacionamento == "D" ? codigoUsuarioInclusao.ToString() : "NULL"
                                                             , novaDataAtivacao
                                                             , novoUsuarioAtivacao
                                                             , novaDataDesativacao
                                                             , novoUsuarioDesativacao);
            }

            codigosSelecionados = codigosSelecionados + "-1";

            comandoSQL = string.Format(@"
                    DELETE FROM [FluxosStatusProjeto]
                     WHERE CodigoStatus NOT IN ({12})
                       AND CodigoProjeto = {3} 
                       AND CodigoFluxo = {2} 

                    UPDATE {0}.{1}.[FluxosProjeto]
                           SET [StatusRelacionamento] = '{4}'
                           ,[TextoOpcaoFluxo] = '{5}'
                           ,[DataAtivacao] = {6}
                           ,[IdentificadorUsuarioAtivacao] = {7}
                           ,[DataDesativacao] = {8}
                           ,[IdentificadorUsuarioDesativacao] = {9}
                           ,[TipoOcorrenciaFluxo] = '{10}'
                      WHERE CodigoProjeto = {3} 
                        AND CodigoFluxo = {2} 

                    {11}
                   ", bancodb
                    , Ownerdb
                    , codigoFluxo
                    , codigoProjeto
                    , statusRelacionamento
                    , nomeOpcao.Replace("'", "''")
                    , novaDataAtivacao
                    , novoUsuarioAtivacao
                    , novaDataDesativacao
                    , novoUsuarioDesativacao
                    , ocorrencia
                    , insertStatus
                    , codigosSelecionados);

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool verificaExclusaoFluxoAssociadoProjeto(int codigoProjeto, int codigoFluxo)
    {
        bool retorno = true;

        string comandoSQL = string.Format(@"
                SELECT 1 
                  FROM {0}.{1}.Workflows w INNER JOIN
			           {0}.{1}.InstanciasWorkflows iw ON iw.CodigoWorkflow = w.CodigoWorkflow
                 WHERE w.CodigoFluxo = {3}
                   AND iw.IdentificadorProjetoRelacionado = {2}
                ", bancodb, Ownerdb, codigoProjeto, codigoFluxo);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            retorno = false;

        return retorno;
    }

    public bool excluiAssociacaoFluxoProjeto(int codigoFluxo, int codigoProjeto)
    {
        string comandoSQL = string.Format(@"
            DELETE FROM {0}.{1}.[FluxosStatusProjeto]
             WHERE CodigoProjeto = {2}
               AND CodigoFluxo = {3}

             DELETE FROM {0}.{1}.[FluxosProjeto]
              WHERE CodigoProjeto = {2}
                AND CodigoFluxo = {3}
            ", bancodb, Ownerdb, codigoProjeto, codigoFluxo);
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

    #region Identificação

    public DataSet getTarefaEdicaoEAP(string IdEdicaoEAP, string IDTarefa)
    {
        comandoSQL = string.Format(@"
        SELECT teap.IDEdicaoEAP,
               teap.IDTarefa,
               teap.Inicio,
               teap.Termino,
               teap.Duracao,
               teap.Trabalho,
               teap.Custo,
               teap.CodigoUsuarioResponsavel as CodigoUsuario,
               u.NomeUsuario,
               teap.Anotacoes
          FROM {0}.{1}.[TarefaEAP] teap
    LEFT JOIN {0}.{1}.[Usuario] u on (u.CodigoUsuario = teap.CodigoUsuarioResponsavel) 
         WHERE teap.[IDEdicaoEAP] = '{2}' AND 
               teap.[IDTarefa] = '{3}' ", bancodb, Ownerdb, IdEdicaoEAP, IDTarefa);

        return getDataSet(comandoSQL);
    }

    public bool incluiTarefaEAP(string idEdicaoEAP, string idTarefa, string inicio, string termino, decimal duracao, decimal trabalho, decimal custo, string codigoUsuario, string anotacoes, ref string msgErro)
    {
        bool retorno = false;
        int regAfetados = 0;
        comandoSQL = string.Format(@"
        INSERT INTO {0}.{1}.[TarefaEAP]([IDEdicaoEAP],[IDTarefa],                   [Inicio],                  [Termino],[Duracao],[Trabalho],[Custo],[CodigoUsuarioResponsavel],[Anotacoes])
                                VALUES (        '{2}',    '{10}',                        {3},                        {4},      {5},       {6},    {7},                       {8},'{9}')"
                      , bancodb, Ownerdb, idEdicaoEAP, inicio, termino, duracao.ToString().Replace(",", "."), trabalho.ToString().Replace(",", "."), custo.ToString().Replace(",", "."), codigoUsuario, anotacoes, idTarefa);
        try
        {
            retorno = execSQL(comandoSQL, ref regAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgErro = ex.Message;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaTarefaEAP(string idEdicaoEAP, string idTarefa, string inicio, string termino, decimal duracao, decimal trabalho, decimal custo, string codigoUsuario, string anotacoes, ref string msgErro)
    {
        int regAfetados = 0;
        comandoSQL = string.Format(@"
        UPDATE {0}.{1}.TarefaEAP
           SET 
               Inicio = {3}
              ,Termino = {4}
              ,Duracao = {5}
              ,Trabalho = {6}
              ,Custo = {7}
              ,CodigoUsuarioResponsavel = {8}
              ,Anotacoes = '{9}'
         WHERE IDEdicaoEAP = '{2}' AND IDTarefa = '{10}'", bancodb, Ownerdb, idEdicaoEAP, inicio, termino, duracao.ToString().Replace(",", "."), trabalho.ToString().Replace(",", "."), custo.ToString().Replace(",", "."), codigoUsuario, anotacoes, idTarefa);
        try
        {
            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch (Exception ex)
        {
            msgErro = ex.Message;
            return false;
        }
    }

    public DataSet getTarefaCronogramaProjetoEdicaoEAP(string IdEdicaoEAP, string IDTarefa)
    {
        comandoSQL = string.Format(@"
        SELECT T.[Inicio]
              ,T.[Termino]
              ,T.[Duracao]
              ,T.[Trabalho]
              ,T.[Custo]
              ,T.[Anotacoes]
              ,u.NomeUsuario
              ,T.CodigoUsuarioResponsavel as CodigoUsuario
         FROM 
            {0}.{1}.ControleEdicaoEAP CEAP 
                INNER JOIN {0}.{1}.CronogramaProjeto CP ON
                        (CP.CodigoProjeto = CEAP.CodigoProjeto)   
                INNER JOIN {0}.{1}.[TarefaCronogramaProjeto] T ON
                        (T.[CodigoCronogramaProjeto] = CP.[CodigoCronogramaProjeto])
                LEFT JOIN {0}.{1}.Usuario u on (u.CodigoUsuario = T.[CodigoUsuarioResponsavel])
        WHERE
                CEAP.[IDEdicaoEAP]  =  '{2}'
            AND T.[IDTarefa]        =  '{3}'", bancodb, Ownerdb, IdEdicaoEAP, IDTarefa);
        return getDataSet(comandoSQL);
    }
    #endregion

    #region _Projeto - Administracao - Programas Do Projetos

    public DataSet getProgramasDoProjeto(string where)
    {
        string comandoSQL = string.Format(@"
                SELECT DISTINCT cp.CodigoProjeto, cp.CodigoMSProject, cp.NomeProjeto, cp.CodigoGerenteProjeto, 
								un.CodigoUnidadeNegocio, un.SiglaUnidadeNegocio, un.NomeUnidadeNegocio,
								g.NomeUsuario AS NomeGerente
                FROM {0}.{1}.Projeto as cp INNER JOIN 
                     {0}.{1}.UnidadeNegocio AS UN ON CP.CodigoUnidadeNegocio = UN.CodigoUnidadeNegocio LEFT JOIN
                     {0}.{1}.Usuario g ON g.CodigoUsuario = cp.CodigoGerenteProjeto INNER JOIN
                     {0}.{1}.TipoProjeto tp on tp.CodigoTipoProjeto = cp.CodigoTipoProjeto
                WHERE cp.IndicaPrograma = 'S' AND CP.DataExclusao IS NULL AND tp.IndicaTipoProjeto = 'PRG'
                  {2} 
                ORDER BY cp.NomeProjeto
                ", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getProjetosToProgramas(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT DISTINCT cp.CodigoProjeto, cp.CodigoMSProject, cp.NomeProjeto, cp.CodigoGerenteProjeto, 
                                un.CodigoUnidadeNegocio, un.SiglaUnidadeNegocio, g.NomeUsuario AS NomeGerente 
                FROM {0}.{1}.Projeto CP INNER JOIN 
                     {0}.{1}.UnidadeNegocio AS UN ON CP.CodigoUnidadeNegocio = UN.CodigoUnidadeNegocio INNER JOIN
                     {0}.{1}.Usuario g ON g.CodigoUsuario = cp.CodigoGerenteProjeto 
                WHERE CP.DataExclusao IS NULL 
                    AND CP.IndicaPrograma = 'N'
                    AND CP.CodigoStatusProjeto <> 4
                    AND CP.CodigoEntidade = {2}
                    {3}
                    AND NOT EXISTS(SELECT 1 
                                     FROM {0}.{1}.LinkProjeto 
                                    WHERE (CodigoProjetoPai = cp.CodigoProjeto OR CodigoProjetoFilho = cp.CodigoProjeto) 
                                      AND TipoLink = 'PP')
                ORDER BY cp.NomeProjeto
                ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getTreeListUnidades(string codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
            SELECT u.CodigoUnidadeNegocio, u.NomeUnidadeNegocio, u.SiglaUnidadeNegocio, u.CodigoUnidadeNegocioSuperior, 
                 u.NivelHierarquicoUnidade, u.CodigoTipoUnidadeNegocio, UN_SUP.NomeUnidadeNegocio AS NomeUnidadeSuperior, 
                 UN_SUP.SiglaUnidadeNegocio AS SiglaUnidadeSuperior, u.CodigoUsuarioGerente, u.CodigoReservado
            FROM {0}.{1}.UnidadeNegocio u LEFT OUTER JOIN
                 {0}.{1}.UnidadeNegocio AS UN_SUP ON u.CodigoUnidadeNegocioSuperior = UN_SUP.CodigoUnidadeNegocio
            WHERE 1=1
                {3}
              AND u.CodigoEntidade = {2}
            ORDER BY NomeUnidadeNegocio
        ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getListaProjetoDisponivel(string codigoUsuario, string codigoEntidade, string codigoProjeto)
    {
        string comandoSQL = string.Format(@"
                SELECT DISTINCT p.CodigoProjeto, p.NomeProjeto
                  FROM {0}.{1}.Projeto p      
                 WHERE p.DataExclusao IS NULL   
                   AND p.IndicaPrograma = 'N' 
                   AND p.CodigoStatusProjeto <> 4 -- Diferente de Cancelado
                   AND p.CodigoProjeto <> {4}
                   AND p.CodigoEntidade = {3} --by Alejandro 17/07/2010
                   AND NOT EXISTS(SELECT 1 
                                  FROM {0}.{1}.LinkProjeto AS lp
                                  WHERE (lp.CodigoProjetoPai = p.CodigoProjeto OR CodigoProjetoFilho = p.CodigoProjeto) 
                                    AND TipoLink = 'PP')
                /* Inclui todos os projetos que estão em unidades onde o usuário é gerente */
                 UNION SELECT p.CodigoProjeto, p.NomeProjeto
                         FROM {0}.{1}.Projeto p INNER JOIN
                              {0}.{1}.UnidadeNegocio AS UN ON p.CodigoUnidadeNegocio = UN.CodigoUnidadeNegocio                      
                        WHERE (UN.CodigoUsuarioGerente = {2} )  
                          AND p.DataExclusao IS NULL  
                          AND p.IndicaPrograma = 'N' 
                          AND p.CodigoStatusProjeto <> 4 -- Diferente de Cancelado
                          AND p.CodigoProjeto <> {4}
                          AND p.CodigoEntidade = {3} --by Alejandro 17/07/2010
                          AND NOT EXISTS(SELECT 1 
                                         FROM {0}.{1}.LinkProjeto AS lp
                                         WHERE (lp.CodigoProjetoPai = p.CodigoProjeto OR CodigoProjetoFilho = p.CodigoProjeto) 
                                           AND TipoLink = 'PP') 
                ORDER BY p.NomeProjeto
        ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, codigoProjeto);
        return getDataSet(comandoSQL);
    }

    public DataSet getListaProjetoSelecionados(string codigoUsuario, string codigoEntidade, string codigoPrograma)
    {
        string comandoSQL = string.Format(@"
                SELECT DISTINCT p.CodigoProjeto, p.NomeProjeto
                  FROM {0}.{1}.Projeto p      
                 WHERE p.DataExclusao IS NULL   
                   AND p.IndicaPrograma = 'N' 
                   AND p.CodigoProjeto IN (SELECT lp.CodigoProjetoFilho
                                  FROM {0}.{1}.LinkProjeto AS lp
                                  WHERE CodigoProjetoPai = {4}
                                    AND TipoLink = 'PP')
                /* Inclui todos os projetos que estão em unidades onde o usuário é gerente */
                 UNION SELECT p.CodigoProjeto, p.NomeProjeto
                         FROM {0}.{1}.Projeto p INNER JOIN
                              {0}.{1}.UnidadeNegocio AS UN ON p.CodigoUnidadeNegocio = UN.CodigoUnidadeNegocio                      
                        WHERE (UN.CodigoUsuarioGerente = {2} )  
                          AND p.DataExclusao IS NULL  
                          AND p.IndicaPrograma = 'N' 
                          AND p.CodigoProjeto IN (SELECT lp.CodigoProjetoFilho
                                  FROM {0}.{1}.LinkProjeto AS lp
                                  WHERE CodigoProjetoPai = {4}
                                    AND TipoLink = 'PP')
                ORDER BY p.NomeProjeto
        ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, codigoPrograma);
        return getDataSet(comandoSQL);
    }

    public DataSet getListaProjetoPrograma(string codigoUsuario, string codigoEntidade, string codigoPrograma)
    {
        string comandoSQL = string.Format(@"

        DECLARE   @l_imgProjeto   varchar(100)
                , @l_imgProcesso  varchar(100)
                , @l_imgAgil      varchar(100)

        SET @l_imgProjeto  = '<img border=''0'' src=''../../imagens/projeto.PNG'' title=''" + Resources.traducao.projeto + @"''/>';
        SET @l_imgProcesso = '<img border=''0'' src=''../../imagens/processo.PNG'' title=''" + Resources.traducao.processo + @"'' style=''width: 21px; height: 18px;'' />';
        SET @l_imgAgil     = '<img border=''0'' src=''../../imagens/agile.PNG'' title=''" + Resources.traducao.projeto__gil + @" '' style=''width: 21px; height: 18px;'' />';

        SELECT DISTINCT p.CodigoProjeto,
                        CASE WHEN tp.IndicaTipoProjeto = 'PRJ' THEN @l_imgProjeto
                             WHEN tp.IndicaTipoProjeto = 'PRC' THEN @l_imgProcesso + '/>'
                             WHEN tp.IndicaTipoProjeto = 'PRG' AND tp.IndicaProjetoAgil = 'S' THEN @l_imgAgil + '/>'
                             ELSE ''
                        END + '&nbsp;' +
                        p.NomeProjeto AS NomeProjeto, 
                        p.NomeProjeto AS NomeProjeto2,
                        'N' AS Selecionado, 1 AS ColunaAgrupamento
                  FROM {0}.{1}.Projeto p      INNER JOIN
                       {0}.{1}.TipoProjeto  tp ON (tp.CodigoTipoProjeto = p.CodigoTipoProjeto)
                 WHERE p.DataExclusao IS NULL   
                   AND p.IndicaPrograma = 'N' 
                   AND IndicaTipoProjeto <> 'SPT'
                   AND p.CodigoStatusProjeto <> 4 -- Diferente de Cancelado
                   AND p.CodigoProjeto <> {4}
                   AND p.CodigoEntidade = {3} --by Alejandro 17/07/2010
                   AND NOT EXISTS(SELECT 1 
                                  FROM {0}.{1}.LinkProjeto AS lp
                                  WHERE (lp.CodigoProjetoPai = p.CodigoProjeto OR CodigoProjetoFilho = p.CodigoProjeto) 
                                    AND TipoLink = 'PP')
                /* Inclui todos os projetos que estão em unidades onde o usuário é gerente */
                 UNION SELECT p.CodigoProjeto,
                                CASE WHEN tp.IndicaTipoProjeto = 'PRJ' THEN @l_imgProjeto
                                     WHEN tp.IndicaTipoProjeto = 'PRC' THEN @l_imgProcesso + '/>'
                                     WHEN tp.IndicaTipoProjeto = 'PRG' AND tp.IndicaProjetoAgil = 'S' THEN @l_imgAgil + '/>'
                                     ELSE ''
                                END + '&nbsp;' +
                                p.NomeProjeto AS NomeProjeto,
                               p.NomeProjeto AS NomeProjeto2,
                              'N', 
                              1
                         FROM {0}.{1}.Projeto p INNER JOIN
                              {0}.{1}.UnidadeNegocio AS UN ON p.CodigoUnidadeNegocio = UN.CodigoUnidadeNegocio    INNER JOIN
                              {0}.{1}.TipoProjeto  tp ON (tp.CodigoTipoProjeto = p.CodigoTipoProjeto)                    
                        WHERE (UN.CodigoUsuarioGerente = {2} )  
                          AND p.DataExclusao IS NULL  
                          AND p.IndicaPrograma = 'N' 
                          AND IndicaTipoProjeto <> 'SPT'
                          AND p.CodigoStatusProjeto <> 4 -- Diferente de Cancelado
                          AND p.CodigoProjeto <> {4}
                          AND p.CodigoEntidade = {3} --by Alejandro 17/07/2010
                          AND NOT EXISTS(SELECT 1 
                                         FROM {0}.{1}.LinkProjeto AS lp
                                         WHERE (lp.CodigoProjetoPai = p.CodigoProjeto OR CodigoProjetoFilho = p.CodigoProjeto) 
                                           AND TipoLink = 'PP') 
                  UNION
                SELECT DISTINCT p.CodigoProjeto,                                 
                                CASE WHEN tp.IndicaTipoProjeto = 'PRJ' THEN @l_imgProjeto
                                     WHEN tp.IndicaTipoProjeto = 'PRC' THEN @l_imgProcesso + '/>'
                                     WHEN tp.IndicaTipoProjeto = 'PRG' AND tp.IndicaProjetoAgil = 'S' THEN @l_imgAgil + '/>'
                                     ELSE ''
                                END + '&nbsp;' +
                                p.NomeProjeto AS NomeProjeto,
                               p.NomeProjeto AS NomeProjeto2,
                                'S', 
                                0
                  FROM {0}.{1}.Projeto p    INNER JOIN
                       {0}.{1}.TipoProjeto  tp ON (tp.CodigoTipoProjeto = p.CodigoTipoProjeto)    
                 WHERE p.DataExclusao IS NULL   
                   AND p.IndicaPrograma = 'N' 
                   AND IndicaTipoProjeto <> 'SPT'
                   AND p.CodigoProjeto IN (SELECT lp.CodigoProjetoFilho
                                  FROM {0}.{1}.LinkProjeto AS lp
                                  WHERE CodigoProjetoPai = {4}
                                    AND TipoLink = 'PP')
                /* Inclui todos os projetos que estão em unidades onde o usuário é gerente */
                 UNION SELECT p.CodigoProjeto,                                 
                                CASE WHEN tp.IndicaTipoProjeto = 'PRJ' THEN @l_imgProjeto
                                     WHEN tp.IndicaTipoProjeto = 'PRC' THEN @l_imgProcesso + '/>'
                                     WHEN tp.IndicaTipoProjeto = 'PRG' AND tp.IndicaProjetoAgil = 'S' THEN @l_imgAgil + '/>'
                                     ELSE ''
                                END + '&nbsp;' +
                                p.NomeProjeto AS NomeProjeto,
                               p.NomeProjeto AS NomeProjeto2,
                              'S', 
                              0
                         FROM {0}.{1}.Projeto p INNER JOIN
                              {0}.{1}.UnidadeNegocio AS UN ON p.CodigoUnidadeNegocio = UN.CodigoUnidadeNegocio    INNER JOIN
                              {0}.{1}.TipoProjeto  tp ON (tp.CodigoTipoProjeto = p.CodigoTipoProjeto)                    
                        WHERE (UN.CodigoUsuarioGerente = {2} )  
                          AND p.DataExclusao IS NULL  
                          AND p.IndicaPrograma = 'N' 
                          AND IndicaTipoProjeto <> 'SPT'
                          AND p.CodigoProjeto IN (SELECT lp.CodigoProjetoFilho
                                  FROM {0}.{1}.LinkProjeto AS lp
                                  WHERE CodigoProjetoPai = {4}
                                    AND TipoLink = 'PP')
                ORDER BY NomeProjeto2
        ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, codigoPrograma);
        return getDataSet(comandoSQL);
    }

    public bool incluiProgramasDoProjeto(string nomeProjeto, string codigoGerenteProjeto, string codigoUnidadeNegocio,
                                            string codigoEntidade, string codigoUsuarioInclusao,
                                            ref string identityCodigoProjeto, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            DateTime agora = DateTime.Now;
            // Insere um registro na tabela complementoProjeto
            // Codigo de Status : ... (...)
            comandoSQL = string.Format(@"
                    DECLARE @codigoProjeto int

                    INSERT INTO {0}.{1}.Projeto (NomeProjeto, CodigoGerenteProjeto, CodigoUnidadeNegocio, DataInclusao,
										 DataUltimaAlteracao, CodigoEntidade, CodigoUsuarioInclusao, CodigoStatusProjeto,
										 IndicaPrograma, CodigoTipoProjeto)
                    VALUES('{2}', {3}, {4}, GETDATE(), GETDATE(), {5}, {6}, 3, 'S', 2)	
                            
                    SET @codigoProjeto = scope_identity()
                    
                    EXEC {0}.{1}.p_AtualizaStatusProjetos 'PROGRAMA', @codigoProjeto

                    SELECT @codigoProjeto As codigoProjeto
                   ", bancodb, Ownerdb, nomeProjeto, codigoGerenteProjeto, codigoUnidadeNegocio, codigoEntidade, codigoUsuarioInclusao);

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                identityCodigoProjeto = ds.Tables[0].Rows[0][0].ToString();

            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool incluiProjetoSelecionados(string[] arrayProjetosFilhos, string codigoProjetoPai)
    {
        bool retorno = false;
        string comandoSQL = "";
        string mensagem = "";
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
                        DELETE FROM {0}.{1}.LinkProjeto WHERE CodigoProjetoPai = {2};
                            --EXEC {0}.{1}.p_atualizaStatusProjetos 'Sys', {2}"
                        , bancodb, Ownerdb, codigoProjetoPai);

            foreach (string codigoProjeto in arrayProjetosFilhos)
            {
                if (codigoProjeto != "")
                {
                    // Insere um registro na tabela complementoProjeto
                    comandoSQL += string.Format(@"
                        INSERT INTO {0}.{1}.LinkProjeto (CodigoProjetoPai, CodigoProjetoFilho, TipoLink)
                        VALUES({2}, {3}, 'PP');
                            --EXEC {0}.{1}.p_atualizaStatusProjetos 'Sys', {3}"
                           , bancodb, Ownerdb, codigoProjetoPai, codigoProjeto);
                }
            }

            comandoSQL += string.Format(@" 
                       -- EXEC {0}.{1}.p_atualizaStatusProjetos 'Sys', {2}"
                        , bancodb, Ownerdb, codigoProjetoPai);

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

    public bool atualizaProgramaDoProjeto(string nomeProjeto, string codigoGerenteProjeto, string codigoUnidadeNegocio,
                                            string codigoEntidade, string CodigoUsuarioUltimaAlteracao,
                                            string CodigoProjeto)
    {
        bool retorno = false;
        string mensagem = "";
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                BEGIN
                 -- begin tran T1

                  UPDATE {0}.{1}.Projeto 
                     SET NomeProjeto = '{2}',
                         CodigoGerenteProjeto = {3}, 
                         CodigoUnidadeNegocio = {4}, 
                         CodigoEntidade = {5},
                         DataUltimaAlteracao = GETDATE(),
                         CodigoUsuarioUltimaAlteracao = {6},
                         CodigoTipoProjeto = 2,
                         IndicaPrograma = 'S'
                   WHERE CodigoProjeto = {7}
                   
                   -- if @@ERROR > 0
                   --     ROLLBACK TRAN T1
                   -- else
                   --     COMMIT TRAN T1

                    EXEC {0}.{1}.p_AtualizaStatusProjetos 'PROGRAMA', {7}

                END
                ", bancodb, Ownerdb, nomeProjeto, codigoGerenteProjeto,
                   codigoUnidadeNegocio, codigoEntidade, CodigoUsuarioUltimaAlteracao, CodigoProjeto);

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

    public bool excluiProgramaDoprojeto(string codigoProjeto, string codigoUsuarioExclusao)
    {
        int registrosAfetados = 0;
        bool retorno = false;
        string mensagem = "";
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
                DELETE FROM {0}.{1}.LinkProjeto WHERE CodigoProjetoPai = {2}

                UPDATE {0}.{1}.Projeto 
                    SET  DataExclusao = GETDATE()
                        ,codigoUsuarioExclusao = {3}
                    WHERE CodigoProjeto = {2};
                    
                    -- EXEC {0}.{1}.p_atualizaStatusProjetos 'Sys', {2}
                    
                ",
                    bancodb, Ownerdb, codigoProjeto, codigoUsuarioExclusao);
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

    #endregion

    #region _Projeto - Administracao - Carteiras de Projetos

    public DataSet getCarteirasDeProjetos(string where)
    {

        string comandoSQL = string.Format(@"
                SELECT cp.CodigoCarteira, cp.NomeCarteira, cp.DescricaoCarteira, cp.CodigoEntidade,
                       cp.IniciaisCarteiraControladaSistema, cp.CodigoObjeto,cp.IndicaCarteiraAtiva,
                       case when cp.IniciaisCarteiraControladaSistema is not null then
                        '" + Resources.traducao.sim + @"' 
                       else 
                        '" + Resources.traducao.n_o + @"' 
                       end as ControladaSistema
                FROM {0}.{1}.Carteira as cp
                where 1 = 1 
                {2} 
                ORDER BY cp.NomeCarteira
                ", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public bool mostraComboCarteirasDeProjetosTela(string caminhoTela)
    {
        string comandoSQL = string.Format(@"               
			
	                SELECT 1
	                  FROM {0}.{1}.CadastroTelasSistema
	                 WHERE CaminhoTela = '{2}'
	                   AND MostraComboCarteira = 'S'
                ", bancodb, Ownerdb, caminhoTela);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            return true;
        }
        else
            return false;
    }

    public bool indicaCarteiraPossuiBoletimStatus(int codigoCarteira)
    {
        string comandoSql = string.Format(@"
SELECT 1
  FROM {0}.{1}.Carteira c
 WHERE ISNULL(c.IniciaisCarteiraControladaSistema,'') <> 'PR'
   AND c.CodigoCarteira = {2}", bancodb, Ownerdb, codigoCarteira);

        DataSet ds = getDataSet(comandoSql);

        return ds.Tables[0].Rows.Count > 0;
    }

    public void defineConfiguracoesComboCarteiras(ASPxComboBox combo, bool isPostback, int codigoUsuario, int codigoEntidade, bool forcarCarteiraPadrao)
    {
        string comandoSQL = string.Format(@"
                BEGIN
	                DECLARE @CodigoUsuario int,
			                @CodigoEntidade int
			
	                SET @CodigoEntidade = {2}			
	                SET @CodigoUsuario = {3}
			
	                SELECT c.NomeCarteira, c.CodigoCarteira
	                  FROM {0}.{1}.Carteira c 
	                 WHERE IndicaCarteiraAtiva = 'S'
                       AND c.CodigoEntidade = @CodigoEntidade
	                   AND (((c.CodigoCarteira IN(SELECT cp.CodigoCarteira FROM {0}.{1}.CarteiraProjeto cp) OR c.IniciaisCarteiraControladaSistema IS NOT NULL)
	                   AND {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, c.CodigoCarteira, NULL, 'CP', 0, NULL, 'CP_VslPrj') > 0) OR
			                c.IniciaisCarteiraControladaSistema = 'PR')
                     ORDER BY c.NomeCarteira
  
                  END
                ", bancodb, Ownerdb, codigoEntidade, codigoUsuario);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds))
        {
            combo.DataSource = ds;
            combo.TextField = "NomeCarteira";
            combo.ValueField = "CodigoCarteira";
            combo.DataBind();

            if (!isPostback)
            {
                if (getInfoSistema("CodigoCarteira") != null && combo.Items.FindByValue(getInfoSistema("CodigoCarteira").ToString()) != null && forcarCarteiraPadrao == false)
                {
                    combo.Value = getInfoSistema("CodigoCarteira").ToString();
                }
                else
                {
                    int codigoCarteiraPadrao = getCodigoCarteiraPadraoUsuario(codigoUsuario, codigoEntidade);

                    if (combo.Items.FindByValue(codigoCarteiraPadrao.ToString()) == null)
                        codigoCarteiraPadrao = getCodigoCarteiraPadraoEntidade(codigoEntidade);

                    combo.Value = codigoCarteiraPadrao.ToString();
                }
            }
        }

        if (!forcarCarteiraPadrao)
            setInfoSistema("CodigoCarteira", combo.SelectedIndex != -1 ? combo.Value.ToString() : "-1");
    }

    public bool verificaCarteiraPossuiStatusReport(int codigoCarteira)
    {
        string comandoSql = string.Format(@"
DECLARE @CodigoCarteira INT
	SET @CodigoCarteira = {2}

 SELECT 
		CASE WHEN EXISTS(SELECT 1
						   FROM {0}.{1}.StatusReport sr 
						  WHERE sr.CodigoTipoAssociacaoObjeto = dbo.f_GetCodigoTipoAssociacao('CP')
							AND sr.CodigoObjeto = @CodigoCarteira
							AND sr.ConteudoStatusReport IS NOT NULL
                            AND sr.DataExclusao IS NULL)
			THEN 'S'
			ELSE 'N'
		END AS CarteiraPossuiStatusReport", bancodb, Ownerdb, codigoCarteira);

        DataSet ds = getDataSet(comandoSql);
        DataRow dr = ds.Tables[0].Rows[0];

        return dr["CarteiraPossuiStatusReport"].Equals("S");
    }

    public int getCodigoCarteiraPadraoUsuario(int codigoUsuario, int codigoEntidade)
    {
        int codigoCarteiraPadrao = -1;

        comandoSQL = string.Format(@"
                    BEGIN
	                    DECLARE @CodigoUsuario int,
			                    @CodigoEntidade int
			
	                    SET @CodigoEntidade = {2}			
	                    SET @CodigoUsuario = {3}

                        IF NOT EXISTS(SELECT 1
					                    FROM {0}.{1}.Usuario u INNER JOIN
					                         {0}.{1}.Carteira c ON (c.CodigoCarteira = u.CodigoCarteiraPadrao)
					                   WHERE CodigoUsuario = @CodigoUsuario
						                 AND CodigoCarteiraPadrao IS NOT NULL)
                        BEGIN
                            UPDATE {0}.{1}.Usuario SET CodigoCarteiraPadrao = (SELECT CodigoCarteira 
                                                                                 FROM {0}.{1}.Carteira 
                                                                                WHERE IniciaisCarteiraControladaSistema = 'PR'
                                                                                  AND CodigoEntidade = @CodigoEntidade)
                             WHERE CodigoUsuario = @CodigoUsuario
                        END
			
                       
                        IF EXISTS(SELECT 1
					                FROM {0}.{1}.Usuario u INNER JOIN
					                     {0}.{1}.Carteira c ON (c.CodigoCarteira = u.CodigoCarteiraPadrao
															AND c.CodigoEntidade = @CodigoEntidade)
					                    WHERE CodigoUsuario = @CodigoUsuario
						                  AND CodigoCarteiraPadrao IS NOT NULL
						                  AND {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, c.CodigoCarteira, NULL, 'CP', 0, NULL, 'CP_VslPrj') > 0
                                )
                        BEGIN
	                        SELECT CodigoCarteiraPadrao
	                          FROM {0}.{1}.Usuario u 
	                         WHERE CodigoUsuario = @CodigoUsuario
                        END
                        ELSE
                           SELECT CodigoCarteira AS CodigoCarteiraPadrao
                             FROM {0}.{1}.Carteira 
                            WHERE IniciaisCarteiraControladaSistema = 'PR'
                              AND CodigoEntidade = @CodigoEntidade                       
  
                  END
                ", bancodb, Ownerdb, codigoEntidade, codigoUsuario);

        DataSet dsPreferencia = getDataSet(comandoSQL);

        if (DataSetOk(dsPreferencia) && DataTableOk(dsPreferencia.Tables[0]) && (dsPreferencia.Tables[0].Rows[0]["CodigoCarteiraPadrao"].ToString() != ""))
            codigoCarteiraPadrao = int.Parse(dsPreferencia.Tables[0].Rows[0]["CodigoCarteiraPadrao"].ToString());

        return codigoCarteiraPadrao;
    }

    public int getCodigoCarteiraPadraoEntidade(int codigoEntidade)
    {
        int codigoCarteiraPadrao = -1;

        comandoSQL = string.Format(@"
                    BEGIN
	                    DECLARE @CodigoEntidade int
			
	                    SET @CodigoEntidade = {2}	

                        SELECT CodigoCarteira 
                          FROM {0}.{1}.Carteira 
                         WHERE IniciaisCarteiraControladaSistema = 'PR'
                           AND CodigoEntidade = @CodigoEntidade
  
                  END
                ", bancodb, Ownerdb, codigoEntidade);

        DataSet dsPreferencia = getDataSet(comandoSQL);

        if (DataSetOk(dsPreferencia) && DataTableOk(dsPreferencia.Tables[0]) && (dsPreferencia.Tables[0].Rows[0]["CodigoCarteira"].ToString() != ""))
            codigoCarteiraPadrao = int.Parse(dsPreferencia.Tables[0].Rows[0]["CodigoCarteira"].ToString());

        return codigoCarteiraPadrao;
    }

    public bool incluiCarteirasDoProjeto(string NomeCarteira, string DescricaoCarteira, string codigoEntidade, string indicaCarteiraAtiva, ref string identityCodigoCarteira, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            DataSet dsVerifica = getCarteirasDeProjetos("and cp.NomeCarteira = '" + NomeCarteira + "'");

            if (DataSetOk(dsVerifica) && DataTableOk(dsVerifica.Tables[0]))
            {
                msgError = "Não foi possível incluir o registro, nome da carteira informado já existe! ";
                return false;
            }

            DateTime agora = DateTime.Now;

            comandoSQL = string.Format(@"
                    DECLARE @codigoCarteira int

                    INSERT INTO {0}.{1}.Carteira (NomeCarteira, DescricaoCarteira, CodigoEntidade, IndicaCarteiraAtiva)
                    VALUES('{2}', '{3}', {4}, '{5}')	
                            
                    SELECT @codigoCarteira = SCOPE_IDENTITY()
                    
                    SELECT @codigoCarteira As codigoCarteira
                   ", bancodb, Ownerdb, NomeCarteira, DescricaoCarteira, codigoEntidade, indicaCarteiraAtiva);

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                identityCodigoCarteira = ds.Tables[0].Rows[0][0].ToString();

            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }



    public DataSet getListaProjetoDisponivelCarteira(string codigoEntidade, string codigoCarteira)
    {
        string comandoSQL = string.Format(@"
                SELECT DISTINCT p.CodigoProjeto, ent.SiglaUnidadeNegocio + ' - ' + CASE WHEN p.IndicaPrograma = 'N' THEN 'Projeto: ' ELSE 'Programa: ' END + p.NomeProjeto  as NomeProjeto
                  FROM {0}.{1}.Projeto p 
                  inner join {0}.{1}.UnidadeNegocio as ent on ent.CodigoUnidadeNegocio = p.CodigoEntidade 
                WHERE p.DataExclusao IS NULL
                AND p.CodigoStatusProjeto != 4 -- não mostrar apenas os cancelados
                AND (p.CodigoEntidade = {3} or dbo.f_GetUnidadeSuperior(ent.CodigoUnidadeNegocio, {3}) = {3})
                AND NOT EXISTS(SELECT 1
                                 FROM {0}.{1}.CarteiraProjeto AS lp
                                WHERE (lp.CodigoCarteira = {2} and lp.CodigoProjeto = p.CodigoProjeto))
                order by NomeProjeto
        ", bancodb, Ownerdb, codigoCarteira, codigoEntidade, getPathSistema());
        return getDataSet(comandoSQL);
    }

    public DataSet getListaProjetoSelecionadosCarteira(string codigoEntidade, string codigoCarteira)
    {
        string comandoSQL = string.Format(@"
               SELECT DISTINCT p.CodigoProjeto, ent.SiglaUnidadeNegocio + ' - ' + CASE WHEN p.IndicaPrograma = 'N' THEN 'Projeto: ' ELSE 'Programa: ' END + p.NomeProjeto  as NomeProjeto
                FROM {0}.{1}.Projeto p 
                  inner join {0}.{1}.UnidadeNegocio as ent on ent.CodigoUnidadeNegocio = p.CodigoEntidade 
                WHERE p.DataExclusao IS NULL
                   AND p.CodigoProjeto IN (SELECT lp.CodigoProjeto
                                  FROM {0}.{1}.CarteiraProjeto AS lp
                                  WHERE lp.CodigoCarteira = {2})
                
                ORDER BY NomeProjeto
        ", bancodb, Ownerdb, codigoCarteira, getPathSistema());
        return getDataSet(comandoSQL);
    }

    public string SelectProjetoSelecionadosCarteira(string codigoCarteira, string codigoProjeto)
    {
        bool retorno = false;
        DataSet IndicaCarteiraPrincipal;
        string comandoSQL = "";
        string mensagem = "";
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
                        SELECT IndicaCarteiraPrincipal FROM {0}.{1}.CarteiraProjeto WHERE CodigoCarteira = {2} AND CodigoProjeto = {2};"
                        , bancodb, Ownerdb, codigoCarteira, codigoProjeto);


            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            mensagem = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }

        IndicaCarteiraPrincipal = getDataSet(comandoSQL);

        if (DataSetOk(IndicaCarteiraPrincipal) && DataTableOk(IndicaCarteiraPrincipal.Tables[0]) && IndicaCarteiraPrincipal.Tables[0].ToString().ToUpper() == "S")
        {
            return "S";
        }
        else
        {
            return "N";
        }
    }

    public bool UpdateProjetoSelecionadosCarteira(string[] arrayProjetosFilhos, string codigoCarteira, string[] IndicaCarteiraPrincipal)
    {
        bool retorno = false;
        string comandoSQL = "";
        string mensagem = "";
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
                        DELETE FROM {0}.{1}.CarteiraProjeto WHERE CodigoCarteira = {2};"
                        , bancodb, Ownerdb, codigoCarteira);

            comandoSQL = "";
            int count = 0;
            foreach (string codigoProjeto in arrayProjetosFilhos)
            {
                if (codigoProjeto != "")
                {
                    // Insere um registro na tabela CarteiraProjeto
                    comandoSQL += string.Format(@"
                        INSERT INTO {0}.{1}.CarteiraProjeto (CodigoCarteira, CodigoProjeto, IndicaCarteiraPrincipal)
                        VALUES({2}, {3}, '{4}');"
                           , bancodb, Ownerdb, codigoCarteira, codigoProjeto, IndicaCarteiraPrincipal[count]);
                }
                count++;
            }

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



    public bool incluiProjetoSelecionadosCarteira(string[] arrayProjetosFilhos, string codigoCarteira)
    {
        bool retorno = false;
        string comandoSQL = "";
        string mensagem = "";
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
                        DELETE FROM {0}.{1}.CarteiraProjeto WHERE CodigoCarteira = {2};"
                        , bancodb, Ownerdb, codigoCarteira);

            foreach (string codigoProjeto in arrayProjetosFilhos)
            {
                if (codigoProjeto != "")
                {

                    char corte = '&';

                    string codigoProjetoString = codigoProjeto.Split(corte)[0];
                    string carteiraPrincipalProjeto = codigoProjeto.Split(corte)[1];

                    // Insere um registro na tabela CarteiraProjeto
                    comandoSQL += string.Format(@"
                        INSERT INTO {0}.{1}.CarteiraProjeto (CodigoCarteira, CodigoProjeto, IndicaCarteiraPrincipal)
                        VALUES({2}, {3}, '{4}');"
                           , bancodb, Ownerdb, codigoCarteira, codigoProjetoString, carteiraPrincipalProjeto);

                }
            }

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


    public bool excluirProjetoSelecionadosCarteira(string[] arrayProjetosFilhos, string codigoCarteira)
    {
        bool retorno = false;
        string comandoSQL = "";
        string mensagem = "";
        int registrosAfetados = 0;
        try
        {
            foreach (string codigoProjeto in arrayProjetosFilhos)
            {
                if (codigoProjeto != "")
                {
                    // Exclui um registro na tabela CarteiraProjeto
                    comandoSQL += string.Format(@"
                        DELETE FROM {0}.{1}.CarteiraProjeto WHERE CodigoCarteira = {2} and CodigoProjeto = {3};", bancodb, Ownerdb, codigoCarteira, codigoProjeto);
                }
            }
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

    public bool excluiCarteira(string codigoCarteira)
    {
        int registrosAfetados = 0;
        bool retorno = false;
        string mensagem = "";
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN 
	                    DECLARE @CodigoCarteira int,
					            @CodigoTipoAssociacaoCarteira int
					
	                    SET @CodigoCarteira = {2}
	
	                    SELECT @CodigoTipoAssociacaoCarteira = CodigoTipoAssociacao
	                      FROM {0}.{1}.TipoAssociacao
	                     WHERE IniciaisTipoAssociacao = 'CP'
	 
	                    DELETE FROM {0}.{1}.ModeloStatusReportObjeto 
	                     WHERE CodigoObjeto = @CodigoCarteira
	                       AND CodigoTipoAssociacaoObjeto = @CodigoTipoAssociacaoCarteira
	
	                    DELETE FROM {0}.{1}.CarteiraProjeto WHERE CodigoCarteira = @CodigoCarteira
                
                        DELETE FROM {0}.{1}.Carteira WHERE CodigoCarteira = @CodigoCarteira
	
                    END",
                    bancodb, Ownerdb, codigoCarteira);
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



    public bool excluiProjetoCarteira(string codigoCarteira, string codigoProjeto)
    {
        int registrosAfetados = 0;
        bool retorno = false;
        string mensagem = "";
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"DELETE FROM {0}.{1}.CarteiraProjeto where CodigoCarteira = {2} and CodigoProjeto = {3}",
                    bancodb, Ownerdb, codigoCarteira, codigoProjeto);
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

    public bool atualizaCarteiraDoProjeto(string NomeCarteira, string DescricaoCarteira, string indicaCarteiraAtiva, string codigoCarteira)
    {
        bool retorno = false;
        string mensagem = "";
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                BEGIN
                 -- begin tran T1

                  UPDATE {0}.{1}.Carteira 
                     SET NomeCarteira = '{2}',
                         DescricaoCarteira = '{3}',
                         IndicaCarteiraAtiva = '{4}' 
                   WHERE CodigoCarteira = {5}
                   
                END
                ", bancodb, Ownerdb, NomeCarteira, DescricaoCarteira, indicaCarteiraAtiva, codigoCarteira);

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


    public bool atualizaCarteiraDoProjetoPorProjeto(string NomeCarteira, string DescricaoCarteira, string indicaCarteiraAtiva, string codigoCarteira)
    {
        bool retorno = false;
        string mensagem = "";
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                BEGIN
                 -- begin tran T1

                  UPDATE {0}.{1}.Carteira 
                     SET NomeCarteira = '{2}',
                         DescricaoCarteira = '{3}',
                         IndicaCarteiraAtiva = '{4}' 
                   WHERE CodigoCarteira = {5}
                   
                END
                ", bancodb, Ownerdb, NomeCarteira, DescricaoCarteira, indicaCarteiraAtiva, codigoCarteira);

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

    public DataSet getDemandasRelacionadas(string where)
    {

        string comandoSQL = string.Format(@"
                select p.NomeProjeto as Demanda, s.DescricaoStatus as Status
                FROM {0}.{1}.Projeto p, {0}.{1}.Status s 
                where s.CodigoStatus = p.CodigoStatusProjeto 
                {2} 
                ORDER BY p.NomeProjeto
                ", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region _Projetos - Administracao - DadosIndicadoresOperacional.aspx

    public DataSet getDadoOperacional(int codigoEntidade)
    {
        comandoSQL = string.Format(@"
            SELECT CodigoDado
                ,DescricaoDado
                ,GlossarioDado
                ,d.CodigoUnidadeMedida
                ,CasasDecimais
                ,CodigoFuncaoAgrupamentoDado
                ,CodigoUsuarioInclusao
                ,tum.SiglaUnidadeMedida
                ,d.CodigoReservado
            FROM {0}.{1}.DadoOperacional d 
                INNER JOIN {0}.{1}.TipoUnidadeMedida tum 
                    ON tum.CodigoUnidadeMedida = d.CodigoUnidadeMedida
            WHERE DataExclusao is NULL  
                and CodigoEntidade = {2}                                      
            ORDER BY DescricaoDado
            ", bancodb, Ownerdb, codigoEntidade);

        return getDataSet(comandoSQL);
    }

    public DataSet incluiDadoIndicadoresOperacional(string descricaoDado, string glossarioDado, string codigoUnidadeMedida,
                                                    string casasDecimais, string CodigoAgrupamentoDado,
                                                    string CodigoUsuarioInclusao, string codigoEntidad, string codigoReservado)
    {
        comandoSQL = string.Format(@"
                DECLARE @codigoDado int

                 INSERT INTO {0}.{1}.DadoOperacional (DescricaoDado, GlossarioDado, CodigoUnidadeMedida, CasasDecimais,CodigoFuncaoAgrupamentoDado, DataInclusao,CodigoUsuarioInclusao,DadoExclusivoProjeto,IndicaDadoControladoSistema,CampoResumoProjeto,CodigoEntidade,CodigoReservado)
                                              VALUES ('{2}'        ,         '{3}',                 {4},          {5},                         {6},    GETDATE(),                  {7},                 'N',                        'N',                '',           {8},            {9})

                SET @codigoDado = scope_identity()

                SELECT @codigoDado AS codigoDado
                ", bancodb, Ownerdb, descricaoDado, glossarioDado, codigoUnidadeMedida, casasDecimais,
                   CodigoAgrupamentoDado, CodigoUsuarioInclusao, codigoEntidad, codigoReservado);

        return getDataSet(comandoSQL);
    }

    public bool atualizaDadoIndicadoresOperacional(string descricaoDado, string glossarioDado, string codigoUnidadeMedida,
                                                   string casasDecimais, string CodigoAgrupamentoDado, string CodigoDado, string codigoReservado)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                UPDATE {0}.{1}.DadoOperacional
                    SET DescricaoDado                = '{2}'
                        ,GlossarioDado               = '{3}'
                        ,CodigoUnidadeMedida         = {4}
                        ,CasasDecimais               = {5}
                        ,CodigoFuncaoAgrupamentoDado = {6}
                        ,CodigoReservado             = {8}
                WHERE CodigoDado = {7}
                ", bancodb, Ownerdb, descricaoDado, glossarioDado, codigoUnidadeMedida, casasDecimais,
                   CodigoAgrupamentoDado, CodigoDado, codigoReservado);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool excluiDadoIndicadoresOperacional(string CodigoDado, string codigoUsuario)
    {
        try
        {
            comandoSQL = string.Format(@"
                BEGIN
                    UPDATE {0}.{1}.DadoOperacional 
                    SET
                        DataExclusao=GETDATE(),
                        CodigoUsuarioExclusao={3}

                    WHERE CodigoDado = {2}

                    --UPDATE {0}.{1}.DadoUnidade
                    --SET DataExclusao=GETDATE()
                    --    ,CodigoUsuarioExclusao={3}
                    --WHERE CodigoDado = {2}
                END
                ", bancodb, Ownerdb, CodigoDado, codigoUsuario);
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

    #region _Projetos - Administracao - IndicadoresOperacional.aspx

    public DataSet getIndicadoresOperacional(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT  
                    ind.CodigoIndicador
                    ,ind.NomeIndicador
                    ,ind.CodigoUnidadeMedida
                    ,tum.SiglaUnidadeMedida 
                    ,ind.GlossarioIndicador
                    ,ind.CasasDecimais
                    ,ind.Polaridade
                    ,ind.CodigoUsuarioResponsavel
                    ,ind.CodigoUnidadeResponsavel
                    ,ind.IndicaMetaDesdobravel
                    ,ind.CodigoGrupoIndicador
                    ,ind.NomeAvaliado
                    ,ind.CodigoFuncaoAgrupamentoMeta
                    ,ind.IndicaApuracaoViaResultado
                    ,tfd.NomeFuncao as NomeFuncaoAgrupamentoMeta
                    ,IndicaCriterio
                    ,CASE WHEN EXISTS(SELECT 1 FROM DesdobramentoMetaOperacional dm INNER JOIN MetaOperacional m on m.CodigoMetaOperacional = dm.CodigoMetaOperacional  WHERE m.CodigoIndicador = ind.CodigoIndicador) OR
							   EXISTS(SELECT 1 FROM ResultadoMetaOperacional r INNER JOIN MetaOperacional m ON m.CodigoMetaOperacional = r.CodigoMetaOperacional WHERE m.CodigoIndicador = ind.CodigoIndicador)
						THEN 'N'
						ELSE 'S'
					 END PermitirAlteracaoCampos
                    , us.NomeUsuario as ResponsavelObjeto
                    , ind.TipoIndicador 
                    , CASE WHEN ind.TipoIndicador = 'O' THEN 'Operacional'
                           WHEN ind.TipoIndicador = 'D' THEN 'Desempenho' 
                           ELSE 'Não informado'
                      END DescTipoIndicador   
                    , tfd.NomeFuncaoBD
                    , ind.DataInicioValidadeMeta
                    , ind.DataTerminoValidadeMeta
                    , ind.IndicaAcompanhaMetaVigencia
                FROM {0}.{1}.IndicadorOperacional AS ind
                LEFT JOIN  {0}.{1}.Usuario as us on ind.CodigoUsuarioResponsavel = us.CodigoUsuario  
                LEFT JOIN  {0}.{1}.TipoFuncaoDado as tfd on tfd.CodigoFuncao = ind.CodigoFuncaoAgrupamentoMeta
                LEFT JOIN  {0}.{1}.TipoUnidadeMedida tum on tum.CodigoUnidadeMedida = ind.CodigoUnidadeMedida     
               WHERE ind.DataExclusao IS NULL
                 AND ind.CodigoIndicador IS NOT NULL
                 AND ind.CodigoEntidade = {2} 
                 AND ind.DataExclusao IS NULL
                 AND ind.IndicadorExclusivoProjeto <> 'S'
                 AND ind.IndicaIndicadorControladoSistema <> 'S'
                 {3}
            ORDER BY ind.NomeIndicador
                    ", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }




    public DataSet getDadosOperacionalGrid(string codigoUnidade)
    {
        string comandoSQL = string.Format(@"select * from {0}.{1}.f_GetDadosIndicadorOperacional({2})", bancodb, Ownerdb, codigoUnidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getIndicadoresOperacionaisDisponiveisProjeto(int codigoProjeto, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"SELECT indO.NomeIndicador, indO.CodigoIndicador 
                                              FROM {0}.{1}.IndicadorOperacional indO
                                             WHERE indO.CodigoEntidade = {3}
                                               AND indO.DataExclusao IS NULL 
                                             ORDER BY indO.NomeIndicador", bancodb, Ownerdb, codigoProjeto, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getDadosIndicadorOperacional(int codigoIndicador, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
                            SELECT i.CodigoIndicador, i.NomeIndicador, i.CodigoUnidadeMedida, i.GlossarioIndicador
                                  ,i.CasasDecimais, i.Polaridade, i.CodigoUsuarioResponsavel, tum.SiglaUnidadeMedida
                                  ,u.NomeUsuario, tfd.NomeFuncao AS Agrupamento
                             FROM {0}.{1}.IndicadorOperacional i 
                                  INNER JOIN {0}.{1}.TipoUnidadeMedida                          AS tum
                                          ON tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida 
                                  INNER JOIN {0}.{1}.Usuario                                    AS u
                                          ON u.CodigoUsuario = i.CodigoUsuarioResponsavel 
                                  LEFT JOIN {0}.{1}.TipoFuncaoDado                             AS tfd
                                          ON tfd.CodigoFuncao = i.CodigoFuncaoAgrupamentoMeta 
                            WHERE i.CodigoEntidade = {3}
                            AND i.DataExclusao IS NULL
                            AND i.CodigoIndicador = {2} ", bancodb, Ownerdb, codigoIndicador, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public bool incluiDadoOperacionalGrid(string nomeDado, string codigoUsuarioLogado, string codigoEntidadLogada)
    {
        bool retorno = false;
        int regAfetados = 0;
        string comandoSQL = "";

        try
        {
            comandoSQL = string.Format(@"
                 INSERT INTO {0}.{1}.DadoOperacional (DescricaoDado, CodigoUnidadeMedida, DataInclusao, CodigoUsuarioInclusao, CodigoFuncaoAgrupamentoDado, CodigoEntidade)
                 VALUES ('{2}', 1, GETDATE(), {3}, 1, {4})
                ", bancodb, Ownerdb, nomeDado, codigoUsuarioLogado, codigoEntidadLogada);

            execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }
        return retorno;
    }

    public bool incluiIndicadorOperacional(string nomeIndicador, string responsavelIndicador,
                                          string unidadeMedida, string polaridade, string codigoUnidade, string usuarioLogado,
                                          string glossarioIndicador, string cassasDecimais, string codigoAgrupamentoMeta,
                                          string criterio, string tipoIndicador, string grupo, string unidade, string avaliado, string metaDesdobravel, string desempenhoComparaMetaResultado, ref int novoCodigoIndicador)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
                BEGIN

                DECLARE @CodigoIndicador int
                DECLARE @DataInclusão datetime
                
                set @DataInclusão = getDate()

                INSERT INTO {0}.{1}.IndicadorOperacional ( 
                              NomeIndicador                     -- 2
                            , CodigoUnidadeMedida               -- 3
                            , GlossarioIndicador                -- 4
                            , CasasDecimais                     -- 5
                            , Polaridade                        -- 6
                            , CodigoUsuarioResponsavel          -- 7
                            , DataInclusao                      -- GETDATE()
                            , CodigoUsuarioInclusao             -- 8
                            , CodigoFuncaoAgrupamentoMeta       -- 9
                            , IndicadorExclusivoProjeto         -- N
                            , IndicaIndicadorControladoSistema  -- N
                            , CodigoEntidade                    -- 10
                            , IndicaCriterio                    -- 11
                            , TipoIndicador                     -- 12
                            , CodigoGrupoIndicador              -- 13
                            , CodigoUnidadeResponsavel          -- 14
                            , NomeAvaliado                      -- 15
                            , IndicaMetaDesdobravel             -- 16
                            , IndicaApuracaoViaResultado        -- 17
                            )


                VALUES ( 
                            '{2}'
                            , {3}
                            , '{4}'
                            , {5}
                            , '{6}'
                            , {7}
                            , @DataInclusão
                            , {8}
                            , {9}
                            , 'N'
                            , 'N'
                            , {10}
                            , '{11}'
                            , '{12}'
                            , {13}
                            , {14}
                            , '{15}'
                            , '{16}'
                            , '{17}'
                )




                SELECT @CodigoIndicador = codigoIndicador from {0}.{1}.IndicadorOperacional
                WHERE   NomeIndicador                     =  '{2}'
                            and CodigoUnidadeMedida         =  {3}
                            and  CasasDecimais               = {5}
                            and  Polaridade                  = '{6}'
                            and  CodigoUsuarioResponsavel    = {7}
                            and  DataInclusao                = @DataInclusão
                            and  CodigoUsuarioInclusao       = {8}
                            and  IndicadorExclusivoProjeto   = 'N'
                            and  IndicaIndicadorControladoSistema = 'N'
                            and  CodigoEntidade              = {10}
                            and  IndicaCriterio              = '{11}'
                            and  TipoIndicador               = '{12}'

                --SET @CodigoIndicador = scope_identity()

                SELECT @CodigoIndicador AS CodigoIndicador

                END
                ", bancodb, Ownerdb, nomeIndicador, unidadeMedida
                 , glossarioIndicador, cassasDecimais, polaridade
                 , responsavelIndicador, usuarioLogado
                 , codigoAgrupamentoMeta, codigoUnidade, criterio, tipoIndicador
                 , grupo, unidade, avaliado, metaDesdobravel, desempenhoComparaMetaResultado);


            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigoIndicador = int.Parse(ds.Tables[0].Rows[0]["CodigoIndicador"].ToString());

            retorno = true;
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);

        }
        return retorno;
    }

    public bool atualizaIndicadorOperacional(string codigoIndicador, string nomeIndicador, string responsavelIndicador,
                                             string unidadeMedida, string polaridade, string usuarioLogado,
                                             string CasasDecimais, string glossarioIndicador, string codigoAgrupamentoMeta, string criterio, string tipoIndicador,
                                             string grupo, string unidade, string avaliado, string metaDesdobravel, string desempenhoComparaMetaResultado)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                BEGIN
                UPDATE {0}.{1}.IndicadorOperacional 
                    SET NomeIndicador               = '{2}'
                    , CodigoUnidadeMedida           = {3}
                    , GlossarioIndicador            = '{7}'
                    , CasasDecimais                 = {8}
                    , Polaridade                    = '{4}'
                    , CodigoUsuarioResponsavel      = {5}
                    , CodigoFuncaoAgrupamentoMeta   = {9}
                    , IndicaCriterio                = '{10}'
                    , TipoIndicador                 = '{11}'
                    , CodigoGrupoIndicador          = {12}
                    , CodigoUnidadeResponsavel      = {13}
                    , NomeAvaliado                  = '{14}'
                    , IndicaMetaDesdobravel         = '{15}'
                    , IndicaApuracaoViaResultado    = '{16}'



                WHERE CodigoIndicador = {6}

                END
              ", bancodb, Ownerdb, nomeIndicador, unidadeMedida, polaridade
               , responsavelIndicador, codigoIndicador
               , glossarioIndicador, CasasDecimais, codigoAgrupamentoMeta, criterio, tipoIndicador
               , grupo, unidade, avaliado, metaDesdobravel, desempenhoComparaMetaResultado);


            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool atualizaIndicadorOperacionalFormula(string codigoIndicador, string formulaIndicador)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                UPDATE {0}.{1}.IndicadorOperacional 
                    SET FormulaIndicador    = '{2}'
                WHERE CodigoIndicador       = {3}
              ", bancodb, Ownerdb, formulaIndicador, codigoIndicador);

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool incluiAutomaticaDadoAoIndicadorOperacional(string codigoIndicador, string codigoDado, string codigoFuncaoDadoIndicador, string criterioIndicador)
    {
        string comandoSQL = "";
        int regAfetados = 0;
        bool respuesta = true;

        try
        {
            if (criterioIndicador == "S")
                comandoSQL = string.Format(@"
                INSERT INTO {0}.{1}.DadoIndicadorOperacional (codigoIndicador, codigoDado, CodigoFuncaoDadoIndicador) 
                VALUES ({2}, {3}, NULL)
                ", bancodb, Ownerdb, codigoIndicador, codigoDado, codigoFuncaoDadoIndicador);
            else
                comandoSQL = string.Format(@"
                INSERT INTO {0}.{1}.DadoIndicadorOperacional (codigoIndicador, codigoDado, CodigoFuncaoDadoIndicador) 
                VALUES ({2}, {3}, {4})
                ", bancodb, Ownerdb, codigoIndicador, codigoDado, codigoFuncaoDadoIndicador);

            execSQL(comandoSQL, ref regAfetados);
        }
        catch
        {
            respuesta = false;
        }

        return respuesta;
    }

    public bool atualizaAutomaticaDadoAoIndicadorOperacional(string codigoIndicador, string descricaoDado, string glossario
                                                            , string codigoUnidadeMedida, string cassasDecimais
                                                            , string codigoAgrupamentoMeta, string criterioIndicador)
    {
        string comandoSQL = "";
        int regAfetados = 0;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"
                BEGIN

                DECLARE @CodigoDado  INT

                SET @CodigoDado = (SELECT CodigoDado FROM {0}.{1}.DadoIndicadorOperacional WHERE CodigoIndicador = {2})

                UPDATE {0}.{1}.DadoOperacional
                SET DescricaoDado                = '{3}'
                    ,GlossarioDado               = '{4}'
                    ,CodigoUnidadeMedida         = {5}
                    ,CasasDecimais               = {6}
                    ,CodigoFuncaoAgrupamentoDado = {7}
                WHERE CodigoDado = @CodigoDado

                UPDATE  {0}.{1}.IndicadorOperacional 
                SET     FormulaIndicador    = '[' + CONVERT(VarChar, @CodigoDado) + ']'
                WHERE   CodigoIndicador     = {2}



                END
            ", bancodb, Ownerdb, codigoIndicador, descricaoDado, glossario, codigoUnidadeMedida, cassasDecimais, codigoAgrupamentoMeta);

            if (criterioIndicador == "S")
                comandoSQL += string.Format(@"
                UPDATE  {0}.{1}.DadoIndicadorOperacional 
                SET     CodigoFuncaoDadoIndicador    = NULL
                WHERE   
                        CodigoIndicador     = {2}
                  AND   CodigoDado          = @CodigoDado
                ", bancodb, Ownerdb, codigoIndicador);
            else
                comandoSQL += string.Format(@"
                UPDATE  {0}.{1}.DadoIndicadorOperacional 
                SET     CodigoFuncaoDadoIndicador    = {3}
                WHERE   
                        CodigoIndicador     = {2}
                  AND   CodigoDado          = @CodigoDado
                ", bancodb, Ownerdb, codigoIndicador, codigoAgrupamentoMeta);

            execSQL(comandoSQL, ref regAfetados);
        }
        catch
        {
            retorno = false;
        }

        return retorno;
    }

    public bool excluiIndicadorOperacional(string CodigoIndicador, string CodigoUsuario)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
            UPDATE {0}.{1}.IndicadorOperacional
            SET
                DataExclusao = GETDATE(),
                CodigoUsuarioExclusao = {2}
            WHERE CodigoIndicador = {3}

            --UPDATE {0}.{1}.IndicadorUnidade
            --SET
            --    DataExclusao = GETDATE(),
            --    CodigoUsuarioExclusao = {2}
            --WHERE CodigoIndicador = {3}
            ", bancodb, Ownerdb, CodigoUsuario, CodigoIndicador);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }


    #endregion

    #region _Projeto - Administracao - ParcelasContrato.aspx

    public DataSet getParcelasDoContrato(int codigoUnidade, int codigoUsuario, string where, string orderBy)
    {
        string comandoSQL = string.Format(@"
                DECLARE @CodigoEntidade int

                SET @CodigoEntidade = {2}

                DECLARE @diasParcelasVencendo Int 
                SELECT @diasParcelasVencendo = CAST(pcs.[Valor] AS Int) FROM {0}.{1}.[ParametroConfiguracaoSistema] AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoEntidade AND pcs.[Parametro] = 'diasParcelasVencendo'
                IF (@diasParcelasVencendo IS NULL)
    		        SET @diasParcelasVencendo = 10

                SELECT  cont.NumeroContrato
                    ,   pcont.CodigoContrato
                    ,   pcont.NumeroAditivoContrato
			 	    ,   pes.[NomePessoa]                AS Fornecedor
                    ,   cont.DescricaoObjetoContrato    AS Objeto
                    ,   cont.CodigoUsuarioResponsavel   AS idResponsavel
                    ,   usu.NomeUsuario
                    ,   pcont.NumeroParcela
                    ,   pcont.ValorPrevisto
                    ,   pcont.DataVencimento
                    ,   pcont.DataPagamento
                    ,   pcont.ValorPago
                    ,   HistoricoParcela
                    ,   ISNULL(lf.CodigoLancamentoFinanceiro, -1) as CodigoLancamentoFinanceiro
	                , CAST( CASE  WHEN pcont.[DataPagamento]	 IS NOT NULL				                THEN 'Paga'
		                        WHEN pcont.[DataVencimento] IS NULL						                    THEN 'Sem Vencimento' 
					            WHEN DATEADD(DD,1,pcont.[DataVencimento])<GETDATE()		                    THEN 'Atrasada'
					            WHEN DATEDIFF(DD, GETDATE(), pcont.[DataVencimento])>@diasParcelasVencendo	THEN 'A Vencer'
					            ELSE											                                 'Vencendo' 
                          END AS Varchar(20)) AS [SituacaoParcela]
                FROM        {0}.{1}.ParcelaContrato AS pcont
                INNER JOIN  {0}.{1}.Contrato        AS cont ON (    cont.CodigoContrato             =   pcont.CodigoContrato        )
                INNER JOIN  {0}.{1}.UnidadeNegocio  AS unid ON (    cont.CodigoUnidadeNegocio       =   unid.CodigoUnidadeNegocio   )
                INNER JOIN  {0}.{1}.Usuario         AS usu  ON (    cont.CodigoUsuarioResponsavel   =   usu.CodigoUsuario           )
    			LEFT JOIN	{0}.{1}.[Pessoa]		AS pes  ON (    pes.[CodigoPessoa]              = cont.[CodigoPessoaContratada] )
                LEFT JOIN   {0}.{1}.LancamentoFinanceiro lf ON (    lf.CodigoObjetoAssociado = pcont.SequencialParcela and 
                                                                    dbo.f_GetIniciaisTipoAssociacao(lf.CodigoTipoAssociacao) = 'PA')
                
                WHERE   pcont.[DataExclusao] IS NULL 
                    AND cont.StatusContrato               = 'A' 
                    AND unid.[DataExclusao]               IS NULL 
                    AND unid.[IndicaUnidadeNegocioAtiva]  = 'S' 
                    AND unid.[CodigoEntidade]             = @CodigoEntidade
                    AND cont.[CodigoContrato] IN ( SELECT f.[CodigoContrato] FROM {0}.{1}.f_GetContratosUsuario({3},{2}) AS [f] ) 
                  {4}

                {5}
                ", bancodb, Ownerdb, codigoUnidade, codigoUsuario, where, orderBy);
        return getDataSet(comandoSQL);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataPagamento">formato europeo: dd/MM/yyyy</param>
    /// <param name="valorPago"></param>
    /// <param name="historicoParcela"></param>
    /// <param name="idUsuarioEdicao"></param>
    /// <param name="chaveContrato"></param>
    /// <param name="chaveAditivo"></param>
    /// <param name="chaveParcela"></param>
    /// <returns></returns>
    public bool atualizaParcelasDoContrato(string dataPagamento, string valorPago, string historicoParcela
                                            , int idUsuarioEdicao
                                            , int chaveContrato, int chaveAditivo, int chaveParcela)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                                UPDATE {0}.{1}.ParcelaContrato
                                SET
                                      DataPagamento     = CONVERT(DateTime, '{2}', 103)
                                    , ValorPago         = {3}
                                    , HistoricoParcela  = {4}
                                    , DataUltimaAlteracao           = GETDATE()
                                    , CodigoUsuarioUltimaAlteracao  = {5}

                                WHERE   CodigoContrato          = {6}
                                  AND   NumeroParcela           = {7}
                                  AND   NumeroAditivoContrato   = {8}
                                ", bancodb, Ownerdb
                                 , dataPagamento
                                 , valorPago != "" ? valorPago.Replace(",", ".") : "NULL"
                                 , historicoParcela != "" ? "'" + historicoParcela.Replace("'", "''") + "'" : "NULL"
                                 , idUsuarioEdicao
                                 , chaveContrato, chaveParcela, chaveAditivo
                                 );

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Projetos - Lições Aprendidas

    public DataSet getLicoesAprendidas(int codigoEntidade, string where)
    {
        DataSet ds = new DataSet();
        string comandoSQL = string.Format(@"
        SELECT li.CodigoLicaoAprendida as cla,
               li.DataLicaoAprendida as data,
               tal.DescricaoAssuntoLicaoAprendida as assunto,
	           li.LicaoAprendida as licao,
	           CASE WHEN li.TipoLicaoAprendida = 'N' THEN 'Negativa' ELSE 'Positiva' end as tipo,
	           p.NomeProjeto as projeto,
	           u.NomeUsuario as IncluidoPor
          FROM {0}.{1}.LicaoAprendida li
    INNER JOIN {0}.{1}.TipoAssuntoLicaoAprendida tal on (tal.CodigoAssuntoLicaoAprendida = li.CodigoAssuntoLicaoAprendida)
    INNER JOIN {0}.{1}.Projeto p on (p.CodigoProjeto = li.CodigoObjetoAssociado)
    INNER JOIN {0}.{1}.TipoProjeto tp on (tp.CodigoTipoProjeto = p.CodigoTipoProjeto) 
    INNER JOIN {0}.{1}.Usuario u on (u.CodigoUsuario = li.CodigoUsuarioInclusao) 
    INNER JOIN {0}.{1}.Formulario f on (f.CodigoFormulario = li.CodigoFormulario)
         WHERE li.CodigoEntidade = {2} {3}", bancodb, Ownerdb, codigoEntidade, where);
        ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getAssuntoLicaoAprendida(int codigoEntidade, string where)
    {
        DataSet ds = new DataSet();
        string comandoSQL = string.Format(@"
        SELECT tal.CodigoAssuntoLicaoAprendida,
               tal.DescricaoAssuntoLicaoAprendida,
               tal.IndicaControladoSistema
          FROM {0}.{1}.TipoAssuntoLicaoAprendida tal 
         WHERE tal.CodigoEntidade = {3} {2}", bancodb, Ownerdb, where, codigoEntidade);
        ds = getDataSet(comandoSQL);
        return ds;
    }

    public bool atualizaAssuntoLicaoAprendida(int codigoTipoAssunto, string descricaoAssuntoLicao, ref string msgErro)
    {
        string comandoSQL = string.Format(@"
        UPDATE {0}.{1}.[TipoAssuntoLicaoAprendida]
           SET DescricaoAssuntoLicaoAprendida = '{2}'
         WHERE CodigoAssuntoLicaoAprendida = {3}", bancodb, Ownerdb, descricaoAssuntoLicao, codigoTipoAssunto);
        int regAfetados = 0;
        bool retorno = false;
        try
        {
            retorno = execSQL(comandoSQL, ref regAfetados);

        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = ex.Message;
        }

        return retorno;
    }

    #endregion

    #region espacoTrabalho - frameEspacoTrabalho_MinhaTarefas

    public DataSet getMinhasTarefasLogado(int codigoProjeto, int codigoUsuarioLogado, int codigoEntidade, string where)
    {
        //Alterado by Alejandro : 19/07/2010
        comandoSQL = string.Format(@"
            SELECT  * 
            FROM    {0}.{1}.f_GetTarefasToDoListProjeto({4}, {2}, {3}) AS fun
            WHERE   1=1 
              {5}
	    ", bancodb, Ownerdb, codigoProjeto, codigoUsuarioLogado, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getTarefasAtualizacao(int codigoUsuarioLogado, int codigoEntidade, string where)
    {
        //Alterado by Alejandro : 19/07/2010
        comandoSQL = string.Format(@"
          SELECT CodigoTarefa
                ,NomeTarefa
                ,NomeProjeto
                ,Trabalho
                ,TrabalhoPrevisto
                ,ISNULL(TrabalhoRestanteInformado, TrabalhoRestante) AS TrabalhoRestante
                ,ISNULL(TrabalhoRealInformado, TrabalhoReal) AS TrabalhoRealTotal
                ,Inicio
                ,Termino
                ,PercentualConcluido AS PercConcluido
                ,CASE WHEN IndicaTarefaAtrasada = 'S' THEN 'Sim' ELSE 'Não' END AS IndicaAtrasada
                ,CASE WHEN PercentualConcluido = 100 THEN 'Sim' ELSE 'Não' END AS IndicaConcluida
                ,CASE WHEN IndicaTarefaCritica = 'S' THEN 'Sim' ELSE 'Não' END AS IndicaCritica
                ,CASE WHEN IndicaTarefaMarco = 'S' THEN 'Sim' ELSE 'Não' END AS IndicaMarco
                ,IndicaAnexoAtribuicao AS IndicaAnexo
                ,StatusAtribuicao AS StatusAprovacao
                ,CASE WHEN IndicaToDoList = 'S' THEN 'T' ELSE 'P' END AS TipoTarefa
                ,CodigoAtribuicao
                ,TarefaSuperior
           FROM {0}.{1}.f_GetTarefasUsuario({2}, {3})
          WHERE 1 = 1
            {4}
	    ", bancodb, Ownerdb, codigoUsuarioLogado, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getTimeSheetRecurso(int codigoRecurso, int codigoProjeto, string tipoAtualizacao, string inicioApontamento, string terminoApontamento, string terminoTarefas, char indicaNaoConcluidas, int codigoEntidade, string terminoToDo, string somenteAtrasadas)
    {
        comandoSQL = string.Format(
            @"/*
              BEGIN
                DECLARE @DataInicioParam SmallDateTime,
		                @DataTerminoParam SmallDateTime,
		                @DataTarefasParam SmallDateTime,
		                @DataToDoParam SmallDateTime,
                        @CodigoRecursoParam Int,
                        @IndicaNaoConcluidasParam Char(1),
                        @CodigoProjeto int,
                        @CodigoEntidade int,
                        @TipoAtualizacao Char(2),
                        @SomenteAtrasadas Char(1)

                SET @DataInicioParam  = CONVERT(SmallDateTime, '{3}', 103)
                SET @DataTerminoParam = CONVERT(SmallDateTime, '{4}', 103)
                SET @DataTarefasParam = {5}
                SET @DataToDoParam = {10}
                SET @CodigoRecursoParam = {2}
                SET @IndicaNaoConcluidasParam = '{6}'
                SET @CodigoProjeto = {7}
                SET @TipoAtualizacao = '{8}'
                SET @CodigoEntidade = {9}
                SET @SomenteAtrasadas = '{11}'

                EXEC {0}.{1}.p_TarefasTimeSheetRecurso @DataInicioParam, @DataTerminoParam, @DataTarefasParam, @DataToDoParam, @CodigoRecursoParam, @IndicaNaoConcluidasParam, @CodigoProjeto, @TipoAtualizacao, @CodigoEntidade, @SomenteAtrasadas

             END
             */



BEGIN
    DECLARE
        @DataInicioParam                DATETIME,
        @DataTerminoParam               DATETIME,
        @DataTarefasParam               DATETIME,
        @DataToDoParam                  DATETIME,
        @CodigoRecursoParam             INT,
        @CodigoProjeto                  INT,
        @CodigoEntidade                 INT,
        @TipoAtualizacao                CHAR(2),
        @Status                         VARCHAR(20),
        @PesquisaExibirCardsAtrasados   BIT,
        @PesquisaDataInicio             VARCHAR(10),
        @PesquisaDataFim                VARCHAR(10),
        @PesquisaTexto                  VARCHAR(100),
        @Ordenacao                      VARCHAR(20),
        @Pagina                         INT;

    SET @DataInicioParam  = CONVERT(SmallDateTime, '{3}', 103)
    SET @DataTerminoParam = CONVERT(SmallDateTime, '{4}', 103)
    SET @DataTarefasParam = {5};
    SET @DataToDoParam = {10};
    SET @CodigoRecursoParam = {2};
    SET @CodigoProjeto = {7};
    SET @TipoAtualizacao = '{8}';
    SET @CodigoEntidade = {9};
    SET @Status = 'todo';
    SET @PesquisaExibirCardsAtrasados = {11};
    SET @PesquisaDataInicio = '2018/10/30';
    SET @PesquisaDataFim = '2018/11/29';
    SET @PesquisaTexto = '';
    SET @Ordenacao = 'prioridade';
    SET @Pagina = 0;

      EXEC dbo.p_TarefasTimeSheetRecursoKanbanPesquisaOrdenacao @DataInicioParam
                                , @DataTerminoParam
                                , @DataTarefasParam
                                , @DataToDoParam
                                , @CodigoRecursoParam
                                , @CodigoProjeto
                                , @TipoAtualizacao
                                , @CodigoEntidade
                                , @Status
                                , @PesquisaExibirCardsAtrasados
                                , @PesquisaDataInicio
                                , @PesquisaDataFim
                                , @PesquisaTexto
                                , @Ordenacao
                                , @Pagina;
                                  END"
            , bancodb, Ownerdb, codigoRecurso, inicioApontamento, terminoApontamento
            , terminoTarefas != "" ? string.Format("CONVERT(SmallDateTime, '{0}', 103)", terminoTarefas) : "null"
            , indicaNaoConcluidas, codigoProjeto, tipoAtualizacao, codigoEntidade
            , terminoToDo != "" ? string.Format("CONVERT(SmallDateTime, '{0}', 103)", terminoToDo) : "null"
            , (somenteAtrasadas == "S") == true ? 1 : 0);

        return getDataSet(comandoSQL);
    }

    public DataSet getTarefasAtualizacaoRecurso(int codigoRecurso, int codigoProjeto, string terminoTarefas, char indicaNaoConcluidas, int codigoEntidade)
    {
        comandoSQL = string.Format(
            @"BEGIN
                DECLARE @DataInicioParam SmallDateTime,
		                @DataTerminoParam SmallDateTime,
		                @DataTarefasParam SmallDateTime,
                        @CodigoRecursoParam Int,
                        @IndicaNaoConcluidasParam Char(1),
                        @CodigoProjeto int,
                        @CodigoEntidade int,
                        @TipoAtualizacao Char(2)

                SET @DataInicioParam  = GETDATE() - 6  --CONVERT(SmallDateTime, {3}, 103)
                SET @DataTerminoParam = GETDATE()      --CONVERT(SmallDateTime, {4}, 103)
                SET @DataTarefasParam = {5}
                SET @CodigoRecursoParam = {2}
                SET @IndicaNaoConcluidasParam = '{6}'
                SET @CodigoProjeto = {7}
                SET @TipoAtualizacao = '{8}'
                SET @CodigoEntidade = {9}

                EXEC {0}.{1}.p_TarefasTimeSheetRecurso @DataInicioParam, @DataTerminoParam, @DataTarefasParam, @CodigoRecursoParam, @IndicaNaoConcluidasParam, @CodigoProjeto, @TipoAtualizacao, @CodigoEntidade

             END
"
            , bancodb, Ownerdb, codigoRecurso, "null", "null"
            , terminoTarefas != "" ? string.Format("CONVERT(SmallDateTime, '{0}', 103)", terminoTarefas) : "null"
            , indicaNaoConcluidas, codigoProjeto, "PC", codigoEntidade);

        return getDataSet(comandoSQL);
    }

    public DataSet getTarefasRecursoGantt(int codigoRecurso, int codigoProjeto, int codigoEntidade, string where)
    {
        comandoSQL = string.Format(
            @"BEGIN
	            DECLARE @CodigoRecursoParam Int,
	       	            @CodigoProjetoParam Int,
	       	            @CodigoEntidadeParam Int 
                     
                -- Tabela auxiliar para apresentação dos resultados
                DECLARE @tblRetorno TABLE 
                 (Descricao			Varchar(255),
                  TrabalhoRestante	Decimal(10,2),
                  TrabalhoRealTotal	Decimal(10,2),
                  PercConcluido		Decimal(6,2),
                  Inicio		    SmallDateTime,
                  Termino    		SmallDateTime,
                  Nivel				Tinyint,
                  CodigoProjeto		Int,
                  EstruturaHierarquica Varchar(50), 
                  StatusAprovacao	Char(2))
                  
                SET @CodigoEntidadeParam = {2}
                SET @CodigoProjetoParam = {3}
                SET @CodigoRecursoParam = {4}   
              
	            INSERT INTO @tblRetorno
	            SELECT t.NomeTarefa,
		               IsNull(a.TrabalhoRestanteInformado,a.Trabalho) AS TrabalhoRestante,
		               IsNull(a.TrabalhoRealInformado,0) AS TrabalhoReal,
		               CASE WHEN IsNull(a.TrabalhoRealInformado,0) + ISNULL(a.TrabalhoRestanteInformado,0) > 0 THEN IsNull(a.TrabalhoRealInformado,0) / (IsNull(a.TrabalhoRealInformado,0) + ISNULL(a.TrabalhoRestanteInformado,0))  * 100 ELSE 0 END,
		               a.Inicio,
		               a.Termino,
		               2,
		               cp.CodigoProjeto,
		               Convert(Varchar,cp.CodigoProjeto) + '.' + Convert(Varchar,t.CodigoTarefa),
		               a.StatusAprovacao
	              FROM {0}.{1}.TarefaCronogramaProjeto AS t INNER JOIN
		               {0}.{1}.CronogramaProjeto AS cp ON (cp.CodigoCronogramaProjeto = t.CodigoCronogramaProjeto) INNER JOIN
		               {0}.{1}.AtribuicaoRecursoTarefa AS a ON (a.CodigoCronogramaProjeto = t.CodigoCronogramaProjeto
										            AND a.CodigoTarefa = t.CodigoTarefa) INNER JOIN											
		               {0}.{1}.RecursoCronogramaProjeto AS rcp ON (rcp.CodigoCronogramaProjeto = a.CodigoCronogramaProjeto
										               AND rcp.CodigoRecursoProjeto = a.CodigoRecursoProjeto) INNER JOIN                                           
		               {0}.{1}.RecursoCorporativo AS rc ON (rc.CodigoRecursoCorporativo = rcp.CodigoRecursoCorporativo) 
	             WHERE rc.CodigoUsuario = @CodigoRecursoParam
	               AND cp.CodigoEntidade = @CodigoEntidadeParam
	               AND t.DataExclusao IS NULL
	               AND (cp.CodigoProjeto = @CodigoProjetoParam 
	                OR @CodigoProjetoParam = -1)   
	           UNION
			    SELECT DescricaoTarefa
					  ,EsforcoPrevisto - EsforcoReal
					  ,EsforcoReal
					  ,CASE WHEN IsNull(tdp.EsforcoPrevisto,0) > 0 THEN IsNull(tdp.EsforcoReal,0) / tdp.EsforcoPrevisto  * 100 ELSE 0 END
					  ,InicioPrevisto
					  ,TerminoPrevisto
					  ,2
					  ,CASE WHEN tdp.CodigoProjeto IS NULL THEN 0 ELSE tdp.CodigoProjeto END
					  ,CASE WHEN tdp.CodigoProjeto IS NULL THEN '0' ELSE Convert(Varchar,tdp.CodigoProjeto) END + '.' + Convert(Varchar,tdp.CodigoTarefa)
					  ,'TD'
			      FROM {0}.{1}.TarefaToDoList AS tdp INNER JOIN
					   {0}.{1}.ToDoList AS orig ON (tdp.CodigoToDoList = orig.CodigoToDoList)
			     WHERE orig.CodigoEntidade = @CodigoEntidadeParam	 
	               AND tdp.CodigoUsuarioResponsavelTarefa = @CodigoRecursoParam
	               AND tdp.DataExclusao IS NULL
	               {5}
            		                                   
               
                /* Insere as linhas de projeto */
                INSERT INTO @tblRetorno
                (Descricao,
                 Inicio,
                 Termino,
                 Nivel,
                 EstruturaHierarquica,
                 CodigoProjeto)
                SELECT DISTINCT CASE WHEN p.CodigoProjeto IS NULL THEN 'SEM PROJETO ASSOCIADO' ELSE p.NomeProjeto END,
		               MIN(t.Inicio),
		               MAX(t.Termino),
                       1,
                       Convert(Varchar,p.CodigoProjeto),
                       p.CodigoProjeto
                  FROM @tblRetorno AS t LEFT JOIN
                       {0}.{1}.Projeto AS p ON (p.CodigoProjeto = t.CodigoProjeto) 
                 GROUP BY p.CodigoProjeto, p.NomeProjeto
                                                             
            	
	            SELECT * FROM @tblRetorno
	             ORDER BY EstruturaHierarquica
            END"
            , bancodb, Ownerdb, codigoEntidade, codigoProjeto, codigoRecurso, where);

        return getDataSet(comandoSQL);
    }

    public string getGraficoGanttTarefasRecurso(DataTable dt)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        xml.Append(@"<anygantt>
                                     <datagrid width=""520"">
                                        <columns>       
                                          <column width=""340"" cell_align=""LeftLevelPadding"">
                                            <header>
                                              <text>Descrição</text>
                                            </header>
                                            <format>{%Name}</format>
                                          </column>
                                          <column width=""90"" cell_align=""Center"">
                                            <header>
                                              <text>Início</text>
                                            </header>
                                            <format>{%PeriodStart}{dateTimeFormat:%dd/%MM/%yyyy}</format>
                                          </column>
                                          <column width=""90"" cell_align=""Center"">
                                            <header>
                                              <text>Término</text>
                                            </header>
                                            <format>{%PeriodEnd}{dateTimeFormat:%dd/%MM/%yyyy}</format>
                                          </column>
                                          <column width=""90"" cell_align=""Center"">
                                            <header>
                                              <text>% Concluído</text>
                                            </header>
                                            <format>{%PercConcluido}</format>
                                          </column>
                                          <column width=""90"" cell_align=""Center"">
                                            <header>
                                              <text>Trabalho Real</text>
                                            </header>
                                            <format>{%TrabalhoReal}</format>
                                          </column>
                                          <column width=""100"" cell_align=""Center"">
                                            <header>
                                              <text>Trabalho Restante</text>
                                            </header>
                                            <format>{%TrabalhoRestante}</format>
                                          </column>
                                          <column width=""90"" cell_align=""Center"">
                                            <header>
                                              <text>Trabalho</text>
                                            </header>
                                            <format>{%Trabalho}</format>
                                          </column>
                                        </columns>
                                      </datagrid>
                                     <settings>
                                     <image_export url=""../../AnyChartPNGSaver.aspx"" />
                                     <context_menu save_as_image=""true"" version_info=""false"" print_chart=""false"" about_anygantt=""false"" >
                                        <save_as_image_item_text>Salvar Gráfico como Imagem</save_as_image_item_text>
                                        <print_chart_item_text>Imprimir Gráfico</print_chart_item_text>
                                    </context_menu>
                                         <print columns_on_every_page=""2"">
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
                                    </timeline>
                                      ");

        xml.Append(string.Format(@"<styles>
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
Início: {1}
Término: {2}
Concluído: {3}%
                                      ]]> 
                                      </text>
                                      </tooltip>
                                      </row_datagrid>
                                    <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
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
Início: {1}
Término: {2}
Concluído: {3}%
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                                <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
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
                                    <![CDATA[Ação: {0}
Início: {1}
Término: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                                  </row_datagrid>
                                  <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Ação: {0}
Início: {1}
Término: {2}
                                      ]]> 
                                      </text>
                                      </tooltip>
                                  <actual>		
                            <bar_style>
                                  <labels>

 <label anchor=""Right"" halign=""Far"" valign=""Center"">
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
				        </styles>", "{%Name}", "{%ActualStart}{dateTimeFormat:%dd/%MM/%yyyy}", "{%ActualEnd}{dateTimeFormat:%dd/%MM/%yyyy}", "{%Complete}"));

        xml.Append(@"<project_chart>
                      <tasks>");

        int count = 0;

        foreach (DataRow dr in dt.Rows)
        {
            string inicio = string.Format(@"{0:dd/MM/yyyy} 00:00:00", DateTime.Parse(dr["Inicio"].ToString()));

            string termino = string.Format(@"{0:dd/MM/yyyy} 23:59:59", DateTime.Parse(dr["Termino"].ToString()));

            string percConcluido = dr["Nivel"] + "" == "1" ? "-" : string.Format("{0:n0}%", dr["PercConcluido"] + "" != "" ? float.Parse(dr["PercConcluido"].ToString()) : 0);
            string trabalhoReal = dr["Nivel"] + "" == "1" ? "-" : string.Format("{0:n0}", dr["TrabalhoRealTotal"] + "" != "" ? float.Parse(dr["TrabalhoRealTotal"].ToString()) : 0);
            string trabalhoRestante = dr["Nivel"] + "" == "1" ? "-" : string.Format("{0:n0}", dr["TrabalhoRestante"] + "" != "" ? float.Parse(dr["TrabalhoRestante"].ToString()) : 0);
            string trabalho = dr["Nivel"] + "" == "1" ? "-" : string.Format("{0:n0}", 0);

            xml.Append(string.Format(@" <task id=""{0}"" name=""{1}"" level=""{4}"" actual_start=""{2}"" actual_end=""{3}"" progress=""{5:n0}%"">
                                         <attributes>
                                          <attribute name=""TrabalhoReal"">{6}</attribute>
                                          <attribute name=""TrabalhoRestante"">{7}</attribute>                                          
                                          <attribute name=""Trabalho"">{8}</attribute>
                                          <attribute name=""PercConcluido"">{9}</attribute>
                                        </attributes></task>"
                , count
                , dr["Descricao"].ToString().Replace("\"", "'").Replace("<", "&lt;").Replace(">", "&gt;").Replace("–", "-")
                , inicio
                , termino
                , int.Parse(dr["Nivel"].ToString()) - 1
                , dr["PercConcluido"] + "" == "" ? 0 : float.Parse(dr["PercConcluido"].ToString())
                , trabalhoReal
                , trabalhoRestante
                , trabalho
                , percConcluido
                ));

            count++;
        }

        xml.Append(string.Format(@"</tasks>
                                </project_chart>
                               </anygantt>"));

        return xml.ToString();
    }

    public DataSet getProjetosTimeSheetRecurso(int codigoRecurso, string inicioApontamento, string terminoApontamento, int codigoEntidade)
    {
        comandoSQL = string.Format(
            @"SELECT DISTINCT P.NomeProjeto, P.CodigoProjeto
                FROM {0}.{1}.Projeto AS P INNER JOIN
                     {0}.{1}.f_GetProjetosTimesheetUsuario ({2}, '{3}', '{4}', {5}) f ON f.CodigoProjeto = p.CodigoProjeto
               WHERE P.CodigoEntidade = {5}
               order by P.NomeProjeto", bancodb, Ownerdb, codigoRecurso, inicioApontamento, terminoApontamento, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getAtualizacaoProjetosRecurso(int codigoRecurso, int codigoProjeto, string inicioApontamento, string terminoApontamento, int codigoEntidade)
    {
        comandoSQL = string.Format(
            @"DECLARE @RC int
                DECLARE @in_dataInicioPeriodo datetime
                DECLARE @in_dataTerminoPeriodo datetime
                DECLARE @in_codigoUsuarioTimeSheet int
                DECLARE @in_codigoProjeto int
                DECLARE @in_codigoEntidade int

                SET @in_dataInicioPeriodo = CONVERT(DateTime, '{3}', 103)
                SET @in_dataTerminoPeriodo = CONVERT(DateTime, '{4}', 103)
                SET @in_codigoUsuarioTimeSheet = {2}
                SET @in_codigoProjeto = {5}
                SET @in_codigoEntidade = {6}

                EXECUTE @RC = {0}.{1}.[p_TimeSheetRecursoProjeto] 
                   @in_dataInicioPeriodo
                  ,@in_dataTerminoPeriodo
                  ,@in_codigoUsuarioTimeSheet
                  ,@in_codigoProjeto
                  ,@in_codigoEntidade
                "
            , bancodb, Ownerdb, codigoRecurso, inicioApontamento, terminoApontamento,
            codigoProjeto, codigoEntidade);

        return getDataSet(comandoSQL);
    }

    public DataSet getTipoTarefasTimeSheet(int codigoEntidade, string where)
    {
        //criado by andre cardoso
        comandoSQL = string.Format(@"
            SELECT ttf.CodigoTipoTarefaTimeSheet,
                   ttf.DescricaoTipoTarefaTimeSheet,
                   ttf.CodigoEntidade 
              FROM {0}.{1}.TipoTarefaTimeSheet AS ttf
             WHERE ttf.CodigoEntidade = {2} {3}
	    ", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getTarefasAtualizacaoUsuario(int codigoUsuario, int codigoEntidade, string where)
    {
        comandoSQL = string.Format(
            @"SELECT * 
                FROM {0}.{1}.f_GetTarefasUsuario({2}, {3})
               WHERE 1 = 1
                 {4}"
            , bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public bool atualizaTarefasUsuarioKanban(int codigoAtribuicao, string trabalhoRestante, string trabalhoRealInformado, string inicioReal, string terminoReal, string comentarioRecurso, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                BEGIN
                    UPDATE {0}.{1}.AtribuicaoRecursoTarefa
                        SET 
                             TrabalhoRestanteInformado = {3}
                            ,TrabalhoRealInformado = {4}
                            ,InicioRealInformado = {5}
                            ,TerminoRealInformado = {6}
                            {7}
                            ,DataStatusAprovacao = GETDATE()
                    WHERE CodigoAtribuicao = {2}
                END
                  ", bancodb, Ownerdb, codigoAtribuicao, trabalhoRestante.Replace(',', '.'), trabalhoRealInformado.Replace(',', '.'),
                    (inicioReal == "NULL" ? "NULL" : "CONVERT(DateTime, '" + inicioReal + "', 103)"),
                    (terminoReal == "NULL" ? "NULL" : "CONVERT(DateTime, '" + terminoReal + "', 103)"),
                    comentarioRecurso != null ? ",ComentariosRecurso = '" + comentarioRecurso.Replace("'", "\"") + "'" : "");

            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public DataSet getTarefasAprovacao(int codigoUsuario, int codigoEntidade, string where)
    {
        comandoSQL = string.Format(
            @"SELECT * 
                FROM {0}.{1}.f_GetTarefasAprovacao({2}, {3})
               WHERE 1 = 1
                 {4}"
            , bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getTimeSheetAprovacao(int codigoUsuario, int codigoEntidade)
    {
        comandoSQL = string.Format(
            @"EXEC {0}.{1}.p_TarefasAprovacaoTimeSheet {2}, {3}"
            , bancodb, Ownerdb, codigoUsuario, codigoEntidade);

        return getDataSet(comandoSQL);
    }

    public DataSet getApontamentoAprovacao(int codigoUsuario, int codigoEntidade)
    {
        comandoSQL = string.Format(
            @"EXEC {0}.{1}.p_GetApontamentosAprovacao {2}, {3}"
            , bancodb, Ownerdb, codigoUsuario, codigoEntidade);

        return getDataSet(comandoSQL);
    }

    public DataSet getTimeSheetProjetoAprovacao(int codigoUsuarioAprovador, int codigoRecurso, int codigoProjeto, int codigoEntidade)
    {
        comandoSQL = string.Format(
            @"EXEC {0}.{1}.p_AprovacaoTimeSheetProjeto {2}, {3}, {4}, {5}"
            , bancodb, Ownerdb, codigoUsuarioAprovador, codigoRecurso, codigoProjeto, codigoEntidade);

        return getDataSet(comandoSQL);
    }

    public bool atualizaStatusTarefas(int[] codigoAtribuicao, string status, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            foreach (int codigo in codigoAtribuicao)
            {
                comandoSQL += string.Format(@"
               
                    UPDATE {0}.{1}.AtribuicaoRecursoTarefa
                       SET StatusAprovacao = '{3}',
                           DataStatusAprovacao = GETDATE()                            
                     WHERE CodigoAtribuicao = {2}
                
                  ", bancodb, Ownerdb, codigo, status);
            }

            if (comandoSQL != "")
            {
                execSQL(comandoSQL, ref registrosAfetados);
            }

            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaStatusApontamentos(int[] arrayCodigosAtribuicoes, string status, int codigoUsuarioSistema, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            foreach (int codigoAtribuicao in arrayCodigosAtribuicoes)
            {
                comandoSQL += string.Format(@"
               
                    UPDATE {0}.{1}.ApontamentoAtribuicao
                       SET StatusAnalise = '{3}',
                           DataStatusAnalise = GETDATE(),
                           CodigoUsuarioStatusAnalise = {4}
                     WHERE CodigoApontamentoAtribuicao = {2}
                
                  ", bancodb, Ownerdb, codigoAtribuicao, status, codigoUsuarioSistema);
            }

            if (comandoSQL != "")
            {
                execSQL(comandoSQL, ref registrosAfetados);
            }

            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public string trataDecimal(object item)
    {
        decimal retornodec = 0;
        if (item != null && item.ToString() != "")
        {
            bool r = decimal.TryParse(item.ToString(), out retornodec);
        }
        return retornodec.ToString().Replace(",", ".");
    }

    public bool atualizaTarefa_PC_TS_HistoricoOC(int codigoUsuario, int codigoEntidade, int codigoAtribuicao, string trabalhoRestante, string trabalhoRealInformado, string inicioReal, string terminoReal, string comentarioRecurso, bool salvaStatusAprovacao, DataTable dtDadosInformados, ref string msgError)
    {
        // Foi incluído nesta função o parâmetro "raia" para compatibilizá-la com o Kanban do usuário.
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;
        string insert_outrosCustos = "";
        if (DataTableOk(dtDadosInformados))
        {
            foreach (DataRow item in dtDadosInformados.Rows)
            {
                if ((bool)item["IndicaLinhaAlterada"] == true)
                {
                    insert_outrosCustos += string.Format(@"
            INSERT INTO @outrosCustos (
            [CodigoAtribuicao]           ,[UnidadeAtribuicaoRealInformado] 
           ,[CustoUnitarioRealInformado] ,[CustoRealInformado])
           VALUES({0}, {1}, {2}  ,{3})",
                    item["CodigoAtribuicao"].ToString(), trataDecimal(item["UnidadeAtribuicaoReal"]), 
                    trataDecimal(item["CustoUnitarioReal"]), trataDecimal(item["CustoReal"]));
                }
            }
        }

        // Reproduz a lógica da Procedure [dbo].[p_SalvaTarefasTimeSheetRecursoKanban] para manter compatibilidade com o Kanban do usuário, e só faz apenas se o parâmetro "raia" for não nulo e diferente de vazio (todo, doing ou done).
        try
        {
            comandoSQL = string.Format(@"
            DECLARE @RC int
            DECLARE @CodigoEntidadeContexto int
            DECLARE @CodigoUsuarioSistema int
            DECLARE @codigoAtribuicao bigint
            DECLARE @trabalhoRestante decimal(15,2)
            DECLARE @trabalhoRealInformado decimal(15,2)
            DECLARE @inicioReal datetime
            DECLARE @terminoReal datetime
            DECLARE @comentarioRecurso varchar(2000)
            DECLARE @salvaStatusAprovacao bit
            
            DECLARE @msgError varchar(max)

            SET @CodigoEntidadeContexto = {0}
            SET @CodigoUsuarioSistema  = {1}
            SET @codigoAtribuicao = {2}
            SET @trabalhoRestante = {3}
            SET @trabalhoRealInformado = {4}
            SET @inicioReal = {5}
            SET @terminoReal = {6}
            SET @comentarioRecurso = '{7}'
            SET @salvaStatusAprovacao = {8}
            


              DECLARE @outrosCustos [dbo].[OutrosCustosAtribuicaoType]
             {9}
          

            EXECUTE @RC = [dbo].[p_art_gravaApontamentoUsuario] 
               @CodigoEntidadeContexto
              ,@CodigoUsuarioSistema
              ,@codigoAtribuicao
              ,@trabalhoRestante
              ,@trabalhoRealInformado
              ,@inicioReal
              ,@terminoReal
              ,@comentarioRecurso
              ,@salvaStatusAprovacao
              ,@outrosCustos
              ,@msgError OUTPUT",
              /*{0}*/codigoEntidade,
              /*{1}*/codigoUsuario,
              /*{2}*/codigoAtribuicao,
              /*{3}*/trabalhoRestante.Replace(',', '.'),
              /*{4}*/trabalhoRealInformado.Replace(',', '.'),
              /*{5}*/(inicioReal == "NULL" ? "NULL" : "CONVERT(DateTime, '" + inicioReal + "', 103)"),
              /*{6}*/(terminoReal == "NULL" ? "NULL" : "CONVERT(DateTime, '" + terminoReal + "', 103)"),
              /*{7}*/comentarioRecurso.Replace("'", "\""),
              /*{8}*/salvaStatusAprovacao == true ? 1 : 0,
              /*{9}*/insert_outrosCustos);
            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }

        return retorno;
    }
    public bool atualizaTarefa_PC_TS(int codigoUsuario, int codigoEntidade, int codigoAtribuicao, string trabalhoRestante, string trabalhoRealInformado, string inicioReal, string terminoReal, string comentarioRecurso, bool salvaStatusAprovacao, DataTable dtDadosInformados, ref string msgError)
    {
        // Foi incluído nesta função o parâmetro "raia" para compatibilizá-la com o Kanban do usuário.
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;
        string insert_outrosCustos = "";
        if (DataTableOk(dtDadosInformados))
        {
            foreach (DataRow item in dtDadosInformados.Rows)
            {
                if ((bool)item["IndicaLinhaAlterada"] == true)
                {
                    insert_outrosCustos += string.Format(@"
            INSERT INTO @outrosCustos (
            [CodigoAtribuicao]           ,[UnidadeAtribuicaoRealInformado]  ,[UnidadeAtribuicaoRestanteInformado]
	       ,[CustoUnitarioRealInformado] ,[CustoUnitarioRestanteInformado]  ,[CustoRealInformado] 
           ,[CustoRestanteInformado])
           VALUES({0}, {1}, {2}
                 ,{3}, {4}, {5}
                 ,{6})",
                    item["CodigoAtribuicao"].ToString(), trataDecimal(item["UnidadeAtribuicaoRealInformado"]), trataDecimal(item["UnidadeAtribuicaoRestanteInformado"]),
                    trataDecimal(item["CustoUnitarioRealInformado"]), trataDecimal(item["CustoUnitarioRestanteInformado"]), trataDecimal(item["CustoRealInformado"]),
                    trataDecimal(item["CustoRestanteInformado"]));
                }
            }
        }

        // Reproduz a lógica da Procedure [dbo].[p_SalvaTarefasTimeSheetRecursoKanban] para manter compatibilidade com o Kanban do usuário, e só faz apenas se o parâmetro "raia" for não nulo e diferente de vazio (todo, doing ou done).
        try
        {
            comandoSQL = string.Format(@"
            DECLARE @RC int
            DECLARE @CodigoEntidadeContexto int
            DECLARE @CodigoUsuarioSistema int
            DECLARE @codigoAtribuicao bigint
            DECLARE @trabalhoRestante decimal(15,2)
            DECLARE @trabalhoRealInformado decimal(15,2)
            DECLARE @inicioReal datetime
            DECLARE @terminoReal datetime
            DECLARE @comentarioRecurso varchar(2000)
            DECLARE @salvaStatusAprovacao bit
            
            DECLARE @msgError varchar(max)

            SET @CodigoEntidadeContexto = {0}
            SET @CodigoUsuarioSistema  = {1}
            SET @codigoAtribuicao = {2}
            SET @trabalhoRestante = {3}
            SET @trabalhoRealInformado = {4}
            SET @inicioReal = {5}
            SET @terminoReal = {6}
            SET @comentarioRecurso = '{7}'
            SET @salvaStatusAprovacao = {8}
            


              DECLARE @outrosCustos [dbo].[OutrosCustosAtribuicaoType]
             {9}
          

            EXECUTE @RC = [dbo].[p_art_gravaApontamentoUsuario] 
               @CodigoEntidadeContexto
              ,@CodigoUsuarioSistema
              ,@codigoAtribuicao
              ,@trabalhoRestante
              ,@trabalhoRealInformado
              ,@inicioReal
              ,@terminoReal
              ,@comentarioRecurso
              ,@salvaStatusAprovacao
              ,@outrosCustos
              ,@msgError OUTPUT",
              /*{0}*/codigoEntidade,
              /*{1}*/codigoUsuario,
              /*{2}*/codigoAtribuicao,
              /*{3}*/trabalhoRestante.Replace(',', '.'),
              /*{4}*/trabalhoRealInformado.Replace(',', '.'),
              /*{5}*/(inicioReal == "NULL" ? "NULL" : "CONVERT(DateTime, '" + inicioReal + "', 103)"),
              /*{6}*/(terminoReal == "NULL" ? "NULL" : "CONVERT(DateTime, '" + terminoReal + "', 103)"),
              /*{7}*/comentarioRecurso.Replace("'", "\""),
              /*{8}*/salvaStatusAprovacao == true ? 1 : 0,
              /*{9}*/insert_outrosCustos);
            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }

        return retorno;
    }

    public bool atualizaComentariosTarefa_PC_TS(int codigoAtribuicao, string trabalhoRestante, string trabalhoRealInformado, string inicioReal, string terminoReal, char indicaTS, string comentarios, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                                        BEGIN
                                            DECLARE @PossuiRegistro Char(1)

                                            SELECT @PossuiRegistro = CASE WHEN COUNT(1) > 0 THEN 'S' ELSE 'N' END
                                              FROM {0}.{1}.AtualizacaoTarefa
                                             WHERE CodigoAtribuicao = {2}

                                        if @PossuiRegistro = 'N'
                                            INSERT INTO {0}.{1}.AtualizacaoTarefa
                                                           (CodigoAtribuicao
                                                           ,DataAtualizacao
                                                           ,TrabalhoRestante
                                                           ,StatusAprovacao
                                                           ,DataStatusAprovacao
                                                           ,TrabalhoRealInformado
                                                           ,DataInicioReal
                                                           ,DataTerminoReal
                                                           ,IndicaAtualizacaoTimeSheet
                                                           ,ComentariosRecurso)
                                                     VALUES
                                                           ({2}
                                                           ,GETDATE()
                                                           ,{3}
                                                           ,'PE'
                                                           ,GETDATE()
                                                           ,{4}
                                                           ,{5}
                                                           ,{6}
                                                           ,'{7}'
                                                           ,'{8}')
                                        else
                                            UPDATE {0}.{1}.AtualizacaoTarefa SET DataAtualizacao = GETDATE()
                                                                                 ,TrabalhoRestante = {3}
                                                                                 ,TrabalhoRealInformado = {4}
                                                                                 ,DataInicioReal = {5}
                                                                                 ,DataTerminoReal = {6}
                                                                                 ,ComentariosRecurso = '{8}'
                                            WHERE CodigoAtribuicao = {2}
                                                        

                                        
                                        END
                  ", bancodb, Ownerdb, codigoAtribuicao, trabalhoRestante.Replace(',', '.'), trabalhoRealInformado.Replace(',', '.'),
                    (inicioReal == "NULL" ? "NULL" : "CONVERT(DateTime, '" + inicioReal + "', 103)"),
                    (terminoReal == "NULL" ? "NULL" : "CONVERT(DateTime, '" + terminoReal + "', 103)"),
                     indicaTS, comentarios);

            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaTarefaDiariaTS(int codigoAtribuicao, string[] datas, double[] valores, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            for (int i = 0; i < datas.Length; i++)
            {
                comandoSQL += string.Format(@"
                                        DELETE FROM {0}.{1}.AtualizacaoDiariaTarefaTimeSheet WHERE CodigoAtribuicao = {2} AND DataRealTrabalho = CONVERT(DateTime, '{3}', 103);
                                        ", bancodb, Ownerdb, codigoAtribuicao, datas[i]);

                if (valores[i] > 0)
                {
                    comandoSQL += string.Format(@"
                                        INSERT INTO {0}.{1}.AtualizacaoDiariaTarefaTimeSheet
                                               (CodigoAtribuicao
                                               ,DataRealTrabalho
                                               ,TrabalhoRealInformado
                                               ,StatusAprovacao
                                               ,DataStatusAprovacao)
                                         VALUES
                                               ({2}
                                               ,CONVERT(DateTime, '{3}', 103)
                                               ,{4}
                                               ,'PP'
                                               ,GetDate())
                  ", bancodb, Ownerdb, codigoAtribuicao, datas[i], valores[i].ToString().Replace(',', '.'));
                }
            }
            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaHorasProjetosRecurso(int codigoProjeto, int codigoTipoTarefa, int codigoRecurso, int codigoEntidade, string[] datas, double[] valores, string[] atualiza, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            for (int i = 0; i < datas.Length; i++)
            {
                if (atualiza[i] == "S")
                {
                    //                    comandoSQL += string.Format(@"
                    //                                                                        
                    //                                        DELETE FROM {0}.{1}.TimeSheetProjeto WHERE CodigoProjeto = {2} 
                    //                                                                             AND CodigoTipoTarefaTimeSheet = {3} 
                    //                                                                             AND CodigoUsuario = {5}
                    //                                                                             AND Data = CONVERT(DateTime, '{4}', 103);
                    //                                        ", bancodb, Ownerdb, codigoProjeto, codigoTipoTarefa, datas[i], codigoRecurso);

                    if (valores[i] > 0)
                    {
                        comandoSQL += string.Format(@"                                       
                                       IF EXISTS(SELECT 1 FROM {0}.{1}.TimeSheetProjeto 
                                                  WHERE CodigoProjeto = {2} 
                                                    AND CodigoTipoTarefaTimeSheet = {3} 
                                                    AND CodigoUsuario = {4}
                                                    AND Data = CONVERT(DateTime, '{5}', 103))
                                            BEGIN
                                                UPDATE {0}.{1}.TimeSheetProjeto SET TrabalhoInformado = {6}
                                                 WHERE CodigoProjeto = {2} 
                                                   AND CodigoTipoTarefaTimeSheet = {3} 
                                                   AND CodigoUsuario = {4}
                                                   AND Data = CONVERT(DateTime, '{5}', 103)
                                            END
                                        ELSE
                                            BEGIN
                                                INSERT INTO {0}.{1}.TimeSheetProjeto
                                                       (CodigoProjeto
                                                       ,CodigoTipoTarefaTimeSheet
                                                       ,CodigoUsuario
                                                       ,Data
                                                       ,TrabalhoInformado
                                                       ,StatusTimesheet
                                                       ,DataStatus
                                                       ,CodigoUsuarioStatus)
                                                 VALUES
                                                       ({2}
                                                       ,{3}
                                                       ,{4}
                                                       ,CONVERT(DateTime, '{5}', 103)
                                                       ,{6}
                                                       ,'PP'
                                                       ,GetDate()
                                                       ,{4})
                                                END
                                    
                  ", bancodb, Ownerdb, codigoProjeto, codigoTipoTarefa, codigoRecurso, datas[i], valores[i].ToString().Replace(',', '.'));
                    }
                    else if (valores[i] == 0)
                    {
                        comandoSQL += string.Format(@"                                       
                                       DELETE {0}.{1}.TimeSheetProjeto
                                                 WHERE CodigoProjeto = {2} 
                                                   AND CodigoTipoTarefaTimeSheet = {3} 
                                                   AND CodigoUsuario = {4}
                                                   AND Data = CONVERT(DateTime, '{5}', 103)                                            
                                    
                  ", bancodb, Ownerdb, codigoProjeto, codigoTipoTarefa, codigoRecurso, datas[i]);
                    }
                }
            }

            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaLinhasAprovacaoTSProjetos(int[] codigosProjetos, int[] codigosRecursos, int[] codigoTiposTarefas, int codigoAprovador, string status, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {

            for (int i = 0; i < codigosProjetos.Length; i++)
            {
                comandoSQL += string.Format(@"               
                     UPDATE {0}.{1}.TimeSheetProjeto 
                        SET StatusTimesheet = '{5}',
                            DataStatus = GetDate(),
                            CodigoUsuarioStatus = {6}
                      WHERE CodigoProjeto = {2} 
                        AND CodigoTipoTarefaTimeSheet = {3} 
                        AND CodigoUsuario = {4}
                        AND StatusTimesheet IN('EA', 'ER', 'PA')
                  ", bancodb, Ownerdb, codigosProjetos[i], codigoTiposTarefas[i], codigosRecursos[i], status, codigoAprovador);
            }

            if (comandoSQL != "")
            {
                execSQL(comandoSQL, ref registrosAfetados);
            }

            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaAprovacaoDataProjeto(int codigoProjeto, int codigoTipoTarefa, int codigoRecurso, int codigoEntidade, string data, string status, int codigoAprovador, string comentarios, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {

            comandoSQL = string.Format(@"UPDATE {0}.{1}.TimeSheetProjeto 
                                            SET StatusTimesheet = '{6}',
                                                DataStatus = GetDate(),
                                                CodigoUsuarioStatus = {7},
                                                ComentariosAprovador = '{8}'
                                          WHERE CodigoProjeto = {2} 
                                            AND CodigoTipoTarefaTimeSheet = {3} 
                                            AND CodigoUsuario = {5}
                                            AND Data = CONVERT(DateTime, '{4}', 103);
                                        ", bancodb, Ownerdb, codigoProjeto, codigoTipoTarefa, data, codigoRecurso, status, codigoAprovador, comentarios.Replace("'", "''"));



            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaComentarioAprovacaoDataProjeto(int codigoProjeto, string data, int codigousuario, string comentarios, int codigoTipoProjeto, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {

            comandoSQL = string.Format(@"UPDATE {0}.{1}.TimeSheetProjeto 
                                            SET ComentariosRecurso = '{2}'
                                          WHERE CodigoProjeto = {3} 
                                            AND Data = CONVERT(DateTime, '{4}', 103)
                                            AND CodigoUsuario = {5}
                                            AND CodigoTipoTarefaTimeSheet = {6}
                                        ", bancodb, Ownerdb, comentarios.Replace("'", "''"), codigoProjeto, data, codigousuario, codigoTipoProjeto);
            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public DataSet getRecursosTSProjetoAprovacao(int codigoUsuarioAprovador, int codigoEntidade)
    {
        comandoSQL = string.Format(
         @"SELECT 
                u.CodigoUsuario
                , u.NomeUsuario
                , u.Email
            FROM  {0}.{1}.TimeSheetProjeto AS tsp INNER JOIN 
                {0}.{1}.Projeto AS p ON (p.CodigoProjeto = tsp.CodigoProjeto) INNER JOIN 
                {0}.{1}.RecursoCorporativo AS rc ON (rc.CodigoUsuario = tsp.CodigoUsuario AND rc.CodigoEntidade = p.CodigoEntidade) INNER JOIN 
                {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = rc.CodigoUnidadeNegocio) INNER JOIN 
                {0}.{1}.Usuario AS u ON (u.CodigoUsuario = tsp.CodigoUsuario)
            WHERE p.CodigoEntidade = {3}
                AND ( p.IndicaAprovadorTarefas IS NULL OR p.IndicaAprovadorTarefas = 'GR' )
                AND ISNULL(un.[CodigoUsuarioGerente], un.[CodigoSuperUsuario]) = {2}
                AND u.DataExclusao IS NULL
                AND tsp.StatusTimesheet	IN ('PA','EA', 'ER')
        UNION 
        SELECT 
                 u.CodigoUsuario
                ,u.NomeUsuario
                ,u.Email
	        FROM {0}.{1}.TimeSheetProjeto AS tsp INNER JOIN 
                {0}.{1}.Projeto AS p ON (p.CodigoProjeto = tsp.CodigoProjeto) INNER JOIN 
                {0}.{1}.Usuario AS u ON (u.CodigoUsuario = tsp.CodigoUsuario) 
            WHERE p.CodigoEntidade = {3}
                AND (p.IndicaAprovadorTarefas = 'GP' OR NOT EXISTS( SELECT 1 FROM {0}.{1}.RecursoCorporativo AS rc
														        WHERE rc.CodigoUsuario = tsp.CodigoUsuario AND rc.CodigoEntidade = p.CodigoEntidade))
                AND p.CodigoGerenteProjeto = {2}
                AND tsp.StatusTimesheet IN ('PA','EA', 'ER')
            ORDER BY NomeUsuario"
            , bancodb, Ownerdb, codigoUsuarioAprovador, codigoEntidade);

        return getDataSet(comandoSQL);
    }

    public DataSet getStatusHoraRecursoProjeto(int codigoProjeto, int codigoTipoTarefa, int codigoRecurso, int codigoEntidade, string data, string where)
    {
        comandoSQL = string.Format(
              @"SELECT StatusTimesheet, ComentariosAprovador, Data, CodigoProjeto, ComentariosRecurso,CodigoTipoTarefaTimeSheet
                  FROM {0}.{1}.TimeSheetProjeto
                 WHERE CodigoProjeto = {4} 
                   AND CodigoTipoTarefaTimeSheet = {5} 
                   AND CodigoUsuario = {2}
                   AND Data = CONVERT(DateTime, '{6}', 103)                   
                "
            , bancodb, Ownerdb, codigoRecurso, codigoEntidade, codigoProjeto, codigoTipoTarefa, data, where);

        return getDataSet(comandoSQL);
    }

    public bool publicaTarefasEntidade(int codigoUsuario, int codigoEntidade, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL += string.Format(@"
                                        EXEC {0}.{1}.p_PublicaTimeSheetRecurso {2}, {3}
                  ", bancodb, Ownerdb, codigoUsuario, codigoEntidade);


            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool publicaHorasProjetoEntidade(int codigoUsuario, int codigoEntidade, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL += string.Format(@"
                                        EXEC {0}.{1}.p_PublicaTimeSheetRecursoProjeto {2}, {3}
                  ", bancodb, Ownerdb, codigoUsuario, codigoEntidade);


            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool publicaAprovacaoTarefasEntidade(int codigoUsuario, int codigoEntidade, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL += string.Format(@"
                                        EXEC {0}.{1}.p_PublicaAprovacaoTimeSheet {2}, {3}
                  ", bancodb, Ownerdb, codigoUsuario, codigoEntidade);


            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool publicaAprovacaoApontamentosEntidade(int codigoUsuario, int codigoEntidade, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL += string.Format(@"
                                        EXEC {0}.{1}.p_art_publicaAnalisesApontamentos {2}, {3}
                  ", bancodb, Ownerdb, codigoUsuario, codigoEntidade);


            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool publicaAprovacaoTSProjetos(int codigoRecurso, int codigoUsuarioAprovador, int codigoEntidade, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL += string.Format(@"
                                        EXEC {0}.{1}.p_PublicaAprovacaoTimeSheetProjeto {2}, {3}, {4}
                  ", bancodb, Ownerdb, codigoUsuarioAprovador, codigoEntidade, codigoRecurso);


            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public DataSet getDadosAtribuicao(int codigoAtribuicao, int codigoEntidade, string where)
    {
        comandoSQL = string.Format(
              @"SELECT art.CodigoAtribuicao, u.NomeUsuario AS Recurso, t.NomeTarefa AS Tarefa, p.NomeProjeto AS Projeto,
	                   art.ComentariosRecurso, art.ComentariosAprovador
                  FROM {0}.{1}.AtribuicaoRecursoTarefa art INNER JOIN
	                   {0}.{1}.Usuario u ON u.CodigoUsuario = art.CodigoRecursoProjeto INNER JOIN
	                   {0}.{1}.CronogramaProjeto cp ON cp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto INNER JOIN
	                   {0}.{1}.vi_Tarefa t ON t.CodigoTarefa = art.CodigoTarefa AND t.CodigoProjeto = cp.CodigoProjeto INNER JOIN
	                   {0}.{1}.Projeto p ON p.CodigoProjeto = cp.CodigoProjeto
                 WHERE art.CodigoAtribuicao = {2}
                   AND cp.CodigoEntidade = {3}
                   {4}"
            , bancodb, Ownerdb, codigoAtribuicao, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getComentariosAtribuicao(int codigoAtribuicao, int codigoEntidade, string where)
    {
        comandoSQL = string.Format(
              @"SELECT art.CodigoAtribuicao, rc.NomeRecurso AS Recurso, t.NomeTarefa AS Tarefa, p.NomeProjeto AS Projeto,
	                   art.ComentariosRecurso, art.ComentariosAprovador
                  FROM {0}.{1}.AtribuicaoRecursoTarefa art INNER JOIN
	                   {0}.{1}.CronogramaProjeto cp ON cp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto INNER JOIN
	                   {0}.{1}.vi_Tarefa t ON t.CodigoTarefa = art.CodigoTarefa AND t.CodigoProjeto = cp.CodigoProjeto INNER JOIN
	                   {0}.{1}.Projeto p ON p.CodigoProjeto = cp.CodigoProjeto INNER JOIN
					   {0}.{1}.RecursoCronogramaProjeto rc ON rc.CodigoRecursoProjeto = art.CodigoRecursoProjeto AND rc.CodigoCronogramaProjeto = cp.CodigoCronogramaProjeto 
                 WHERE art.CodigoAtribuicao = {2}
                   AND cp.CodigoEntidade = {3}
                   {4}"
            , bancodb, Ownerdb, codigoAtribuicao, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public bool atualizaComentariosAtribuicao(int codigoAtribuicao, string comentariosAprovador, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"                                        
                                         UPDATE {0}.{1}.AtribuicaoRecursoTarefa SET ComentariosAprovador = '{3}'
                                          WHERE CodigoAtribuicao = {2}                                            
                  ", bancodb, Ownerdb, codigoAtribuicao, comentariosAprovador);

            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    #endregion

    #region espacioTrabalho - DetalhesTS.aspx

    public DataSet getDetalhesTS(string codigoAtribucao)
    {
        comandoSQL = string.Format(@"
        -- Consulta para trazer as informações somente leitura da atribuição (o que é apresentado no topo)
            SELECT art.CodigoAtribuicao,
                   art.CodigoCronogramaProjeto,
                   art.CodigoTarefa,
                   tcp.NomeTarefa,
                   CASE WHEN p.IndicaTipoAtualizacaoTarefas = 'T' THEN 'TS' ELSE 'PC' END AS TipoAtualizacao,
                   tcp.Anotacoes AS AnotacoesCodigoCronogramaProjeto,
                   p.NomeProjeto,
                   art.InicioLB AS InicioPrevisto,
                   art.TerminoLB AS TerminoPrevisto,
                   art.TrabalhoLB AS TrabalhoPrevisto,
                   art.Inicio,
                   art.Termino,
                   art.Trabalho,
                   tcp.IndicaMarco,
                   art.Anotacoes AS AnotacoesAtribucaoRecursoTarefa,
                   art.ComentariosAprovador,
                   art.ComentariosRecurso,
                   tcpSup.NomeTarefa AS TarefaSuperior,
                   art.DataStatusAprovacao AS DataStatusAprovacao,
                   CASE WHEN art.StatusAprovacao = 'AP' THEN 'Aprovação:'	
						 WHEN art.StatusAprovacao = 'RP' THEN 'Reprovação:'	
						 ELSE '' END AS StatusAprovacao,
                    art.StatusAprovacao AS SiglaStatusAprovacao,
                   (SELECT TOP 1 u.CodigoUsuario 
                      FROM {0}.{1}.Usuario u INNER JOIN 
					       {0}.{1}.f_GetAprovadorAtribuicao(art.CodigoAtribuicao) ON CodigoAprovador = CodigoUsuario) AS CodigoAprovador,
                   (SELECT TOP 1 u.NomeUsuario 
                      FROM {0}.{1}.Usuario u INNER JOIN 
					       {0}.{1}.f_GetAprovadorAtribuicao(art.CodigoAtribuicao) ON CodigoAprovador = CodigoUsuario) AS Aprovador,
                   tcp.Predecessoras,
                   rp.Inicio AS InicioProjeto, 
                   rp.Termino AS TerminoProjeto
              FROM {0}.{1}.AtribuicaoRecursoTarefa AS art INNER JOIN
                   {0}.{1}.TarefaCronogramaProjeto AS tcp ON (tcp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto
                                                  AND tcp.CodigoTarefa = art.CodigoTarefa
                                                  AND tcp.DataExclusao IS NULL)  INNER JOIN
                   {0}.{1}.TarefaCronogramaProjeto AS tcpSup ON (tcpSup.CodigoTarefa = tcp.CodigoTarefaSuperior 
                                                             AND tcpSup.CodigoCronogramaProjeto = tcp.CodigoCronogramaProjeto) INNER JOIN
                   {0}.{1}.CronogramaProjeto AS cp ON (cp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto) INNER JOIN
                   {0}.{1}.Projeto AS p ON (p.CodigoProjeto = cp.CodigoProjeto)  LEFT JOIN 
                   {0}.{1}.vi_Projeto rp ON rp.CodigoProjeto = p.CodigoProjeto
             WHERE art.CodigoAtribuicao = '{2}' 
             
            /*
             Consulta para trazer as informações para atualização, caso o tipo de atualização seja igual a 'PC'
            */                                      
            SELECT art.CodigoAtribuicao,
                   art.InicioRealInformado               AS DataInicioReal,
                   art.TerminoRealInformado              AS DataTerminoReal,
                   CASE WHEN tcp.[IndicaMarco] = 'S' AND art.TerminoRealInformado IS NOT NULL OR art.TrabalhoRestanteInformado = 0 THEN 1						
                        WHEN (art.TrabalhoRestanteInformado + art.TrabalhoRealInformado) <> 0 THEN (art.TrabalhoRealInformado/(art.TrabalhoRealInformado + art.TrabalhoRestanteInformado)                   )
                        ELSE 0 
                   END * 100 AS PercentualConcluido,
                   art.TrabalhoRealInformado,
                   IsNull( art.TrabalhoRestanteInformado, art.Trabalho ) as TrabalhoRestanteInformado,
                   'PC' AS TipoAtualizacao,
                   art.ComentariosAprovador,
                   art.ComentariosRecurso
              FROM {0}.{1}.AtribuicaoRecursoTarefa AS art INNER JOIN
                   {0}.{1}.TarefaCronogramaProjeto AS tcp ON (tcp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto
                                                  AND tcp.CodigoTarefa = art.CodigoTarefa
                                                  AND tcp.DataExclusao IS NULL) INNER JOIN
                   {0}.{1}.CronogramaProjeto AS cp ON (cp.CodigoCronogramaProjeto = tcp.CodigoCronogramaProjeto) INNER JOIN
                   {0}.{1}.Projeto AS p ON (p.CodigoProjeto = cp.CodigoProjeto
                                                           AND p.IndicaTipoAtualizacaoTarefas = 'P') 
             WHERE art.CodigoAtribuicao = '{2}' 
            UNION
            SELECT art.CodigoAtribuicao,
                   MIN(adt.DataRealTrabalho),
                   MAX(adt.DataRealTrabalho),
                   /* CASE WHEN IsNull(art.TrabalhoRestanteInformado,0) + SUM(IsNull(adt.TrabalhoRealInformado,0)) > 0 THEN SUM(IsNull(adt.TrabalhoRealInformado,0)) / (IsNull(art.TrabalhoRestanteInformado,0) + SUM(IsNull(adt.TrabalhoRealInformado,0))) ELSE 0 END, */
				   CASE WHEN SUM(ISNULL(COALESCE(NULLIF(art.TrabalhoLB, 0), NULLIF(art.Trabalho, 0)), 0)) > 0 THEN (SUM(ISNULL(COALESCE(NULLIF(art.TrabalhoReal, 0), NULLIF(art.TrabalhoRealInformado, 0)), 0)) / SUM(COALESCE(NULLIF(art.TrabalhoLB, 0), NULLIF(art.Trabalho, 0)))) * 100 ELSE 0 END,
                   SUM(adt.TrabalhoRealInformado),
                   IsNull( art.TrabalhoRestanteInformado, art.Trabalho),
                   'TS',
                   art.ComentariosAprovador,
                   art.ComentariosRecurso
              FROM {0}.{1}.AtribuicaoRecursoTarefa AS art INNER JOIN
                   {0}.{1}.AtualizacaoDiariaTarefaTimeSheet AS adt ON (adt.CodigoAtribuicao = art.CodigoAtribuicao) INNER JOIN
                   {0}.{1}.TarefaCronogramaProjeto AS tcp ON (tcp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto
                                                  AND tcp.CodigoTarefa = art.CodigoTarefa
                                                  AND tcp.DataExclusao IS NULL) INNER JOIN
                   {0}.{1}.CronogramaProjeto AS cp ON (cp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto) INNER JOIN
                   {0}.{1}.Projeto AS p ON (p.CodigoProjeto = cp.CodigoProjeto
                                                           AND p.IndicaTipoAtualizacaoTarefas = 'T')
             WHERE art.CodigoAtribuicao = '{2}'
             GROUP BY art.CodigoAtribuicao,
                      IsNull( art.TrabalhoRestanteInformado, art.Trabalho),
                      art.ComentariosAprovador,
                      art.ComentariosRecurso   
        ", bancodb, Ownerdb, codigoAtribucao);

        return getDataSet(comandoSQL);
    }

    public DataSet getDetalhesTSMSProject(string codigoAtribucao)
    {
        comandoSQL = string.Format(@"
            SELECT art.CodigoAtribuicao,
                   art.CodigoTarefa,
                   tcp.NomeTarefa,
                   tcp.AnotacoesTarefa AS Anotacoes,
                   p.NomeProjeto,
                   art.InicioLB AS InicioPrevisto,
                   art.TerminoLB AS TerminoPrevisto,
                   art.TrabalhoLB AS TrabalhoPrevisto,
                   art.Inicio,
                   art.Termino,
                   art.Trabalho,
                   tcpSup.NomeTarefa AS TarefaSuperior,
                   CASE WHEN art.Trabalho > 0 THEN art.TrabalhoReal/art.Trabalho*100 ELSE 0 END AS PercentualConcluido,
                   art.TrabalhoReal,
                   art.TrabalhoRestante,
                   art.InicioReal,
                   art.TerminoReal                   
              FROM {0}.{1}.vi_Atribuicao AS art INNER JOIN
                   {0}.{1}.vi_Tarefa AS tcp ON (tcp.CodigoProjeto = art.CodigoProjeto
                                                  AND tcp.CodigoTarefa = art.CodigoTarefa)  INNER JOIN
                   {0}.{1}.vi_Tarefa AS tcpSup ON (tcpSup.CodigoTarefa = tcp.TarefaSuperior 
                                                             AND tcpSup.CodigoProjeto = tcp.CodigoProjeto) INNER JOIN                  
                   {0}.{1}.Projeto AS p ON (p.CodigoProjeto = tcp.CodigoProjeto) 
             WHERE art.CodigoAtribuicao = '{2}'
        ", bancodb, Ownerdb, codigoAtribucao);

        return getDataSet(comandoSQL);
    }

    public DataSet getPredecessorasAtribuicao(string codigoAtribucao)
    {
        comandoSQL = string.Format(@"
                  SELECT tcp.NomeTarefa, tcp.CodigoTarefa
                    FROM {0}.{1}.AtribuicaoRecursoTarefa AS art  INNER JOIN
		                 {0}.{1}.TarefaCronogramaProjetoPredecessoras tcpp ON (tcpp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto
										               AND tcpp.CodigoTarefa = art.CodigoTarefa) INNER JOIN
                         {0}.{1}.TarefaCronogramaProjeto AS tcp ON (tcp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto
                                                              AND tcp.CodigoTarefa = tcpp.CodigoTarefaPredecessora
                                                              AND tcp.DataExclusao IS NULL)  
                   WHERE art.CodigoAtribuicao = {2}         
        ", bancodb, Ownerdb, codigoAtribucao);

        DataSet ds = getDataSet(comandoSQL);

        return ds;
    }

    public DataSet getSucessorasAtribuicao(string codigoAtribucao)
    {
        comandoSQL = string.Format(@"
                  SELECT tcp.NomeTarefa, tcp.CodigoTarefa
                    FROM {0}.{1}.AtribuicaoRecursoTarefa AS art  INNER JOIN
		                 {0}.{1}.TarefaCronogramaProjetoPredecessoras tcpp ON (tcpp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto
										               AND tcpp.CodigoTarefaPredecessora = art.CodigoTarefa) INNER JOIN
                         {0}.{1}.TarefaCronogramaProjeto AS tcp ON (tcp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto
                                                              AND tcp.CodigoTarefa = tcpp.CodigoTarefa
                                                              AND tcp.DataExclusao IS NULL)  
                   WHERE art.CodigoAtribuicao = {2}         
        ", bancodb, Ownerdb, codigoAtribucao);

        DataSet ds = getDataSet(comandoSQL);

        return ds;
    }

    #endregion

    #region L.O.V. (lista de valores)

    public DataSet getLovNomeValor2(string tabela, string colunaValor, string colunaNome, string colunaEmail, string valorPesquisa, bool likeTotal, string where, string order, out string valor, out string nome)
    {
        nome = "";
        valor = "";

        if (order != "")
            order = "ORDER BY " + order;

        // Monta o like
        valorPesquisa += "%";
        if (likeTotal)
            valorPesquisa = "%" + valorPesquisa;
        comandoSQL = string.Format(
            @"SELECT {3} as ColunaValor, {4} as ColunaNome, {8} as ColunaEmail
                FROM {0}.{1}.{2}
               WHERE {4} like '{5}' {6}
                {7} ", bancodb, Ownerdb, tabela, colunaValor, colunaNome, valorPesquisa, where, order, colunaEmail);

        DataSet ds = getDataSet(comandoSQL);
        if (DataSetOk(ds))
        {
            DataTable dt = ds.Tables[0];
            if (dt.Rows.Count == 1)
            {
                nome = dt.Rows[0]["ColunaNome"].ToString();
                valor = dt.Rows[0]["ColunaValor"].ToString();
            }
            return ds;
        }
        return null;
    }
    public DataSet getLov_NomeValor(string tabela, string colunaValor, string colunaNome, string valorPesquisa, bool likeTotal, string where, string order, out string valor, out string nome)
    {
        nome = "";
        valor = "";

        if (order != "")
            order = "ORDER BY " + order;

        // Monta o like
        valorPesquisa += "%";
        if (likeTotal)
            valorPesquisa = "%" + valorPesquisa;
        comandoSQL = string.Format(
            @"SELECT {3} as ColunaValor, {4} as ColunaNome
                FROM {0}.{1}.{2}
               WHERE {4} like '{5}' {6}
                {7} ", bancodb, Ownerdb, tabela, colunaValor, colunaNome, valorPesquisa, where, order);

        DataSet ds = getDataSet(comandoSQL);
        if (DataSetOk(ds))
        {
            DataTable dt = ds.Tables[0];
            if (dt.Rows.Count == 1)
            {
                nome = dt.Rows[0]["ColunaNome"].ToString();
                valor = dt.Rows[0]["ColunaValor"].ToString();
            }
            return ds;
        }
        return null;
    }

    #endregion

    #region Reunioes - reunioes.aspx

    public DataSet getTipoAssociacaoEventos(string iniciais, string where)
    {
        comandoSQL = string.Format(@"
                    SELECT CodigoTipoAssociacao 
                    FROM {0}.{1}.TipoAssociacao
                    WHERE IniciaisTipoAssociacao = '{2}'
                    ", bancodb, Ownerdb, iniciais, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    //Ok
    public DataSet getTiposEventos(int CodigoEntidade, string where)
    {
        comandoSQL = string.Format(@"
                    SELECT  CodigoTipoEvento
                        ,   DescricaoTipoEvento
                        ,   CodigoModuloSistema
                        ,   CodigoEntidade 

                    FROM {0}.{1}.TipoEvento 

                    WHERE CodigoEntidade = {2}
                      {3}

                    ORDER BY DescricaoTipoEvento                        
                    ", bancodb, Ownerdb, CodigoEntidade, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }
    public DataSet getModuloSistema()
    {
        comandoSQL = string.Format(@"SELECT [CodigoModuloSistema]
                                           ,[DescricaoModuloSistema]
                                       FROM {0}.{1}.[ModuloSistema]", bancodb, Ownerdb);
        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }
    //Ok
    public DataSet getPossiveisResponsaveisReuniao(string codigoModulo, string iniciaisTipoProjeto,
                                                   string codigoObjeto, string codigoEntidad)
    {
        comandoSQL = string.Format(@"
                    SELECT CodigoUsuario, NomeUsuario, EMail
                    FROM Usuario
                    WHERE CodigoUsuario IN (SELECT * 
                                            FROM {0}.{1}.f_GetPossiveisResponsaveisReuniao('{2}', '{3}', {4}, {5}))
                    ORDER BY NomeUsuario
            ", bancodb, Ownerdb, codigoModulo, iniciaisTipoProjeto, codigoObjeto, codigoEntidad);
        return getDataSet(comandoSQL);
    }

    //Ok
    public DataSet getEventosEntidade(string codigoEntidade, string where, string codigoUsuario, string permissaoAta)
    {
        comandoSQL = string.Format(@"
                    Declare @fusoHorario int, @dataHoje Date;
                    set @fusoHorario = DATEPART(TZOFFSET, SYSDATETIMEOFFSET());
                    set @dataHoje = CAST(GETDATE() AS date);
                    
                    SELECT      CodigoEvento, 
                                DescricaoResumida,
                                CodigoResponsavelEvento,
			                    CONVERT(DATETIME,SWITCHOFFSET(InicioPrevisto, @fusoHorario)) as InicioPrevisto,
                                CONVERT(VARCHAR(10), SWITCHOFFSET(InicioPrevisto, @fusoHorario), CONVERT(INT, dbo.f_GetTraducao('103'))) AS inicioPrevistoData,
                                CONVERT(VARCHAR(8), SWITCHOFFSET(InicioPrevisto, @fusoHorario), 108) AS inicioPrevistoHora,
                                CONVERT(DATETIME,SWITCHOFFSET(TerminoPrevisto, @fusoHorario)) AS TerminoPrevisto,
                                CONVERT(VARCHAR(10), SWITCHOFFSET(TerminoPrevisto, @fusoHorario), CONVERT(INT, dbo.f_GetTraducao('103'))) AS TerminoPrevistoData,
                                CONVERT(VARCHAR(8), SWITCHOFFSET(TerminoPrevisto, @fusoHorario), 108) AS TerminoPrevistoHora,
			                    InicioReal,
			                    CONVERT(Varchar(10), InicioReal, convert(int, dbo.f_GetTraducao('103'))) AS InicioRealData,
			                    CONVERT(Varchar(8), InicioReal, 108) AS InicioRealHora,
			                    TerminoReal,
			                    CONVERT(Varchar(10), TerminoReal, convert(int, dbo.f_GetTraducao('103'))) AS TerminoRealData,
			                    CONVERT(Varchar(8), TerminoReal, 108) AS TerminoRealHora,
                                 ev.CodigoTipoAssociacao,
                                 ev.CodigoObjetoAssociado,
                                 LocalEvento,
                                 Pauta,
                                 ResumoEvento,
                                 ev.CodigoTipoEvento,
                                 ev.CodigoObjetoAssociado,
                            CASE
                                WHEN CAST(SWITCHOFFSET(InicioPrevisto, @fusoHorario) AS date) < @dataHoje THEN dbo.f_GetTraducao('Passado')
                                WHEN CAST(SWITCHOFFSET(InicioPrevisto, @fusoHorario) AS date) = @dataHoje THEN dbo.f_GetTraducao('Hoje')
                                WHEN CAST(SWITCHOFFSET(InicioPrevisto, @fusoHorario) AS date) <= DATEADD(day,10, @dataHoje) THEN dbo.f_GetTraducao('Próximos 10 dias')
                                ELSE dbo.f_GetTraducao('Futuro')
                            END AS Quando,
                            ta.IniciaisTipoAssociacao,
                            CASE 
                                WHEN TerminoPrevisto < SYSDATETIMEOFFSET() AND TerminoReal IS NULL THEN dbo.f_GetTraducao('S') 
                                ELSE 'N' 
                            END AS IndicaAtrasada,
                            u.NomeUsuario AS NomeResponsavel ,
                            {0}.{1}.f_VerificaAcessoConcedido({5}, {2}, {2}, null, 'PR', 0, null, '{4}') AS PermissaoEditarAtaEntidade,
                            case when CodigoResponsavelEvento = {5} then 'True' else 'False' end as PermissaoEditarAtaResponsavel,
                            u.EMail AS EmailResponsavel
                    FROM {0}.{1}.Evento AS ev INNER JOIN 
                         {0}.{1}.TipoEvento AS tev ON ev.CodigoTipoEvento = tev.CodigoTipoEvento INNER JOIN
                         {0}.{1}.TipoAssociacao AS ta ON ta.CodigoTipoAssociacao = ev.CodigoTipoAssociacao INNER JOIN
                         {0}.{1}.Usuario u ON u.CodigoUsuario = CodigoResponsavelEvento
                    WHERE ev.CodigoEntidade = {2}
                       {3}                
                    ORDER BY ISNULL(InicioReal,InicioPrevisto) DESC
                    ", bancodb, Ownerdb, codigoEntidade, where, permissaoAta, codigoUsuario);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getEventosTarefaCronograma(int in_CodigoEntidade, int in_CodigoUsuario, int in_CodigoCarteira, string in_CodigoProjeto, string in_CodigoUnidadeNegocio, string in_CodigoUsuarioParticipante, string in_DataInicio, string in_DataTermino, string in_IndicaEventosQueParticipo, string in_CodigoTipoEvento)
    {
        string where = "";

        if (in_CodigoTipoEvento != "null")
        {
            where = string.Format(" where CodigoTipoTarefaCronograma = '{0}' ", in_CodigoTipoEvento);
        }

        comandoSQL = string.Format(@"
        SELECT * FROM {0}.{1}.f_GetEventosTarefaCronograma(
        {2}                         /*@in_CodigoEntidade*/,
        {3}                         /*@in_CodigoUsuario*/,
        {4}                         /*@in_CodigoCarteira*/,
        {5}                         /*@in_CodigoProjeto*/,
        {6}                         /*@in_CodigoUnidadeNegocio*/,
        {7}                         /*@in_CodigoUsuarioParticipante*/,
        convert(datetime,{8},103) /*@in_DataInicio*/,
        convert(datetime,{9},103) /*@in_DataTermino*/,
        '{10}')
         {11}", getDbName(), getDbOwner(),
                in_CodigoEntidade,
                in_CodigoUsuario,
                in_CodigoCarteira,
                in_CodigoProjeto,
                in_CodigoUnidadeNegocio,
                in_CodigoUsuarioParticipante,
                in_DataInicio,
                in_DataTermino,
                in_IndicaEventosQueParticipo,
                where);
        DataSet ds = getDataSet(comandoSQL);
        return ds;

    }

    public DataSet getPendenciasEvento(int codigoEvento, string where)
    {
        comandoSQL = string.Format(@"SELECT f.CodigoTarefa, f.DescricaoTarefa, f.InicioPrevisto, f.TerminoPrevisto, 
                                            f.InicioReal, f.TerminoReal, f.DescricaoStatusTarefa, f.NomeUsuarioResponsavel, ISNULL(f.Anotacoes,'') as Anotacoes,
                                            e.DescricaoResumida + 
                                                CASE WHEN e.InicioReal IS NULL 
                                                  THEN ' ' 
                                                  ELSE ' (' + CONVERT(VARCHAR, e.InicioReal, 103) + ')' 
                                                END as DescricaoReuniao,
                                            f.Estagio
                                       FROM {0}.{1}.f_GetPendenciasReuniaoAnterior({2}) AS f INNER JOIN
                                            {0}.{1}.ToDoList AS td ON ( td.CodigoToDoList = f.CodigoToDoList ) LEFT JOIN
                                            {0}.{1}.Evento AS e ON ( e.CodigoEvento = td.CodigoObjetoAssociado ) LEFT JOIN 
                                            {0}.{1}.TipoAssociacao ta ON ( ta.CodigoTipoAssociacao = td.CodigoTipoAssociacao )
                                      WHERE 1 = 1 {3}
                                      AND ta.IniciaisTipoAssociacao = 'RE'
                                      ORDER BY f.TerminoPrevisto
                    ", bancodb, Ownerdb, codigoEvento, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }


    public DataSet getTarefasEvento(int codigoEvento, string iniciaisObjeto, string where)
    {
        comandoSQL = string.Format(@"
                         SELECT ttd.CodigoTarefa,
                                ttd.DescricaoTarefa,
                                CONVERT(VarChar(10), ttd.InicioPrevisto, 103) AS InicioPrevisto,
                                CONVERT(VarChar(10), ttd.TerminoPrevisto, 103) AS TerminoPrevisto,
                                ttd.InicioReal,
                                ttd.TerminoReal,
                                ttd.CodigoUsuarioResponsavelTarefa,
                                u.NomeUsuario,
                                ttd.PercentualConcluido,
                                ttd.Anotacoes,
                                ttd.CodigoStatusTarefa,
                                st.DescricaoStatusTarefa,
                                ttd.EsforcoPrevisto,
                                ttd.EsforcoReal,
                                ttd.CustoPrevisto,
                                ttd.CustoReal,
                                CASE WHEN TerminoReal IS NOT NULL THEN 'Concluida'
                                     WHEN TerminoReal IS NULL AND TerminoPrevisto >= GetDate() THEN 'Futura'
                                     WHEN TerminoReal IS NULL AND TerminoPrevisto < GetDate() THEN 'Atrasada' END,
                                CASE ttd.Prioridade WHEN 'B' THEN 'Baixa'
                                                    WHEN 'M' THEN 'Média'
                                                    ELSE 'Alta' END                                   
                           FROM {0}.{1}.TarefaToDoList AS ttd INNER JOIN
                                {0}.{1}.ToDoList AS orig ON (ttd.CodigoToDoList = orig.CodigoToDoList) INNER JOIN
                                {0}.{1}.Usuario AS u ON (u.CodigoUsuario = ttd.CodigoUsuarioResponsavelTarefa
                                             AND u.DataExclusao IS NULL) INNER JOIN
                                {0}.{1}.StatusTarefa AS st ON (st.CodigoStatusTarefa = ttd.CodigoStatusTarefa) 
                          WHERE ttd.DataExclusao IS NULL
                            AND orig.CodigoTipoAssociacao IN (SELECT CodigoTipoAssociacao FROM {0}.{1}.TipoAssociacao WHERE IniciaisTipoAssociacao = '{3}') --> Tipo de associação reunião
                            AND orig.CodigoObjetoAssociado = {2}
                            AND orig.DataExclusao IS NULL  
                    ", bancodb, Ownerdb, codigoEvento, iniciaisObjeto, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getProjetosEvento(int codigoEvento, string where)
    {
        comandoSQL = string.Format(@"SELECT f.CodigoProjeto, LTRIM(RTRIM(f.DesempenhoAnterior)) AS DesempenhoAnterior, LTRIM(RTRIM(f.DesempenhoAtual)) AS DesempenhoAtual, f.NomeProjeto, f.SiglaUnidade, f.StatusAnterior, f.StatusAnterior, f.StatusAtual
                                       FROM {0}.{1}.f_GetProjetosReuniaoAnterior({2}) f
                                      WHERE 1 = 1 {3}
                                      ORDER BY NomeProjeto
                    ", bancodb, Ownerdb, codigoEvento, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getObjetivosEvento(int codigoEvento, string where)
    {
        comandoSQL = string.Format(@"SELECT f.CodigoObjetivoEstrategico, f.DescricaoObjetivoEstrategico, f.DesempenhoAtual
		                               FROM {0}.{1}.f_GetObjetivosEstrategicosReuniao({2}) f 
                    ", bancodb, Ownerdb, codigoEvento, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getProjetosPlanejamentoEvento(int codigoEvento, int codigoEntidade, int CodigoUnidade, int codigoUsuario, int codigoCarteira, string where)
    {
        comandoSQL = string.Format(@"BEGIN
	                                    DECLARE @tbResumo Table(Codigo int,
							                                    Descricao VarChar(255),
                                                                Desempenho VarChar(10),
							                                    Selecionado Char(1))
                                    							
	                                    DECLARE @CodigoReuniao int,
			                                    @CodigoUnidade int,
			                                    @CodigoUsuario int,
			                                    @CodigoEntidade int,
                                                @CodigoUnidadeCursor Int,  
                                                @CodigoCarteira Int 
                                    			
	                                    SET @CodigoReuniao = {2}
	                                    SET @CodigoUnidade = {3}
	                                    SET @CodigoUsuario = {4}
	                                    SET @CodigoEntidade = {5}
                                        SET @CodigoCarteira = {6}   
                                    							
	                                    INSERT INTO @tbResumo
                                            SELECT ep.CodigoProjeto, p.NomeProjeto, rp.CorGeral AS DesempenhoAtual, 'S'
                                              FROM {0}.{1}.EventoProjeto ep INNER JOIN 
												   {0}.{1}.Projeto p ON p.CodigoProjeto = ep.CodigoProjeto INNER JOIN
												   {0}.{1}.ResumoProjeto rp ON rp.CodigoProjeto = ep.CodigoProjeto
											 WHERE ep.CodigoEvento = @CodigoReuniao
                                    	    	
    	                                DECLARE cCursor CURSOR LOCAL FOR
    	                                  SELECT DISTINCT CodigoUnidadeNegocio
    	                                    FROM {0}.{1}.Projeto AS p INNER JOIN
    	                                         {0}.{1}.f_GetProjetosUsuario(@CodigoUsuario, @CodigoEntidade, @CodigoCarteira) AS f ON (f.CodigoProjeto = p.CodigoProjeto)
    	                                   WHERE CodigoUnidadeNegocio IS NOT NULL              
                                    	
    	                                OPEN cCursor
                                    	
    	                                FETCH NEXT FROM cCursor INTO @CodigoUnidadeCursor
                                    	
    	                                WHILE @@FETCH_STATUS = 0
    	                                  BEGIN
    	                                    IF ({0}.{1}.f_GetUnidadeSuperior(@CodigoUnidadeCursor, @CodigoUnidade) IS NOT NULL) OR (@CodigoUnidadeCursor = @CodigoUnidade)
    	                                       INSERT INTO @tbResumo
                                               SELECT p.CodigoProjeto, p.NomeProjeto, rp.CorGeral, 'N'
                                                 FROM {0}.{1}.Projeto p INNER JOIN
                                                      {0}.{1}.f_GetProjetosUsuario(@CodigoUsuario, @CodigoEntidade, @CodigoCarteira) f ON f.CodigoProjeto = p.CodigoProjeto INNER JOIN
                                                      {0}.{1}.ResumoProjeto rp ON rp.CodigoProjeto = p.CodigoProjeto INNER JOIN 
                                                      {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                                                       AND s.IndicaAcompanhamentoExecucao = 'S'
                                                                                       AND s.CodigoStatus = 3)                      
                                                WHERE p.CodigoUnidadeNegocio = @CodigoUnidadeCursor
                                                  AND p.DataExclusao IS NULL          
				                                  AND p.IndicaPrograma = 'N'  
                                                  AND p.CodigoProjeto NOT IN(SELECT Codigo FROM @tbResumo) 
                                               
    	                                    FETCH NEXT FROM cCursor INTO @CodigoUnidadeCursor    	
    	                                  END
                                    	  
    	                                CLOSE cCursor
    	                                DEALLOCATE cCursor  
                                    	         	  
                                        SELECT Codigo, Descricao, RTRIM(LTRIM(Desempenho)) AS Desempenho, Selecionado 
                                          FROM @tbResumo
                                         ORDER BY Descricao
                                      
                                    END
                    ", bancodb, Ownerdb, codigoEvento, CodigoUnidade, codigoUsuario, codigoEntidade, codigoCarteira, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getObjetivosPlanejamentoEvento(int codigoEvento, int codigoEntidade, int CodigoMapa, int codigoUsuario, string where)
    {
        comandoSQL = string.Format(@"BEGIN
	                                    DECLARE @tbResumo Table(Codigo int,
							                                    Descricao VarChar(255),
                                                                Desempenho VarChar(10),
							                                    Selecionado Char(1))
                                    							
	                                    DECLARE @CodigoReuniao int,
			                                    @CodigoMapa int,
												@CodigoEntidade int,
			                                    @CodigoUsuario int
                                    			
	                                    SET @CodigoReuniao = {2}
	                                    SET @CodigoUsuario = {3}
	                                    SET @CodigoMapa = {4}
	                                    SET @CodigoEntidade = {5}
                                    							
	                                    INSERT INTO @tbResumo
		                                    SELECT f.CodigoObjetivoEstrategico, f.DescricaoObjetivoEstrategico, f.DesempenhoAtual, 'S'
		                                      FROM {0}.{1}.f_GetObjetivosEstrategicosReuniao(@CodigoReuniao) f 
                                    		  
	                                    INSERT INTO @tbResumo
		                                    SELECT obj.CodigoObjetoEstrategia, obj.DescricaoObjetoEstrategia, 
												   {0}.{1}.f_GetCorObjetivo(@CodigoEntidade,obj.CodigoObjetoEstrategia,Year(GetDate()), Month(GetDate())), 'N'
		                                      FROM {0}.{1}.ObjetoEstrategia obj INNER JOIN
												   {0}.{1}.TipoObjetoEstrategia toe ON (toe.CodigoTipoObjetoEstrategia = obj.CodigoTipoObjetoEstrategia  
																				AND toe.IniciaisTipoObjeto = 'OBJ')        
		                                     WHERE obj.CodigoMapaEstrategico = @CodigoMapa
											   AND obj.DataExclusao IS NULL
		                                       AND obj.CodigoObjetoEstrategia NOT IN(SELECT Codigo FROM @tbResumo) 
                                    			   
	                                    SELECT Codigo, Descricao, RTRIM(LTRIM(Desempenho)) AS Desempenho, Selecionado 
                                          FROM @tbResumo
                                         WHERE 1 = 1 
                                           {6}
                                         ORDER BY Descricao
                                      
                                    END
                    ", bancodb, Ownerdb, codigoEvento, codigoUsuario, CodigoMapa, codigoEntidade, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }


    //Ok
    public DataSet getParticipantesConfirmacaoEventos(string codigoEvento, string where)
    {
        comandoSQL = string.Format(@"
                    SELECT u.CodigoUsuario, u.NomeUsuario,u.EMail,'Unidade'
                    FROM {0}.{1}.Usuario u INNER JOIN
		                 {0}.{1}.ParticipanteEvento pe ON pe.CodigoParticipante = u.CodigoUsuario
                    WHERE CodigoEvento = {2}
                     {3}
                    ORDER BY NomeUsuario
                    ", bancodb, Ownerdb, codigoEvento, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getPossiveisDestinatariosMensagem(string iniciaisTipoObjeto, int codigoObjetoAssociado
                                                    , int idUnidadeLogada, string where)
    {
        comandoSQL = string.Format(@"
                    SELECT usu.CodigoUsuario, usu.NomeUsuario

                    FROM {0}.{1}.Usuario AS usu

                    WHERE CodigoUsuario IN (SELECT * 
                                              FROM {0}.{1}.f_GetPossiveisDestinatariosMensagem('{2}', {3}, {4}))
                      {5}

                    ORDER BY NomeUsuario
                    ", bancodb, Ownerdb, iniciaisTipoObjeto, codigoObjetoAssociado, idUnidadeLogada, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    //Ok
    public DataSet getParticipantesEventos(string codigoModulo, string iniciaisTipoProjeto,
                                           string codigoObjeto, string codigoEntidad, string where)
    {
        /* Código original WI 2440 para aumentar a performance, alterou a regra de negócio.
		comandoSQL = string.Format(@"
                    SELECT u.CodigoUsuario, u.NomeUsuario
                    FROM {0}.{1}.Usuario u INNER JOIN
                         {0}.{1}.UsuarioUnidadeNegocio uun ON ( uun.CodigoUsuario = u.CodigoUsuario )
                    WHERE u.CodigoUsuario IN (SELECT * 
                                              FROM {0}.{1}.f_GetPossiveisConvidadosReuniao('{2}', '{3}', {4}, {5}))
                    AND u.DataExclusao IS NULL
                    AND uun.CodigoUnidadeNegocio = {5}
                    AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S'
                        {6}                    
                    ORDER BY u.NomeUsuario
                    ", bancodb, Ownerdb, codigoModulo, iniciaisTipoProjeto, codigoObjeto, codigoEntidad, where);
         */
        comandoSQL = string.Format(@"
                    SELECT u.CodigoUsuario, u.NomeUsuario
                    FROM {0}.{1}.Usuario u INNER JOIN
                         {0}.{1}.UsuarioUnidadeNegocio uun ON ( uun.CodigoUsuario = u.CodigoUsuario )
                    WHERE u.DataExclusao IS NULL
                    AND uun.CodigoUnidadeNegocio = {2}
                    AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S'
                        {3}                    
                    ORDER BY u.NomeUsuario
                    ", bancodb, Ownerdb, codigoEntidad, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getParticipantesGrupoNaoSelecionados(string codigoEntidad, string codigoGrupo, string where)
    {
        comandoSQL = string.Format(@"                    
        SELECT u.NomeUsuario
               ,u.CodigoUsuario
          FROM {0}.{1}.Usuario u 
    INNER JOIN {0}.{1}.UsuarioUnidadeNegocio uun ON ( uun.CodigoUsuario = u.CodigoUsuario )
                    WHERE u.CodigoUsuario NOT IN ((SELECT CodigoUsuarioParticipante
                                              FROM {0}.{1}.UsuarioGrupoParticipantesEvento WHERE CodigoGrupoParticipantes =  {4}))
                    AND u.DataExclusao IS NULL
                    AND uun.CodigoUnidadeNegocio = {2}
                    AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S'
                        {3}                    
                    ORDER BY u.NomeUsuario
                    ", getDbName(), getDbOwner(), codigoEntidad, where, codigoGrupo);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getParticipantesGrupoSelecionados(string codigoEntidad, string codigoGrupo, string moduloSistema, string iniciaisObjeto, string idProjeto, string where)
    {
        /*
          
		 */
        comandoSQL = string.Format(@"                    
        SELECT u.NomeUsuario
               ,u.CodigoUsuario
          FROM {0}.{1}.Usuario u 
    INNER JOIN {0}.{1}.UsuarioUnidadeNegocio uun ON ( uun.CodigoUsuario = u.CodigoUsuario )
                    WHERE u.CodigoUsuario IN (SELECT CodigoUsuarioParticipante
                                              FROM {0}.{1}.UsuarioGrupoParticipantesEvento where CodigoGrupoParticipantes = {3})
                    AND u.CodigoUsuario IN (SELECT * FROM {0}.{1}.f_GetPossiveisConvidadosReuniao('{5}', '{6}', {7}, {2}))
                    AND u.DataExclusao IS NULL
                    AND uun.CodigoUnidadeNegocio = {2}
                    AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S'
                        {4}                    
                    ORDER BY u.NomeUsuario
                    ", getDbName(), getDbOwner(), codigoEntidad, codigoGrupo, where, moduloSistema, iniciaisObjeto, idProjeto);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    //Ok
    public bool incluiEvento(string DescricaoResumida, string CodigoResponsavelEvento, string InicioPrevisto,
                             string TerminoPrevisto, string InicioReal, string TerminoReal, string CodigoTipoAssociacao,
                             string CodigoObjetoAssociado, string LocalEvento, string Pauta,
                             string ResumoEvento, string CodigoEntidade, string DataEnvioPauta, string DataEnvioAta,
                             string CodigoTipoEvento, string CodigoUsuarioInclusao,
                             ref string identityCodigoProjeto, ref string msgError)
    {

        IFormatProvider iFormatProvider = new CultureInfo(System.Globalization.CultureInfo.CurrentCulture.Name, false);
        DateTime objDataInicioReal = DateTime.Now;
        DateTime objDataTerminoReal = DateTime.Now;
        if (InicioReal != "NULL")
            objDataInicioReal = DateTime.Parse(InicioReal, iFormatProvider);
        if (TerminoReal != "NULL")
            objDataTerminoReal = DateTime.Parse( TerminoReal, iFormatProvider);
        
        DateTime objDataInicioPrevisto = DateTime.Parse(InicioPrevisto, iFormatProvider);
        DateTime objDataTerminoPrevisto = DateTime.Parse(TerminoPrevisto, iFormatProvider);

        bool retorno = false;
        string comandoSQL = "";
        try
        {

            comandoSQL = string.Format(@"
                        BEGIN
                            --begin tran T1
                            DECLARE @CodigoEvento int,
                                    @DataAtual DateTime,
                                    @fusoHorario int

                            set @fusoHorario = DATEPART(TZOFFSET, SYSDATETIMEOFFSET());

                            SET @DataAtual = GETDATE()
                            IF NOT EXISTS(SELECT 1 FROM EVENTO 
							               WHERE DescricaoResumida = '{2}'
                                             AND CodigoResponsavelEvento = {3} 
                                             AND InicioPrevisto = {4}
                                             AND TerminoPrevisto = {5})
                            BEGIN
                                    INSERT INTO {0}.{1}.Evento (
                                                        DescricaoResumida, CodigoResponsavelEvento, InicioPrevisto, 
                                                        TerminoPrevisto, InicioReal, TerminoReal, 
                                                        CodigoTipoAssociacao, CodigoObjetoAssociado, LocalEvento, 
                                                        Pauta, ResumoEvento, CodigoEntidade, DataEnvioPauta, 
                                                        DataEnvioAta, CodigoTipoEvento, DataInclusao, CodigoUsuarioInclusao)
                                    VALUES('{2}', {3}, {4}, 
                                            {5},  {6}, {7},
                                            {8}, {9}, '{10}', 
                                            '{11}', '{12}', {13}, {14}, 
                                            {15}, {16}, @DataAtual, {17})

                                    SET @CodigoEvento = scope_identity();
                          END
                            SELECT @CodigoEvento AS CodigoEvento

                            --if @@ERROR > 0
                            --    ROLLBACK TRAN T1
                            --else
                            --    COMMIT TRAN T1
                        END                    
                     ", bancodb, Ownerdb, DescricaoResumida, CodigoResponsavelEvento,
                        ("TODATETIMEOFFSET(CONVERT(datetime, '" + objDataInicioPrevisto.ToString("dd/MM/yyyy HH:mm:ss") + "', 103), @fusoHorario)"),
                        ("TODATETIMEOFFSET(CONVERT(datetime, '" + objDataTerminoPrevisto.ToString("dd/MM/yyyy HH:mm:ss") + "', 103), @fusoHorario)"),
                        (InicioReal == "NULL" ? "NULL" : ("TODATETIMEOFFSET(CONVERT(datetime, '" + objDataInicioReal.ToString("dd/MM/yyyy HH:mm:ss") + "', 103), @fusoHorario)")),
                        (TerminoReal == "NULL" ? "NULL" : ("TODATETIMEOFFSET(CONVERT(datetime, '" + objDataTerminoReal.ToString("dd/MM/yyyy HH:mm:ss") + "', 103), @fusoHorario)")),
                        CodigoTipoAssociacao, CodigoObjetoAssociado, LocalEvento, Pauta, ResumoEvento, CodigoEntidade,
                        (DataEnvioPauta == "NULL" ? "NULL" : "CONVERT(DateTime, '" + DataEnvioPauta + "', convert(int, dbo.f_GetTraducao('103')))"),
                        (DataEnvioAta == "NULL" ? "NULL" : "CONVERT(DateTime, '" + DataEnvioAta + "', convert(int, dbo.f_GetTraducao('103')))"),
                        CodigoTipoEvento, CodigoUsuarioInclusao);

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                identityCodigoProjeto = ds.Tables[0].Rows[0][0].ToString();

            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    //Ok
    public bool incluiParticipantesSelecionados(int codigoEntidade, int codigoUsuario, string[] arrayParticipantesEventos, string codigoEvento)
    {
        bool retorno = false;
        string comandoSQL = "";
        string mensagem = "";
        //string codigoUsuario = "";
        //string whereDestinatarios = "";
        int registrosAfetados = 0;

        //foreach (string codigo in arrayParticipantesEventos)
        //    whereDestinatarios += codigo + ",";

        //whereDestinatarios = " AND ParticipanteEvento NOT IN(" + whereDestinatarios + ")";

        try
        {
            comandoSQL = string.Format(@"

				-- retirando notificações para os participantes existentes
				EXEC {0}.{1}.[p_brk_AtualizaNotificacoesReunioes]
					@CodigoEntidadeContexto			= {2}
				, @CodigoUsuarioSistema				= {3}
				, @CodigoEvento						= {4}
				, @CodigoParticipante				= NULL
				, @IndicaTipoEvento					= 'EXCLUSAO'
				, @ValorAnterior					= NULL
				, @ValorAtual						= NULL;

                        --1ro, apago todos os Participantes que ja tam selecionado pra o Evento.
                        DELETE FROM {0}.{1}.ParticipanteEvento 
                         WHERE CodigoEvento = {4};

                        --2do, Insero os selecionados na lista...
                        ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoEvento);

            foreach (string codigoParticipante in arrayParticipantesEventos)
            {
                if (codigoParticipante != "")
                {
                    // Insere um registro na tabela complementoProjeto
                    comandoSQL += string.Format(@"
                            INSERT INTO {0}.{1}.ParticipanteEvento(CodigoEvento, CodigoParticipante)
                            values({2}, {3})
                            ", bancodb, Ownerdb, codigoEvento, codigoParticipante);
                }
            }

            comandoSQL += string.Format(@"
				-- reinserindo as notificações para os participantes `atuais´
				EXEC {0}.{1}.[p_brk_AtualizaNotificacoesReunioes]
					    @CodigoEntidadeContexto			= {2}
				    , @CodigoUsuarioSistema				= {3}
				    , @CodigoEvento						= {4}
				    , @CodigoParticipante				= NULL
				    , @IndicaTipoEvento					= 'INCLUSAO'
				    , @ValorAnterior					= NULL
				    , @ValorAtual						= NULL;
                        ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoEvento);

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

    public bool incluiParticipantesGruposSelecionados(string[] arrayParticipantesGrupos, string codigoGrupo)
    {
        bool retorno = false;
        string comandoSQL = "";
        string mensagem = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                        --1ro, apago todos os Participantes que ja tam selecionado pra o Evento.
                        DELETE FROM {0}.{1}.UsuarioGrupoParticipantesEvento 
                         WHERE CodigoGrupoParticipantes = {2}
                            --3;

                        --2do, Insero os selecionados na lista...
                        ", bancodb, Ownerdb, codigoGrupo);//, whereDestinatarios);

            foreach (string codigoParticipanteGrupo in arrayParticipantesGrupos)
            {
                if (codigoParticipanteGrupo != "")
                {
                    // Insere um registro na tabela complementoProjeto
                    comandoSQL += string.Format(@"
                            --IF NOT EXISTS(SELECT 1
                            --               FROM {0}.{1}.UsuarioGrupoParticipantesEvento
                            --              WHERE CodigoUsuarioParticipante = {3}
                            --                AND CodigoGrupoParticipantes = {2})
                            INSERT INTO {0}.{1}.UsuarioGrupoParticipantesEvento(CodigoGrupoParticipantes, CodigoUsuarioParticipante)
                            values({2}, {3})
                            ", bancodb, Ownerdb, codigoGrupo, codigoParticipanteGrupo);
                }
            }

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

    public bool incluiProjetosSelecionados(int[] arrayProjetosEventos, string codigoEvento)
    {
        bool retorno = false;
        string comandoSQL = "";
        string mensagem = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                        DELETE FROM {0}.{1}.EventoProjeto 
                         WHERE CodigoEvento = {2}
                        ", bancodb, Ownerdb, codigoEvento);

            foreach (int codigoProjeto in arrayProjetosEventos)
            {
                comandoSQL += string.Format(@"INSERT INTO {0}.{1}.EventoProjeto
								                SELECT {2}, CodigoProjeto, CodigoStatusProjeto, CorGeral 
                                                  FROM {0}.{1}.ResumoProjeto 
                                                 WHERE CodigoProjeto = {3}                            
                            ", bancodb, Ownerdb, codigoEvento, codigoProjeto);
            }

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

    public bool incluiObjetivosSelecionados(int[] arrayObjetivosEventos, int codigoEvento, int codigoEntidade)
    {
        bool retorno = false;
        string comandoSQL = "";
        string mensagem = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                        DELETE FROM {0}.{1}.EventoObjetivoEstrategico 
                         WHERE CodigoEvento = {2}
                        ", bancodb, Ownerdb, codigoEvento);

            foreach (int codigoObj in arrayObjetivosEventos)
            {
                comandoSQL += string.Format(@"INSERT INTO {0}.{1}.EventoObjetivoEstrategico
								                SELECT {2}, CodigoObjetoEstrategia, {0}.{1}.f_GetCorObjetivo({4}, CodigoObjetoEstrategia,Year(GetDate()), Month(GetDate())) 
                                                  FROM {0}.{1}.ObjetoEstrategia 
                                                 WHERE CodigoObjetoEstrategia = {3}                            
                            ", bancodb, Ownerdb, codigoEvento, codigoObj, codigoEntidade);
            }

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

    //Ok
    public bool atualizaEvento(string DescricaoResumida, string CodigoResponsavelEvento, string InicioPrevisto,
                             string TerminoPrevisto, string InicioReal, string TerminoReal, string CodigoTipoAssociacao,
                             string CodigoObjetoAssociado, string LocalEvento, string Pauta,
                             string ResumoEvento, string CodigoEntidade, string DataEnvioPauta, string DataEnvioAta,
                             string CodigoTipoEvento, string CodigoUsuarioInclusao, string chave, bool indicaRealizacaoReuniao, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            IFormatProvider iFormatProvider = new CultureInfo(System.Globalization.CultureInfo.CurrentCulture.Name, false);
            DateTime objDataInicioReal = DateTime.Now;
            DateTime objDataTerminoReal = DateTime.Now;
            if (InicioReal != "NULL")
                objDataInicioReal = DateTime.Parse(InicioReal, iFormatProvider);
            if (TerminoReal != "NULL")
                objDataTerminoReal = DateTime.Parse(TerminoReal, iFormatProvider);

            DateTime objDataInicioPrevisto = DateTime.Parse(InicioPrevisto, iFormatProvider);
            DateTime objDataTerminoPrevisto = DateTime.Parse(TerminoPrevisto, iFormatProvider);

            string colunasRealizacaoReuniao = indicaRealizacaoReuniao ?
                @"
                        InicioReal              = {6},
	                    TerminoReal             = {7},
	                    ResumoEvento            = '{12}'," :
                        string.Empty;

            comandoSQL = string.Format(@"
                    Declare @fusoHorario int;
                    set @fusoHorario = DATEPART(TZOFFSET, SYSDATETIMEOFFSET());

                    UPDATE {0}.{1}.Evento SET 
	                    DescricaoResumida       = '{2}',
	                    CodigoResponsavelEvento = {3},
	                    InicioPrevisto          = {4},
	                    TerminoPrevisto         = {5},"
                        + colunasRealizacaoReuniao +
                        @"CodigoTipoAssociacao    = {8},
	                    CodigoObjetoAssociado   = {9},
	                    LocalEvento             = '{10}',
	                    Pauta                   = '{11}',
	                    CodigoEntidade          = {13},
	                    DataEnvioPauta          = {14},
	                    DataEnvioAta            = {15},
	                    CodigoTipoEvento        = {16},
	                    DataUltimaAlteracao            = GETDATE(),
	                    CodigoUsuarioUltimaAlteracao   = {17}

                    WHERE CodigoEvento = {18}
                  ", bancodb, Ownerdb, DescricaoResumida, CodigoResponsavelEvento,
                    ("TODATETIMEOFFSET(CONVERT(datetime, '" + objDataInicioPrevisto.ToString("dd/MM/yyyy HH:mm:ss") + "', 103), @fusoHorario)"),
                    ("TODATETIMEOFFSET(CONVERT(datetime, '" + objDataTerminoPrevisto.ToString("dd/MM/yyyy HH:mm:ss") + "', 103), @fusoHorario)"),
                    (InicioReal == "NULL" ? "NULL" : ("TODATETIMEOFFSET(CONVERT(datetime, '" + objDataInicioReal.ToString("dd/MM/yyyy HH:mm:ss") + "', 103), @fusoHorario)")),
                    (TerminoReal == "NULL" ? "NULL" : ("TODATETIMEOFFSET(CONVERT(datetime, '" + objDataTerminoReal.ToString("dd/MM/yyyy HH:mm:ss") + "', 103), @fusoHorario)")),
                    CodigoTipoAssociacao, CodigoObjetoAssociado, LocalEvento, Pauta, ResumoEvento, CodigoEntidade,
                    (DataEnvioPauta == "NULL" ? "NULL" : "CONVERT(DateTime, '" + DataEnvioPauta + "', convert(int, dbo.f_GetTraducao('103')))"),
                    (DataEnvioAta == "NULL" ? "NULL" : "CONVERT(DateTime, '" + DataEnvioAta + "', convert(int, dbo.f_GetTraducao('103')))"),
                    CodigoTipoEvento, CodigoUsuarioInclusao, chave);

            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaExecucaoEvento(int codigoEvento, string InicioReal, string TerminoReal, string ataEvento, string DataEnvioAta, int codigoUsuarioLogado, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {

            comandoSQL = string.Format(@"
                    UPDATE {0}.{1}.Evento SET 
	                    ResumoEvento                   = '{3}',
	                    InicioReal                     = {4},
	                    TerminoReal                    = {5},
	                    DataEnvioAta                   = {6},
	                    DataUltimaAlteracao            = GETDATE(),
	                    CodigoUsuarioUltimaAlteracao   = {7}
                    WHERE CodigoEvento = {2}
                  ", bancodb, Ownerdb, codigoEvento, ataEvento,
                    (InicioReal == "NULL" ? "NULL" : "CONVERT(DateTime, '" + InicioReal + "', convert(int, dbo.f_GetTraducao('103')))"),
                    (TerminoReal == "NULL" ? "NULL" : "CONVERT(DateTime, '" + TerminoReal + "', convert(int, dbo.f_GetTraducao('103')))"),
                    (DataEnvioAta == "NULL" ? "NULL" : "CONVERT(DateTime, '" + DataEnvioAta + "', convert(int, dbo.f_GetTraducao('103')))"),
                     codigoUsuarioLogado);

            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    //Ok
    public bool excluiEvento(int codigoEntidade, int codigoUsuario, string codigoEvento, ref string mensagem)
    {
        int registrosAfetados = 0;
        bool retorno = false;

        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
    EXEC {0}.{1}.[p_brk_reunioes_excluiReuniao]
		    @CodigoEntidadeContexto					= {2}
	    , @CodigoUsuarioSistema						= {3}
	    , @CodigoEventoReuniao						= {4};
                ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoEvento);

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

    // todo: Verificar atualizaEnvioPauta
    public bool atualizaEnvioPauta(string codigoEvento, string codigoUsuarioAlteracao, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                    UPDATE {0}.{1}.Evento SET 
	                    DataEnvioPauta       = GETDATE(),
	                    DataUltimaAlteracao  = GETDATE(),
	                    CodigoUsuarioUltimaAlteracao   = {2}
                    WHERE CodigoEvento = {3}

                    UPDATE [ParticipanteEvento]
                       SET [DataEnvioAta] = GETDATE()
                     WHERE [CodigoEvento] = {3}
                  ", bancodb, Ownerdb, codigoUsuarioAlteracao, codigoEvento);

            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaParticipantesEnvioPautaEmail(string[] arrayParticipantesEventos, string codigoEvento, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;
        try
        {
            foreach (string codigoParticipante in arrayParticipantesEventos)
            {
                if (codigoParticipante != "")
                {
                    // Insere um registro na tabela complementoProjeto
                    comandoSQL += string.Format(@"
                                UPDATE {0}.{1}.ParticipanteEvento SET 
	                                DataEnvioPauta = GETDATE()
                                WHERE CodigoEvento = {2}
                                  AND CodigoParticipante = {3}
                            ", bancodb, Ownerdb, codigoEvento, codigoParticipante);
                }
            }

            if ("" != comandoSQL)
            {
                execSQL(comandoSQL, ref registrosAfetados);
            }
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    // todo: Verificar atualizaEnvioAta
    public bool atualizaEnvioAta(string codigoEvento, string codigoUsuarioAlteracao, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                    UPDATE {0}.{1}.Evento SET 
	                    DataEnvioAta         = GETDATE(),
	                    DataUltimaAlteracao  = GETDATE(),
	                    CodigoUsuarioUltimaAlteracao   = {2}

                    WHERE CodigoEvento = {3}
                  ", bancodb, Ownerdb, codigoUsuarioAlteracao, codigoEvento);

            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaParticipantesEnvioAtaEmail(string[] arrayParticipantesEventos, string codigoEvento, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;
        try
        {
            foreach (string codigoParticipante in arrayParticipantesEventos)
            {
                if (codigoParticipante != "")
                {
                    // Insere um registro na tabela complementoProjeto
                    comandoSQL += string.Format(@"
                                UPDATE {0}.{1}.ParticipanteEvento SET 
	                                DataEnvioAta = GETDATE()
                                WHERE CodigoEvento = {2}
                                  AND CodigoParticipante = {3}
                            ", bancodb, Ownerdb, codigoEvento, codigoParticipante);
                }
            }

            if (comandoSQL != "")
            {
                execSQL(comandoSQL, ref registrosAfetados);
                retorno = true;
            }
            else
            {
                msgError = Resources.traducao.dados_nenhum_participante_foi_selecionado_;
                retorno = false;
            }

        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    // todo: verificar icnluirExecucaoEvento
    // incluido um novo parâmetro string desativadaem24092013 para invalidade qualquer chamada existente a este procedimento pois a tabela acao não mais existe
    //    public bool incluiExecucaoEvento(int codigoEvento, string resumoReuniao, string inicioRealReuniao, string terminoRealReuniao,
    //       DataTable dtTarefas, int idUsuarioInclusao, ref int registrosAfetados, string desativadaem24092013)
    //    {
    //        string descricaoTarefa, terminoPrevistoTarefa, inicioPrevistoTarefa, comentariosTarefa;
    //        int codigoResponsavelTarefa;

    //        string insertTarefas = "";

    //        registrosAfetados = 0;

    //        try
    //        {
    //            int i = 0;

    //            for (i = 0; i < dtTarefas.Rows.Count; i++)
    //            {
    //                descricaoTarefa = dtTarefas.Rows[i]["DescricaoAcao"].ToString();
    //                inicioPrevistoTarefa = dtTarefas.Rows[i]["InicioPrevisto"].ToString();
    //                terminoPrevistoTarefa = dtTarefas.Rows[i]["TerminoPrevisto"].ToString();
    //                comentariosTarefa = dtTarefas.Rows[i]["Anotacoes"].ToString();
    //                codigoResponsavelTarefa = int.Parse(dtTarefas.Rows[i]["CodigoResponsavel"].ToString());

    //                insertTarefas += string.Format(@"INSERT INTO {0}.{1}.Acao (DescricaoAcao, InicioPrevisto, TerminoPrevisto, CodigoResponsavelAcao, Anotacoes, CodigoEvento, CodigoUsuarioInclusao, DataInclusao, PercentualConcluido)
    //	                                                    VALUES ('{2}', CONVERT(DateTime, '{3}', 103), CONVERT(DateTime, '{4}', 103), {5}, '{6}', @CodigoEvento, {7}, GetDate(), 0);",
    //                                            bancodb, Ownerdb, descricaoTarefa, inicioPrevistoTarefa, terminoPrevistoTarefa, codigoResponsavelTarefa, comentariosTarefa, idUsuarioInclusao);
    //            }

    //            string comandoSQL = string.Format(
    //                    @"BEGIN
    //                            begin tran T1
    //                            DECLARE @CodigoEvento int
    //                            SET @CodigoEvento = {2}
    //
    //                            DELETE FROM {0}.{1}.Acao WHERE CodigoEvento = @CodigoEvento;
    //
    //                            UPDATE {0}.{1}.Evento SET ResumoEvento = '{3}', 
    //                                                      {4} 
    //                                                      {5}
    //                                                      DataUltimaAlteracao = GetDate()                                                      
    //                             WHERE Codigo = @CodigoEvento;
    //                                                        
    //                            {6}
    //
    //                            if @@ERROR > 0
    //                                ROLLBACK TRAN T1
    //                            else
    //                                COMMIT TRAN T1
    //                        END                    
    //                     ", bancodb, Ownerdb, codigoEvento, resumoReuniao,
    //                  inicioRealReuniao != "" ? "InicioReal = CONVERT(DateTime, '" + inicioRealReuniao + "', 103)," : "",
    //                  terminoRealReuniao != "" ? "TerminoReal = CONVERT(DateTime, '" + terminoRealReuniao + "', 103)," : "",
    //                  insertTarefas);

    //            execSQL(comandoSQL, ref registrosAfetados);
    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            throw new Exception(getErroIncluiRegistro(ex.Message));
    //        }
    //    }

    // todo: verificar getTarefasEventos
    // incluido um novo parâmetro string desativadaem24092013 para invalidade qualquer chamada existente a este procedimento pois a tabela acao não mais existe
    //    public DataSet getTarefasEventos(int codigoEvento, string where, string desativadaem24092013)
    //    {
    //        comandoSQL = string.Format(@"SELECT t.CodigoAcao AS Codigo, t.DescricaoAcao, CodigoResponsavelAcao AS CodigoResponsavel, u.NomeUsuario AS Responsavel, 
    //                                           InicioPrevisto,
    //                                           TerminoPrevisto, 
    //                                           Anotacoes
    //                                      FROM {0}.{1}.Acao t
    //                                      LEFT JOIN {0}.{1}.Usuario u ON u.CodigoUsuario = CodigoResponsavelAcao
    //                                     WHERE CodigoEvento = {2} {3}", bancodb, Ownerdb, codigoEvento, where);

    //        DataSet ds = getDataSet(comandoSQL);
    //        return ds;
    //    }

    // Ok

    public string enviarEmail(string _Assunto, string _EmailDestinatario, string _EmailCopia, string _Mensagem, string anexo, string invitation, ref int retornoStatus)
    {
        // a princípio, o e-mail será enviado pelo servidor IIS
        // --------------------------------------------------------------
        bool EnviarEmailPeloBancoDeDados = false;

        //busca o responsável por enviar o e-mail na tabela de parametros
        // --------------------------------------------------------------
        DataSet ds = getParametrosSistema("EnviarEmailPeloBancoDeDados");
        if (ds != null && ds.Tables[0] != null)
            EnviarEmailPeloBancoDeDados = ds.Tables[0].Rows[0]["EnviarEmailPeloBancoDeDados"] != null ? ds.Tables[0].Rows[0]["EnviarEmailPeloBancoDeDados"].ToString() == "S" : false;

        // Se não for para ser enviado pelo banco...
        if (!EnviarEmailPeloBancoDeDados) // O EMAIL SERÁ ENVIADO PELO SERVIDOR DE APLICAÇÃO
        {
            return enviarEmailAppWEB(_Assunto, _EmailDestinatario, _EmailCopia, _Mensagem, anexo, invitation, ref retornoStatus);
        }
        else  // O EMAIL SERÁ ENVIADO PELO BANCO DE DADOS
        {
            // Retira as sequencias de espaços em branco e troca as aspas simples por "aspas aspas"
            // *** Não removam as linhas abaixo. Se der algum problema me avise - ACG ***
            // --------------------------------------------------------------------------
            _Assunto = _Assunto.Replace("  ", "").Replace("'", "''");
            _Mensagem = _Mensagem.Replace("  ", "").Replace("'", "''");
            invitation = invitation.Replace("  ", "").Replace("'", "''");

            retornoStatus = 1;

            // se está configurado para enviar e-mail
            if (enviarEmailSistema == "S")
            {
                if (_EmailDestinatario == "" || _Assunto == "" || _Mensagem == "")
                {
                    retornoStatus = 0;
                    return Resources.traducao.dados_informa__es_insuficientes_para_o_envio_do_e_mail__email_do_destinat_rio__assunto__mensagem__;
                }

                try
                {
                    // se houver convite, trocaremos a quebra de linha de "/n/r" apenas por "/r" e será incluída o comando "SELECT" para ser tratado pelo SQLSERVER
                    string convite = invitation.Replace("/n/r", "/r");
                    if (convite != "")
                        convite = string.Format("SET NOCOUNT ON; SELECT ''{0}''", convite);

                    string comandoSQL = "";
                    string guidAnexo = "";
                    // se houver anexo, este será salvo na tabela "CDIS_ANEXOENVIOEMAIL"
                    if (anexo != "")
                    {
                        guidAnexo = Guid.NewGuid().ToString();
                        byte[] arquivo = File.ReadAllBytes(anexo);
                        comandoSQL = string.Format(
                            @"INSERT INTO CDIS_ANEXOENVIOEMAIL (ID, Nome, data, Conteudo) values ('{0}', '{1}', getdate(),  @conteudo)

                              ", guidAnexo, Path.GetFileName(anexo));

                        using (SqlConnection conn = new SqlConnection(strConn))
                        {
                            conn.Open();
                            SqlCommand comm = new SqlCommand(comandoSQL, conn);
                            //comm.Parameters.Add(new SqlParameter[] { new SqlParameter("@conteudo", SqlDbType.Image) });
                            comm.Parameters.Add(new SqlParameter("@conteudo", SqlDbType.Image, 0, ParameterDirection.Input, 0, 0, "conteudo", DataRowVersion.Current, false, null, "", "", ""));
                            comm.Parameters["@conteudo"].Value = arquivo;

                            comm.CommandTimeout = CDIS.ClasseDados.TimeOutSqlCommand;
                            comm.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                    // O envio de e-mail será feito pelo procedimento "p_EnviaEmail" do banco de dados
                    // -------------------------------------------------------------------------------
                    comandoSQL = string.Format("exec p_EnviaEmailPortal {0}, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}'", getInfoSistema("CodigoEntidade"), _EmailDestinatario, _EmailCopia, _Assunto, _Mensagem, convite, guidAnexo);

                    int registrosAfetados = 0;
                    execSQL(comandoSQL, ref registrosAfetados);

                    return Resources.traducao.dados_e_mail_inclu_do_na_fila_de_entrega__destinat_rio_s___ + _EmailDestinatario;
                }
                catch (Exception ex)
                {
                    retornoStatus = 0;
                    return Resources.traducao.dados_ocorreram_problemas_no_envio_do_e_mail__error___ + ex.Message;
                }
            }
            else
            {
                retornoStatus = 0;
                return Resources.traducao.dados_as_configura__es_do_sistema_n_o_permitem_envio_de_emails;
            }
        }
    }


    //Esse Método garante que independente dos dados da Parametros sistema o sistem irá enviar o email via sistema e não por banco de dados.
    public string enviarEmailSistemaNetSend(string _Assunto, string _EmailDestinatario, string _EmailCopia, string _Mensagem, string anexo, string invitation, ref int retornoStatus)
    {
        return enviarEmailAppWEB(_Assunto, _EmailDestinatario, _EmailCopia, _Mensagem, anexo, invitation, ref retornoStatus);
    }

    public string enviarEmailCalendar(string _Assunto, string _EmailDestinatario, string _EmailCopia, string _Mensagem, string anexo, string invitation, string calendario, ref int retornoStatus)
    {
        // a princípio, o e-mail será enviado pelo servidor IIS
        // --------------------------------------------------------------
        bool EnviarEmailPeloBancoDeDados = true;

        //busca o responsável por enviar o e-mail na tabela de parametros
        // --------------------------------------------------------------
        DataSet ds = getParametrosSistema("EnviarEmailPeloBancoDeDados");
        if (ds != null && ds.Tables[0] != null)
            EnviarEmailPeloBancoDeDados = ds.Tables[0].Rows[0]["EnviarEmailPeloBancoDeDados"] != null ? ds.Tables[0].Rows[0]["EnviarEmailPeloBancoDeDados"].ToString() == "S" : false;

        // Se não for para ser enviado pelo banco...
        if (EnviarEmailPeloBancoDeDados) // O EMAIL SERÁ ENVIADO PELO SERVIDOR DE APLICAÇÃO
        {
            return enviarEmailAppWEB(_Assunto, _EmailDestinatario, _EmailCopia, _Mensagem, "", calendario, calendario, ref retornoStatus);
        }
        else  // O EMAIL SERÁ ENVIADO PELO BANCO DE DADOS
        {
            // Retira as sequencias de espaços em branco e troca as aspas simples por "aspas aspas"
            // *** Não removam as linhas abaixo. Se der algum problema me avise - ACG ***
            // --------------------------------------------------------------------------
            _Assunto = _Assunto.Replace("  ", "").Replace("'", "''");
            _Mensagem = _Mensagem.Replace("  ", "").Replace("'", "''");
            invitation = invitation.Replace("  ", "").Replace("'", "''");

            retornoStatus = 1;

            // se está configurado para enviar e-mail
            if (enviarEmailSistema == "S")
            {
                if (_EmailDestinatario == "" || _Assunto == "" || _Mensagem == "")
                {
                    retornoStatus = 0;
                    return Resources.traducao.dados_informa__es_insuficientes_para_o_envio_do_e_mail__email_do_destinat_rio__assunto__mensagem__;
                }

                try
                {
                    // se houver convite, trocaremos a quebra de linha de "/n/r" apenas por "/r" e será incluída o comando "SELECT" para ser tratado pelo SQLSERVER
                    string convite = invitation.Replace("/n/r", "/r");
                    if (convite != "")
                        convite = string.Format("SET NOCOUNT ON; SELECT ''{0}''", convite);

                    string comandoSQL = "";
                    string guidAnexo = "";
                    // se houver anexo, este será salvo na tabela "CDIS_ANEXOENVIOEMAIL"
                    if (anexo != "")
                    {
                        guidAnexo = Guid.NewGuid().ToString();
                        byte[] arquivo = File.ReadAllBytes(anexo);
                        comandoSQL = string.Format(
                            @"INSERT INTO CDIS_ANEXOENVIOEMAIL (ID, Nome, data, Conteudo) values ('{0}', '{1}', getdate(),  @conteudo)

                              ", guidAnexo, Path.GetFileName(anexo));

                        using (SqlConnection conn = new SqlConnection(strConn))
                        {
                            conn.Open();
                            SqlCommand comm = new SqlCommand(comandoSQL, conn);
                            //comm.Parameters.Add(new SqlParameter[] { new SqlParameter("@conteudo", SqlDbType.Image) });
                            comm.Parameters.Add(new SqlParameter("@conteudo", SqlDbType.Image, 0, ParameterDirection.Input, 0, 0, "conteudo", DataRowVersion.Current, false, null, "", "", ""));
                            comm.Parameters["@conteudo"].Value = arquivo;

                            comm.CommandTimeout = CDIS.ClasseDados.TimeOutSqlCommand;
                            comm.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                    //Caso passe o Objeto Calendário Irá Gerar Um Calendário para Outlook e Google objeto iCalendar
                    if (calendario != null)
                    {
                        guidAnexo = Guid.NewGuid().ToString();
                        byte[] arquivo = File.ReadAllBytes(calendario);
                        comandoSQL = string.Format(
                            @"INSERT INTO CDIS_ANEXOENVIOEMAIL (ID, Nome, data, Conteudo) values ('{0}', '{1}', getdate(),  @conteudo)

                              ", guidAnexo, Path.GetFileName(calendario));

                        using (SqlConnection conn = new SqlConnection(strConn))
                        {
                            conn.Open();
                            SqlCommand comm = new SqlCommand(comandoSQL, conn);
                            //comm.Parameters.Add(new SqlParameter[] { new SqlParameter("@conteudo", SqlDbType.Image) });
                            comm.Parameters.Add(new SqlParameter("@conteudo", SqlDbType.Image, 0, ParameterDirection.Input, 0, 0, "conteudo", DataRowVersion.Current, false, null, "", "", ""));
                            comm.Parameters["@conteudo"].Value = arquivo;

                            comm.CommandTimeout = CDIS.ClasseDados.TimeOutSqlCommand;
                            comm.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                    // O envio de e-mail será feito pelo procedimento "p_EnviaEmail" do banco de dados
                    // -------------------------------------------------------------------------------
                    comandoSQL = string.Format("exec p_EnviaEmailPortal {0}, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}'", getInfoSistema("CodigoEntidade"), _EmailDestinatario, _EmailCopia, _Assunto, _Mensagem, convite, guidAnexo);

                    int registrosAfetados = 0;
                    execSQL(comandoSQL, ref registrosAfetados);

                    return Resources.traducao.dados_e_mail_inclu_do_na_fila_de_entrega__destinat_rio_s___ + _EmailDestinatario;
                }
                catch (Exception ex)
                {
                    retornoStatus = 0;
                    return Resources.traducao.dados_ocorreram_problemas_no_envio_do_e_mail__error___ + ex.Message;
                }
            }
            else
            {
                retornoStatus = 0;
                return Resources.traducao.dados_as_configura__es_do_sistema_n_o_permitem_envio_de_emails;
            }
        }
    }

    public string enviarEmailAppWEB(string _Assunto, string _EmailDestinatario, string _EmailCopia, string _Mensagem, string anexo, string invitation, ref int retornoStatus)
    {
        retornoStatus = 1;

        //string enviarEmailSistema = "N";

        //DataSet ds = getParametrosSistema("EnviarEmailSistema");

        //if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        //{
        //    enviarEmailSistema = ds.Tables[0].Rows[0]["EnviarEmailSistema"].ToString();
        //}

        if (enviarEmailSistema == "N")
        {
            retornoStatus = 0;
            return Resources.traducao.dados_as_configura__es_do_sistema_n_o_permitem_envio_de_emails;
        }

        string smtpServer = "";
        string smtpUser = "";
        string smtpPassword = "";
        string smtpPort = "";
        bool ssl = false;

        //busca as informações do smtp na tabela de parametros
        DataSet ds = getParametrosSistema("smtpServer", "smtpUser", "smtpPassword", "smtpPort", "smtpUtilizaSSL", "remetenteEmailProjeto");

        string _EmailRemetente = "Portal da Estratégia<admin@project.com>";

        if (ds != null && ds.Tables[0] != null)
        {
            smtpServer = ds.Tables[0].Rows[0]["smtpServer"] != null ? ds.Tables[0].Rows[0]["smtpServer"].ToString() : "";
            smtpUser = ds.Tables[0].Rows[0]["smtpUser"] != null ? ds.Tables[0].Rows[0]["smtpUser"].ToString() : "";
            smtpPassword = ds.Tables[0].Rows[0]["smtpPassword"] != null ? ds.Tables[0].Rows[0]["smtpPassword"].ToString() : "";
            smtpPort = ds.Tables[0].Rows[0]["smtpPort"] + "" != "" ? ds.Tables[0].Rows[0]["smtpPort"].ToString() : "";
            ssl = ds.Tables[0].Rows[0]["smtpUtilizaSSL"] + "" == "S";
            _EmailRemetente = ds.Tables[0].Rows[0]["remetenteEmailProjeto"] != null ? ds.Tables[0].Rows[0]["remetenteEmailProjeto"].ToString() : _EmailRemetente;
        }

        if (smtpServer.Trim() == "")
        {
            retornoStatus = 0;
            return Resources.traducao.dados_as_configura__es_do_servidor_de_emails_n_o_foram_encontradas_no_banco_de_dados_;
        }

        //cria objeto com dados do e-mail
        System.Net.Mail.MailMessage objEmail = new System.Net.Mail.MailMessage();

        if (anexo != "")
        {
            objEmail.Attachments.Add(new Attachment(anexo));
        }

        if (invitation != "")
        {
            System.Net.Mime.ContentType typeC = new System.Net.Mime.ContentType("text/calendar");
            typeC.Parameters.Add("method", "REQUEST");
            typeC.Parameters.Add("name", "Evento.ics");
            AlternateView m_calV = AlternateView.CreateAlternateViewFromString(invitation, typeC);

            objEmail.AlternateViews.Add(m_calV);
        }

        //remetente do e-mail
        try
        {
            objEmail.From = new System.Net.Mail.MailAddress(_EmailRemetente);
        }
        catch (Exception ex)
        {
            retornoStatus = 0;
            return Resources.traducao.dados_email_remetente_inv_lido__consulte_os_par_metros_do_sistema_ + Environment.NewLine + ex.Message;
        }

        //destinatários do e-mail
        foreach (string emailDest in _EmailDestinatario.Split(';'))
        {
            if (emailDest.Trim() != "")
            {
                try
                {
                    if (objEmail.To.Contains(new MailAddress(emailDest)) == false)
                        objEmail.To.Add(new MailAddress(emailDest));
                }
                catch { }
            }
        }

        if (_EmailCopia != "")
        {
            foreach (string emailCopia in _EmailCopia.Split(';'))
            {
                if (objEmail.To.Contains(new MailAddress(emailCopia)) == false && objEmail.Bcc.Contains(new MailAddress(emailCopia)) == false)
                    objEmail.Bcc.Add(emailCopia);
            }
        }

        //prioridade do e-mail
        objEmail.Priority = System.Net.Mail.MailPriority.Normal;

        //formato do e-mail HTML (caso não queira HTML alocar valor false)
        objEmail.IsBodyHtml = true;

        //título do e-mail
        objEmail.Subject = _Assunto;

        System.Net.Mime.ContentType typeMsg = new System.Net.Mime.ContentType("text/html");
        AlternateView m_Message = AlternateView.CreateAlternateViewFromString(_Mensagem, typeMsg);

        objEmail.AlternateViews.Add(m_Message);

        //corpo do e-mail
        //objEmail.Body = _Mensagem;

        //Para evitar problemas de caracteres "estranhos", configuramos o charset para "ISO-8859-1"
        objEmail.SubjectEncoding = System.Text.Encoding.GetEncoding("UTF-8");
        objEmail.BodyEncoding = System.Text.Encoding.GetEncoding("UTF-8");

        //cria objeto com os dados do SMTP
        SmtpClient objSmtp = new SmtpClient();

        //alocamos o endereço do host para enviar os e-mails, localhost(recomendado) ou smtp2.locaweb.com.br
        objSmtp.Host = smtpServer;

        if (smtpPort.Trim() != "")
            objSmtp.Port = int.Parse(smtpPort);

        objSmtp.EnableSsl = ssl;

        //enviamos o e-mail através do método .send()
        try
        {
            /* Bloco comentado em 08/07/2009 por ACG - Para contas internar não precisa autenticação
			 objSmtp.UseDefaultCredentials = false;

			 //to authenticate we set the username and password properites on the SmtpClient
			 objSmtp.Credentials = new NetworkCredential(smtpUser, smtpPassword);
			 */
            //se tem senha, vamos utilizar.
            if (smtpPassword != "")
                objSmtp.Credentials = new NetworkCredential(smtpUser, smtpPassword);

            objSmtp.Send(objEmail);

            //excluímos o objeto de e-mail da memória
            objEmail.Dispose();

            return Resources.traducao.dados_e_mail_enviado_com_sucesso_para__ + _EmailDestinatario;
        }
        catch (Exception ex)
        {
            //excluímos o objeto de e-mail da memória
            objEmail.Dispose();
            retornoStatus = 0;
            return Resources.traducao.dados_ocorreram_problemas_no_envio_do_e_mail__error___ + ex.Message;
        }
    }


    public string enviarEmailAppWEB(string _Assunto, string _EmailDestinatario, string _EmailCopia, string _Mensagem, string anexo, string invitation, string calendario, ref int retornoStatus)
    {
        retornoStatus = 1;
        if (enviarEmailSistema == "N")
        {
            retornoStatus = 0;
            return Resources.traducao.dados_as_configura__es_do_sistema_n_o_permitem_envio_de_emails;
        }
        string smtpServer = "";
        string smtpUser = "";
        string smtpPassword = "";
        string smtpPort = "";
        bool ssl = false;
        //busca as informações do smtp na tabela de parametros
        DataSet ds = getParametrosSistema("smtpServer", "smtpUser", "smtpPassword", "smtpPort", "smtpUtilizaSSL", "remetenteEmailProjeto");
        string _EmailRemetente = "Portal da Estratégia<admin@project.com>";
        if (ds != null && ds.Tables[0] != null)
        {
            smtpServer = ds.Tables[0].Rows[0]["smtpServer"] != null ? ds.Tables[0].Rows[0]["smtpServer"].ToString() : "";
            smtpUser = ds.Tables[0].Rows[0]["smtpUser"] != null ? ds.Tables[0].Rows[0]["smtpUser"].ToString() : "";
            smtpPassword = ds.Tables[0].Rows[0]["smtpPassword"] != null ? ds.Tables[0].Rows[0]["smtpPassword"].ToString() : "";
            smtpPort = ds.Tables[0].Rows[0]["smtpPort"] + "" != "" ? ds.Tables[0].Rows[0]["smtpPort"].ToString() : "";
            ssl = ds.Tables[0].Rows[0]["smtpUtilizaSSL"] + "" == "S";
            _EmailRemetente = ds.Tables[0].Rows[0]["remetenteEmailProjeto"] != null ? ds.Tables[0].Rows[0]["remetenteEmailProjeto"].ToString() : _EmailRemetente;
        }

        if (smtpServer.Trim() == "")
        {
            retornoStatus = 0;
            return Resources.traducao.dados_as_configura__es_do_servidor_de_emails_n_o_foram_encontradas_no_banco_de_dados_;
        }
        //cria objeto com dados do e-mail
        System.Net.Mail.MailMessage objEmail = new System.Net.Mail.MailMessage();
        if (anexo != "")
        {
            objEmail.Attachments.Add(new Attachment(anexo));
        }

        if (invitation != "")
        {
            System.Net.Mime.ContentType typeC = new System.Net.Mime.ContentType("text/calendar");
            typeC.Parameters.Add("method", "REQUEST");
            typeC.Parameters.Add("name", "Evento.ics");
            AlternateView m_calV = AlternateView.CreateAlternateViewFromString(invitation, typeC);

            objEmail.AlternateViews.Add(m_calV);
        }

        if (calendario != "")
        {
            System.Net.Mime.ContentType typeC = new System.Net.Mime.ContentType("text/calendar");

            typeC.Parameters.Add("method", "REQUEST");
            typeC.Parameters.Add("name", "Evento.ics");
            AlternateView m_calV = AlternateView.CreateAlternateViewFromString(calendario, typeC);
            objEmail.AlternateViews.Add(m_calV);

            objEmail.Attachments.Add(new Attachment(calendario, typeC));
        }






        //remetente do e-mail
        try
        {
            objEmail.From = new System.Net.Mail.MailAddress(_EmailRemetente);
        }
        catch (Exception ex)
        {
            retornoStatus = 0;
            return Resources.traducao.dados_email_remetente_inv_lido__consulte_os_par_metros_do_sistema_ + Environment.NewLine + ex.Message;
        }

        //destinatários do e-mail
        foreach (string emailDest in _EmailDestinatario.Split(';'))
        {
            if (emailDest.Trim() != "")
            {
                try
                {
                    if (objEmail.To.Contains(new MailAddress(emailDest)) == false)
                        objEmail.To.Add(new MailAddress(emailDest));
                }
                catch { }
            }
        }

        if (_EmailCopia != "")
        {
            foreach (string emailCopia in _EmailCopia.Split(';'))
            {
                if (objEmail.To.Contains(new MailAddress(emailCopia)) == false && objEmail.Bcc.Contains(new MailAddress(emailCopia)) == false)
                    objEmail.Bcc.Add(emailCopia);
            }
        }

        //prioridade do e-mail
        objEmail.Priority = System.Net.Mail.MailPriority.Normal;

        //formato do e-mail HTML (caso não queira HTML alocar valor false)
        objEmail.IsBodyHtml = true;

        //título do e-mail
        objEmail.Subject = _Assunto;

        System.Net.Mime.ContentType typeMsg = new System.Net.Mime.ContentType("text/html");
        AlternateView m_Message = AlternateView.CreateAlternateViewFromString(_Mensagem, typeMsg);

        objEmail.AlternateViews.Add(m_Message);

        //corpo do e-mail
        //objEmail.Body = _Mensagem;

        //Para evitar problemas de caracteres "estranhos", configuramos o charset para "ISO-8859-1"
        objEmail.SubjectEncoding = System.Text.Encoding.GetEncoding("UTF-8");
        objEmail.BodyEncoding = System.Text.Encoding.GetEncoding("UTF-8");

        //cria objeto com os dados do SMTP
        SmtpClient objSmtp = new SmtpClient();

        //alocamos o endereço do host para enviar os e-mails, localhost(recomendado) ou smtp2.locaweb.com.br
        objSmtp.Host = smtpServer;

        if (smtpPort.Trim() != "")
            objSmtp.Port = int.Parse(smtpPort);

        objSmtp.EnableSsl = ssl;

        //enviamos o e-mail através do método .send()
        try
        {
            /* Bloco comentado em 08/07/2009 por ACG - Para contas internar não precisa autenticação
			 objSmtp.UseDefaultCredentials = false;

			 //to authenticate we set the username and password properites on the SmtpClient
			 objSmtp.Credentials = new NetworkCredential(smtpUser, smtpPassword);
			 */
            //se tem senha, vamos utilizar.
            if (smtpPassword != "")
                objSmtp.Credentials = new NetworkCredential(smtpUser, smtpPassword);

            objSmtp.Send(objEmail);

            //excluímos o objeto de e-mail da memória
            objEmail.Dispose();

            return Resources.traducao.dados_e_mail_enviado_com_sucesso_para__ + _EmailDestinatario;
        }
        catch (Exception ex)
        {
            //excluímos o objeto de e-mail da memória
            objEmail.Dispose();
            retornoStatus = 0;
            return Resources.traducao.dados_ocorreram_problemas_no_envio_do_e_mail__error___ + ex.Message;
        }
    }

    /// <summary>
    /// Função criada para enviar o e-mail com o 'anexo' CALENDAR sempre pelo aplication server.
    /// Diferentemente da enviarEmailCalendar(), que envia o e-mail pelo AppServer quando o parâmetro "EnviarEmailPeloBancoDeDados" está configurado como "S" (???!!!); e pelo banco de dados 
    /// quando esse mesmo parâmetro está "N" (???!!!), essa rotina sempre enviará o e-mail pelo AppServer, indiferente ao parâmetro.
    /// Essa abordagem foi adotada em virtude da seginte situação: 
    ///    1. Na implementação atual de envio de e-mail pelo SQL Server, para enviar e-mails com anexos (arquivos CALENDAR *.ics), o usuário da conexão ao banco de dados precisa ter permissões que são vistas 
    ///       como fontes de falhas de segurança (acionar comando do DOS dentro do SQL Server, acionar as bibliotecas da API do SQLCMD etc)
    ///    2. Por conta disso, o envio de tais e-mails pelo AppServer representam menos riscos. E essa deve ser a razão pela qual foi trocado na enviarEmailCalendar() o entendimento do 
    ///       parâmetro "EnviarEmailPeloBancoDeDados".
    ///    3. A versão original da enviarEmailCalendar(), a enviarEmail() manteve o entendimento original e correto do parâmetro "EnviarEmailPeloBancoDeDados", de forma que quando este está com o valor "S",
    ///       ela 'tenta' o envio pelo banco de dados, que falha na maioria das vezes em razão do exposto no item 1.
    /// </summary>
    /// <param name="_Assunto">Texto a ser utilizado no assunto do e-mail</param>
    /// <param name="_EmailDestinatario">Lista dos e-mails destinatários da mensagem, separados por ponto e vírgula (;)</param>
    /// <param name="_EmailCopia">Lista dos e-mails que receberão a mensagem em cópia, separados por ponto e vírgula (;)</param>
    /// <param name="_Mensagem">Texto a ser usado como o corpo do e-mail</param>
    /// <param name="conteudoInvitation">Conteúdo VCALENDAR a ser enviado no e-mail.</param>
    /// <param name="retornoStatus">Valor indicando sucesso ou falha do envio de e-mail. O valor 1 indica sucesso, enquanto que o valor 0 indica falha</param>
    /// <returns></returns>
    public string enviarEmailCalendarAppWeb(string _Assunto, string _EmailDestinatario, string _EmailCopia, string _Mensagem, string conteudoInvitation, ref int retornoStatus)
    {
        // chamando a mesma sobrecarga da rotina enviarEmailAppWEB utilizada na enviarEmailCalendar()
        return enviarEmailAppWEB(_Assunto, _EmailDestinatario, _EmailCopia, _Mensagem, "", conteudoInvitation, "", ref retornoStatus);
    }

    // todo: verificar incluiNovoResponsavel
    public DataSet incluiNovoResponsavel(string nome, string email, int codigoUnidade, int usuarioLogado, string senha)
    {
        string comandoSQL = string.Format(
            @"USE {0}
                    DECLARE	@Value int
                    EXEC	@Value = {0}.{1}.[p_IncluiNovoUsuarioSindicato] '{2}', '{3}', {4}, {5}, {6}
                    SELECT	'VAlue' = @Value
              
                    SELECT CodigoUsuario As idUsuarioNovo
                    FROM {0}.{1}.Usuario 
                    WHERE EMail = '{3}'", bancodb, Ownerdb, nome, email, codigoUnidade, usuarioLogado, senha);

        return getDataSet(comandoSQL);
    }

    #endregion
}
}