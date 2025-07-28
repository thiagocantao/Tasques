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
    #region agenda

    public DataSet getAgenda(string where, int codigoUsuario, int codigoEntidade)
    {
        comandoSQL = string.Format(
                                    @"BEGIN
	                                    DECLARE @CodigoUsuario int,
			                                    @CodigoEntidade int
                                    			
	                                    SET @CodigoEntidade = {2}
	                                    SET @CodigoUsuario = {3}
                                    			
	                                    DECLARE @tbResumo TABLE(CodigoCompromissoUsuario bigint,
					                                            CodigoUsuario int,
							                                    Status int,
							                                    Assunto VarChar(100),
							                                    Anotacoes Text,
							                                    Rotulo int,
							                                    DataInicio DateTime,
                                                                HorarioInicio varchar(5),
							                                    DataTermino DateTime,
                                                                HorarioTermino varchar(5),
							                                    Local VarChar(250),
							                                    DiaInteiro bit,
							                                    TipoEvento int,
							                                    DescricaoRecorrencia nText,
							                                    DescricaoAlerta nText,
                                                                Tipo Char(1))
                                    							
	                                    INSERT INTO @tbResumo							
		                                    SELECT CodigoCompromissoUsuario
				                                    ,CodigoUsuario
				                                    ,Status
				                                    ,Assunto
				                                    ,Anotacoes
				                                    ,Rotulo
				                                    ,DataInicio
                                                    ,(select CONVERT(VARCHAR(5),DataInicio, 108) AS HorarioInicio)
				                                    ,DataTermino
                                                    ,(select CONVERT(VARCHAR(5),DataTermino, 108) AS HorarioTermino)
				                                    ,Local
				                                    ,DiaInteiro
				                                    ,TipoEvento
				                                    ,DescricaoRecorrencia
				                                    ,DescricaoAlerta
                                                    ,'C'
		                                     FROM {0}.{1}.CompromissoUsuario
		                                    WHERE CodigoUsuario = @CodigoUsuario
		                                      AND CodigoEntidade = @CodigoEntidade 
                                    		  
	                                    INSERT INTO @tbResumo							
		                                    SELECT e.CodigoEvento * -1, 
			                                       e.CodigoResponsavelEvento, 
			                                       2,
			                                       e.DescricaoResumida,
			                                       e.Pauta,
			                                       0,
			                                       CASE WHEN e.InicioReal IS NULL THEN e.InicioPrevisto ELSE e.InicioReal END,
                                                   (select CONVERT(VARCHAR(5), CASE WHEN e.InicioReal IS NULL THEN e.InicioPrevisto ELSE e.InicioReal END, 108)),
			                                       CASE WHEN e.TerminoReal IS NULL THEN e.TerminoPrevisto ELSE e.TerminoReal END,
                                                   (select CONVERT(VARCHAR(5), CASE WHEN e.TerminoReal IS NULL THEN e.TerminoPrevisto ELSE e.TerminoReal END, 108)),
			                                       e.LocalEvento,
			                                       0,
			                                       0,
			                                       null,
			                                       null,
                                                   'R'
                                         FROM {0}.{1}.Evento as e
                                        WHERE (e.CodigoEvento IN (SELECT pe.CodigoEvento FROM {0}.{1}.ParticipanteEvento pe WHERE pe.CodigoParticipante = @CodigoUsuario)
                                                    OR e.CodigoResponsavelEvento = @CodigoUsuario)
                                          AND CodigoEntidade = @CodigoEntidade 

		                                      --FROM {0}.{1}.Evento as e INNER JOIN
			                                  --     {0}.{1}.ParticipanteEvento AS pe  ON (pe.CodigoEvento = e.CodigoEvento)
		                                     --WHERE (pe.CodigoParticipante = @CodigoUsuario
				                              --      OR e.CodigoResponsavelEvento = @CodigoUsuario)
		                                      -- AND CodigoEntidade = @CodigoEntidade 
                                    		   
	                                    SELECT * FROM @tbResumo
                                    END", bancodb, Ownerdb, codigoEntidade, codigoUsuario, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public void incluiAgenda(int type, string inicio, string termino, int diaTodo, int status, int categoria, string descricao, string local, string recurrenceInfo, string reminderInfo, int resourceID, string assunto, int ID, int codigoUsuario, ref int registrosAfetados, int codigoEntidade)
    {

        comandoSQL = string.Format(@"INSERT INTO {0}.{1}.CompromissoUsuario (CodigoUsuario,Status,Assunto,Anotacoes,Rotulo,DataInicio,DataTermino,Local,DiaInteiro,TipoEvento,DescricaoRecorrencia,DescricaoAlerta,CodigoEntidade, IndicaEntradaDiretaAgenda)
                                                                      VALUES({2}, {3}, '{4}', '{5}',   {6},     CONVERT(DateTime, '{7}', 103),      CONVERT(DateTime, '{8}', 103),'{9}',      {10},      {11},              '{12}',         '{13}',          {14},'{15}')
               ", bancodb, Ownerdb, codigoUsuario, status, assunto.Replace("'", "''"), descricao.Replace("'", "''"), categoria, inicio, termino, local.Replace("'", "''"), diaTodo, type, recurrenceInfo, reminderInfo, codigoEntidade, 'S');

        execSQL(comandoSQL, ref registrosAfetados);

    }

    public void atualizaAgenda(int type, string inicio, string termino, int diaTodo, int status, int categoria, string descricao, string local, string recurrenceInfo, string reminderInfo, int resourceID, string assunto, int ID, int codigoUsuario, int codigoAgenda, ref int registrosAfetados, int codigoEntidade)
    {

        try
        {
            //Valida o Formato da data no padrão 103 como pede na proc.
            inicio = Convert.ToDateTime(inicio).Day.ToString() + "/" + Convert.ToDateTime(inicio).Month.ToString() + "/" + Convert.ToDateTime(inicio).Year.ToString() + " " + Convert.ToDateTime(inicio).TimeOfDay.ToString();
            termino = Convert.ToDateTime(termino).Day.ToString() + "/" + Convert.ToDateTime(termino).Month.ToString() + "/" + Convert.ToDateTime(termino).Year.ToString() + " " + Convert.ToDateTime(termino).TimeOfDay.ToString();
        }
        catch (Exception)
        {

        }


        comandoSQL = string.Format(@"UPDATE {0}.{1}.CompromissoUsuario
                                       SET CodigoUsuario = {2}
                                          ,Status = {3}
                                          ,Assunto = '{4}'
                                          ,Anotacoes = '{5}'
                                          ,Rotulo = {6}
                                          ,DataInicio = CONVERT(DateTime, '{7}', 103)
                                          ,DataTermino = CONVERT(DateTime, '{8}', 103)
                                          ,Local = '{9}'
                                          ,DiaInteiro = {10}
                                          ,TipoEvento = {11}
                                          ,DescricaoRecorrencia = '{12}'
                                          ,DescricaoAlerta = '{13}'
                                          ,CodigoEntidade = {14}
                                     WHERE CodigoCompromissoUsuario = {15}
               ", bancodb, Ownerdb, codigoUsuario, status, assunto.Replace("'", "''"), descricao.Replace("'", "''"), categoria, inicio, termino, local.Replace("'", "''"), diaTodo, type, recurrenceInfo, reminderInfo, codigoEntidade, codigoAgenda);

        execSQL(comandoSQL, ref registrosAfetados);

    }

    public void excluiAgenda(int codigoAgenda, ref int registrosAfetados)
    {

        comandoSQL = string.Format(@"DELETE FROM {0}.{1}.CompromissoUsuario
                                     WHERE CodigoCompromissoUsuario = {2}
               ", bancodb, Ownerdb, codigoAgenda);

        execSQL(comandoSQL, ref registrosAfetados);
    }

    public void incluiAgendaInstitucional(int type, string inicio, string termino, int diaTodo, int status, int categoria, string descricao, string local, string recurrenceInfo, string reminderInfo, int resourceID, string assunto, int ID, int codigoUsuario, ref int registrosAfetados, int codigoEntidade)
    {
        comandoSQL = string.Format(@"INSERT INTO {0}.{1}.EventoInstitucional (CodigoUsuarioResponsavel,Status,Assunto,Anotacoes,Rotulo,Inicio,Termino,LocalEvento,DiaInteiro,TipoEvento,DescricaoRecorrencia,DescricaoAlerta,CodigoEntidadeResponsavel, origemLancamento )
                                                                      VALUES({2}, {3}, '{4}', '{5}',   {6},     CONVERT(DateTime, '{7}', 103),      CONVERT(DateTime, '{8}', 103),'{9}',      {10},      {11},              '{12}',         '{13}',          {14},'{15}')
               ", bancodb, Ownerdb, codigoUsuario, status, assunto.Replace("'", "''"), descricao.Replace("'", "''"), categoria, inicio, termino, local.Replace("'", "''"), diaTodo, type, recurrenceInfo, reminderInfo, codigoEntidade, "Agenda");

        execSQL(comandoSQL, ref registrosAfetados);
    }

    public void atualizaAgendaInstitucional(int type, string inicio, string termino, int diaTodo, int status, int categoria, string descricao, string local, string recurrenceInfo, string reminderInfo, int resourceID, string assunto, int ID, int codigoUsuario, int codigoEvento, ref int registrosAfetados, int codigoEntidade)
    {
        comandoSQL = string.Format(@"UPDATE {0}.{1}.EventoInstitucional
                                       SET CodigoUsuarioResponsavel = {2}
                                          ,Status = {3}
                                          ,Assunto = '{4}'
                                          ,Anotacoes = '{5}'
                                          ,Rotulo = {6}
                                          ,Inicio = CONVERT(DateTime, '{7}', 103)
                                          ,Termino = CONVERT(DateTime, '{8}', 103)
                                          ,LocalEvento = '{9}'
                                          ,DiaInteiro = {10}
                                          ,TipoEvento = {11}
                                          ,DescricaoRecorrencia = '{12}'
                                          ,DescricaoAlerta = '{13}'
                                          ,CodigoEntidadeResponsavel = {14}
                                     WHERE CodigoEvento = {15}
               ", bancodb, Ownerdb, codigoUsuario, status, assunto.Replace("'", "''"), descricao.Replace("'", "''"), categoria, inicio, termino, local.Replace("'", "''"), diaTodo, type, recurrenceInfo, reminderInfo, codigoEntidade, codigoEvento);

        execSQL(comandoSQL, ref registrosAfetados);
    }

    public void excluiAgendaInstitucional(int codigoEvento, ref int registrosAfetados)
    {

        comandoSQL = string.Format(@"DELETE FROM {0}.{1}.EventoInstitucional
                                     WHERE CodigoEvento = {2}
               ", bancodb, Ownerdb, codigoEvento);

        execSQL(comandoSQL, ref registrosAfetados);
    }

    #endregion

    #region agenda individual

    public DataSet getAgendaIndividual(
     string where, int codigoUsuario, int codigoEntidade,
     bool mostrarEventosCorporativos, bool mostrarAtividadesProjAnalise,
     bool mostrarAtividadesProjAprovados, bool mostrarMeusCompromissos, bool mostrarReunioes)
    {
        comandoSQL = string.Format(@"
	 SELECT * 
       FROM {0}.{1}.f_getDadosAgenda({2}, {3}, {4}, {5}, {6}, {7}, {8}) {9}"
            , bancodb
            , Ownerdb
            , codigoEntidade
            , codigoUsuario
            , Convert.ToInt32(mostrarMeusCompromissos)
            , Convert.ToInt32(mostrarEventosCorporativos)
            , Convert.ToInt32(mostrarAtividadesProjAnalise)
            , Convert.ToInt32(mostrarAtividadesProjAprovados)
            , Convert.ToInt32(mostrarReunioes)
            , where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    #endregion

    #region Workflow

    private string getErroIncluiRegistro(string Mensagem)
    {
        string padrao = Delimitador_Erro + "Ocorreu um erro ao salvar o registro: ";
        if (Mensagem.IndexOf(Delimitador_Erro) > 0)
            padrao += Mensagem + Delimitador_Erro;
        else
            padrao += Delimitador_Erro + Mensagem;
        return padrao;
    }

    private string getErroAtualizaRegistro(string Mensagem)
    {
        string padrao = Delimitador_Erro + "Ocorreu um erro ao atualizar o registro: ";
        if (Mensagem.IndexOf(Delimitador_Erro) > 0)
            padrao += Mensagem + Delimitador_Erro;
        else
            padrao += Delimitador_Erro + Mensagem;
        return padrao;
    }

    private string getErroExcluiRegistro(string Mensagem)
    {
        string padrao = Delimitador_Erro + "Ocorreu um erro ao excluir o registro: ";
        if (Mensagem.IndexOf(Delimitador_Erro) > 0)
            padrao += Mensagem + Delimitador_Erro;
        else
            padrao += Delimitador_Erro + Mensagem;
        return padrao;
    }

    public string getNomeFluxo(int codigoFluxo)
    {
        string comandoSQL = "";
        string nomeFluxo = "";

        try
        {
            comandoSQL = string.Format(@"
                    SELECT  [NomeFluxo]
                    FROM    {0}.{1}.[Fluxos]
                    WHERE   [CodigoFluxo] = {2}
                    ", bancodb, Ownerdb, codigoFluxo);

            DataSet ds = getDataSet(comandoSQL);

            if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
            {
                nomeFluxo = ds.Tables[0].Rows[0]["NomeFluxo"].ToString();
            }
        }
        catch
        {
            nomeFluxo = "";
        }

        return nomeFluxo;
    }

    public string getProximaVersaoFluxo(int codigoFluxo)
    {
        string comandoSQL = "";
        string proximaVersao = "";
        try
        {
            comandoSQL = string.Format(@"SELECT {0}.{1}.[f_wf_GetProximaVersaoFluxo]({2})", bancodb, Ownerdb, codigoFluxo);

            DataSet ds = getDataSet(comandoSQL);

            if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
                proximaVersao = ds.Tables[0].Rows[0][0].ToString();
        }
        catch
        {
            proximaVersao = "1";
        }

        return proximaVersao;
    }

    public string getCodigoAcaoAutomatica(string acaoAutomatica)
    {
        string comandoSQL = "";
        string codigoAcaoAutomatica = "";

        try
        {
            comandoSQL = string.Format(@"
                    SELECT CodigoAcaoAutomaticaWf
                    FROM {0}.{1}.AcoesAutomaticasWf 
                    WHERE Nome = '{2}'
                    ", bancodb, Ownerdb, acaoAutomatica);

            DataSet ds = getDataSet(comandoSQL);

            if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
                codigoAcaoAutomatica = ds.Tables[0].Rows[0]["CodigoAcaoAutomaticaWf"].ToString();
        }
        catch
        {

        }

        return codigoAcaoAutomatica;
    }

    public string getCodigoGrupoPessoasWf(string acaoAutomatica)
    {
        string comandoSQL = "";
        string codigoTipoGrupoPessoasWf = "";

        try
        {
            comandoSQL = string.Format(@"
                SELECT CodigoPerfilWf
                FROM {0}.{1}.PerfisWf
                WHERE NomePerfilWf = '{2}'
                ", bancodb, Ownerdb, acaoAutomatica);

            DataSet ds = getDataSet(comandoSQL);

            if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
                codigoTipoGrupoPessoasWf = ds.Tables[0].Rows[0]["CodigoPerfilWf"].ToString();
        }
        catch
        {

        }

        return codigoTipoGrupoPessoasWf;
    }

    public DataSet getConfiguracaoSistemaWorflow()
    {
        try
        {
            string comandoSQL = string.Format(@"
                    SELECT Parametro, Valor
                    FROM {0}.{1}.ParametroConfiguracaoSistema
                    WHERE  Parametro IN ( 
                                        'corElementoEtapa_EdicaoWf', 
                                        'corLinhaConector_EdicaoWf', 
                                        'corElementoInicio_EdicaoWf' 
                                        )
					", bancodb, Ownerdb);

            DataSet ds = getDataSet(comandoSQL);
            ds.Tables[0].TableName = "WorkFlows";
            return ds;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public DataSet getNotificacoesWf(int codigoWf, int instanciaWf, int etapaWf, string where)
    {
        try
        {
            string comandoSQL = string.Format(@"
                    SELECT CodigoNotificacaoWf, Assunto, TextoNotificacao, DataNotificacao
                      FROM {0}.{1}.NotificacoesWf
                     WHERE CodigoWorkflow = {2}
                       AND CodigoInstanciaWf = {3}
                       AND CodigoEtapaWf = {4}
                       AND CodigoAcaoWf IS NULL
                       {5}
					", bancodb, Ownerdb
                     , codigoWf
                     , instanciaWf
                     , etapaWf
                     , where);

            return getDataSet(comandoSQL);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public bool incluiNotificacao(int codigoWf, int codigoInstanciaWf, int etapaWf, int sequenciaWf, string descricaoResumida, string mensagem, ref string novoCodigo, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {

            comandoSQL = string.Format(@"
                        BEGIN                            
                            DECLARE @CodigoNotificacaoWf int
                                                       
                            INSERT INTO {0}.{1}.NotificacoesWf (
                                                Assunto, DataNotificacao, TextoNotificacao, CodigoWorkflow, CodigoInstanciaWf, SequenciaOcorrenciaEtapaWf, CodigoEtapaWf)
                            VALUES( '{2}', 
                                    GetDate(), 
                                    '{3}',
                                     {4}, 
                                     {5}, 
                                     {6},
                                     {7})

                            SET @CodigoNotificacaoWf = scope_identity();

                            SELECT @CodigoNotificacaoWf AS CodigoNotificacaoWf
                            
                        END                    
                     ", bancodb, Ownerdb, descricaoResumida, mensagem, codigoWf, codigoInstanciaWf, sequenciaWf, etapaWf);

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = ds.Tables[0].Rows[0][0].ToString();

            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool incluiUsuariosNotificacaoWf(string[] arrayUsuarios, string codigoNotificacaoWf, string assunto, string mensagem, int codigoEntidade)
    {
        bool retorno = false;
        string comandoSQL = "";
        string mensagemRetorno = "";
        int registrosAfetados = 0;


        try
        {

            comandoSQL = string.Format(@"
                       BEGIN
                          DECLARE @CodigoTipoAssociacaoWf int

                          SELECT @CodigoTipoAssociacaoWf = CodigoTipoAssociacao 
                            FROM {0}.{1}.TipoAssociacao 
                           WHERE IniciaisTipoAssociacao = 'NF'
                       
                        ", bancodb, Ownerdb);

            foreach (string codigoParticipante in arrayUsuarios)
            {
                if (codigoParticipante != "")
                {
                    comandoSQL += string.Format(@"
                            INSERT INTO {0}.{1}.NotificacoesRecursosWf(CodigoNotificacaoWf, IdentificadorRecurso, CaixaNotificacao)
                                           values({4}, '{3}', 'E')

                            INSERT INTO {0}.{1}.CaixaMensagem(Assunto, Descricao, DataInclusao, DataRetiradaMensagemCaixa, IndicaTipoMensagem, CodigoUsuario, CodigoTipoAssociacao, CodigoObjetoAssociado, CodigoEntidade, TextoNotificacao)
                                           values('{2}', '{2}', GetDate(), GetDate(), 'E', {3}, @CodigoTipoAssociacaoWf, {4}, {5}, '{6}')
                            ", bancodb, Ownerdb, assunto, codigoParticipante, codigoNotificacaoWf, codigoEntidade, mensagem.Replace(Environment.NewLine, "<BR>"));
                }
            }

            comandoSQL += @"
                       END";

            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            mensagemRetorno = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    /// <summary>
    /// Devolve um dataset com os dados sobre os workflows do sistema
    /// </summary>
    /// <param name="key">Chave criptografada para acesso ao Web Service</param>
    /// <param name="where">Cláusula where a ser adicionada à instrução SELECT, 
    /// permitindo restringir os registros que serão trazidos da base de dados</param>
    /// <exception cref="Exception">Gerada caso ocorre algum erro durante a 'projeção' dos registros</exception>
    public DataSet getWorkFlows(string where)
    {
        try
        {
            string comandoSQL = string.Format(@"
                    SELECT wf.[CodigoWorkflow]
                            ,wf.[CodigoFluxo]
                            ,f.[Nomefluxo]
                            ,wf.[VersaoWorkflow]
                            ,wf.[DescricaoVersao]
                            ,wf.[Observacao]
                            ,wf.[DataCriacao]
                            ,wf.[DataPublicacao]
                            ,wf.[DataRevogacao]
                            ,wf.[textoXML]
                    FROM {0}.{1}.[Workflows]        AS [wf] 
                        INNER JOIN {0}.{1}.[Fluxos] AS [f] 
                            ON (f.[CodigoFluxo] = wf.[CodigoFluxo])
                    WHERE NOT EXISTS( SELECT 1 FROM {0}.{1}.[SubWorkflows] AS [swf]
                    WHERE swf.[CodigoSubWorkflow] = wf.[CodigoWorkflow]) 
                        {2} 
                    ORDER BY CAST(wf.[VersaoWorkflow] AS Float)
                     ", bancodb, Ownerdb, where);

            DataSet ds = getDataSet(comandoSQL);
            ds.Tables[0].TableName = "WorkFlows";
            return ds;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public DataSet getFormulariosWorkflow(string key, string where)
    {
        if (key == _key)
        {
            string comandoSQL = string.Format(
                @"SELECT codigoModeloFormulario AS codigoModeloFormulario , 
                        nomeFormulario	 AS nomeFormulario 
                    FROM {0}.{1}.modeloFormulario 
                   WHERE dataExclusao IS NULL {2}
                    ORDER BY nomeFormulario", bancodb, Ownerdb, where);
            DataSet ds = getDataSet(comandoSQL);
            ds.Tables[0].TableName = "FormulariosWorkflow";
            return ds;
        }
        else
            return null;
    }

    public DataSet getResumoProcessosReport(int codWokflow, int codInstanciaWf)
    {

        string comandoSQL = string.Format(
            @"BEGIN
	                DECLARE @CodigoWorkflowParam Int, @CodigoInstanciaWfParam Int
	        
                    SET @CodigoWorkflowParam = {2}
                    SET @CodigoInstanciaWfParam = {3}
  
	             SELECT ewf.NomeEtapaReduzidoWf AS Etapa,
				        Max(DataInicioEtapa) Inicio,
				        Max(DataTerminoEtapa) Termino,
				        Max(u.NomeUsuario) AS ResponsavelEtapa       
		           FROM {0}.{1}.EtapasInstanciasWf AS eiwf 
			 INNER JOIN {0}.{1}.Usuario AS u ON (u.CodigoUsuario = eiwf.IdentificadorUsuarioFinalizador) 
			 INNER JOIN {0}.{1}.EtapasWf AS ewf ON (ewf.CodigoEtapaWf = eiwf.CodigoEtapaWf)
	              WHERE eiwf.CodigoWorkflow = @CodigoWorkflowParam
		            AND eiwf.CodigoInstanciaWf = @CodigoInstanciaWfParam      
	           GROUP BY ewf.NomeEtapaReduzidoWf
                    END", bancodb, Ownerdb, codWokflow, codInstanciaWf);
        //int registrosAfetados = 0;
        DataSet ds = getDataSet(comandoSQL);
        //ds.Tables[0].TableName = "FormulariosWorkflow";
        return ds;

    }

    public DataSet getTiposGruposPessoasWf(string key, string where)
    {
        if (key == _key)
        {   // 8-6-2010 by Alejandro : troca de sql, mudanças do origem dos dados.
            //                          TipoGruposPessoasWF -> PerfisWF
            //string comandoSQL = string.Format(@" SELECT [NomeTipoGrupoPessoasWf], [IndicadorTipoGrupo]
            //                                     FROM {0}.{1}.[TiposGruposPessoasWf]
            //                                     WHERE 1 = 1 {2} 
            //                                     ORDER BY NomeTipoGrupoPessoasWf", bancodb, Ownerdb, where);

            string comandoSQL = string.Format(@"
            SELECT NomePerfilWf, TipoPerfilWf, CodigoPerfilWf
            FROM {0}.{1}.PerfisWf
            WHERE  StatusPerfilWf = 'A'
              {2}
            ORDER BY NomePerfilWf
            ", bancodb, Ownerdb, where);

            DataSet ds = getDataSet(comandoSQL);
            ds.Tables[0].TableName = "TiposGruposPessoas";
            return ds;
        }
        else
            return null;
    }

    public DataSet getTiposAcoesAutomaticasWf(string key, string where)
    {
        if (key == _key)
        {
            string comandoSQL = string.Format(
                @"SELECT CodigoAcaoAutomaticaWf
                        ,Nome
                        ,Descricao   
                  FROM {0}.{1}.[AcoesAutomaticasWf]
                 WHERE 1 = 1 {2} 
                 ORDER BY Nome", bancodb, Ownerdb, where);
            DataSet ds = getDataSet(comandoSQL);
            ds.Tables[0].TableName = "TiposGruposPessoas";
            return ds;
        }
        else
            return null;
    }

    /// <summary>
    /// Função para obter os fluxos cadastrados no sistema.
    /// </summary>
    public DataSet getFluxos(string where)
    {
        string comandoSQL = string.Format(@"
            SELECT f.[CodigoFluxo]
                 , f.[NomeFluxo]
                 , f.[Descricao]
                 , f.[Observacao]
                 , f.[StatusFluxo]
                 , CAST( CASE f.[StatusFluxo] WHEN 'A' THEN 'Ativo' ELSE 'Desativado' END AS Varchar(10) ) AS [Status]
                 , [CodigoGrupoFluxo]
                 , (select top 1 1 from Workflows wf2 where wf2.CodigoFluxo = f.CodigoFluxo and wf2.DataPublicacao IS NOT NULL ) as podeExcluir
                 , f.IniciaisFluxo
                 , f.[IndicaPossivelAcessoViaAPI]
                 , f.[NomeProcValidacaoAcnAPI]
            FROM {0}.{1}.[Fluxos] f
            WHERE 1=1 
              {2} 
            ORDER BY [NomeFluxo]
            ", bancodb, Ownerdb, where);
        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds))
            ds.Tables[0].TableName = "Fluxos";

        return ds;
    }

    /// <summary>
    /// Função para obter os fluxos cadastrados no sistema.
    /// </summary>
    public DataSet getListaTiposProjetos(string where)
    {
        string comandoSQL = string.Format(@"
SELECT
          tp.[CodigoTipoProjeto]
        , tp.[TipoProjeto]
	FROM
		{0}.{1}.[TipoProjeto]	AS [tp]
    WHERE 1=1 {2}
    ORDER BY tp.[TipoProjeto]", bancodb, Ownerdb, where);
        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds))
            ds.Tables[0].TableName = "TiposProjetos";

        return ds;
    }

    /// <summary>
    /// Grava um novo registro na tabela Workflows.
    /// </summary>
    /// <param name="key">Chave criptografada para acesso ao Web Service</param>
    /// <param name="CodigoFluxo">Código do Fluxo a que se refere o workflow</param>
    /// <param name="versaoWorkflow">Versão do fluxo sendo gravado</param>
    /// <param name="idUsuario">Identificador do usuário que está incluindo o workflow</param>
    /// <param name="XML">O XML com a definição do workflow</param>
    /// <exception cref="Exception">Gerada caso ocorre algum erro durante a inclusão do registro</exception>
    public int gravaWorkflow(string key, int CodigoFluxo, string versaoWorkflow, string idUsuario,
                             string descricaoVersao, string observacaoVersao, string XML)
    {
        try
        {
            // Insere um registro na tabela Carteira
            string comandoSQL = string.Format(@"
                    INSERT INTO {0}.{1}.[Workflows]
                   ([CodigoFluxo]
                   ,[VersaoWorkflow]
                   ,[DataCriacao]
                   ,[IdentificadorUsuarioCriacao]
                   ,[TextoXml]
                   ,[VersaoFormatoXml]
                   ,[DescricaoVersao]
                   ,[Observacao])
     VALUES
           ({2}, '{3}', GetDate(), '{4}', N'{5}', '001.1.015', {6}, {7} );
            SELECT SCOPE_IDENTITY()"
                , bancodb, Ownerdb, CodigoFluxo, versaoWorkflow, idUsuario, XML.Replace("'", "''")
                , descricaoVersao != "" ? "'" + descricaoVersao.Replace("'", "''") + "'" : "NULL"
                , observacaoVersao != "" ? "'" + observacaoVersao.Replace("'", "''") + "'" : "NULL");

            DataSet ds = getDataSet(comandoSQL);
            int codigoWorkflow = int.Parse(ds.Tables[0].Rows[0][0].ToString());


            return codigoWorkflow;
        }
        catch (Exception ex)
        {
            throw new Exception(getErroIncluiRegistro(ex.Message));
        }
    }

    /// <summary>
    /// Atualizar os dados do Workflows na base de dados.
    /// </summary>
    /// <param name="key">Chave criptografada para acesso ao Web Service</param>
    /// <param name="CodigoWorkflow">Código do workflow</param>
    /// <param name="xmlWorkflow">XML com a definição do workflow</param>
    /// <exception cref="Exception">Gerada caso ocorre algum erro durante a inclusão do registro</exception>
    public bool atualizaWorkflow(string key, int CodigoWorkflow, string xmlWorkflow, string descricaoVersao, string observacaoWf)
    {
        int registrosAfetados = 0;
        try
        {
            // Insere um registro na tabela Carteira
            string comandoSQL = string.Format(@"
                    UPDATE {0}.{1}.[Workflows] 
                        SET [textoXML]          = N'{3}'
                            ,[DescricaoVersao]  = {4}
                            ,[Observacao]       = {5}
                    WHERE [CodigoWorkflow] = {2}"
                , bancodb, Ownerdb, CodigoWorkflow
                , xmlWorkflow.Replace("'", "''")
                , descricaoVersao != "" ? "'" + descricaoVersao.Replace("'", "''") + "'" : "NULL"
                , observacaoWf != "" ? "'" + observacaoWf.Replace("'", "''") + "'" : "NULL");
            execSQL(comandoSQL, ref registrosAfetados);

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(getErroIncluiRegistro(ex.Message));
        }
    }

    /// <summary>
    /// Exclui da base de dados um workflow cadastrado.
    /// </summary>
    /// <param name="key">Chave criptografada para acesso ao Web Service</param>
    /// <param name="CodigoWorkflow">Código do workflow</param>
    /// <exception cref="Exception">Gerada caso ocorre algum erro durante a inclusão do registro</exception>
    public bool excluiWorkflow(int codigoWorkflow, ref int registrosAfetados)
    {
        try
        {
            string comandoSQL = string.Format(@"
                    DELETE {0}.{1}.[Workflows] 
                    WHERE [CodigoWorkflow] = {2}"
                , bancodb, Ownerdb, codigoWorkflow);
            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(getErroIncluiRegistro(ex.Message));
        }
    }

    #endregion

    #region wf - Engenharia

    /// <summary>
    /// funcão para obter o código do fluxo e do workflow a ser usado para o fluxo de nova porposta.
    /// </summary>
    /// <remarks>
    /// Busca no banco o parâmetro que contém o código referente ao fluxo Nova Proposta de Iniciativa
    /// Com o código retornado, localiza o código do workflow que corresponde à versão atual para este fluxo.
    /// </remarks>
    /// <param name="codigoFluxo"></param>
    /// <param name="codigoWorkflow"></param>
    /// <exception cref="Exception">Gerada caso ocorre algum erro durante a 'projeção' dos registros</exception>
    public bool getCodigoWfAtualFluxoNovaProposta(int codigoEntidade, out int codigoFluxo, out int codigoWorkflow)
    {
        return getCodigoWfAtual(codigoEntidade, out codigoFluxo, out codigoWorkflow, "codigoFluxoNovaPropostaIniciativa");
    }

    public bool getCodigoWfAtualFluxoNovaPropostaProcesso(int codigoEntidade, out int codigoFluxo, out int codigoWorkflow)
    {
        return getCodigoWfAtual(codigoEntidade, out codigoFluxo, out codigoWorkflow, "codigoFluxoNovaPropostaProcesso");
    }

    public bool getCodigoWfAtualFluxoNovaDemanda(int codigoEntidade, out int codigoFluxo, out int codigoWorkflow)
    {
        return getCodigoWfAtual(codigoEntidade, out codigoFluxo, out codigoWorkflow, "codigoFluxoNovaDemanda");
    }

    public bool getCodigoWfAtualFluxoEmissaoOS(int codigoEntidade, out int codigoFluxo, out int codigoWorkflow)
    {
        return getCodigoWfAtual(codigoEntidade, out codigoFluxo, out codigoWorkflow, "codigoFluxoEmissaoOS");
    }

    /// <summary>
    /// Devolve o código do Fluxo e do Workflow que deve ser usado para se referir ao fluxo indicado no parâmetro 
    /// <paramref name="nomeParametroFluxo"/>
    /// </summary>
    /// <param name="codigoEntidade"></param>
    /// <param name="codigoFluxo"></param>
    /// <param name="codigoWorkflow"></param>
    /// <param name="nomeParametroFluxo"></param>
    /// <returns></returns>
    private bool getCodigoWfAtual(int codigoEntidade, out int codigoFluxo, out int codigoWorkflow, string nomeParametroFluxo)
    {
        string comandoSQL = "";
        bool bRet = true;

        codigoFluxo = 0;
        codigoWorkflow = 0;

        try
        {
            DataSet dsFluxo = getParametrosSistema(codigoEntidade, nomeParametroFluxo);

            if (DataSetOk(dsFluxo) && DataTableOk(dsFluxo.Tables[0]))
            {
                if (int.TryParse(dsFluxo.Tables[0].Rows[0][0].ToString(), out codigoFluxo))
                {
                    bRet = true;
                }
            }


            if (0 != codigoFluxo)
            {
                comandoSQL = string.Format(@"
                    SELECT MAX([CodigoWorkflow]) 
                        FROM 
                            {0}.{1}.[Fluxos] AS [f]  
                            INNER JOIN {0}.{1}.[Workflows]  AS [wf] ON 
                                ( wf.[CodigoFluxo]  = f.[CodigoFluxo] )
                        WHERE   f.[CodigoFluxo]     = {2} 
                            AND f.[CodigoEntidade]  = {3}
                            AND wf.[DataRevogacao]  IS NULL 
                            AND wf.[DataPublicacao] IS NOT NULL
                    ", bancodb, Ownerdb, codigoFluxo, codigoEntidade);

                DataSet ds = getDataSet(comandoSQL);

                if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
                    codigoWorkflow = int.Parse(ds.Tables[0].Rows[0][0].ToString());
            } // if ("" != codigoFluxoNovaProposta)
        }
        catch (Exception ex)
        {
            string mensagemerro = ex.Message;
            codigoFluxo = 0;
            codigoWorkflow = 0;
            bRet = false;
        }

        return bRet;
    }

    public bool getCodigoWfAtualPorIniciais(int codigoEntidade, out int codigoFluxo, out int codigoWorkflow, string iniciaisFluxo)
    {
        string comandoSQL = "";
        bool bRet = true;

        codigoFluxo = 0;
        codigoWorkflow = 0;

        try
        {

            comandoSQL = string.Format(@"
                    SELECT MAX([CodigoWorkflow]), wf.CodigoFluxo
                        FROM 
                            {0}.{1}.[Fluxos] AS [f]  
                            INNER JOIN {0}.{1}.[Workflows]  AS [wf]ON 
                                ( wf.[CodigoFluxo]  = f.[CodigoFluxo] )
                        WHERE   f.[IniciaisFluxo]     = '{2}' 
                            AND f.[CodigoEntidade]  = {3}
                            AND wf.[DataRevogacao]  IS NULL 
                            AND wf.[DataPublicacao] IS NOT NULL
                       GROUP BY wf.CodigoFluxo
                    ", bancodb, Ownerdb, iniciaisFluxo, codigoEntidade);

            DataSet ds = getDataSet(comandoSQL);

            if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
            {
                codigoWorkflow = int.Parse(ds.Tables[0].Rows[0][0].ToString());
                codigoFluxo = int.Parse(ds.Tables[0].Rows[0][1].ToString());
            }

        }
        catch
        {
            codigoFluxo = 0;
            codigoWorkflow = 0;
            bRet = false;
        }

        return bRet;
    }

    public bool incluiFluxoAberturaProjetoPorIniciais(int codigoEntidade, int codigoProjeto, int codigoUsuario, out int codigoFluxo, out int codigoWorkflow, out int codigoInstancia, out int codigoEtapa, out int codigoSequencia, string iniciaisFluxo)
    {
        string comandoSQL = "";
        bool bRet = true;

        codigoFluxo = 0;
        codigoWorkflow = 0;
        codigoInstancia = 0;
        codigoEtapa = 0;
        codigoSequencia = 0;

        try
        {

            comandoSQL = string.Format(@"
                    BEGIN
                      DECLARE
                              @CodigoNovoFluxo Int   
                            , @CodigoWorkflowNovoFluxo Int   
                            , @CodigoInstanciaWfNovoFluxo BigInt      
                            , @OcorrenciaAtual Int
                            , @CodigoEtapaAtual Int            
                            , @l_nAuxRet Int
            
                      EXEC @l_nAuxRet = {0}.{1}.[p_wf_pa_iniciaFluxo]
                                      @in_iniciaisNovoFluxo = '{2}'
                                    , @in_codigoNovoFluxo = NULL
                                    , @in_codigoWorkFlowOrigem = NULL
                                    , @in_codigoInstanciaWfOrigem = NULL
                                    , @in_codigoProjeto = {3}
                                    , @in_codigoEntidade = {5}
                                    , @in_identificadorUsuarioAcao = {4}
                                    , @ou_codigoNovofluxo = @CodigoNovoFluxo OUTPUT
                                    , @ou_codigoWorkflowNovofluxo = @CodigoWorkflowNovoFluxo OUTPUT
                                    , @ou_codigoInstanciaWfNovoFluxo = @CodigoInstanciaWfNovoFluxo OUTPUT
                                    , @ou_ocorrenciaAtual = @OcorrenciaAtual OUTPUT
                                    , @ou_codigoEtapaAtual = @CodigoEtapaAtual OUTPUT

                     EXEC {0}.{1}.[p_wf_pa_pbh_ImportaPropostaProjetoAbertura] @CodigoWorkflowNovoFluxo, @CodigoInstanciaWfNovoFluxo

                      SELECT 
                              @CodigoNovoFluxo AS [CodigoFluxo]        
                            , @CodigoWorkflowNovoFluxo AS [CodigoWorkflow]
                            , @CodigoInstanciaWfNovoFluxo AS [CodigoInstanciaWf]
                            , @OcorrenciaAtual AS [OcorrenciaAtual]
                            , @CodigoEtapaAtual AS [EtapaAtual]
                END

                    ", bancodb, Ownerdb, iniciaisFluxo, codigoProjeto, codigoUsuario, codigoEntidade);

            DataSet ds = getDataSet(comandoSQL);

            if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
            {
                codigoWorkflow = int.Parse(ds.Tables[0].Rows[0]["CodigoWorkflow"].ToString());
                codigoFluxo = int.Parse(ds.Tables[0].Rows[0]["CodigoFluxo"].ToString());
                codigoInstancia = int.Parse(ds.Tables[0].Rows[0]["CodigoInstanciaWf"].ToString());
                codigoEtapa = int.Parse(ds.Tables[0].Rows[0]["EtapaAtual"].ToString());
                codigoSequencia = int.Parse(ds.Tables[0].Rows[0]["OcorrenciaAtual"].ToString());
            }

        }
        catch
        {
            codigoFluxo = 0;
            codigoWorkflow = 0;
            codigoInstancia = 0;
            codigoEtapa = 0;
            codigoSequencia = 0;
            bRet = false;
        }

        return bRet;
    }


    /// <summary>
    /// Devolve o código do Fluxo e do Workflow que deve ser usado para se referir ao fluxo indicado no parâmetro. Devolve também o código 
    /// da etapa inicial do workflow.
    /// <paramref name="nomeParametroFluxo"/>
    /// </summary>
    /// <param name="codigoEntidade"></param>
    /// <param name="codigoFluxo"></param>
    /// <param name="codigoWorkflow"></param>
    /// <param name="codigoEtapaInicial"></param>
    /// <param name="nomeParametroFluxo"></param>
    /// <returns></returns>
    public bool getCodigoWfAtual(int codigoEntidade, out int codigoFluxo, out int codigoWorkflow, out int codigoEtapaInicial, string nomeParametroFluxo)
    {
        bool bRet = getCodigoWfAtual(codigoEntidade, out codigoFluxo, out codigoWorkflow, nomeParametroFluxo);
        codigoEtapaInicial = 0;
        if (bRet)
        {
            string comandoSQL = "";

            try
            {
                comandoSQL = string.Format(@"
                    SELECT [CodigoEtapaInicial] 
                        FROM 
                            {0}.{1}.[Workflows]  AS [wf] 
                        WHERE   wf.[CodigoWorkflow]  = {2} 
                    ", bancodb, Ownerdb, codigoWorkflow);

                DataSet ds = getDataSet(comandoSQL);

                if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
                    codigoEtapaInicial = int.Parse(ds.Tables[0].Rows[0][0].ToString());
            }
            catch
            {
                codigoFluxo = 0;
                codigoWorkflow = 0;
                bRet = false;
            }
        }

        return bRet;
    }


    /// <summary>
    /// Grava data de início de uma etapa
    /// </summary>
    /// <remarks>
    /// função para gravar a data de início de uma etapa. Chamada na tela de interação 
    /// de etapa. Após o usuário visualizar uma etapa na tela, a data de início estará 
    /// registrada o que não permitirá mais que a etapa seja 'revertida'.
    /// Os parâmetros desta função são os valores identificantes na tabela [EtapasInstanciasWf].
    /// e etapa passados como parâmetro.
    /// </remarks>
    /// <param name="workflow">Código do workflow ao qual o parâmetro <paramref name="instanciaWf"/> se refere</param>
    /// <param name="instanciaWf">Código da instância do workflow cuja etapa será atualizada</param>
    /// <param name="seqEtapa">Número sequencial de ocorrência de etapa. 
    /// Este número é usado para identificar mais precisamente a etapa que será atualizada</param>
    /// <param name="etapa">Código da Etapa a atualizar</param>
    /// <param name="idUsuario">Identificador do usuário que está iniciando a etapa</param>
    /// <param name="mensagemErro">Parâmetro de saída. Mensagem do erro caso o valor retornado seja
    /// false.</param>
    /// <returns>caso tudo ocorra sem problema na atualização da etapa, retorna true.
    /// Caso contrário, retorna false.</returns>
    public bool atualizaDataInicioEtapaWf(int? workflow, long? instanciaWf, int? seqEtapa, int? etapa, string idUsuario, out string mensagemErro)
    {
        bool bRet;
        int registrosAfetados = 0;
        string comandoSQL = "";
        try
        {
            registrosAfetados = 0;

            comandoSQL = string.Format(
                @"
                    UPDATE {0}.{1}.[EtapasInstanciasWf] 
	                    SET
			                [DataInicioEtapa]				    = GETDATE()
			                , [IdentificadorUsuarioIniciador]   = '{6}'
	                    WHERE
				                [CodigoWorkflow]				= {2}
		                AND [CodigoInstanciaWf]					= {3}
		                AND [SequenciaOcorrenciaEtapaWf]	    = {4}
		                AND [CodigoEtapaWf]						= {5}
		                AND [DataInicioEtapa]					IS NULL
                "
                           , bancodb
                           , Ownerdb
                           , workflow, instanciaWf, seqEtapa, etapa, idUsuario);

            execSQL(comandoSQL, ref registrosAfetados);

            mensagemErro = "";
            bRet = true;
        }
        catch (Exception ex)
        {
            mensagemErro = string.Format(@"Falha ao atualizar informações no banco de dados.
Entre em contato com o administrador do sistema. Mensagem original do erro: {0}", ex.Message);
            bRet = false;
        }

        return bRet;
    }

    /// <summary>
    /// Reverte o fluxo de um workflow estágio anterior à última ação executada.
    /// </summary>
    /// <param name="workflow">Código do workflow ao qual o parâmetro <paramref name="instanciaWf"/> se refere</param>
    /// <param name="instanciaWf">Código da instância do workflow cuja etapa será revertida</param>
    /// <param name="seqEtapa">Número sequencial de ocorrência de etapa. 
    /// Este número é usado para identificar mais precisamente a etapa que será revertida</param>
    /// <param name="etapa">Código da Etapa a reverter</param>
    /// <param name="idUsuario">Identificador do usuário que está revertendo a etapa</param>
    /// <param name="mensagemErro">Parâmetro de saída. Mensagem do erro caso o valor retornado seja
    /// false.</param>
    /// <returns>caso tudo ocorra sem problema na atualização da etapa, retorna true.
    /// Caso contrário, retorna false.</returns>
    public bool reverteAcaoWorkflow(int workflow, long instanciaWf, int seqEtapa, string etapa, string idUsuario, out string mensagemErro)
    {
        bool bRet;

        etapa = etapa.Length == 0 ? "" : etapa;
        string comandoSQL = "";
        try
        {

            comandoSQL = string.Format(
                @"
                DECLARE @RC int 
                DECLARE @in_codigoWorkFlow int 
                DECLARE @in_codigoInstanciaWf bigint 
                DECLARE @in_SequenciaOcorrenciaEtapaWf int 
                DECLARE @in_codigoEtapaWf varchar(5) 
                DECLARE @in_identificadorUsuarioAcao varchar(50)
                DECLARE @ou_resultado int 
                DECLARE @ou_codigoRetorno int 
                DECLARE @ou_mensagemErro nvarchar(2048) 

                SET @in_codigoWorkFlow              = {2} 
                SET @in_codigoInstanciaWf           = {3} 
                SET @in_SequenciaOcorrenciaEtapaWf  = {4} 
                SET @in_codigoEtapaWf               = '{5}' 
                SET @in_identificadorUsuarioAcao    = '{6}'

                if (len(@in_codigoEtapaWf) = 0)
                    SET @in_codigoEtapaWf = null;
        

                EXECUTE @RC = {0}.{1}.[p_wf_reverteUltimaAcaoWorkflow] 
                   @in_codigoWorkFlow               = @in_codigoWorkFlow 
                  ,@in_codigoInstanciaWf            = @in_codigoInstanciaWf 
                  ,@in_SequenciaOcorrenciaEtapaWf   = @in_SequenciaOcorrenciaEtapaWf 
                  ,@in_codigoEtapaWf                = @in_codigoEtapaWf 
                  ,@in_identificadorUsuarioAcao     = @in_identificadorUsuarioAcao
                  ,@ou_resultado                    = @ou_resultado                 OUTPUT 
                  ,@ou_codigoRetorno                = @ou_codigoRetorno             OUTPUT 
                  ,@ou_mensagemErro                 = @ou_mensagemErro              OUTPUT 

                SELECT 
                        @RC						AS RetornoProc,  
                        @ou_resultado			AS Resultado, 
                        @ou_codigoRetorno		AS CodigoRetorno, 
                        @ou_mensagemErro		AS MensagemErro "
                           , bancodb
                           , Ownerdb
                           , workflow, instanciaWf, seqEtapa, etapa, idUsuario);

            DataSet ds = getDataSet(comandoSQL);

            if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
            {
                string msgWhere = "", msgWhat = "";
                int retornoProc, resultado, codigoRetorno;
                string msgProc;

                retornoProc = int.Parse(ds.Tables[0].Rows[0]["RetornoProc"].ToString());
                resultado = int.Parse(ds.Tables[0].Rows[0]["Resultado"].ToString());
                codigoRetorno = int.Parse(ds.Tables[0].Rows[0]["CodigoRetorno"].ToString());
                msgProc = ds.Tables[0].Rows[0]["MensagemErro"].ToString();

                if (0 == retornoProc)
                {
                    mensagemErro = "";
                    bRet = true;
                }
                else
                {
                    if ((16 & resultado) > 0)
                        msgWhere = "no processamento de tempo limite da etapa";
                    else if ((32 & resultado) > 0)
                        msgWhere = "na alteração da etapa do processo";
                    else if ((64 & resultado) > 0)
                        msgWhere = "na reversão da etapa do processo";
                    else if (((4 & resultado) > 0) || ((8 & resultado) > 0))
                        msgWhere = "na execução de ações automáticas da etapa do processo";
                    else if (((1 & resultado) > 0) || ((2 & resultado) > 0))
                        msgWhere = "no envio de notificações";

                    switch (codigoRetorno)
                    {
                        case 1:
                            msgWhat = string.Format(@"Não foi encontrada, na origem, a informação referenciada no modelo do workflow. 
Entrar em contato com o administrador do sistema.Código do erro: {0}", codigoRetorno);
                            break;
                        case 2:
                            msgWhat = string.Format(@"A informação na origem não é sufuciente para a execução da ação (notificação, ação autom., etc). 
Entrar em contato com o administrador do sistema.Código do erro: {0}", codigoRetorno);
                            break;
                        case 3:
                            msgWhat = string.Format(@"As informações recebidas pela proc são insuficientes para a execução dos procedimentos. 
Entrar em contato com o administrador do sistema.Código do erro: {0}", codigoRetorno);
                            break;
                        case 4:
                            msgWhat = string.Format(@"Não foram localizadas as informações sobre o estado do workflow/etapa identificados nos parâmetros.
Entrar em contato com o administrador do sistema.Código do erro: {0}", codigoRetorno);
                            break;
                        case 5:
                            msgWhat = @"O workflow encontra-se em estado diferente do estado mencionado nos parâmetros.
Acesse o processo novamente para ver as informações atualizadas";
                            break;
                        case 6:
                            msgWhat = string.Format(@"A ação executada não produziu efeito e não foi possível determinar a causa. 
Entrar em contato com o administrador do sistema.Código do erro: {0}", codigoRetorno);
                            break;
                        case 7:
                            msgWhat = @"A ação não pode ser executada. Usuário sem permissão de ação para esta etapa do processo.
Entrar em contato com o administrador do sistema.";
                            break;
                        case 8:
                            msgWhat = @"A ação não pode ser executada pois a etapa atual já foi iniciada.";
                            break;
                        default:
                            msgWhat = string.Format(@"Falha na execução do procedimento no servidor.
Entre em contato com o administrador do sistema. 
Código do erro: {0}, {1}.", codigoRetorno, retornoProc);
                            break;

                    }

                    if (msgProc.Length > 0)
                    {
                        msgWhat += " Mensagem original do erro: " + msgProc;
                    }

                    mensagemErro = string.Format(@"Falha {0}.{1}.", msgWhere, msgWhat);
                    bRet = false;
                }
            }
            else
            {
                mensagemErro = @"Falha interna da aplicação.
A execução de procedimento no servidor não retornou nenhuma informação.
Entre em contato com o administrador do sistema.";
                bRet = false;
            }
        }
        catch (Exception ex)
        {
            mensagemErro = string.Format(@"Falha ao executar procedimento no servidor.
Entre em contato com o administrador do sistema. Mensagem original do erro: {0}", ex.Message);
            bRet = false;
        }

        return bRet;
    }

    /// <summary>
    /// Cancela uma execução de um fluxo de um workflow.
    /// </summary>
    /// <param name="workflow">Código do workflow ao qual o parâmetro <paramref name="instanciaWf"/> se refere</param>
    /// <param name="instanciaWf">Código da instância do workflow cuja etapa será revertida</param>
    /// <param name="seqEtapa">Número sequencial de ocorrência de etapa. 
    /// Este número é usado para identificar mais precisamente a etapa que será revertida</param>
    /// <param name="etapa">Código da Etapa a reverter</param>
    /// <param name="idUsuario">Identificador do usuário que está revertendo a etapa</param>
    /// <param name="mensagemErro">Parâmetro de saída. Mensagem do erro caso o valor retornado seja
    /// false.</param>
    /// <returns>caso tudo ocorra sem problema no cancelamento da instância, retorna true.
    /// Caso contrário, retorna false.</returns>
    public bool cancelaInstanciaWorkflow(int workflow, long instanciaWf, int seqEtapa, int etapa, string idUsuario, out string mensagemErro)
    {
        bool bRet;
        string comandoSQL = "";
        try
        {

            comandoSQL = string.Format(
                @"
                DECLARE @RC int 
                DECLARE @in_codigoWorkFlow int 
                DECLARE @in_codigoInstanciaWf bigint 
                DECLARE @in_SequenciaOcorrenciaEtapaWf int 
                DECLARE @in_codigoEtapaWf int 
                DECLARE @in_identificadorUsuarioAcao varchar(50)
                DECLARE @ou_resultado int 
                DECLARE @ou_codigoRetorno int 
                DECLARE @ou_mensagemErro nvarchar(2048) 

                SET @in_codigoWorkFlow              = {2} 
                SET @in_codigoInstanciaWf           = {3} 
                SET @in_SequenciaOcorrenciaEtapaWf  = {4} 
                SET @in_codigoEtapaWf               = {5} 
                SET @in_identificadorUsuarioAcao    = '{6}'

                EXECUTE @RC = {0}.{1}.[p_wf_cancelaInstanciaWorkflow] 
                   @in_codigoWorkFlow               = @in_codigoWorkFlow 
                  ,@in_codigoInstanciaWf            = @in_codigoInstanciaWf 
                  ,@in_SequenciaOcorrenciaEtapaWf   = @in_SequenciaOcorrenciaEtapaWf 
                  ,@in_codigoEtapaWf                = @in_codigoEtapaWf 
                  ,@in_identificadorUsuarioAcao     = @in_identificadorUsuarioAcao
                  ,@ou_resultado                    = @ou_resultado                 OUTPUT 
                  ,@ou_codigoRetorno                = @ou_codigoRetorno             OUTPUT 
                  ,@ou_mensagemErro                 = @ou_mensagemErro              OUTPUT 

                SELECT 
                        @RC						AS RetornoProc,  
                        @ou_resultado			AS Resultado, 
                        @ou_codigoRetorno		AS CodigoRetorno, 
                        @ou_mensagemErro		AS MensagemErro "
                           , bancodb
                           , Ownerdb
                           , workflow, instanciaWf, seqEtapa, etapa, idUsuario);

            DataSet ds = getDataSet(comandoSQL);

            if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
            {
                string msgWhere = "", msgWhat = "";
                int retornoProc, resultado, codigoRetorno;
                string msgProc;

                retornoProc = int.Parse(ds.Tables[0].Rows[0]["RetornoProc"].ToString());
                resultado = int.Parse(ds.Tables[0].Rows[0]["Resultado"].ToString());
                codigoRetorno = int.Parse(ds.Tables[0].Rows[0]["CodigoRetorno"].ToString());
                msgProc = ds.Tables[0].Rows[0]["MensagemErro"].ToString();

                if (0 == retornoProc)
                {
                    mensagemErro = "";
                    bRet = true;
                }
                else
                {
                    if ((16 & resultado) > 0)
                        msgWhere = "no processamento de tempo limite da etapa";
                    else if ((32 & resultado) > 0)
                        msgWhere = "na alteração da etapa do processo";
                    else if ((64 & resultado) > 0)
                        msgWhere = "no cancelamento da do processo";
                    else if (((4 & resultado) > 0) || ((8 & resultado) > 0))
                        msgWhere = "na execução de ações automáticas da etapa do processo";
                    else if (((1 & resultado) > 0) || ((2 & resultado) > 0))
                        msgWhere = "no envio de notificações";

                    switch (codigoRetorno)
                    {
                        case 1:
                            msgWhat = string.Format(@"Não foi encontrada, na origem, a informação referenciada no modelo do workflow. 
Entrar em contato com o administrador do sistema.Código do erro: {0}", codigoRetorno);
                            break;
                        case 2:
                            msgWhat = string.Format(@"A informação na origem não é sufuciente para a execução da ação (notificação, ação autom., etc). 
Entrar em contato com o administrador do sistema.Código do erro: {0}", codigoRetorno);
                            break;
                        case 3:
                            msgWhat = string.Format(@"As informações recebidas pela proc são insuficientes para a execução dos procedimentos. 
Entrar em contato com o administrador do sistema.Código do erro: {0}", codigoRetorno);
                            break;
                        case 4:
                            msgWhat = string.Format(@"Não foram localizadas as informações sobre o estado do workflow/etapa identificados nos parâmetros.
Entrar em contato com o administrador do sistema.Código do erro: {0}", codigoRetorno);
                            break;
                        case 5:
                            msgWhat = @"O workflow encontra-se em estado diferente do estado mencionado nos parâmetros.
Acesse o processo novamente para ver as informações atualizadas";
                            break;
                        case 6:
                            msgWhat = string.Format(@"A ação executada não produziu efeito e não foi possível determinar a causa. 
Entrar em contato com o administrador do sistema.Código do erro: {0}", codigoRetorno);
                            break;
                        case 7:
                            msgWhat = @"A ação não pode ser executada. Usuário sem permissão de ação para esta etapa do processo.
Entrar em contato com o administrador do sistema.";
                            break;
                        case 8:
                            msgWhat = @"A ação não pode ser executada pois a etapa atual já foi iniciada.";
                            break;
                        default:
                            msgWhat = string.Format(@"Falha na execução do procedimento no servidor.
Entre em contato com o administrador do sistema. 
Código do erro: {0}, {1}.", codigoRetorno, retornoProc);
                            break;

                    }

                    if (msgProc.Length > 0)
                    {
                        msgWhat += " Mensagem original do erro: " + msgProc;
                    }

                    mensagemErro = string.Format(@"Falha {0}.{1}.", msgWhere, msgWhat);
                    bRet = false;
                }
            }
            else
            {
                mensagemErro = @"Falha interna da aplicação.
A execução de procedimento no servidor não retornou nenhuma informação.
Entre em contato com o administrador do sistema.";
                bRet = false;
            }
        }
        catch (Exception ex)
        {
            mensagemErro = string.Format(@"Falha ao executar procedimento no servidor.
Entre em contato com o administrador do sistema. Mensagem original do erro: {0}", ex.Message);
            bRet = false;
        }

        return bRet;
    }

    /// <summary>
    /// Devolve o nível de acesso a uma instância de um workflow, considerando "suas" etapas atuais
    /// </summary>
    /// <remarks>
    /// Chama a proc que Verifica o nível de acesso do usuário à etapa atual de uma instância e às 
    /// etapas atuais das instâncias filhas da instância em questão.
    /// </remarks>
    /// <param name="workflow">Código do workflow da etapa que se quer obter o nível de acesso</param>
    /// <param name="instanciaWf">Código da instância do workflow para a qual se quer obter o nível de acesso</param>
    /// <param name="usuario">Identificador do usuário para o qual se deseja obter o nível de acesso.</param>
    /// <returns>um conjunto de bit indicando o nível de acesso.
    /// nenhum bit ligado (valor 0) -> nenhum acesso
    /// bit 1 ligado -> acesso de leitura
    /// bit 2 ligado -> acesso de ação (obviamente, quando o bit 2 estiver ligado, o bit 1 também estará
    /// </returns>
    public int obtemNivelAcessoInstanciaWf(int? workflow, long? instanciaWf, string usuario)
    {
        int nivelAcesso = 0;
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(
            @"
                DECLARE @RC int
                DECLARE @in_codigoWorkFlow int
                DECLARE @in_codigoInstanciaWf bigint
                DECLARE @in_identificadorUsuario varchar(50)
                DECLARE @ou_nivelAcesso int

                SET @in_codigoWorkFlow								= {2} 
                SET @in_codigoInstanciaWf							= {3}
                SET @in_identificadorUsuario					    = '{4}'

                EXECUTE @RC = {0}.{1}.[p_wf_verificaAcessoEtapaAtual] 
                   @in_codigoWorkFlow
                  ,@in_codigoInstanciaWf
                  ,@in_identificadorUsuario
                  ,@ou_nivelAcesso OUTPUT

                SELECT 
                    @RC                     AS RetornoProc, 
                    @ou_nivelAcesso         AS NivelAcesso "
                           , bancodb
                           , Ownerdb
                           , workflow, instanciaWf, usuario);

            DataSet ds = getDataSet(comandoSQL);

            if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
            {
                // não incluindo neste procedimento tratamento de erro.
                // em qualquer situaçãod e erro, será devolvido apenas que o usuário não tem acesso.
                nivelAcesso = int.Parse(ds.Tables[0].Rows[0]["NivelAcesso"].ToString());
            }
        }
        catch (Exception)
        {
            nivelAcesso = 0;
        }

        return nivelAcesso;
    }

    /// <summary>
    /// Devolve o nível de acesso a uma etapa de uma instância de um workflow para um determinado usuário
    /// </summary>
    /// <param name="workflow">Código do workflow da etapa que se quer obter o nível de acesso</param>
    /// <param name="instanciaWf">Código da instância do workflow para a qual se quer obter o nível de acesso</param>
    /// <param name="seqEtapa">Número sequencial de ocorrência de etapa. 
    /// Este número é usado para identificar em qual ocorrência da etapa se quer verificar o nível de acesso</param>
    /// <param name="etapa">Código da Etapa</param>
    /// <param name="usuario">Identificador do usuário para o qual se deseja obter o nível de acesso.</param>
    /// <returns>um conjunto de bit indicando o nível de acesso.
    /// nenhum bit ligado (valor 0) -> nenhum acesso
    /// bit 1 ligado -> acesso de leitura
    /// bit 2 ligado -> acesso de ação (obviamente, quando o bit 2 estiver ligado, o bit 1 também estará
    /// </returns>
    public int obtemNivelAcessoEtapaWf(int? workflow, long? instanciaWf, int? seqEtapa, int? etapa, string usuario)
    {
        int nivelAcesso = 0;
        string comandoSQL = "";
        try
        {

            comandoSQL = string.Format(
            @"
                DECLARE @RC int
                DECLARE @in_codigoWorkFlow int
                DECLARE @in_codigoInstanciaWf bigint
                DECLARE @in_SequenciaOcorrenciaEtapaWf int
                DECLARE @in_codigoEtapaWf int
                DECLARE @in_identificadorUsuario varchar(50)
                DECLARE @ou_nivelAcesso int
                DECLARE @ou_codigoRetorno int
                DECLARE @ou_mensagemErro nvarchar(2048)

                SET @in_codigoWorkFlow								= {2} 
                SET @in_codigoInstanciaWf							= {3}
                SET @in_SequenciaOcorrenciaEtapaWf		            = {4}
                SET @in_codigoEtapaWf								= {5}
                SET @in_identificadorUsuario					    = '{6}'

                EXECUTE @RC = {0}.{1}.[p_wf_verificaAcessoEtapaWf] 
                   @in_codigoWorkFlow
                  ,@in_codigoInstanciaWf
                  ,@in_SequenciaOcorrenciaEtapaWf
                  ,@in_codigoEtapaWf
                  ,@in_identificadorUsuario
                  ,@ou_nivelAcesso OUTPUT
                  ,@ou_codigoRetorno OUTPUT
                  ,@ou_mensagemErro OUTPUT

                SELECT 
                    @RC                     AS RetornoProc, 
                    @ou_nivelAcesso         AS NivelAcesso, 
                    @ou_codigoRetorno       AS CodigoRetorno, 
                    @ou_mensagemErro        AS MensagemErro "
                           , bancodb
                           , Ownerdb
                           , workflow, instanciaWf, seqEtapa, etapa, usuario);

            DataSet ds = getDataSet(comandoSQL);

            if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
            {
                // não incluindo neste procedimento tratamento de erro.
                // em qualquer situaçãod e erro, será devolvido apenas que o usuário não tem acesso.
                nivelAcesso = int.Parse(ds.Tables[0].Rows[0]["NivelAcesso"].ToString());
            }
        }
        catch
        {
            nivelAcesso = 0;
        }

        return nivelAcesso;
    }

    /// <summary>
    /// Devolve o nível de acesso a uma etapa de um workflow para um determinado usuário
    /// </summary>
    /// <remarks>
    /// Esta função deve ser usada quando se deseja obter o nível de acesso para uma etapa quando ela for instanciada.
    /// Como se trata de uma instância hipotética, deve ser fornecido o identificador do projeto ao qual 'hipoteticamente'
    /// a instância estaria relacionada, pois o acesso de um recurso a uma etapa de workflow
    /// Se a instância já existe, deve ser usada a função obtemNivelAcessoEtapaWf ao invés desta.
    /// </remarks>
    /// <param name="workflow">Código do workflow da etapa que se quer obter o nível de acesso</param>
    /// <param name="identificadorProjeto">Identificador do projeto ao qual a instância estaria relacionada
    /// caso ela existisse.</param>
    /// <param name="etapa">Código da Etapa</param>
    /// <param name="usuario">Identificador do usuário para o qual se deseja obter o nível de acesso.</param>
    /// <returns>um conjunto de bit indicando o nível de acesso.
    /// nenhum bit ligado (valor 0) -> nenhum acesso
    /// bit 1 ligado -> acesso de leitura
    /// bit 2 ligado -> acesso de ação (obviamente, quando o bit 2 estiver ligado, o bit 1 também estará
    /// </returns>
    public int obtemNivelAcessoEtapaWfNaoInstanciada(int? workflow, string identificadorProjeto, int? etapa, string usuario)
    {
        int nivelAcesso = 0;
        string comandoSQL = "";
        try
        {

            comandoSQL = string.Format(
            @"
                DECLARE @RC int
                DECLARE @in_codigoWorkFlow int
                DECLARE @in_identificadorProjeto Varchar(50)
                DECLARE @in_codigoEtapaWf int
                DECLARE @in_identificadorUsuario varchar(50)
                DECLARE @ou_nivelAcesso int
                DECLARE @ou_codigoRetorno int
                DECLARE @ou_mensagemErro nvarchar(2048)

                SET @in_codigoWorkFlow								= {2} 
                SET @in_identificadorProjeto    					= {3}
                SET @in_codigoEtapaWf								= {4}
                SET @in_identificadorUsuario					    = '{5}'

                EXECUTE @RC = {0}.{1}.[p_wf_verificaAcessoEtapaWfNaoInstanciada] 
                   @in_codigoWorkFlow
                  ,@in_identificadorProjeto
                  ,@in_codigoEtapaWf
                  ,@in_identificadorUsuario
                  ,@ou_nivelAcesso OUTPUT
                  ,@ou_codigoRetorno OUTPUT
                  ,@ou_mensagemErro OUTPUT

                SELECT 
                    @RC                     AS RetornoProc, 
                    @ou_nivelAcesso         AS NivelAcesso, 
                    @ou_codigoRetorno       AS CodigoRetorno, 
                    @ou_mensagemErro        AS MensagemErro "
                           , bancodb
                           , Ownerdb
                           , workflow, identificadorProjeto, etapa, usuario);

            DataSet ds = getDataSet(comandoSQL);

            if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
            {
                // não incluindo neste procedimento tratamento de erro.
                // em qualquer situaçãod e erro, será devolvido apenas que o usuário não tem acesso.
                nivelAcesso = int.Parse(ds.Tables[0].Rows[0]["NivelAcesso"].ToString());
            }
        }
        catch
        {
            nivelAcesso = 0;
        }

        return nivelAcesso;
    }

    /// <summary>
    /// Devolve um valor boleano indicando se pode iniciar ou não uma nova instância do fluxo.
    /// </summary>
    /// <remarks>
    /// Se um fluxo tiver um limite máximo de ocorrência para um projeto, esta função devolverá falso 
    /// quando este limite for atingido
    /// </remarks>
    /// <param name="codigoFluxo"></param>
    /// <param name="codigoProjeto"></param>
    /// <returns></returns>
    public bool podeIniciarNovaInstanciaFluxo(int codigoFluxo, int codigoProjeto)
    {
        bool podeIniciar = false;
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(
            @" SELECT {0}.{1}.f_wf_podeIniciarNovaInstancia({2},{3}) AS [podeIniciar] "
                           , bancodb
                           , Ownerdb
                           , codigoProjeto, codigoFluxo);

            DataSet ds = getDataSet(comandoSQL);

            if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
            {
                // não incluindo neste procedimento tratamento de erro.
                // em qualquer situaçãod e erro, será devolvido apenas que não será possível iniciar nova instância
                podeIniciar = (bool)ds.Tables[0].Rows[0]["podeIniciar"];
            }
        }
        catch
        {
            podeIniciar = false;
        }

        return podeIniciar;

    }


    /// <summary>
    /// Devolve um DataSet com os dados da etapa para a qual remete uma notificação de uma ação no workflow
    /// </summary>
    /// <param name="CodigoNotificacaoWf">Código da notificação recebida</param>
    /// <param name="CodigoUsuario">Código do usuário para validação do acesso à etapa</param>
    /// <returns>DataSet</returns>
    public DataSet getDadosLinkEtapaNotificacaoWf(int CodigoNotificacaoWf, int CodigoUsuario)
    {
        comandoSQL = string.Format(@"
            DECLARE
		            @CodigoFluxo		    Int						
	            , @CodigoWorkflow			Int						
	            , @CodigoInstancia			BigInt				
	            , @SequenciaOcorrencia		Int						
	            , @CodigoEtapa				Int						
	            , @CodigoProjeto			Int						
	            , @NivelAcesso				Int	

	            EXEC {0}.{1}.p_wf_obtemDadosLinkNotificacao 
			            @in_CodigoNotificacaoWf			    = {2}
		            , @in_identificadorUsuario				= {3}
		            , @ou_codigoFluxo						= @CodigoFluxo			OUTPUT
		            , @ou_codigoWorkflow					= @CodigoWorkflow		OUTPUT
		            , @ou_codigoInstanciaWf					= @CodigoInstancia		OUTPUT
		            , @ou_sequenciaOcorrenciaEtapaWf	    = @SequenciaOcorrencia	OUTPUT
		            , @ou_codigoEtapaWf						= @CodigoEtapa			OUTPUT
		            , @ou_codigoProjeto						= @CodigoProjeto		OUTPUT
		            , @ou_nivelAcessoUsuario				= @NivelAcesso			OUTPUT

	            SELECT 
		            @CodigoFluxo				AS [CodigoFluxo]
	            , @CodigoWorkflow				AS [CodigoWorkflow]
	            , @CodigoInstancia				AS [CodigoInstanciaWf]
	            , @SequenciaOcorrencia			AS [SequenciaOcorrenciaEtapaWf]
	            , @CodigoEtapa					AS [CodigoEtapaWf]
	            , @CodigoProjeto				AS [CodigoProjeto]
	            , @NivelAcesso					AS [NivelAcesso]    "
            , bancodb, Ownerdb, CodigoNotificacaoWf, CodigoUsuario);
        return classeDados.getDataSet(comandoSQL);
    }

    public DataSet getIniciaisPermissaoWf(string iniciaisFormulario)
    {
        comandoSQL = string.Format(
            @"
                SELECT ps.IniciaisPermissao                
	                FROM 
		                {0}.{1}.PermissaoSistema ps INNER JOIN
                        {0}.{1}.ObjetoMenuPermissaoSistema omps ON omps.CodigoPermissao = ps.CodigoPermissao INNER JOIN
                        {0}.{1}.ObjetoMenu om ON (om.CodigoObjetoMenu = omps.CodigoObjetoMenu AND om.Iniciais = '{2}')
            ", bancodb, Ownerdb, iniciaisFormulario);
        return classeDados.getDataSet(comandoSQL);
    }

    public bool getCodigoWfAtualPorIniciaisFluxo(int codigoEntidade, out int codigoFluxo, out int codigoWorkflow, out int codigoEtapaInicial, string iniciaisFluxo)
    {
        bool bRet = getCodigoWfAtualPorIniciais(codigoEntidade, out codigoFluxo, out codigoWorkflow, iniciaisFluxo);
        codigoEtapaInicial = 0;
        if (bRet)
        {
            bRet = getCodigoEtapaInicialWorkflow(codigoWorkflow, out codigoEtapaInicial);
        }

        return bRet;
    }

    public bool getCodigoEtapaInicialWorkflow(int codigoWorkflow, out int codigoEtapaInicial)
    {
        bool bRet = false;
        codigoEtapaInicial = 0;

        string comandoSQL = "";

            comandoSQL = string.Format(@"
                    SELECT [CodigoEtapaInicial] 
                        FROM 
                            {0}.{1}.[Workflows]  AS [wf] 
                        WHERE   wf.[CodigoWorkflow]  = {2} 
                    ", bancodb, Ownerdb, codigoWorkflow);

            DataSet ds = getDataSet(comandoSQL);

        if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
        {
            codigoEtapaInicial = int.Parse(ds.Tables[0].Rows[0][0].ToString());
            bRet = true;
        }

        return bRet;
    }

    #endregion

    #region _Estrategia - Metas

    /// <summary>
    /// Obtem os indicadores do Objetivos
    /// 
    /// MUDANÇAS
    /// 23/11/2010: Foi comentada a linha [AND me.CodigoUnidadeNegocio = {2}] el filtro esta indicado en el where. 
    ///             E posee uma restrinção maior quando compara o codigo pasado como parâmetro [codigoUnidade] 
    ///             com o [CodigoUnidadeNegocio] da tabela [MapaEstrategico].
    /// 16/12/2010: Foi incrementado no SELECT o campo 'poe.CodigoProjeto'
    /// </summary>
    /// <param name="codigoUnidade"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    public DataSet getIndicadoresObjetivosUnidade(int codigoUnidade, string where)
    {
        string comandoSQL = string.Format(@"
                                     SELECT i.CodigoIndicador, i.NomeIndicador, i.CodigoUnidadeMedida, i.GlossarioIndicador
                                          ,i.CasasDecimais, i.Polaridade, i.FormulaIndicador
                                          ,i.IndicaIndicadorCompartilhado, i.CodigoPeriodicidadeCalculo, tp.DescricaoPeriodicidade_PT
                                          ,i.FonteIndicador, i.CodigoUsuarioResponsavel, tum.SiglaUnidadeMedida, oe.CodigoObjetoEstrategia
                                          ,oe.DescricaoObjetoEstrategia, u.NomeUsuario, (SELECT {0}.{1}.f_GetFormulaPorExtenso(i.CodigoIndicador)) AS formulaPorExtenso
                                          , poe.CodigoProjeto 
                                      FROM {0}.{1}.Indicador i INNER JOIN 
			                                     {0}.{1}.IndicadorObjetivoEstrategico ioe ON ioe.CodigoIndicador = i.CodigoIndicador INNER JOIN
			                                     {0}.{1}.ObjetoEstrategia oe ON oe.CodigoObjetoEstrategia = ioe.CodigoObjetivoEstrategico INNER JOIN
			                                     {0}.{1}.MapaEstrategico me ON me.CodigoMapaEstrategico = oe.CodigoMapaEstrategico INNER JOIN
			                                     {0}.{1}.TipoPeriodicidade tp ON tp.CodigoPeriodicidade = i.CodigoPeriodicidadeCalculo INNER JOIN
			                                     {0}.{1}.TipoUnidadeMedida tum ON tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida INNER JOIN
			                                     {0}.{1}.Usuario u ON u.CodigoUsuario = i.CodigoUsuarioResponsavel LEFT JOIN 
			                                     {0}.{1}.ProjetoObjetivoEstrategico poe ON poe.CodigoObjetivoEstrategico = ioe.CodigoObjetivoEstrategico AND poe.CodigoIndicador = ioe.CodigoIndicador 
                                     WHERE 1=1
                                       --AND me.CodigoUnidadeNegocio = {2}
                                       {3} 
                                     ORDER BY oe.DescricaoObjetoEstrategia, i.NomeIndicador

                                     ", bancodb, Ownerdb, codigoUnidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getIndicadoresMetaUnidadeNegocio(int codigoUsuario, int codigoEntidade, int codigoUnidade, string select, string podeAgruparPorMapa, string where)
    {
        string comandoSelect = "";
        string comandoFrom = "";
        string comandoWhere = "";

        comandoSelect = string.Format(@"
                  SELECT  i.CodigoIndicador           , i.NomeIndicador               , i.CodigoUnidadeMedida
                        , CAST(i.GlossarioIndicador AS Varchar(MAX)) AS GlossarioIndicador        , i.CasasDecimais               , i.Polaridade
                        , i.FormulaIndicador          , iu.Meta                       , i.IndicaIndicadorCompartilhado
                        , i.CodigoPeriodicidadeCalculo, tp.DescricaoPeriodicidade_PT  , i.FonteIndicador, iu.CodigoUnidadeNegocio, un.NomeUnidadeNegocio

                        , CASE WHEN EXISTS( 
					        SELECT TOP 1 1 
						        FROM {0}.{1}.[IndicadorUnidade]	AS [iu2]
							        INNER JOIN {0}.{1}.[UnidadeNegocio]	AS [un]	ON 
								        (			un.[CodigoUnidadeNegocio]   = iu2.[CodigoUnidadeNegocio]
									        AND un.[DataExclusao]				IS NULL 
									        AND un.[CodigoEntidade]			    = {3} )
						        WHERE
									        iu2.[CodigoIndicador]		        = i.[CodigoIndicador]
                                    AND iu2.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio
							        AND iu2.[CodigoResponsavelIndicadorUnidade]  = {2} 
                                  ) 
                          THEN {2} ELSE i.CodigoUsuarioResponsavel END AS CodigoUsuarioResponsavel

                        , tum.SiglaUnidadeMedida        

                        , CASE WHEN EXISTS( 
					        SELECT TOP 1 1 
						        FROM {0}.{1}.[IndicadorUnidade]	AS [iu2]
							        INNER JOIN {0}.{1}.[UnidadeNegocio]	AS [un]	ON 
								        (	un.[CodigoUnidadeNegocio]   = iu2.[CodigoUnidadeNegocio]
									        AND un.[DataExclusao]				IS NULL 
									        AND un.[CodigoEntidade]			    = {3} )
						        WHERE
									        iu2.[CodigoIndicador]		        = i.[CodigoIndicador]
                                    AND iu2.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio
							        AND iu2.[CodigoResponsavelIndicadorUnidade]  = {2} ) 
                          THEN (SELECT u0.NomeUsuario FROM {0}.{1}.[Usuario] u0 WHERE u0.[CodigoUsuario] = {2}) ELSE u.[NomeUsuario]  END AS NomeUsuario       

                        , CASE WHEN EXISTS( 
					        SELECT TOP 1 1 
						        FROM {0}.{1}.[IndicadorUnidade]	AS [iu2]
							        INNER JOIN {0}.{1}.[UnidadeNegocio]	AS [un]	ON 
								        (			un.[CodigoUnidadeNegocio]   = iu2.[CodigoUnidadeNegocio]
									        AND un.[DataExclusao]				IS NULL 
									        AND un.[CodigoEntidade]			    = {3} )
						        WHERE
									    iu2.[CodigoIndicador]		        = i.[CodigoIndicador]
                                    AND iu2.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio
							        AND iu2.[CodigoResponsavelAtualizacaoIndicador]  = {2} ) 
                          THEN (SELECT u0.NomeUsuario FROM {0}.{1}.[Usuario] u0 WHERE u0.[CodigoUsuario] = {2}) ELSE uat.[NomeUsuario]  END AS NomeUsuarioAtualizacao

                        , {0}.{1}.f_GetFormulaPorExtenso(i.CodigoIndicador)             AS formulaPorExtenso

			            , (
					        SELECT MIN({0}.{1}.f_GetDataVencimentoAtualizacaoIndicador(iu2.[CodigoUnidadeNegocio],iu2.[CodigoIndicador]))
						        FROM {0}.{1}.[IndicadorUnidade]	AS [iu2]
							        INNER JOIN {0}.{1}.[UnidadeNegocio]	AS [un]	ON 
								        (			un.[CodigoUnidadeNegocio]   = iu2.[CodigoUnidadeNegocio]
									        AND un.[DataExclusao]				IS NULL 
									        AND un.[CodigoEntidade]			    = {3} )
						        WHERE
									        iu2.[CodigoIndicador]		= i.[CodigoIndicador]
							        AND (iu2.[CodigoUnidadeNegocio]		= iu.CodigoUnidadeNegocio OR iu2.[CodigoResponsavelIndicadorUnidade] = {2})
				            ) AS Atualizacao
			            , CASE WHEN EXISTS (
					        SELECT TOP 1 1 
						        FROM {0}.{1}.[IndicadorUnidade]	AS [iu2]
							        INNER JOIN {0}.{1}.[UnidadeNegocio]	AS [un]	ON 
								        (			un.[CodigoUnidadeNegocio]   = iu2.[CodigoUnidadeNegocio]
									        AND un.[DataExclusao]				IS NULL 
									        AND un.[CodigoEntidade]			    = {3} )
						        WHERE
									        iu2.[CodigoIndicador]		= i.[CodigoIndicador]
							        AND (iu2.[CodigoUnidadeNegocio]		= iu.CodigoUnidadeNegocio OR iu2.[CodigoResponsavelIndicadorUnidade] = {2})
							            AND {0}.{1}.f_GetDataVencimentoAtualizacaoIndicador(iu2.[CodigoUnidadeNegocio],iu2.[CodigoIndicador]) < GetDate() ) 
                          THEN 'Sim' ELSE 'Não' END AS Atrasado
                        {5} ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, codigoUnidade, select);
        comandoFrom = string.Format(@"
                   FROM @tbResumo tb INNER JOIN 
                        {0}.{1}.Indicador i ON i.CodigoIndicador = tb.CodigoIndicador  INNER JOIN 
                        {0}.{1}.IndicadorUnidade iu ON iu.CodigoIndicador = tb.CodigoIndicador AND iu.CodigoUnidadeNegocio = tb.CodigoUnidadeNegocio INNER JOIN
                        {0}.{1}.TipoPeriodicidade tp ON tp.CodigoPeriodicidade = i.CodigoPeriodicidadeCalculo INNER JOIN
                        {0}.{1}.TipoUnidadeMedida tum ON tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida LEFT JOIN
                        {0}.{1}.Usuario u ON u.CodigoUsuario = iu.CodigoResponsavelIndicadorUnidade LEFT JOIN
                        {0}.{1}.Usuario uat ON uat.CodigoUsuario = iu.CodigoResponsavelAtualizacaoIndicador INNER JOIN
                        {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = tb.CodigoUnidadeNegocio
                        ", bancodb, Ownerdb, codigoUnidade);
        comandoWhere = string.Format(@" 
                    WHERE i.DataExclusao IS NULL  
                      {1} ", codigoUnidade, where);

        StringBuilder comandosql = new StringBuilder();
        comandosql.AppendLine(string.Format(@"
	DECLARE @tbResumo TABLE 
		(
			  [CodigoIndicador]					        INT
			, [IndicaUnidadeCriadoraIndicador]	        CHAR(1)
			, [CodigoResponsavelIndicadorUnidade]	    INT
			, [CodigoResponsavelAtualizacaoIndicador]   INT
            , [CodigoUnidadeNegocio]                    INT
		)

	INSERT INTO @tbResumo			
		SELECT -- dos indicadores criados na ENTIDADE atual
					i.[CodigoIndicador]
        , 'S'
        , iu.[CodigoResponsavelIndicadorUnidade]
        , iu.[CodigoResponsavelAtualizacaoIndicador]
        , iu.[CodigoUnidadeNegocio]
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
					)

	INSERT INTO @tbResumo			
		SELECT -- dos indicadores criados em outra entidade, mas compartilhado com a entidade atual
		  i.[CodigoIndicador]
        , 'N'
        , iu.[CodigoResponsavelIndicadorUnidade]
        , iu.[CodigoResponsavelAtualizacaoIndicador]
        , iu.[CodigoUnidadeNegocio]
			FROM
				{0}.{1}.[Indicador]					AS [i]
        INNER JOIN {0}.{1}.[IndicadorUnidade]		AS [iu]		ON 
					(			iu.[CodigoIndicador]	= i.[CodigoIndicador]
								AND iu.[CodigoUnidadeNegocio]		        = {2}
						AND iu.[DataExclusao]			IS NULL
						AND iu.[IndicaUnidadeCriadoraIndicador] != 'S'
					)
      WHERE   i.[DataExclusao]          IS NULL
        "
            , bancodb, Ownerdb, codigoEntidade));

        if (podeAgruparPorMapa == "S")
        {
            comandosql.Append(comandoSelect);
            comandosql.Append(@"
            , CAST('(Sem mapa associado)' AS Varchar(100)) AS MapaEstrategico");
            comandosql.Append(comandoFrom);
            comandosql.Append(comandoWhere);
            comandosql.Append(string.Format(@" AND NOT EXISTS
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
    )", bancodb, Ownerdb, codigoEntidade));
            comandosql.Append("\n UNION \n");
            comandosql.Append(comandoSelect);
            comandosql.Append(@"
            , me.[TituloMapaEstrategico] AS MapaEstrategico");
            comandosql.Append(comandoFrom);
            comandosql.Append(string.Format(@"
    INNER JOIN {0}.{1}.IndicadorObjetivoEstrategico ioe ON tb.CodigoIndicador = ioe.CodigoIndicador 
    INNER JOIN {0}.{1}.ObjetoEstrategia oe ON oe.CodigoObjetoEstrategia = ioe.CodigoObjetivoEstrategico and oe.DataExclusao IS NULL 
    INNER JOIN {0}.{1}.MapaEstrategico me ON me.CodigoMapaEstrategico = oe.CodigoMapaEstrategico and me.IndicaMapaEstrategicoAtivo = 'S' 
    INNER JOIN {0}.{1}.UnidadeNegocio un2 ON un2.CodigoUnidadeNegocio = me.CodigoUnidadeNegocio and un2.DataExclusao IS NULL and un2.IndicaUnidadeNegocioAtiva = 'S' 
                        AND ( un2.CodigoEntidade = {3} OR EXISTS
						        (	SELECT TOP 1 1 
								    FROM {0}.{1}.PermissaoMapaEstrategicoUnidade pmeu
								    WHERE pmeu.CodigoMapaEstrategico = me.CodigoMapaEstrategico
									    and pmeu.CodigoUnidadeNegocio = {3} 
						    )  )
					                        ", bancodb, Ownerdb, codigoUsuario, codigoEntidade));
            comandosql.Append(comandoWhere);
        }
        else
        {
            comandosql.Append(comandoSelect);
            comandosql.Append(@"
            , CAST('' AS VArchar(100)) AS MapaEstrategico");
            comandosql.Append(comandoFrom);
            comandosql.Append(comandoWhere);
        }

        comandosql.Append(@" 
            ORDER BY i.NomeIndicador");
        comandoSQL = comandosql.ToString();
        return getDataSet(comandoSQL);
    }

    public DataSet getIndicadoresUnidadeNegocio(int codigoUsuario, int codigoEntidade, int codigoUnidade, string select, string podeAgruparPorMapa, string where)
    {
        string comandoSelect = "";
        string comandoFrom = "";
        string comandoWhere = "";

        comandoSelect = string.Format(@"
                  SELECT  i.CodigoIndicador           , i.NomeIndicador               , i.CodigoUnidadeMedida
                        , CAST(i.GlossarioIndicador AS Varchar(MAX)) AS GlossarioIndicador        , i.CasasDecimais               , i.Polaridade
                        , i.FormulaIndicador          , iu.Meta                       , i.IndicaIndicadorCompartilhado
                        , i.CodigoPeriodicidadeCalculo, tp.DescricaoPeriodicidade_PT  , i.FonteIndicador, iu.CodigoUnidadeNegocio, un.NomeUnidadeNegocio

                        , CASE WHEN EXISTS( 
					        SELECT TOP 1 1 
						        FROM {0}.{1}.[IndicadorUnidade]	AS [iu2]
							        INNER JOIN {0}.{1}.[UnidadeNegocio]	AS [un]	ON 
								        (			un.[CodigoUnidadeNegocio]   = iu2.[CodigoUnidadeNegocio]
									        AND un.[DataExclusao]				IS NULL 
									        AND un.[CodigoEntidade]			    = {3} )
						        WHERE
									        iu2.[CodigoIndicador]		        = i.[CodigoIndicador]
                                    AND iu2.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio
							        AND iu2.[CodigoResponsavelIndicadorUnidade]  = {2} 
                                  ) 
                          THEN {2} ELSE i.CodigoUsuarioResponsavel END AS CodigoUsuarioResponsavel

                        , tum.SiglaUnidadeMedida        

                        , CASE WHEN EXISTS( 
					        SELECT TOP 1 1 
						        FROM {0}.{1}.[IndicadorUnidade]	AS [iu2]
							        INNER JOIN {0}.{1}.[UnidadeNegocio]	AS [un]	ON 
								        (	un.[CodigoUnidadeNegocio]   = iu2.[CodigoUnidadeNegocio]
									        AND un.[DataExclusao]				IS NULL 
									        AND un.[CodigoEntidade]			    = {3} )
						        WHERE
									        iu2.[CodigoIndicador]		        = i.[CodigoIndicador]
                                    AND iu2.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio
							        AND iu2.[CodigoResponsavelIndicadorUnidade]  = {2} ) 
                          THEN (SELECT u0.NomeUsuario FROM {0}.{1}.[Usuario] u0 WHERE u0.[CodigoUsuario] = {2}) ELSE u.[NomeUsuario]  END AS NomeUsuario       

                        , CASE WHEN EXISTS( 
					        SELECT TOP 1 1 
						        FROM {0}.{1}.[IndicadorUnidade]	AS [iu2]
							        INNER JOIN {0}.{1}.[UnidadeNegocio]	AS [un]	ON 
								        (			un.[CodigoUnidadeNegocio]   = iu2.[CodigoUnidadeNegocio]
									        AND un.[DataExclusao]				IS NULL 
									        AND un.[CodigoEntidade]			    = {3} )
						        WHERE
									    iu2.[CodigoIndicador]		        = i.[CodigoIndicador]
                                    AND iu2.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio
							        AND iu2.[CodigoResponsavelAtualizacaoIndicador]  = {2} ) 
                          THEN (SELECT u0.NomeUsuario FROM {0}.{1}.[Usuario] u0 WHERE u0.[CodigoUsuario] = {2}) ELSE uat.[NomeUsuario]  END AS NomeUsuarioAtualizacao

                        , {0}.{1}.f_GetFormulaPorExtenso(i.CodigoIndicador)             AS formulaPorExtenso

			            , (
					        SELECT MIN({0}.{1}.f_GetDataVencimentoAtualizacaoIndicador(iu2.[CodigoUnidadeNegocio],iu2.[CodigoIndicador]))
						        FROM {0}.{1}.[IndicadorUnidade]	AS [iu2]
							        INNER JOIN {0}.{1}.[UnidadeNegocio]	AS [un]	ON 
								        (			un.[CodigoUnidadeNegocio]   = iu2.[CodigoUnidadeNegocio]
									        AND un.[DataExclusao]				IS NULL 
									        AND un.[CodigoEntidade]			    = {3} )
						        WHERE
									        iu2.[CodigoIndicador]		= i.[CodigoIndicador]
							        AND (iu2.[CodigoUnidadeNegocio]		= iu.CodigoUnidadeNegocio OR iu2.[CodigoResponsavelIndicadorUnidade] = {2})
				            ) AS Atualizacao
			            , CASE WHEN EXISTS (
					        SELECT TOP 1 1 
						        FROM {0}.{1}.[IndicadorUnidade]	AS [iu2]
							        INNER JOIN {0}.{1}.[UnidadeNegocio]	AS [un]	ON 
								        (			un.[CodigoUnidadeNegocio]   = iu2.[CodigoUnidadeNegocio]
									        AND un.[DataExclusao]				IS NULL 
									        AND un.[CodigoEntidade]			    = {3} )
						        WHERE
									        iu2.[CodigoIndicador]		= i.[CodigoIndicador]
							        AND (iu2.[CodigoUnidadeNegocio]		= iu.CodigoUnidadeNegocio OR iu2.[CodigoResponsavelIndicadorUnidade] = {2})
							            AND {0}.{1}.f_GetDataVencimentoAtualizacaoIndicador(iu2.[CodigoUnidadeNegocio],iu2.[CodigoIndicador]) < GetDate() ) 
                          THEN '{6}' ELSE '{7}' END AS Atrasado
                        {5} ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, codigoUnidade, select, Resources.traducao.sim, Resources.traducao.nao);
        comandoFrom = string.Format(@"
                   FROM @tbResumo tb INNER JOIN 
                        {0}.{1}.Indicador i ON i.CodigoIndicador = tb.CodigoIndicador  INNER JOIN 
                        {0}.{1}.IndicadorUnidade iu ON iu.CodigoIndicador = tb.CodigoIndicador AND iu.CodigoUnidadeNegocio = tb.CodigoUnidadeNegocio INNER JOIN
                        {0}.{1}.TipoPeriodicidade tp ON tp.CodigoPeriodicidade = i.CodigoPeriodicidadeCalculo INNER JOIN
                        {0}.{1}.TipoUnidadeMedida tum ON tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida LEFT JOIN
                        {0}.{1}.Usuario u ON u.CodigoUsuario = iu.CodigoResponsavelIndicadorUnidade LEFT JOIN
                        {0}.{1}.Usuario uat ON uat.CodigoUsuario = iu.CodigoResponsavelAtualizacaoIndicador INNER JOIN
                        {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = tb.CodigoUnidadeNegocio
                        ", bancodb, Ownerdb, codigoUnidade);
        comandoWhere = string.Format(@" 
                    WHERE i.DataExclusao IS NULL  
                      {1} ", codigoUnidade, where);

        StringBuilder comandosql = new StringBuilder();
        comandosql.AppendLine(string.Format(@"
	DECLARE @tbResumo TABLE 
		(
			  [CodigoIndicador]					        INT
			, [IndicaUnidadeCriadoraIndicador]	        CHAR(1)
			, [CodigoResponsavelIndicadorUnidade]	    INT
			, [CodigoResponsavelAtualizacaoIndicador]   INT
            , [CodigoUnidadeNegocio]                    INT
		)

	INSERT INTO @tbResumo			
		SELECT -- dos indicadores criados na ENTIDADE atual
					i.[CodigoIndicador]
        , 'S'
        , iu.[CodigoResponsavelIndicadorUnidade]
        , iu.[CodigoResponsavelAtualizacaoIndicador]
        , iu.[CodigoUnidadeNegocio]
			FROM
				{0}.{1}.[Indicador]					AS [i]
        INNER JOIN {0}.{1}.[IndicadorUnidade]		AS [iu]		ON 
					(			iu.[CodigoIndicador]		= i.[CodigoIndicador]
						AND iu.[CodigoUnidadeNegocio]		= {2}
						AND iu.[DataExclusao]			    IS NULL
						AND iu.[IndicaUnidadeCriadoraIndicador] = 'S'
					)
      WHERE   i.[DataExclusao]          IS NULL
        AND ((GETDATE() BETWEEN IsNull(i.DataInicioValidadeMeta,CONVERT(datetime, '01/01/1900', 103)) AND IsNull(i.DataTerminoValidadeMeta,CONVERT(datetime, '31/12/2999',103))) OR i.IndicaAcompanhamentoMetaVigencia = 'N')
		UNION ALL
		SELECT -- dos indicadores criados em UNIDADE da ENTIDADE atual
					i.[CodigoIndicador]
        , 'S'
        , iu.[CodigoResponsavelIndicadorUnidade]
        , iu.[CodigoResponsavelAtualizacaoIndicador]
        , iu.[CodigoUnidadeNegocio]
			FROM
				{0}.{1}.[Indicador]					AS [i]
        INNER JOIN {0}.{1}.[IndicadorUnidade]		AS [iu]		ON 
					(			iu.[CodigoIndicador]	    = i.[CodigoIndicador]
						AND iu.[DataExclusao]			IS NULL
					)
				INNER JOIN {0}.{1}.[UnidadeNegocio]			AS [un]		ON 
					(			un.[CodigoUnidadeNegocio]	= iu.[CodigoUnidadeNegocio]
						AND un.DataExclusao			        IS NULL
						AND un.[IndicaUnidadeNegocioAtiva]	= 'S'
						AND un.[CodigoEntidade]			    = {2}
                        AND un.CodigoUnidadeNegocio <> {2}
					)
      WHERE   i.[DataExclusao]          IS NULL
        AND ((GETDATE() BETWEEN IsNull(i.DataInicioValidadeMeta,CONVERT(datetime, '01/01/1900', 103)) AND IsNull(i.DataTerminoValidadeMeta,CONVERT(datetime, '31/12/2999',103))) OR i.IndicaAcompanhamentoMetaVigencia = 'N')
		

	INSERT INTO @tbResumo			
		SELECT -- dos indicadores criados em outra entidade, mas compartilhado com a entidade atual
		  i.[CodigoIndicador]
        , 'N'
        , iu.[CodigoResponsavelIndicadorUnidade]
        , iu.[CodigoResponsavelAtualizacaoIndicador]
        , iu.[CodigoUnidadeNegocio]
			FROM
				{0}.{1}.[Indicador]					AS [i]
        INNER JOIN {0}.{1}.[IndicadorUnidade]		AS [iu]		ON 
					(			iu.[CodigoIndicador]	= i.[CodigoIndicador]
								AND iu.[CodigoUnidadeNegocio]		        = {2}
						AND iu.[DataExclusao]			IS NULL
						AND iu.[IndicaUnidadeCriadoraIndicador] != 'S'
					)
      WHERE   i.[DataExclusao]          IS NULL
        AND ((GETDATE() BETWEEN IsNull(i.DataInicioValidadeMeta,CONVERT(datetime, '01/01/1900', 103)) AND IsNull(i.DataTerminoValidadeMeta,CONVERT(datetime, '31/12/2999',103))) OR i.IndicaAcompanhamentoMetaVigencia = 'N')
		"
            , bancodb, Ownerdb, codigoEntidade));

        if (podeAgruparPorMapa == "S")
        {
            comandosql.Append(comandoSelect);
            comandosql.Append(@"
            , CAST('(Sem mapa associado)' AS Varchar(100)) AS MapaEstrategico");
            comandosql.Append(comandoFrom);
            comandosql.Append(comandoWhere);
            comandosql.Append(string.Format(@" AND NOT EXISTS
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
    )", bancodb, Ownerdb, codigoEntidade));
            comandosql.Append("\n UNION \n");
            comandosql.Append(comandoSelect);
            comandosql.Append(@"
            , me.[TituloMapaEstrategico] AS MapaEstrategico");
            comandosql.Append(comandoFrom);
            comandosql.Append(string.Format(@"
    INNER JOIN {0}.{1}.IndicadorObjetivoEstrategico ioe ON tb.CodigoIndicador = ioe.CodigoIndicador 
    INNER JOIN {0}.{1}.ObjetoEstrategia oe ON oe.CodigoObjetoEstrategia = ioe.CodigoObjetivoEstrategico and oe.DataExclusao IS NULL 
    INNER JOIN {0}.{1}.MapaEstrategico me ON me.CodigoMapaEstrategico = oe.CodigoMapaEstrategico and me.IndicaMapaEstrategicoAtivo = 'S' 
    INNER JOIN {0}.{1}.UnidadeNegocio un2 ON un2.CodigoUnidadeNegocio = me.CodigoUnidadeNegocio and un2.DataExclusao IS NULL and un2.IndicaUnidadeNegocioAtiva = 'S' 
                        AND ( un2.CodigoEntidade = {3} OR EXISTS
						        (	SELECT TOP 1 1 
								    FROM {0}.{1}.PermissaoMapaEstrategicoUnidade pmeu
								    WHERE pmeu.CodigoMapaEstrategico = me.CodigoMapaEstrategico
									    and pmeu.CodigoUnidadeNegocio = {3} 
						    )  )
					                        ", bancodb, Ownerdb, codigoUsuario, codigoEntidade));
            comandosql.Append(comandoWhere);
        }
        else
        {
            comandosql.Append(comandoSelect);
            comandosql.Append(@"
            , CAST('' AS VArchar(100)) AS MapaEstrategico");
            comandosql.Append(comandoFrom);
            comandosql.Append(comandoWhere);
        }

        comandosql.Append(@" 
            ORDER BY i.NomeIndicador");
        comandoSQL = comandosql.ToString();
        return getDataSet(comandoSQL);
    }

    public DataSet getIndicadoresProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(@"
                        DECLARE
                        @Data   DateTime
                      , @Ano    SmallInt
                      , @Mes    SmallInt
                        	
                        SET @Data = GETDATE()
                        SET @Ano = YEAR(@Data)
                        SET @Mes = MONTH(@Data)

                        SELECT mo.CodigoMetaOperacional, i.CodigoIndicador, i.NomeIndicador, i.CodigoUnidadeMedida, i.GlossarioIndicador
                                  ,i.CasasDecimais, i.Polaridade, mo.CodigoPeriodicidade
                                  ,mo.CodigoPeriodicidade AS CodigoPeriodicidadeCalculo, tp.DescricaoPeriodicidade_PT
                                  ,mo.FonteIndicador, i.CodigoUsuarioResponsavel, tum.SiglaUnidadeMedida
                                  ,u.NomeUsuario, mo.MetaDescritiva AS Meta, mo.MetaNumerica, tfd.NomeFuncao AS Agrupamento
                                  ,{0}.{1}.f_GetCorDesempenhoResultado(rmo.ValorMetaAcumuladaAno, rmo.ValorResultadoAcumuladoAno, i.Polaridade, 'IO', mo.CodigoIndicador) AS Desempenho
                                  , CASE WHEN EXISTS(SELECT 1
                                                        FROM {0}.{1}.DesdobramentoMetaOperacional dmo INNER JOIN
															 {0}.{1}.MetaOperacional mo ON mo.CodigoMetaOperacional = dmo.CodigoMetaOperacional
                                                       WHERE mo.CodigoIndicador = i.CodigoIndicador
                                                         AND mo.CodigoProjeto = {2}) 
                                           OR EXISTS(SELECT 1
                                                        FROM {0}.{1}.ResultadoMetaOperacional rmo INNER JOIN
															 {0}.{1}.MetaOperacional mo ON mo.CodigoMetaOperacional = rmo.CodigoMetaOperacional
                                                       WHERE mo.CodigoIndicador = i.CodigoIndicador
                                                         AND mo.CodigoProjeto = {2})
                                         THEN 'N' ELSE 'S' END AS EditaPeriodicidade
                                , i.TipoIndicador 
                                , CASE WHEN i.TipoIndicador = 'O' THEN 'Operacional'
                                        WHEN i.TipoIndicador = 'D' THEN 'Desempenho' 
                                        ELSE 'Não informado'
                                    END DescTipoIndicador
                                , mo.CodigoUsuarioResponsavelAtualizacao
                                , ua.NomeUsuario AS UsuarioAtualizacao
                                , mo.DataInicioValidadeMeta
                                , mo.DataTerminoValidadeMeta
                                , mo.IndicaAcompanhaMetaVigencia
								, CASE WHEN 
								(
											(EXISTS (SELECT 1 FROM ResumoMetaOperacional WHERE CodigoMetaOperacional = mo.CodigoMetaOperacional))
										 OR (EXISTS (SELECT 1 FROM DesdobramentoMetaOperacional WHERE CodigoMetaOperacional = mo.CodigoMetaOperacional))
										 OR (EXISTS (SELECT 1 FROM ResultadoMetaOperacional WHERE CodigoMetaOperacional = mo.CodigoMetaOperacional)) 
								) THEN 'S' ElSE 'N' END AS ExisteDependencia
                             FROM {0}.{1}.IndicadorOperacional i 
                                  INNER JOIN {0}.{1}.MetaOperacional               AS mo
                                          ON mo.CodigoIndicador = i.CodigoIndicador 
                                  INNER JOIN {0}.{1}.TipoPeriodicidade                          AS tp
                                          ON tp.CodigoPeriodicidade = mo.CodigoPeriodicidade 
                                  INNER JOIN {0}.{1}.TipoUnidadeMedida                          AS tum
                                          ON tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida 
                                  LEFT JOIN {0}.{1}.Usuario                                    AS u
                                          ON u.CodigoUsuario = i.CodigoUsuarioResponsavel 
                                  LEFT JOIN {0}.{1}.TipoFuncaoDado                             AS tfd
                                          ON tfd.CodigoFuncao = i.CodigoFuncaoAgrupamentoMeta 
                                  LEFT JOIN  {0}.{1}.ResumoMetaOperacional	                AS rmo
                                          ON rmo.CodigoMetaOperacional = mo.CodigoMetaOperacional
                                          AND rmo.Ano =  YEAR({0}.{1}.f_GetDataRefUltimoDadoMetaOperacional(mo.CodigoMetaOperacional, @Ano, @Mes))
                                          AND rmo.Mes = MONTH({0}.{1}.f_GetDataRefUltimoDadoMetaOperacional(mo.CodigoMetaOperacional, @Ano, @Mes))
                                 LEFT JOIN  {0}.{1}.Usuario ua ON ua.CodigoUsuario = mo.CodigoUsuarioResponsavelAtualizacao
                         WHERE mo.CodigoProjeto = {2}  {3}
                           AND i.DataExclusao IS NULL  
                         ORDER BY i.NomeIndicador", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getIndicadoresOperacionaisProjetos(int codigoUsuario, int codigoEntidade, string where)
    {
        bool perfilAdm = PerfilAdministrador(codigoUsuario, codigoEntidade);

        if (!perfilAdm)
            where += "AND mo.CodigoUsuarioResponsavelAtualizacao = " + codigoUsuario;

        string comandoSQL = string.Format(@"
                        
                        SELECT mo.CodigoMetaOperacional, i.CodigoIndicador, i.NomeIndicador, i.CasasDecimais, mo.CodigoPeriodicidade
                              ,i.CodigoUsuarioResponsavel, tum.SiglaUnidadeMedida, u.NomeUsuario, p.NomeProjeto, p.CodigoProjeto
                              ,(SELECT MAX(ISNULL(DataAlteracao, DataInclusao)) 
                                  FROM {0}.{1}.ResultadoMetaOperacional 
                                 WHERE CodigoMetaOperacional = mo.CodigoMetaOperacional) AS UltimaAtualizacao
                              , CASE WHEN i.TipoIndicador = 'O' THEN 'Operacional'
                                        WHEN i.TipoIndicador = 'D' THEN 'Desempenho' 
                                        ELSE 'Não informado'
                                    END DescTipoIndicador   
                             FROM {0}.{1}.IndicadorOperacional i INNER JOIN 
								  {0}.{1}.MetaOperacional AS mo ON mo.CodigoIndicador = i.CodigoIndicador INNER JOIN 
								  {0}.{1}.TipoUnidadeMedida AS tum ON tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida INNER JOIN 
								  {0}.{1}.Usuario AS u ON u.CodigoUsuario = i.CodigoUsuarioResponsavel INNER JOIN
								  {0}.{1}.Projeto AS p ON p.CodigoProjeto = mo.CodigoProjeto
                         WHERE i.DataExclusao IS NULL  
                           {4}
                         ORDER BY i.NomeIndicador
                                 ,p.NomeProjeto
                        ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getIndicadoresGestaoMetas(int codigoUsuario, int codigoEntidade, string select, string where)
    {
        comandoSQL = string.Format(@"
                  SELECT i.CodigoIndicador, i.NomeIndicador, i.CasasDecimais, iu.Meta, tum.SiglaUnidadeMedida, ioe.CodigoObjetoEstrategia, iu.Descricao, iu.Comentario, oe.CodigoMapaEstrategico
                        ,CASE WHEN ta.IniciaisTipoAssociacao = 'PP' THEN 'PSP'  
                              WHEN ta.IniciaisTipoAssociacao = 'OB' THEN 'OBJ' END AS Tipo
                        ,CASE WHEN EXISTS(SELECT TOP 1 1 
						                    FROM {0}.{1}.[IndicadorUnidade] AS [iu] INNER JOIN 
                                                 {0}.{1}.[UnidadeNegocio]	AS [un]	ON (un.[CodigoUnidadeNegocio] = iu.[CodigoUnidadeNegocio] AND un.[DataExclusao] IS NULL AND un.[CodigoEntidade] = {3})
						                   WHERE iu.[CodigoIndicador] = i.[CodigoIndicador]
							                 AND iu.[CodigoResponsavelIndicadorUnidade]  = {2}) 
                              THEN (SELECT u0.NomeUsuario FROM {0}.{1}.[Usuario] u0 WHERE u0.[CodigoUsuario] = {2}) ELSE u.[NomeUsuario]  END AS NomeUsuario
                        {4} 
                 FROM {0}.{1}.Indicador i INNER JOIN 
                      {0}.{1}.IndicadorUnidade iu ON iu.CodigoIndicador = i.CodigoIndicador INNER JOIN
                      {0}.{1}.TipoUnidadeMedida tum ON tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida INNER JOIN
                      {0}.{1}.Usuario u ON u.CodigoUsuario = iu.CodigoResponsavelIndicadorUnidade LEFT JOIN
                      {0}.{1}.IndicadorObjetoEstrategia ioe ON ioe.CodigoIndicador = i.CodigoIndicador LEFT JOIN 
                      {0}.{1}.TipoAssociacao ta ON ta.CodigoTipoAssociacao = ioe.CodigoTipoAssociacao LEFT JOIN
                      {0}.{1}.ObjetoEstrategia oe ON (oe.CodigoObjetoEstrategia = ioe.CodigoObjetoEstrategia)
                WHERE iu.CodigoUnidadeNegocio = {3} 
                  AND i.DataExclusao IS NULL  
                  AND iu.DataExclusao IS NULL  
                  {5} 
                ORDER BY i.NomeIndicador", bancodb, Ownerdb, codigoUsuario, codigoEntidade, select, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getObjetosAssociacaoMetas(string codigoMapa, string iniciaisTipoObjeto, int codigoIndicador, string where)
    {
        comandoSQL = string.Format(@"
            BEGIN
                 DECLARE @CodigoIndicador INT

                 SET @CodigoIndicador = {5}

                 SELECT CodigoObjetoEstrategia
			           ,CASE WHEN toe.IniciaisTipoObjeto = 'PSP' THEN oe.TituloObjetoEstrategia
				             ELSE oe.DescricaoObjetoEstrategia END AS Descricao
                  FROM {0}.{1}.ObjetoEstrategia oe INNER JOIN
			           {0}.{1}.TipoObjetoEstrategia toe ON (toe.CodigoTipoObjetoEstrategia = oe.CodigoTipoObjetoEstrategia
				                                AND toe.IniciaisTipoObjeto = '{3}')
                 WHERE NOT EXISTS(SELECT 1 
								    FROM {0}.{1}.IndicadorObjetoEstrategia ioe 
								   WHERE ioe.CodigoObjetoEstrategia = oe.CodigoObjetoEstrategia
                                     AND ioe.CodigoIndicador <> @CodigoIndicador
									 AND @CodigoIndicador <> -1)
	                 AND oe.DataExclusao IS NULL
                     AND oe.CodigoMapaEstrategico = {2}
                     {4}
                 ORDER BY Descricao
            END", bancodb, Ownerdb, codigoMapa, iniciaisTipoObjeto, where, codigoIndicador);

        return getDataSet(comandoSQL);
    }

    public bool atualizaGestaoMetas(int codigoIndicador, int codigoEntidade, int codigoUsuario, int codigoObjetoEstrategia, string meta, string descricao, string comentario, string iniciaisTipoAssociacao)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                    BEGIN
                        DECLARE @CodigoIndicador INT,
                                @CodigoEntidade INT,
                                @CodigoTipoAssociacao INT,
                                @CodigoUsuario INT,
                                @CodigoObjetoEstrategia INT
        
                        SET @CodigoIndicador          = {2}
                        SET @CodigoEntidade           = {3}
                        SET @CodigoUsuario            = {4}
                        SET @CodigoObjetoEstrategia   = {5}

                        SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao
                          FROM {0}.{1}.TipoAssociacao
                         WHERE IniciaisTipoAssociacao = '{9}'

                        DELETE FROM {0}.{1}.IndicadorObjetoEstrategia 
                         WHERE CodigoIndicador = @CodigoIndicador
                        
                        INSERT INTO {0}.{1}.IndicadorObjetoEstrategia(CodigoIndicador, CodigoObjetoEstrategia, CodigoTipoAssociacao)
                                                               VALUES(@CodigoIndicador, @CodigoObjetoEstrategia, @CodigoTipoAssociacao) 

                        UPDATE {0}.{1}.IndicadorUnidade
                           SET Meta                         = '{6}'
                              ,Descricao                    = '{7}'
                              ,Comentario                   = '{8}'
                              ,DataUltimaAlteracao          = GETDATE()
                              ,CodigoUsuarioUltimaAlteracao = {5}
                        WHERE CodigoIndicador      = @CodigoIndicador
                          AND CodigoUnidadeNegocio = @CodigoEntidade
                    END
                        ", bancodb, Ownerdb
                         , codigoIndicador
                         , codigoEntidade
                         , codigoUsuario
                         , codigoObjetoEstrategia
                         , meta
                         , descricao
                         , comentario
                         , iniciaisTipoAssociacao);

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool excluiGestaoMetas(int codigoIndicador, int codigoEntidade, int codigoUsuario, int codigoObjetoEstrategia, string iniciaisTipoAssociacao)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                    BEGIN
                        DECLARE @CodigoIndicador INT,
                                @CodigoEntidade INT,
                                @CodigoTipoAssociacao INT,
                                @CodigoUsuario INT,
                                @CodigoObjetoEstrategia INT
        
                        SET @CodigoIndicador          = {2}
                        SET @CodigoEntidade           = {3}
                        SET @CodigoUsuario            = {4}
                        SET @CodigoObjetoEstrategia   = {5}

                        SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao
                          FROM {0}.{1}.TipoAssociacao
                         WHERE IniciaisTipoAssociacao = '{6}'

                        DELETE FROM {0}.{1}.IndicadorObjetoEstrategia 
                         WHERE CodigoIndicador = @CodigoIndicador

                        UPDATE {0}.{1}.IndicadorUnidade
                           SET Meta                         = null
                              ,Descricao                    = null
                              ,Comentario                   = null
                              ,DataUltimaAlteracao          = GETDATE()
                              ,CodigoUsuarioUltimaAlteracao = {5}
                        WHERE CodigoIndicador      = @CodigoIndicador
                          AND CodigoUnidadeNegocio = @CodigoEntidade
                    END
                        ", bancodb, Ownerdb
                         , codigoIndicador
                         , codigoEntidade
                         , codigoUsuario
                         , codigoObjetoEstrategia
                         , iniciaisTipoAssociacao);

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public DataSet getMetasAtualizacao(int codEntidade, int codigoIndicador, int codigoUnidade, int casasDecimais, string where)
    {
        DataSet ds;
        string comandoSQL = string.Format(@"			
            BEGIN
              DECLARE @CodigoEntidade Int, --> Parâmetro
                      @CodigoUnidade Int, --> Parâmetro
                      @codigoIndicador Int, --> Parâmetro
                      @IntervaloMeses Int,
                      @AnoMesInicioValidadeMeta Varchar(7),
                      @AnoMesTerminoValidadeMeta Varchar(7)
              
              /* Parâmetros a serem passados para consulta */

              SET @CodigoEntidade = {2}
              SET @codigoIndicador = {3}
              
              SELECT @CodigoUnidade = MIN(iu.CodigoUnidadeNegocio)
                FROM IndicadorUnidade AS iu
               WHERE iu.CodigoIndicador = @codigoIndicador
                 AND iu.IndicaUnidadeCriadoraIndicador = 'S'
                 AND iu.DataExclusao IS NULL


              /* Retorno final da consulta */  
              DECLARE @tmp TABLE 
                (_Ano SmallInt,
                 Periodo Varchar(50),
                 _Mes SmallInt,
                 _CodigoIndicador Int,
                 _NomeIndicador Varchar(255),
                 Valor Decimal(22,5),
                 Editavel Char(1),
                 CodigoUnidade  Int,
                 NomeUnidade  Varchar(250),
                 SiglaUnidade  Varchar(30))
                 
              /* Cursor */     
              DECLARE @Ano SmallInt, @Mes SmallInt, @wAno SmallInt, @DataRefPeriodo SmallDateTime, @Periodo Varchar(50),
                      @TipoDetalhe Char(1),
                      @CodigoPeriodicidade SmallInt,
                      @NomeIndicador Varchar(255),
                      @Editavel Char(1)
              
               SELECT @CodigoPeriodicidade = CodigoPeriodicidadeCalculo,
                      @NomeIndicador = NomeIndicador,
                      @IntervaloMeses = tp.IntervaloMeses,
                      @AnoMesInicioValidadeMeta = Convert(Varchar,Year(DataInicioValidadeMeta)) +  Right('00' +  Convert(Varchar,Month(DataInicioValidadeMeta)),2),
                      @AnoMesTerminoValidadeMeta = Convert(Varchar,Year(DataTerminoValidadeMeta))  + Right('00' +  Convert(Varchar,Month(DataTerminoValidadeMeta)),2)    
                 FROM {0}.{1}.Indicador AS i INNER JOIN
                      {0}.{1}.TipoPeriodicidade AS tp ON (tp.CodigoPeriodicidade = i.CodigoPeriodicidadeCalculo)
                WHERE CodigoIndicador = @CodigoIndicador
               
                IF @AnoMesInicioValidadeMeta IS NULL
                   SET @AnoMesInicioValidadeMeta = '190001'
                
                IF @AnoMesTerminoValidadeMeta IS NULL
                   SET @AnoMesTerminoValidadeMeta = '207812'
                                  
               /* Alterado por Ericsson em 17/04/2010. É necessário obedecer o mesmo período estratégico definido
                  pela entidade que criou o indicador. */
               DECLARE cCursor CURSOR FOR
                SELECT DISTINCT 
                       pe.Ano,
                       pe.IndicaMetaEditavel
                  FROM {0}.{1}.PeriodoEstrategia AS pe 
                 WHERE pe.IndicaAnoAtivo = 'S'
                   AND pe.CodigoUnidadeNegocio IN (SELECT un.CodigoEntidade
                                                     FROM {0}.{1}.IndicadorUnidade iu INNER JOIN
                                                          {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio
                                                    WHERE CodigoIndicador = @CodigoIndicador
                                                      AND IndicaUnidadeCriadoraIndicador = 'S')
                   AND EXISTS(SELECT 1
                                FROM {0}.{1}.IndicadorUnidade
                               WHERE CodigoIndicador = @codigoIndicador 
                                 AND CodigoUnidadeNegocio = @CodigoUnidade)
                 
              OPEN cCursor
              
              FETCH NEXT FROM cCursor INTO @Ano, @Editavel
                                           
              WHILE @@FETCH_STATUS = 0 BEGIN
                SET @DataRefPeriodo = {0}.{1}.f_GetDataReferenciaPeriodoIndicador(@codigoIndicador, @Ano, 1);
                SET @wAno = YEAR(@DataRefPeriodo);
                SET @Mes = MONTH(@DataRefPeriodo);

                WHILE @wAno = @Ano BEGIN
                    SET @Periodo = {0}.{1}.f_GetNomePeriodoIndicador(@codigoIndicador, @wAno, @Mes, 4, 4);

                    INSERT INTO @tmp (_Ano, Periodo, _Mes, _CodigoIndicador, _NomeIndicador, Valor, Editavel, CodigoUnidade, NomeUnidade, SiglaUnidade)
                    SELECT @wAno, 
                           @Periodo, 
                           @Mes, 
                           @codigoIndicador, 
                           @NomeIndicador, 
                           null, 
                           @Editavel,
                           un.CodigoUnidadeNegocio,
                           un.NomeUnidadeNegocio,
                           un.SiglaUnidadeNegocio
                      FROM {0}.{1}.IndicadorUnidade AS iu INNER JOIN
                           {0}.{1}.UnidadeNegocio AS un ON (iu.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio)
                     WHERE iu.CodigoIndicador = @codigoIndicador      
                       AND iu.CodigoUnidadeNegocio IN (SELECT uno.CodigoUnidadeNegocio FROM {0}.{1}.UnidadeNegocio AS uno INNER JOIN {0}.{1}.UnidadeNegocio AS ent ON (ent.CodigoEntidade = uno.CodigoEntidade AND ent.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio))
                    --VALUES (@wAno, @Periodo, @Mes, @codigoIndicador, @NomeIndicador, null, @Editavel);

                    SET @DataRefPeriodo = DATEADD(MI,1,@DataRefPeriodo);
                    SET @wAno = YEAR(@DataRefPeriodo);
                    SET @Mes = MONTH(@DataRefPeriodo);

                    SET @DataRefPeriodo = {0}.{1}.f_GetDataReferenciaPeriodoIndicador(@codigoIndicador, @wAno, @Mes);
                    SET @wAno = YEAR(@DataRefPeriodo);
                    SET @Mes = MONTH(@DataRefPeriodo);
                END -- WHILE @wAno = @Ano 
                              
                FETCH NEXT FROM cCursor INTO @Ano, @Editavel
                
              END
                
              CLOSE cCursor
              DEALLOCATE cCursor
              
              UPDATE @tmp
                 SET Valor = ValorMeta
                FROM {0}.{1}.MetaIndicadorUnidade
               WHERE CodigoUnidadeNegocio = CodigoUnidade
                 AND _Ano = MetaIndicadorUnidade.Ano
                 AND _Mes = MetaIndicadorUnidade.Mes
                 AND _CodigoIndicador = MetaIndicadorUnidade.CodigoIndicador
                 
             UPDATE @tmp
                 SET Editavel = 'N'
               WHERE Convert(Varchar,_Ano) + Right('00' + Convert(Varchar,_Mes),2) Not Between @AnoMesInicioValidadeMeta AND @AnoMesTerminoValidadeMeta
               
                  
              SELECT * FROM @tmp 
               WHERE 1 = 1 
                 {5}
               ORDER BY NomeUnidade, _Ano, _Mes, Periodo, _NomeIndicador  
               
            END", bancodb, Ownerdb, codEntidade, codigoIndicador, casasDecimais, where, codigoUnidade);
        ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getListaUnidadesIndicador(int codEntidade, int codigoIndicador, string where)
    {
        DataSet ds;
        string comandoSQL = string.Format(@"			
            BEGIN
              DECLARE @CodigoEntidade Int, --> Parâmetro
                      @codigoIndicador Int --> Parâmetro
              
              /* Parâmetros a serem passados para consulta */

              SET @CodigoEntidade = {2}
              SET @codigoIndicador = {3}              
             
             SELECT DISTINCT un.CodigoUnidadeNegocio AS CodigoUnidade
                  FROM {0}.{1}.UnidadeNegocio un
                 WHERE EXISTS(SELECT 1
                                FROM {0}.{1}.IndicadorUnidade iu
                               WHERE CodigoIndicador = @codigoIndicador
								 AND iu.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio )                               
                         
              
               
            END", bancodb, Ownerdb, codEntidade, codigoIndicador, where);
        ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getMetasProjetoAtualizacao(int codEntidade, int codigoMeta, int codigoIndicador, string dataInicio, string dataTermino, int codigoProjeto, int casasDecimais, string where)
    {
        DataSet ds;
        string comandoSQL = string.Format(@"			
        			
    BEGIN
      DECLARE @CodigoMeta Int, --> Parâmetro
              @CodigoEntidade Int, --> Parâmetro
              @codigoIndicador Int, --> Parâmetro
			  @DataInicio DateTime, --> Data de início do parâmetro
			  @DataTermino DateTime,  --> Data de término do parâmetro
			  @CodigoProjeto Int,
			  @AnoInicio SmallInt,
			  @AnoTermino SmallInt
			  
      /* Parâmetros a serem passados para consulta */

      SET @CodigoEntidade = {2}
      SET @codigoMeta = {3}
      SET @codigoIndicador = {4}
      SET @DataInicio = {5}
      SET @DataTermino = {6}
      SET @CodigoProjeto = {7}

      /* Retorno final da consulta */  
      DECLARE @tmp TABLE 
        (_Data SmallDateTime,
         _Ano SmallInt,
         Periodo Varchar(50),
         _Mes SmallInt,
         _CodigoMeta Int,
         _NomeIndicador Varchar(255),
         Valor Decimal(18,{9}),
         Editavel Char(1))
         
      /* Tabela auxiliar criada para simplificar o resultado final */
      DECLARE @ControlePeriodo TABLE
        (Data SmallDateTime,
         Mes SmallInt,
         Bimestre Tinyint,
         Trimestre Tinyint,
         Quadrimestre Tinyint,
         Semestre  Tinyint,
         DescMes Varchar(20),
         DescBim Varchar(20),
         DescTri Varchar(20),
         DescQua Varchar(20),
         DescSem Varchar(20),
         DescDia Varchar(20),
         DescAno Varchar(20))

       /* Variáveis  */     
      DECLARE @Ano SmallInt,
              @TipoDetalhe Char(1),
              @CodigoPeriodicidade SmallInt,
              @NomeIndicador Varchar(255),
              @Editavel Char(1),
              @IntervaloDias SmallInt
      
       SELECT @CodigoPeriodicidade = iop.CodigoPeriodicidade,
              @NomeIndicador = NomeIndicador,
              @IntervaloDias = IntervaloDias
         FROM {0}.{1}.MetaOperacional iop INNER JOIN
              {0}.{1}.IndicadorOperacional AS i ON i.CodigoIndicador = iop.CodigoIndicador INNER JOIN
              {0}.{1}.TipoPeriodicidade AS t ON (t.CodigoPeriodicidade = iop.CodigoPeriodicidade)
        WHERE iop.CodigoMetaOperacional = @codigoMeta
        
      
      IF @IntervaloDias < 30 --> A periodicidade é menor que mensal! 
         BEGIN
           DECLARE @DataControle SmallDateTime
           
           SET @DataControle = @DataInicio
           WHILE @DataControle <= @DataTermino
             BEGIN
                INSERT INTO @ControlePeriodo
				  (Data, DescDia)
				VALUES (Convert(Varchar,@DataControle,112),Convert(Varchar,@DataControle,112))
               SET @DataControle = DATEADD(DD,1,@DataControle)
             END
         END  
      ELSE
         BEGIN
		  /* Atualiza a tabela auxiliar */
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes)
		  VALUES (1,'Jan')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes, Bimestre,DescBim)
		  VALUES(2,'Fev',1,'1º Bim')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes, Trimestre,DescTri)
		  VALUES(3,'Mar',1,'1º Tri')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes,Bimestre,DescBim, Quadrimestre, DescQua)
		  VALUES(4,'Abr',2,'2º Bim', 1, '1º Quad')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes)
		  VALUES(5,'Mai')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes, Bimestre, DescBim, Trimestre, DescTri, Semestre, DescSem)
		  VALUES(6,'Jun',3,'3º Bim',2,'2º Tri',1,'1º Sem')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes)
		  VALUES(7,'Jul')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes, Bimestre,DescBim, Quadrimestre, DescQua)
		  VALUES(8,'Ago',4,'4º Bim', 2, '2º Quad')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes, Trimestre,DescTri)
		  VALUES(9,'Set',3,'3º Tri')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes,Bimestre,DescBim)
		  VALUES(10,'Out',5,'5º Bim')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes)
		  VALUES(11,'Nov')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes, Bimestre, DescBim, Trimestre, DescTri, Semestre, DescSem, DescAno, Quadrimestre, DescQua)
		  VALUES(12,'Dez',6,'6º Bim',4,'4º Tri',2,'2º Sem','Ano', 3, '3º Quad')
		END  
                
         /* Alterado por Ericsson em 17/04/2010. É necessário obedecer o mesmo período estratégico definido
	                  pela entidade que criou o indicador. */
	               DECLARE cCursor CURSOR FOR
	                SELECT DISTINCT 
	                       pe.Ano,
	                       pe.IndicaMetaEditavel
	                  FROM {0}.{1}.PeriodoAnalisePortfolio AS pe 
	                 WHERE pe.IndicaAnoAtivo = 'S'
					   AND pe.CodigoEntidade = @CodigoEntidade
	                 
	              OPEN cCursor
	              
	              FETCH NEXT FROM cCursor INTO @Ano,
	                                           @Editavel
	                                           
	              WHILE @@FETCH_STATUS = 0
                BEGIN
                      
		   IF @IntervaloDias = 30 --  Periodicidade Mensal 
			  INSERT INTO @tmp
			  (_Ano, Periodo, _Mes, _CodigoMeta, _NomeIndicador, Valor, Editavel)
			  SELECT @Ano,
					 DescMes + '/' + + CONVERT(VarChar, @Ano),
					 Mes,
					 @codigoMeta,
					 @NomeIndicador,
					 null,
					 @Editavel
				FROM @ControlePeriodo
                           
            ELSE IF @IntervaloDias = 60 
                         INSERT INTO @tmp
                           (_Ano, Periodo, _Mes, _CodigoMeta, _NomeIndicador, Valor, Editavel)
                         SELECT @Ano,
                                   DescBim + '/' + + CONVERT(VarChar, @Ano),
                                   Mes,
                                   @codigoMeta,
								   @NomeIndicador,
                                   null,
                                   @Editavel
                             FROM @ControlePeriodo
                            WHERE Bimestre IS NOT NULL  
                              
            ELSE IF @IntervaloDias = 90 --> Trimestral 
                         INSERT INTO @tmp
                            (_Ano, Periodo, _Mes, _CodigoMeta, _NomeIndicador, Valor, Editavel)
                         SELECT @Ano,
                                   DescTri + '/' + + CONVERT(VarChar, @Ano),
                                   Mes,
                                   @codigoMeta,
								   @NomeIndicador,
                                   null,
                                   @Editavel
                             FROM @ControlePeriodo
                            WHERE Trimestre IS NOT NULL  
                              
            ELSE IF @IntervaloDias = 120 --> Quadrimestral
                         INSERT INTO @tmp
                            (_Ano, Periodo, _Mes, _CodigoMeta, _NomeIndicador, Valor, Editavel)
                         SELECT @Ano,
                                   DescQua + '/' + + CONVERT(VarChar, @Ano),
                                   Mes,
                                   @codigoMeta,
								   @NomeIndicador,
                                   null,
                                   @Editavel
                             FROM @ControlePeriodo
                            WHERE Quadrimestre IS NOT NULL  

            ELSE IF @IntervaloDias = 180 --> Semestral
                         INSERT INTO @tmp
                           (_Ano, Periodo, _Mes, _CodigoMeta, _NomeIndicador, Valor, Editavel)
                         SELECT @Ano,
                                   DescSem + '/' + + CONVERT(VarChar, @Ano),
                                   Mes,
                                   @codigoMeta,
								   @NomeIndicador,
                                   null,
                                   @Editavel
                             FROM @ControlePeriodo
                            WHERE Semestre IS NOT NULL   
                        
              ELSE IF @IntervaloDias = 360 --> Anual
                         INSERT INTO @tmp
                           (_Ano, Periodo, _Mes, _CodigoMeta, _NomeIndicador, Valor, Editavel)
                            SELECT @Ano,
                                   @Ano,
                                   12,
                                   @codigoMeta,
								   @NomeIndicador,
                                   null,
                                   @Editavel
                             FROM @ControlePeriodo   
                            WHERE DescAno IS NOT NULL
                      
               ELSE IF @IntervaloDias = 1
                               INSERT INTO @tmp
								   (_Ano, Periodo, _Mes, _CodigoMeta, _NomeIndicador, Valor, Editavel, _Data)
									SELECT YEAR(Data),
										   Convert(Varchar,Data,103),
										   MONTH(Data),
										   @codigoMeta,
										   @NomeIndicador,
										   null,
										   @Editavel,
										   Convert(Varchar,Data,112)
									 FROM @ControlePeriodo 
									 FETCH NEXT FROM cCursor INTO @Ano,
									                                                @Editavel
									                 
									                 END
									                 
									               CLOSE cCursor
              DEALLOCATE cCursor
                      
      IF @IntervaloDias >= 30 
         BEGIN       
			  UPDATE @tmp
				 SET Valor = ValorMeta
				FROM {0}.{1}.DesdobramentoMetaOperacional
			   WHERE _Ano = Year(DesdobramentoMetaOperacional.DataReferenciaPeriodo)
				 AND _Mes = Month(DesdobramentoMetaOperacional.DataReferenciaPeriodo)
				 AND _CodigoMeta = DesdobramentoMetaOperacional.CodigoMetaOperacional
			
			     
			  SELECT _Ano,
			         Periodo,
			         _Mes,
			         _CodigoMeta,
			         _NomeIndicador,
			         Valor,
			         Editavel,
			         CONVERT(DateTime, '01/' + Convert(Varchar,_Mes) + '/' +  Convert(Varchar,_Ano), 103) AS Data
			    FROM @tmp 
			   WHERE 1 = 1 {8}		             
			   ORDER BY _Ano, _Mes, Periodo, _NomeIndicador  
       	 
		 END		 
	  ELSE
	     BEGIN
	      UPDATE @tmp
			 SET Valor = ValorMeta
			FROM {0}.{1}.DesdobramentoMetaOperacional
		   WHERE _Data = DesdobramentoMetaOperacional.DataReferenciaPeriodo
			 AND _CodigoMeta = DesdobramentoMetaOperacional.CodigoMetaOperacional
			 
				      
		  SELECT _Ano,
			     Periodo,
			     _Mes,
			     _CodigoMeta,
			     _NomeIndicador,
			     Valor,
			     Editavel,
			     _Data AS Data		     
			FROM @tmp 
		   WHERE 1 = 1 {8}	         
		   ORDER BY _Data, Periodo, _NomeIndicador  
       
	     END		 	 
    
    END    			
        ", bancodb, Ownerdb, codEntidade, codigoMeta, codigoIndicador, dataInicio, dataTermino, codigoProjeto, where, casasDecimais);
        ds = getDataSet(comandoSQL);
        return ds;
    }

    public void getValoresMetaIndicadorProjeto(int codigoMeta, int codigoIndicador, int codigoProjeto, int codigoEntidade, ref double metaNumerica, ref double metaTotalInformada
        , ref double minimo, ref double maximo, ref double media, ref double quantidadeMetas, ref double ultimaMeta, ref string nomeAgrupamento, ref string siglaAgrupamento)
    {
        DataSet ds;

        string comandoSQL = string.Format(@"
            BEGIN
	            DECLARE @ValorMeta decimal(18,4),
			            @ValorTotalMetaMeses decimal(18,4),
			            @CodigoEntidade int,
			            @CodigoProjeto int,
			            @CodigoIndicador int,
			            @CodigoMetaOperacional int,
			            @ValorMinimo decimal(18,4),
			            @ValorMaximo decimal(18,4),
			            @NumeroMetas int,
                        @SiglaAgrupamento VarChar(50),
                        @NomeAgrupamento VarChar(50),
                        @UltimaMeta decimal(18,4)
			
	            SET @CodigoMetaOperacional = {2}
                SET @CodigoIndicador = {3}
	            SET @CodigoProjeto = {4}
	            SET @CodigoEntidade = {5}
	
	            SELECT @ValorMeta = MetaNumerica 
	              FROM {0}.{1}.MetaOperacional
	             WHERE CodigoMetaOperacional = @CodigoMetaOperacional
	   
	            SELECT @ValorTotalMetaMeses = SUM(ValorMeta) 
					  ,@ValorMinimo = Min(ValorMeta)
					  ,@ValorMaximo = Max(ValorMeta)
					  ,@NumeroMetas = COUNT(1)
                      ,@UltimaMeta = (SELECT Top 1 ValorMeta
	                                   FROM {0}.{1}.DesdobramentoMetaOperacional
	                                  WHERE CodigoMetaOperacional = @CodigoMetaOperacional
				                        AND ValorMeta IS NOT NULL
                                        AND YEAR(DataReferenciaPeriodo) IN (SELECT pe.Ano	                       
	                                                                          FROM {0}.{1}.PeriodoAnalisePortfolio AS pe 
	                                                                         WHERE pe.IndicaAnoAtivo = 'S'
					                                                           AND pe.CodigoEntidade = @CodigoEntidade)
				                      ORDER BY DataReferenciaPeriodo DESC)
	              FROM {0}.{1}.DesdobramentoMetaOperacional
	             WHERE CodigoMetaOperacional = @CodigoMetaOperacional
				   AND ValorMeta IS NOT NULL
                   AND YEAR(DataReferenciaPeriodo) IN ( SELECT pe.Ano	                       
	                                                      FROM {0}.{1}.PeriodoAnalisePortfolio AS pe 
	                                                     WHERE pe.IndicaAnoAtivo = 'S'
					                                       AND pe.CodigoEntidade = @CodigoEntidade)

                SELECT @NomeAgrupamento = CASE WHEN iop.IndicaCriterio = 'S' THEN 'STATUS' ELSE tfd.NomeFuncao END
                     , @SiglaAgrupamento = CASE WHEN iop.IndicaCriterio = 'S' THEN 'STT' ELSE tfd.NomeFuncaoBD END
                  FROM {0}.{1}.IndicadorOperacional iop LEFT JOIN
	                   {0}.{1}.TipoFuncaoDado tfd ON tfd.CodigoFuncao = iop.CodigoFuncaoAgrupamentoMeta
                 WHERE iop.CodigoIndicador = @CodigoIndicador
	   
	            SELECT @NomeAgrupamento AS NomeAgrupamento
                      ,@SiglaAgrupamento AS SiglaAgrupamento
                      ,ISNULL(@ValorMeta, 0) AS MetaNumerica
                      ,ISNULL(@ValorTotalMetaMeses, 0) AS MetaTotalInformada
                      ,ISNULL(@ValorMinimo, 0) AS ValorMinimo
                      ,ISNULL(@ValorMaximo, 0) AS ValorMaximo
                      ,ISNULL(@NumeroMetas, 0) AS NumeroMetas
                      ,ISNULL(@UltimaMeta, 0) AS UltimaMeta
                      ,CASE WHEN ISNULL(@NumeroMetas, 0) = 0 THEN 0
						ELSE ISNULL(@ValorTotalMetaMeses, 0) / ISNULL(@NumeroMetas, 0) END AS Media
	
            END
        ", bancodb, Ownerdb, codigoMeta, codigoIndicador, codigoProjeto, codigoEntidade);
        ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            metaNumerica = double.Parse(ds.Tables[0].Rows[0]["MetaNumerica"].ToString());
            metaTotalInformada = (double)Math.Round(double.Parse(ds.Tables[0].Rows[0]["MetaTotalInformada"].ToString()), 2);
            minimo = double.Parse(ds.Tables[0].Rows[0]["ValorMinimo"].ToString());
            maximo = double.Parse(ds.Tables[0].Rows[0]["ValorMaximo"].ToString());
            media = double.Parse(ds.Tables[0].Rows[0]["Media"].ToString());
            quantidadeMetas = double.Parse(ds.Tables[0].Rows[0]["NumeroMetas"].ToString());
            ultimaMeta = double.Parse(ds.Tables[0].Rows[0]["UltimaMeta"].ToString());
            nomeAgrupamento = ds.Tables[0].Rows[0]["NomeAgrupamento"].ToString();
            siglaAgrupamento = ds.Tables[0].Rows[0]["SiglaAgrupamento"].ToString();
        }

    }

    public bool atualizaMetaIndicadorProjeto(int codigoMeta, string data, string valor, int codigoUsuario)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(
            @"BEGIN
	                DECLARE @CodigoMeta  int,
			                @Data DateTime,
			                @Valor Decimal(18,4),
			                @CodigoUsuario int,
			                @PossuiRegistro int
                	
	                SET @CodigoMeta = {2}
	                SET @Data = CONVERT(DateTime, '{3}', 103)
	                Set @Valor = {4}
	                Set @CodigoUsuario = {5} 
                		
	                SELECT @PossuiRegistro = COUNT(1) 
	                  FROM {0}.{1}.DesdobramentoMetaOperacional
	                 WHERE CodigoMetaOperacional = @CodigoMeta
	                   AND DataReferenciaPeriodo = @Data
                	
	                IF @PossuiRegistro = 0
		                INSERT INTO {0}.{1}.DesdobramentoMetaOperacional(CodigoMetaOperacional, CodigoUsuarioInclusao, DataInclusao, DataReferenciaPeriodo, ValorMeta) VALUES
										                (@CodigoMeta, @CodigoUsuario, GETDATE(), @Data, @Valor)
	                ELSE
		                UPDATE {0}.{1}.DesdobramentoMetaOperacional SET ValorMeta = @Valor,
										                        CodigoUsuarioAlteracao = @CodigoUsuario,
										                        DataAlteracao = GETDATE()
		                 WHERE CodigoMetaOperacional = @CodigoMeta
	                      AND DataReferenciaPeriodo = @Data
                								
                END", bancodb, Ownerdb, codigoMeta, data, valor.Replace(',', '.'), codigoUsuario);

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool atualizaMetaDescritivaIndicador(int codigoIndicador, string metaDescritiva, int codigoEntidade, int codigoUsuario)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"EXEC {0}.{1}.p_dme_atualizaMetaDescritiva {3}, {4}, {2}, {5};", bancodb, Ownerdb, metaDescritiva, codigoIndicador, codigoEntidade, codigoUsuario);

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool atualizaValorMetaPrenchida(string chave, string metaNumerica, string codigoUsuario)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
		                UPDATE {0}.{1}.MetaOperacional
                           SET MetaNumerica = {2}
                              ,DataAlteracao = GETDATE()
                              ,CodigoUsuarioAlteracao = {4}
		                 WHERE CodigoMetaOperacional = {3}
            ", bancodb, Ownerdb, metaNumerica.Replace(',', '.'), chave, codigoUsuario);

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }
    /*
	string dataInicioVigencia = (ddlInicioVigencia.Value != null) ? string.Format("CONVERT(DateTime, '{0:dd/MM/yyyy}', 103)", ddlInicioVigencia.Date) : "null";
	string dataTerminoVigencia = (ddlTerminoVigencia.Value != null) ? string.Format("CONVERT(DateTime, '{0:dd/MM/yyyy}', 103)", ddlTerminoVigencia.Date) : "null";
	string indicaVigencia = cbVigencia.Value.ToString();
	*/
    public bool atualizaMetaPrenchida(string chave, string meta, string periodicidade, string codigoUsuario, string fonte, string codigoResponsavelAtualizacao, string DataInicioValidadeMeta, string DataTerminoValidadeMeta, string IndicaAcompanhaMetaVigencia)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
		                UPDATE {0}.{1}.MetaOperacional
                           SET MetaDescritiva = '{2}'
                            , CodigoPeriodicidade = {5}
                            , DataAlteracao = GETDATE()
                            , CodigoUsuarioAlteracao = {4}
                            , FonteIndicador = '{6}'
                            , CodigoUsuarioResponsavelAtualizacao = {7}
                            ,[DataInicioValidadeMeta] = {8}
                            ,[DataTerminoValidadeMeta] = {9}
                            ,[IndicaAcompanhaMetaVigencia] = '{10}'
		                 WHERE CodigoMetaOperacional = {3}
            ", bancodb, Ownerdb, meta.Replace("'", "''"), chave, codigoUsuario, periodicidade, fonte, codigoResponsavelAtualizacao, DataInicioValidadeMeta, DataTerminoValidadeMeta, IndicaAcompanhaMetaVigencia);

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool atualizaResultadoIndicadorProjeto(int codigoMeta, string data, string valor, int codigoUsuario)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(
            @"BEGIN
	                DECLARE @CodigoMeta  int,
			                @Data DateTime,
			                @Valor Decimal(18,4),
			                @CodigoUsuario int,
			                @PossuiRegistro int
                	
	                SET @CodigoMeta = {2}
	                SET @Data = CONVERT(DateTime, '{3}', 103)
	                Set @Valor = {4}
	                Set @CodigoUsuario = {5} 
                		
	                SELECT @PossuiRegistro = COUNT(1) 
	                  FROM {0}.{1}.ResultadoMetaOperacional
	                 WHERE CodigoMetaOperacional = @CodigoMeta
	                   AND DataReferenciaPeriodo = @Data
                	
	                IF @PossuiRegistro = 0
		                INSERT INTO {0}.{1}.ResultadoMetaOperacional(CodigoMetaOperacional, CodigoUsuarioInclusao, DataInclusao, DataReferenciaPeriodo, ValorResultado) VALUES
										                (@CodigoMeta, @CodigoUsuario, GETDATE(), @Data, @Valor)
	                ELSE
		                UPDATE {0}.{1}.ResultadoMetaOperacional SET ValorResultado = @Valor,
										                        CodigoUsuarioAlteracao = @CodigoUsuario,
										                        DataAlteracao = GETDATE()
		                 WHERE CodigoMetaOperacional = @CodigoMeta
	                      AND DataReferenciaPeriodo = @Data
                								
                END", bancodb, Ownerdb, codigoMeta, data, valor.Replace(',', '.'), codigoUsuario);

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool excluiResultadosProjeto(int codigoMeta, string data)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(
            @"BEGIN
	                DECLARE @codigoMeta int,
			                @Data DateTime
                	
	                SET @codigoMeta = {2}
	                SET @Data = CONVERT(DateTime, '{3}', 103)
                		
		                DELETE FROM {0}.{1}.ResultadoMetaOperacional 
		                 WHERE CodigoMetaOperacional = @codigoMeta
		                   AND DataReferenciaPeriodo = @Data
                								
                END", bancodb, Ownerdb, codigoMeta, data);

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool excluiMetasProjeto(int codigoMeta, string data)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(
            @"BEGIN
	                DECLARE @codigoMeta int,
			                @Data DateTime
                	
	                SET @codigoMeta = {2}
	                SET @Data = CONVERT(DateTime, '{3}', 103)
                		
		                DELETE FROM {0}.{1}.DesdobramentoMetaOperacional 
		                 WHERE CodigoMetaOperacional = @codigoMeta
		                   AND DataReferenciaPeriodo = @Data
                								
                END", bancodb, Ownerdb, codigoMeta, data);

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool atualizaMetaIndicador(int codUnidade, int codigoIndicador, int mes, int ano, string valor, int codigoUsuario)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(
            @"BEGIN
	                DECLARE @CodigoIndicador int,
			                @CodigoUnidade int,
			                @Mes int,
			                @Ano int,
			                @Valor Decimal(22,8),
			                @CodigoUsuario int,
			                @PossuiRegistro int
                	
	                SET @CodigoIndicador = {2}
	                SET @CodigoUnidade = {3}
	                SET @Mes = {4}
	                Set @Ano = {5}
	                Set @Valor = {6}
	                Set @CodigoUsuario = {7} 
                		
	                SELECT @PossuiRegistro = COUNT(1) 
	                  FROM {0}.{1}.MetaIndicadorUnidade
	                 WHERE CodigoIndicador = @CodigoIndicador
	                   AND Mes = @Mes
	                   AND Ano = @Ano
                       AND CodigoUnidadeNegocio = @CodigoUnidade
                	
	                IF @PossuiRegistro = 0
		                INSERT INTO {0}.{1}.MetaIndicadorUnidade(CodigoIndicador, CodigoUnidadeNegocio, CodigoUsuarioInclusao, DataInclusao, Ano, Mes, ValorMeta) VALUES
										                (@CodigoIndicador, @CodigoUnidade, @CodigoUsuario, GETDATE(), @Ano, @Mes, @Valor)
	                ELSE
		                UPDATE {0}.{1}.MetaIndicadorUnidade SET ValorMeta = @Valor,
										                        CodigoUsuarioUltimaAlteracao = @CodigoUsuario,
										                        DataUltimaAlteracao = GETDATE()
		                 WHERE CodigoIndicador = @CodigoIndicador
		                   AND Mes = @Mes
		                   AND Ano = @Ano
                           AND CodigoUnidadeNegocio = @CodigoUnidade
                								
                END", bancodb, Ownerdb, codigoIndicador, codUnidade, mes, ano, valor.Replace(',', '.'), codigoUsuario);

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public bool incluiIndicadorProjeto(int codigoIndicador, int codigoProjeto, string metaDescritiva, string periodicidade, int codigoUsuario, string fonte, string codigoResponsavelAtualizacao, string dataInicioVigencia, string dataTerminoVigencia, string indicaVigencia, ref int novoCodigoMeta)
    {
        string comandoSQL = "";

        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @CodigoMeta int

		                    INSERT {0}.{1}.MetaOperacional (CodigoIndicador, CodigoProjeto, MetaDescritiva, CodigoPeriodicidade, DataInclusao, CodigoUsuarioInclusao, FonteIndicador, CodigoUsuarioResponsavelAtualizacao, DataInicioValidadeMeta,DataTerminoValidadeMeta,IndicaAcompanhaMetaVigencia)
                                                    VALUES (            {2},           {3},          '{4}',                 {5},    GetDate(),                   {6},          '{7}',                                 {8},                    {9},                   {10},'{11}')

                            SELECT @CodigoMeta = SCOPE_IDENTITY()

                            SELECT @CodigoMeta AS CodigoMeta
                        END
            ", bancodb, Ownerdb,
                /*{2}*/codigoIndicador,
                /*{3}*/codigoProjeto,
                /*{4}*/metaDescritiva.Replace("'", "''"),
                /*{5}*/periodicidade,
                /*{6}*/codigoUsuario,
                /*{7}*/fonte,
                /*{8}*/codigoResponsavelAtualizacao,
                /*{9}*/dataInicioVigencia,
                /*{10}*/dataTerminoVigencia,
                /*{11}*/indicaVigencia);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigoMeta = int.Parse(ds.Tables[0].Rows[0]["CodigoMeta"].ToString());

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool excluiIndicadorProjeto(int codigoMeta)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
BEGIN

    DECLARE @CodigoMetaOperacional INT
	    SET @CodigoMetaOperacional = {2}

    DELETE FROM DesdobramentoMetaOperacional WHERE CodigoMetaOperacional = @CodigoMetaOperacional

    DELETE FROM ResultadoMetaOperacional WHERE CodigoMetaOperacional = @CodigoMetaOperacional

    DELETE FROM ResumoMetaOperacional WHERE CodigoMetaOperacional = @CodigoMetaOperacional

    DELETE FROM MetaOperacional WHERE CodigoMetaOperacional = @CodigoMetaOperacional

END", bancodb, Ownerdb, codigoMeta);

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    #endregion

    #region Estrategia - Resultados

    public DataSet getDadosIndicador(string codigoIndicador)
    {
        string comandoSQL = string.Format(
            @"SELECT d.CodigoDado, 
                     d.DescricaoDado
                FROM {0}.{1}.Dado d INNER JOIN 
                     {0}.{1}.DadoIndicador di ON d.CodigoDado = di.CodigoDado
               WHERE di.CodigoIndicador = {2};", bancodb, Ownerdb, codigoIndicador);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getResultadosIndicador(int codEntidade, int codigoIndicador, int codigoUnidade, int casasDecimais, bool usuarioPodeAtualizarMesesBloqueados, string where)
    {
        DataSet ds;

        string bloqueiaMeses = string.Format(@"UPDATE @tmp SET Editavel = 'N' WHERE DATEADD(DAY, @LimiteDiasBloqueio, DateAdd(Month, 1, CONVERT(DateTime, '01/' + CONVERT(VarChar, RIGHT('00' + CONVERT(VarChar, (_Mes)), 2)) + '/' + CONVERT(VarChar, _Ano), 103))) < GETDATE()");

        if (usuarioPodeAtualizarMesesBloqueados)
            bloqueiaMeses = "";

        string comandoSQL = string.Format(@"			
            BEGIN
              DECLARE @CodigoEntidade Int, --> Parâmetro
                      @codigoIndicador Int, --> Parâmetro
                      @CodigoUnidade Int, --> Parâmetro
                      @codigoDado Int, --> Parâmetro
                      @IntervaloMeses Int,
                      @LimiteDiasBloqueio int,
                      @AnoMesInicioValidadeMeta Varchar(7),
                      @AnoMesTerminoValidadeMeta Varchar(7)
              
              /* Parâmetros a serem passados para consulta */

              SET @CodigoEntidade = {2}
              SET @codigoIndicador = {3}
              SET @CodigoUnidade = {7}

              SELECT @LimiteDiasBloqueio = LimiteAlertaEdicaoIndicador,
                      @AnoMesInicioValidadeMeta = Convert(Varchar,Year(DataInicioValidadeMeta)) +  Right('00' +  Convert(Varchar,Month(DataInicioValidadeMeta)),2),
                      @AnoMesTerminoValidadeMeta = Convert(Varchar,Year(DataTerminoValidadeMeta)) +  Right('00' +  Convert(Varchar,Month(DataTerminoValidadeMeta)),2)    
                FROM {0}.{1}.Indicador
               WHERE CodigoIndicador = @codigoIndicador
               
              IF @AnoMesInicioValidadeMeta IS NULL
                 SET @AnoMesInicioValidadeMeta = '190001'
                
              IF @AnoMesTerminoValidadeMeta IS NULL
                 SET @AnoMesTerminoValidadeMeta = '207812'

              /* Retorno final da consulta */  
              DECLARE @tmp TABLE 
                (_Ano SmallInt,
                 Periodo Varchar(50),
                 _Mes SmallInt,
                 _CodigoDado Int,
                 _NomeDado Varchar(255),
                 Valor Decimal(22,8),
                 Editavel Char(1))
                 
              /* Tabela auxiliar criada para simplificar o resultado final */
              DECLARE @ControlePeriodo TABLE
                (Mes SmallInt,
                 Bimestre Tinyint,
                 Trimestre Tinyint,
                 Quadrimestre  Tinyint,
                 Semestre  Tinyint,
                 DescMes Varchar(20),
                 DescBim Varchar(20),
                 DescTri Varchar(20),
                 DescQua Varchar(20),
                 DescSem Varchar(20))
                 
              /* Atualiza a tabela auxiliar */
              INSERT INTO @ControlePeriodo
              (Mes, DescMes)
              VALUES (1,'Jan')
              INSERT INTO @ControlePeriodo
              (Mes, DescMes, Bimestre,DescBim)
              VALUES(2,'Fev',1,'1º Bim')
              INSERT INTO @ControlePeriodo
              (Mes, DescMes, Trimestre,DescTri)
              VALUES(3,'Mar',1,'1º Tri')
              INSERT INTO @ControlePeriodo
              (Mes, DescMes,Bimestre,DescBim, Quadrimestre, DescQua)
              VALUES(4,'Abr',2,'2º Bim', 1, '1º Quad' )
              INSERT INTO @ControlePeriodo
              (Mes, DescMes)
              VALUES(5,'Mai')
              INSERT INTO @ControlePeriodo
              (Mes, DescMes, Bimestre, DescBim, Trimestre, DescTri, Semestre, DescSem)
              VALUES(6,'Jun',3,'3º Bim',2,'2º Tri',1,'1º Sem')
              INSERT INTO @ControlePeriodo
              (Mes, DescMes)
              VALUES(7,'Jul')
              INSERT INTO @ControlePeriodo
              (Mes, DescMes, Bimestre,DescBim, Quadrimestre, DescQua)
              VALUES(8,'Ago',4,'4º Bim', 2, '2º Quad' )
              INSERT INTO @ControlePeriodo
              (Mes, DescMes, Trimestre,DescTri)
              VALUES(9,'Set',3,'3º Tri')
              INSERT INTO @ControlePeriodo
              (Mes, DescMes,Bimestre,DescBim)
              VALUES(10,'Out',5,'5º Bim')
              INSERT INTO @ControlePeriodo
              (Mes, DescMes)
              VALUES(11,'Nov')
              INSERT INTO @ControlePeriodo
              (Mes, DescMes, Bimestre, DescBim, Trimestre, DescTri, Semestre, DescSem, Quadrimestre, DescQua)
              VALUES(12,'Dez',6,'6º Bim',4,'4º Tri',2,'2º Sem', 3, '3º Quad' )
              
               
              
              /* Cursor  */     
              DECLARE @Ano SmallInt,
                      @CodigoPeriodicidade SmallInt,                
                      @Editavel Char(1)
              
                /*Periodicidade Alterada por Thiago em 7/12/2010 para trazer sempre atualização Mensal*/
                SET @CodigoPeriodicidade = 1
                SET @IntervaloMeses = 1

               /*SELECT @CodigoPeriodicidade = CodigoPeriodicidadeCalculo,
                      @IntervaloMeses = tp.IntervaloMeses    
                 FROM {0}.{1}.Indicador AS i INNER JOIN
                      {0}.{1}.TipoPeriodicidade AS tp ON (tp.CodigoPeriodicidade = i.CodigoPeriodicidadeCalculo)
                WHERE CodigoIndicador = @CodigoIndicador*/
                                  
              DECLARE cCursor CURSOR FOR
               SELECT DISTINCT 
                       pe.Ano,
                       pe.IndicaResultadoEditavel
                  FROM {0}.{1}.PeriodoEstrategia AS pe 
                 WHERE pe.IndicaAnoAtivo = 'S'
                   AND pe.CodigoUnidadeNegocio IN (SELECT un.CodigoEntidade
                                                     FROM {0}.{1}.IndicadorUnidade iu INNER JOIN
                                                          {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio
                                                    WHERE CodigoIndicador = @CodigoIndicador
                                                      AND IndicaUnidadeCriadoraIndicador = 'S')
                   AND EXISTS(SELECT 1
                                FROM {0}.{1}.IndicadorUnidade
                               WHERE CodigoIndicador = @codigoIndicador 
                                 AND CodigoUnidadeNegocio = @CodigoUnidade)
                 
              OPEN cCursor
              
              FETCH NEXT FROM cCursor INTO @Ano,
                                           @Editavel
                                           
              WHILE @@FETCH_STATUS = 0
                BEGIN
                      
                       
                       IF @IntervaloMeses = 1 
                                 INSERT INTO @tmp
                                    (_Ano, Periodo, _Mes, _CodigoDado, _NomeDado, Valor, Editavel)
                                 SELECT @Ano,
                                           DescMes + '/' + + CONVERT(VarChar, @Ano),
                                           Mes,
                                           CodigoDado,
										   DescricaoDado,
                                           null,
                                           @Editavel
                                     FROM {0}.{1}.Dado AS d,
                                          @ControlePeriodo
                                    WHERE d.CodigoDado IN (SELECT di.CodigoDado
                                                           FROM {0}.{1}.DadoUnidade AS du INNER JOIN 
                                                                {0}.{1}.DadoIndicador AS di ON (di.CodigoDado = du.CodigoDado)
                                                          WHERE du.CodigoUnidadeNegocio = @CodigoUnidade
                                                            AND di.CodigoIndicador = @codigoIndicador)      
                                   
                         ELSE IF @IntervaloMeses = 2 
                                 INSERT INTO @tmp
                                   (_Ano, Periodo, _Mes, _CodigoDado, _NomeDado, Valor, Editavel)
                                 SELECT @Ano,
                                           DescBim + '/' + + CONVERT(VarChar, @Ano),
                                           Mes,
                                           CodigoDado,
										   DescricaoDado,
                                           null,
                                           @Editavel
                                     FROM @ControlePeriodo,
                                          {0}.{1}.Dado AS d
                                    WHERE Bimestre IS NOT NULL  
                                      AND d.CodigoDado IN (SELECT di.CodigoDado
                                                           FROM {0}.{1}.DadoUnidade AS du INNER JOIN 
                                                                {0}.{1}.DadoIndicador AS di ON (di.CodigoDado = du.CodigoDado)
                                                          WHERE du.CodigoUnidadeNegocio = @CodigoUnidade
                                                            AND di.CodigoIndicador = @codigoIndicador)   
                                      
                        ELSE IF @IntervaloMeses = 3 
                                 INSERT INTO @tmp
                                    (_Ano, Periodo, _Mes, _CodigoDado, _NomeDado, Valor, Editavel)
                                 SELECT @Ano,
                                           DescTri + '/' + + CONVERT(VarChar, @Ano),
                                           Mes,
                                           CodigoDado,
										   DescricaoDado,
                                           null,
                                           @Editavel
                                     FROM @ControlePeriodo,
                                          {0}.{1}.Dado AS d
                                    WHERE Trimestre IS NOT NULL  
                                      AND d.CodigoDado IN (SELECT di.CodigoDado
                                                           FROM {0}.{1}.DadoUnidade AS du INNER JOIN 
                                                                {0}.{1}.DadoIndicador AS di ON (di.CodigoDado = du.CodigoDado)
                                                          WHERE du.CodigoUnidadeNegocio = @CodigoUnidade
                                                            AND di.CodigoIndicador = @codigoIndicador)   
                                      

                        ELSE IF @IntervaloMeses = 4
                                 INSERT INTO @tmp
                                    (_Ano, Periodo, _Mes, _CodigoDado, _NomeDado, Valor, Editavel)
                                 SELECT @Ano,
                                           DescQua + '/' + + CONVERT(VarChar, @Ano),
                                           Mes,
                                           CodigoDado,
										   DescricaoDado,
                                           null,
                                           @Editavel
                                     FROM @ControlePeriodo,
                                          {0}.{1}.Dado AS d
                                    WHERE Quadrimestre IS NOT NULL  
                                      AND d.CodigoDado IN (SELECT di.CodigoDado
                                                           FROM {0}.{1}.DadoUnidade AS du INNER JOIN 
                                                                {0}.{1}.DadoIndicador AS di ON (di.CodigoDado = du.CodigoDado)
                                                          WHERE du.CodigoUnidadeNegocio = @CodigoUnidade
                                                            AND di.CodigoIndicador = @codigoIndicador)   


                            ELSE IF @IntervaloMeses = 6
                                 INSERT INTO @tmp
                                   (_Ano, Periodo, _Mes, _CodigoDado, _NomeDado, Valor, Editavel)
                                 SELECT @Ano,
                                           DescSem + '/' + + CONVERT(VarChar, @Ano),
                                           Mes,
                                           CodigoDado,
										   DescricaoDado,
                                           null,
                                           @Editavel
                                     FROM @ControlePeriodo,
                                          {0}.{1}.Dado As d
                                    WHERE Semestre IS NOT NULL   
                                      AND d.CodigoDado IN (SELECT di.CodigoDado
                                                           FROM {0}.{1}.DadoUnidade AS du INNER JOIN 
                                                                {0}.{1}.DadoIndicador AS di ON (di.CodigoDado = du.CodigoDado)
                                                          WHERE du.CodigoUnidadeNegocio = @CodigoUnidade
                                                            AND di.CodigoIndicador = @codigoIndicador)   
                                
                              ELSE IF @IntervaloMeses = 12
                                 INSERT INTO @tmp
                                   (_Ano, Periodo, _Mes, _CodigoDado, _NomeDado, Valor, Editavel)
                                    SELECT @Ano,
                                           @Ano,
                                           12,
                                           CodigoDado,
										   DescricaoDado,
                                           null,
                                           @Editavel
                                     FROM @ControlePeriodo,
                                          {0}.{1}.Dado As d
                                    WHERE Mes = 12    
                                      AND d.CodigoDado IN (SELECT di.CodigoDado
                                                           FROM {0}.{1}.DadoUnidade AS du INNER JOIN 
                                                                {0}.{1}.DadoIndicador AS di ON (di.CodigoDado = du.CodigoDado)
                                                          WHERE du.CodigoUnidadeNegocio = @CodigoUnidade
                                                            AND di.CodigoIndicador = @codigoIndicador)                   
                                   
                              
                  FETCH NEXT FROM cCursor INTO @Ano,
                                               @Editavel
                
                END
                
              CLOSE cCursor
              DEALLOCATE cCursor
              
              UPDATE @tmp
                 SET Valor = ValorResultado
                FROM {0}.{1}.ResultadoDadoUnidade
               WHERE CodigoUnidadeNegocio = @CodigoUnidade
                 AND _Ano = ResultadoDadoUnidade.Ano
                 AND _Mes = ResultadoDadoUnidade.Mes
                 AND _CodigoDado = ResultadoDadoUnidade.CodigoDado

              {6}
              
                UPDATE @tmp
                 SET Editavel = 'N'
               WHERE Convert(Varchar,_Ano) + Right('00' + Convert(Varchar,_Mes),2) Not Between @AnoMesInicioValidadeMeta AND @AnoMesTerminoValidadeMeta
                 
              SELECT _Ano, Periodo, _Mes, _CodigoDado, _NomeDado, Valor, CASE WHEN Editavel = 'N' THEN 'N' 
																			  WHEN _Ano > YEAR(GETDATE()) THEN 'N'
                                                                              WHEN (_Mes > MONTH(GETDATE()) AND _Ano = YEAR(GETDATE())) THEN 'N'																			  
                                                                              ELSE 'S' END AS Editavel
                                                                              
                FROM @tmp 
               WHERE 1 = 1 
                 {5}
               ORDER BY _Ano, _Mes, Periodo, _NomeDado   
               
            END", bancodb, Ownerdb, codEntidade, codigoIndicador, casasDecimais, where, bloqueiaMeses, codigoUnidade);
        ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getResultadosIndicadorProjeto(int codEntidade, int codigoMeta, string dataInicio, string dataTermino, int casasDecimais, bool usuarioPodeAtualizarMesesBloqueados, string where)
    {
        DataSet ds;

        string bloqueiaMeses = string.Format(@"UPDATE @tmp SET Editavel = 'N' WHERE @paramTempoToleranciaLancamento  IS NOT NULL AND 
							 		          dbo.f_GetUltimoDiaMes(_Mes, _Ano) + @paramTempoToleranciaLancamento < GETDATE()");

        if (usuarioPodeAtualizarMesesBloqueados)
            bloqueiaMeses = "";

        string comandoSQL = string.Format(@"			
        BEGIN
      DECLARE @CodigoMeta Int, --> Parâmetro
              @CodigoEntidade Int, --> Parâmetro
			  @DataInicio DateTime, --> Data de início do parâmetro
			  @DataTermino DateTime,  --> Data de término do parâmetro
			  @AnoInicio SmallInt,
			  @AnoTermino SmallInt,
			  @paramTempoToleranciaLancamento  SmallInt
			  
      /* Parâmetros a serem passados para consulta */

      SET @CodigoEntidade = {2}
      SET @codigoMeta = {3}
      SET @DataInicio = {4}
      SET @DataTermino = {5}

      /* Retorno final da consulta */  
      DECLARE @tmp TABLE 
        (_Data SmallDateTime,
         _Ano SmallInt,
         Periodo Varchar(50),
         _Mes SmallInt,
         _CodigoMeta Int,
         _NomeIndicador Varchar(255),
         Valor Decimal(18,{6}),
         Editavel Char(1))
         
      /* Tabela auxiliar criada para simplificar o resultado final */
      DECLARE @ControlePeriodo TABLE
        (Data SmallDateTime,
         Mes SmallInt,
         Bimestre Tinyint,
         Trimestre Tinyint,
         Quadrimestre Tinyint,
         Semestre  Tinyint,
         DescMes Varchar(20),
         DescBim Varchar(20),
         DescTri Varchar(20),
         DescQua Varchar(20),
         DescSem Varchar(20),
         DescDia Varchar(20),
         DescAno Varchar(20))
              
        
      /* Obtém o valor do parâmetro que define a tolerância, em dias, para a edição dos resultados das metas de período anteriores */
      SET @paramTempoToleranciaLancamento   = null;
      SELECT @paramTempoToleranciaLancamento = CAST( ISNULL(Valor,'0') as SmallInt )
      FROM {0}.{1}.ParametroConfiguracaoSistema
      WHERE CodigoEntidade = @CodigoEntidade
      AND Parametro = 'diasToleranciaLancamentoExecMetas';
     

       /* Variáveis  */     
      DECLARE @Ano SmallInt,
              @TipoDetalhe Char(1),
              @CodigoPeriodicidade SmallInt,
              @NomeIndicador Varchar(255),
              @Editavel Char(1),
              @IntervaloDias SmallInt,
              @IntervaloMeses SmallInt
      
       SELECT @CodigoPeriodicidade = iop.CodigoPeriodicidade,
              @NomeIndicador = NomeIndicador,
              @IntervaloDias = IntervaloDias,
              @IntervaloMeses = IntervaloMeses
         FROM {0}.{1}.MetaOperacional iop INNER JOIN
              {0}.{1}.IndicadorOperacional AS i ON i.CodigoIndicador = iop.CodigoIndicador INNER JOIN
              {0}.{1}.TipoPeriodicidade AS t ON (t.CodigoPeriodicidade = iop.CodigoPeriodicidade)
        WHERE iop.CodigoMetaOperacional = @codigoMeta
        
      
      IF @IntervaloDias < 30 --> A periodicidade é menor que mensal! 
         BEGIN
           DECLARE @DataControle SmallDateTime
           
           SET @DataControle = @DataInicio
           WHILE @DataControle <= @DataTermino
             BEGIN
                INSERT INTO @ControlePeriodo
				  (Data, DescDia)
				VALUES (Convert(Varchar,@DataControle,112),Convert(Varchar,@DataControle,112))
               SET @DataControle = DATEADD(DD,1,@DataControle)
             END
         END  
      ELSE
         BEGIN
		  /* Atualiza a tabela auxiliar */
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes)
		  VALUES (1,'Jan')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes, Bimestre,DescBim)
		  VALUES(2,'Fev',1,'1º Bim')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes, Trimestre,DescTri)
		  VALUES(3,'Mar',1,'1º Tri')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes,Bimestre,DescBim, Quadrimestre, DescQua)
		  VALUES(4,'Abr',2,'2º Bim', 1, '1º Quad')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes)
		  VALUES(5,'Mai')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes, Bimestre, DescBim, Trimestre, DescTri, Semestre, DescSem)
		  VALUES(6,'Jun',3,'3º Bim',2,'2º Tri',1,'1º Sem')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes)
		  VALUES(7,'Jul')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes, Bimestre,DescBim, Quadrimestre, DescQua)
		  VALUES(8,'Ago',4,'4º Bim', 2, '2º Quad')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes, Trimestre,DescTri)
		  VALUES(9,'Set',3,'3º Tri')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes,Bimestre,DescBim)
		  VALUES(10,'Out',5,'5º Bim')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes)
		  VALUES(11,'Nov')
		  INSERT INTO @ControlePeriodo
		  (Mes, DescMes, Bimestre, DescBim, Trimestre, DescTri, Semestre, DescSem, DescAno, Quadrimestre, DescQua)
		  VALUES(12,'Dez',6,'6º Bim',4,'4º Tri',2,'2º Sem','Ano', 3, '3º Quad')
		END  
                
         /* Alterado por Ericsson em 17/04/2010. É necessário obedecer o mesmo período estratégico definido
	                  pela entidade que criou o indicador. */
	               DECLARE cCursor CURSOR FOR
	                SELECT DISTINCT 
	                       pe.Ano,
	                       pe.IndicaMetaEditavel
	                  FROM {0}.{1}.PeriodoAnalisePortfolio AS pe 
	                 WHERE pe.IndicaAnoAtivo = 'S'
					   AND pe.CodigoEntidade = @CodigoEntidade
	                 
	              OPEN cCursor
	              
	              FETCH NEXT FROM cCursor INTO @Ano,
	                                           @Editavel
	                                           
	              WHILE @@FETCH_STATUS = 0
                BEGIN
                      
		   IF @IntervaloDias = 30 --  Periodicidade Mensal 
			  INSERT INTO @tmp
			  (_Ano, Periodo, _Mes, _CodigoMeta, _NomeIndicador, Valor, Editavel)
			  SELECT @Ano,
					 DescMes + '/' + + CONVERT(VarChar, @Ano),
					 Mes,
					 @codigoMeta,
					 @NomeIndicador,
					 null,
					 @Editavel
				FROM @ControlePeriodo
                           
            ELSE IF @IntervaloDias = 60 
                         INSERT INTO @tmp
                           (_Ano, Periodo, _Mes, _CodigoMeta, _NomeIndicador, Valor, Editavel)
                         SELECT @Ano,
                                   DescBim + '/' + + CONVERT(VarChar, @Ano),
                                   Mes,
                                   @codigoMeta,
								   @NomeIndicador,
                                   null,
                                   @Editavel
                             FROM @ControlePeriodo
                            WHERE Bimestre IS NOT NULL  
                              
            ELSE IF @IntervaloDias = 90 --> Trimestral 
                         INSERT INTO @tmp
                            (_Ano, Periodo, _Mes, _CodigoMeta, _NomeIndicador, Valor, Editavel)
                         SELECT @Ano,
                                   DescTri + '/' + + CONVERT(VarChar, @Ano),
                                   Mes,
                                   @codigoMeta,
								   @NomeIndicador,
                                   null,
                                   @Editavel
                             FROM @ControlePeriodo
                            WHERE Trimestre IS NOT NULL  

            ELSE IF @IntervaloDias = 120 --> Quadrimestral
                         INSERT INTO @tmp
                            (_Ano, Periodo, _Mes, _CodigoMeta, _NomeIndicador, Valor, Editavel)
                         SELECT @Ano,
                                   DescQua + '/' + + CONVERT(VarChar, @Ano),
                                   Mes,
                                   @codigoMeta,
								   @NomeIndicador,
                                   null,
                                   @Editavel
                             FROM @ControlePeriodo
                            WHERE Quadrimestre IS NOT NULL  

             ELSE IF @IntervaloDias = 180 --> Semestral
                         INSERT INTO @tmp
                           (_Ano, Periodo, _Mes, _CodigoMeta, _NomeIndicador, Valor, Editavel)
                         SELECT @Ano,
                                   DescSem + '/' + + CONVERT(VarChar, @Ano),
                                   Mes,
                                   @CodigoMeta,
								   @NomeIndicador,
                                   null,
                                   @Editavel
                             FROM @ControlePeriodo
                            WHERE Semestre IS NOT NULL   
                        
              ELSE IF @IntervaloDias = 360 --> Anual
                         INSERT INTO @tmp
                           (_Ano, Periodo, _Mes, _CodigoMeta, _NomeIndicador, Valor, Editavel)
                            SELECT @Ano,
                                   @Ano,
                                   12,
                                   @CodigoMeta,
								   @NomeIndicador,
                                   null,
                                   @Editavel
                             FROM @ControlePeriodo   
                            WHERE DescAno IS NOT NULL                       
               ELSE IF @IntervaloDias = 1
                               INSERT INTO @tmp
								   (_Ano, Periodo, _Mes, _CodigoMeta, _NomeIndicador, Valor, Editavel, _Data)
									SELECT YEAR(Data),
										   Convert(Varchar,Data,103),
										   MONTH(Data),
										   @CodigoMeta,
										   @NomeIndicador,
										   null,
										   @Editavel,
										   Convert(Varchar,Data,112)
									 FROM @ControlePeriodo 
									 FETCH NEXT FROM cCursor INTO @Ano,
									                                                @Editavel
									                 
									                 END
									                 
									               CLOSE cCursor
              DEALLOCATE cCursor
       
     {8}
               
      IF @IntervaloDias >= 30 
         BEGIN       
			  UPDATE @tmp
				 SET Valor = ValorResultado
				FROM {0}.{1}.ResultadoMetaOperacional
			   WHERE _Ano = Year(ResultadoMetaOperacional.DataReferenciaPeriodo)
				 AND _Mes = Month(ResultadoMetaOperacional.DataReferenciaPeriodo)
				 AND _CodigoMeta = ResultadoMetaOperacional.CodigoMetaOperacional
			
			     
			  SELECT CONVERT(DateTime, '01/' + Convert(Varchar,_Mes) + '/' +  Convert(Varchar,_Ano)) AS Data,
			         _Ano,
			         Periodo,
			         _Mes,
			         _CodigoMeta,
			         _NomeIndicador,
			         Valor,
			         CASE WHEN Editavel = 'N' THEN 'N' 
										WHEN _Ano > YEAR(GETDATE())  THEN 'N'  
							 		    WHEN (_Mes-(@IntervaloMeses-1) > MONTH(GETDATE()) AND _Ano = YEAR(GETDATE()) ) THEN 'N'																			  
                      ELSE 'S' END AS Editavel 
			   FROM @tmp 
			   WHERE 1 = 1 	 
		             {7}        
			   ORDER BY _Ano, _Mes, Periodo, _NomeIndicador
       	 
		 END		 
	  ELSE
	     BEGIN
	      UPDATE @tmp
			 SET Valor = ValorResultado
			FROM {0}.{1}.ResultadoMetaOperacional
		   WHERE _Data = ResultadoMetaOperacional.DataReferenciaPeriodo
			 AND _CodigoMeta = ResultadoMetaOperacional.CodigoMetaOperacional
			 
				      
		  SELECT _Data AS Data,
			     _Ano,
			     Periodo,
			     _Mes,
			     _CodigoMeta,
			     _NomeIndicador,
			     Valor,
			     CASE WHEN Editavel = 'N' THEN 'N' 
										WHEN _Ano > YEAR(GETDATE())  THEN 'N'
							 		    WHEN ( CONVERT(datetime,  '01/' + CAST(_Mes as varchar) + '/' + CAST(_Ano as varchar), 103)-(@IntervaloDias-1) > GETDATE() ) THEN 'N'																			  
                      ELSE 'S' END AS Editavel  
			FROM @tmp 
		   WHERE 1 = 1 	   	 
		     {7}             
		   ORDER BY _Data, Periodo, _NomeIndicador  
       
	     END		 	 
    
    END", bancodb, Ownerdb, codEntidade, codigoMeta, dataInicio, dataTermino, casasDecimais, where, bloqueiaMeses);
        ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getListaResultadosIndicador(int codEntidade, int codigoIndicador, string where)
    {
        DataSet ds;
        string comandoSQL = string.Format(@"			
            BEGIN
              DECLARE @CodigoEntidade Int, --> Parâmetro
                      @codigoIndicador Int --> Parâmetro
              
              /* Parâmetros a serem passados para consulta */

              SET @CodigoEntidade = {2}
              SET @codigoIndicador = {3}              
             
             SELECT CodigoDado,
					DescricaoDado                                           
               FROM {0}.{1}.Dado AS d
              WHERE d.CodigoDado IN (SELECT di.CodigoDado
                                       FROM {0}.{1}.DadoUnidade AS du INNER JOIN 
                                            {0}.{1}.DadoIndicador AS di ON (di.CodigoDado = du.CodigoDado)
                                      WHERE du.CodigoUnidadeNegocio = @CodigoEntidade
                                        AND di.CodigoIndicador = @codigoIndicador)      
              ORDER BY DescricaoDado                               
                         
              
               
            END", bancodb, Ownerdb, codEntidade, codigoIndicador, where);
        ds = getDataSet(comandoSQL);
        return ds;
    }
    public bool atualizaResultadosIndicador(int codEntidade, int codigoDado, int mes, int ano, string valor, int codigoUsuario)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            comandoSQL = string.Format(
            @"BEGIN
	                DECLARE @CodigoDado int,
			                @CodigoUnidade int,
			                @Mes int,
			                @Ano int,
			                @Valor Decimal(18,4),
			                @CodigoUsuario int,
			                @PossuiRegistro int
                	
	                SET @CodigoDado = {2}
	                SET @CodigoUnidade = {3}
	                SET @Mes = {4}
	                Set @Ano = {5}
	                Set @Valor = {6}
	                Set @CodigoUsuario = {7} 
                		
	                SELECT @PossuiRegistro = COUNT(1) 
	                  FROM {0}.{1}.ResultadoDadoUnidade
	                 WHERE CodigoDado = @CodigoDado
	                   AND Mes = @Mes
	                   AND Ano = @Ano
                       AND CodigoUnidadeNegocio = @CodigoUnidade
                	
	                IF @PossuiRegistro = 0
		                INSERT INTO {0}.{1}.ResultadoDadoUnidade(CodigoDado, CodigoUnidadeNegocio, CodigoUsuarioInclusao, DataInclusao, Ano, Mes, ValorResultado) VALUES
										                (@CodigoDado, @CodigoUnidade, @CodigoUsuario, GETDATE(), @Ano, @Mes, @Valor)
	                ELSE
		                UPDATE {0}.{1}.ResultadoDadoUnidade SET ValorResultado = @Valor,
										                        CodigoUsuarioUltimaAlteracao = @CodigoUsuario,
										                        DataUltimaAlteracao = GETDATE()
		                 WHERE CodigoDado = @CodigoDado
		                   AND Mes = @Mes
		                   AND Ano = @Ano
                           AND CodigoUnidadeNegocio = @CodigoUnidade
                								
                END", bancodb, Ownerdb, codigoDado, codEntidade, mes, ano, valor.Replace(',', '.'), codigoUsuario);

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    #endregion
}
}