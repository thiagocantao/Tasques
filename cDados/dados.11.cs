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
    #region Visão Cliente

    public DataSet getProjetosVisaoCorporativaCliente(int codigoEntidade, int codigoUsuarioLogado, int codigoCarteira, string where)
    {
        string comandoSQL = string.Format(@"             
                         
            BEGIN
                /* Obtém a relação de todos os indicadores de um objetivo estratégico, mostrando as cores
                   de desempenho de cada um deles.
                */
                DECLARE @Cor Varchar(8),
                        @CodigoProjeto Int,
                        @CodigoUsuario Int,
                        @CodigoUnidade Int,
                        @NomeProjeto Varchar(255),
                        @GerenteProjeto VarChar(60),
                        @CodigoGerenteProjeto int,
                        @DataUltimaMensagem DateTime,
                        @GerenteUnidade VarChar(60),
                        @CodigoGerenteUnidade int,
						@CodigoTipoAssociacao Int,
                        @NomeUnidadeNegocio VarChar(10),
						@Concluido Decimal(18,4),
						@CodigoTipoAssociacaoEntidade Int,
                        @CodigoEntidade Int,
                        @Previsto Decimal(18,4),
                        @CodigoCarteira Int,
                        @DataUltimaAlteracao DateTime

                DECLARE @tmp 
                TABLE (CodigoProjeto Int,
                       NomeProjeto Varchar(255),
                       Desempenho Varchar(8),
                       GerenteProjeto VarChar(60),
                       CodigoGerenteProjeto int,
                       CodigoUltimaMensagem int,
                       GerenteUnidade VarChar(60),
                       CodigoGerenteUnidade int,
                       NomeUnidadeNegocio VarChar(10),
					   Concluido Decimal(18,4),
                       Previsto Decimal(18,4),
                       CodigoUltimaMensagemEntidade int,
                       CodigoUnidade Int,
                       DataUltimaAlteracao DateTime)

                SET @CodigoEntidade = {2}
                SET @CodigoUsuario = {3}
                SET @CodigoCarteira = {4}

                SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao
                  FROM {0}.{1}.TipoAssociacao 
                 WHERE IniciaisTipoAssociacao = 'PR'

                SELECT @CodigoTipoAssociacaoEntidade = CodigoTipoAssociacao
                  FROM {0}.{1}.TipoAssociacao 
                 WHERE IniciaisTipoAssociacao = 'EN'
               
                DECLARE cCursorProjeto 
                CURSOR FOR SELECT DISTINCT
                                  p.CodigoProjeto,
                                  p.NomeProjeto,
                                  rp.CorGeral,
                                  gp.NomeUsuario,
                                  gp.CodigoUsuario,
                                  gu.NomeUsuario,
                                  gu.CodigoUsuario,
                                  un.SiglaUnidadeNegocio,
                                  rp.PercentualRealizacao,
                                  rp.PercentualPrevistoRealizacao,
                                  un.CodigoUnidadeNegocio,
                                  p.DataUltimaAlteracao
                             FROM {0}.{1}.Projeto p INNER JOIN 
							      {0}.{1}.f_GetProjetosUsuario(@CodigoUsuario, @CodigoEntidade, @CodigoCarteira) f ON f.CodigoProjeto = p.CodigoProjeto INNER JOIN
								  {0}.{1}.ResumoProjeto rp ON rp.CodigoProjeto = p.CodigoProjeto INNER JOIN
								  {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio LEFT JOIN 
								  {0}.{1}.Usuario gp ON gp.CodigoUsuario = p.CodigoGerenteProjeto LEFT JOIN
								  {0}.{1}.Usuario gu ON gu.CodigoUsuario = un.CodigoUsuarioGerente
                            WHERE p.DataExclusao IS NULL 
                              AND p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ')
                              AND p.CodigoStatusProjeto = 3
                              
                              

                OPEN cCursorProjeto

                FETCH NEXT FROM cCursorProjeto INTO @CodigoProjeto,
                                                      @NomeProjeto,
                                                      @Cor,
                                                      @GerenteProjeto,
                                                      @CodigoGerenteProjeto,
                                                      @GerenteUnidade,
                                                      @CodigoGerenteUnidade,
                                                      @NomeUnidadeNegocio,
                                                      @Concluido,
                                                      @Previsto,
                                                      @CodigoUnidade,
                                                      @DataUltimaAlteracao

                WHILE @@FETCH_STATUS = 0
                  BEGIN
                   
                   
                    INSERT INTO @tmp
                      (CodigoProjeto,
                       NomeProjeto,
                       Desempenho,
                       GerenteProjeto,
                       CodigoGerenteProjeto,
                       GerenteUnidade,
                       CodigoGerenteUnidade,
                       NomeUnidadeNegocio,
                       Concluido,
                       Previsto,
                       CodigoUnidade,
                       DataUltimaAlteracao)
                    VALUES
                      (@CodigoProjeto,
                       @NomeProjeto,
                       @Cor,
                       @GerenteProjeto,
                       @CodigoGerenteProjeto,
                       @GerenteUnidade,
                       @CodigoGerenteUnidade,
                       @NomeUnidadeNegocio,
                       @Concluido,
                       @Previsto,
                       @CodigoUnidade,
                       @DataUltimaAlteracao)

                    
                    FETCH NEXT FROM cCursorProjeto INTO @CodigoProjeto,
                                                      @NomeProjeto,
                                                      @Cor,
                                                      @GerenteProjeto,
                                                      @CodigoGerenteProjeto,
                                                      @GerenteUnidade,
                                                      @CodigoGerenteUnidade,
                                                      @NomeUnidadeNegocio,
                                                      @Concluido,
                                                      @Previsto,
                                                      @CodigoUnidade,
                                                      @DataUltimaAlteracao
                  END

                CLOSE cCursorProjeto

                DEALLOCATE cCursorProjeto
                                
                UPDATE @tmp 
                       SET CodigoUltimaMensagem = (SELECT Max(CodigoMensagem)
											   FROM {0}.{1}.Mensagem 
											  WHERE CodigoTipoAssociacao = @CodigoTipoAssociacao
											    AND CodigoObjetoAssociado = CodigoProjeto
											    AND DataResposta IS NULL
											    AND CodigoEntidade = @CodigoEntidade)

                    UPDATE @tmp 
                       SET CodigoUltimaMensagemEntidade = (SELECT Max(CodigoMensagem)
											   FROM {0}.{1}.Mensagem 
											  WHERE CodigoTipoAssociacao = @CodigoTipoAssociacaoEntidade
											    AND CodigoObjetoAssociado = CodigoUnidade
											    AND DataResposta IS NULL
											    AND CodigoEntidade = @CodigoEntidade)

                SELECT *, 
					   ISNULL((SELECT CASE WHEN m.DataLimiteResposta IS NULL THEN 'SM' 
							WHEN m.DataLimiteResposta + 1 > GETDATE() THEN 'MN'
							ELSE 'MA' END TipoMensagem 
					     FROM {0}.{1}.Mensagem m 
						WHERE m.CodigoMensagem = tmp.CodigoUltimaMensagem), 'SM') AS TipoMensagem, 
					   ISNULL((SELECT CASE WHEN m.DataLimiteResposta IS NULL THEN 'SM' 
							WHEN m.DataLimiteResposta + 1 > GETDATE() THEN 'MN'
							ELSE 'MA' END TipoMensagem 
					     FROM {0}.{1}.Mensagem m 
						WHERE m.CodigoMensagem = tmp.CodigoUltimaMensagemEntidade), 'SM') AS TipoMensagemEntidade,
						{0}.{1}.f_GetCorFisico(tmp.CodigoProjeto) AS Fisico,
						{0}.{1}.f_GetCorFinanceiro(tmp.CodigoProjeto) AS Financeiro,
						{0}.{1}.f_GetCorRisco(tmp.CodigoProjeto) AS Risco,
						{0}.{1}.f_GetCorQuestao(tmp.CodigoProjeto) AS Questao
                  FROM @tmp tmp
                 WHERE 1 = 1
                    {5}
                 ORDER BY NomeProjeto
            END
                                      
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuarioLogado, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getMarcosTarefasCliente(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN 
	                DECLARE @CodigoProjeto int       
        	
	                SET @CodigoProjeto = {2}
                    
                    SELECT CodigoTarefa, NomeTarefa, Termino, TerminoReal, TerminoPrevisto, DesvioPrazo AS Desvio, StatusTarefa AS Status
                      FROM {0}.{1}.f_GetMarcosCronogramaProjeto(@CodigoProjeto) f
                      WHERE 1 = 1 
                        {3}
                      ORDER BY Inicio 
                    END
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }


    public DataSet getMarcosTarefasClienteDescricaoTipoTarefa(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN 
	                DECLARE @CodigoProjeto int       
        	
	                SET @CodigoProjeto = {2}
                    
                    SELECT CodigoTarefa, NomeTarefa, Termino, TerminoReal, TerminoPrevisto, DesvioPrazo AS Desvio, StatusTarefa AS Status, DescricaoTipoTarefaCronograma
                      FROM {0}.{1}.f_GetMarcosCronogramaProjeto(@CodigoProjeto) f
                      WHERE 1 = 1 
                        {3}
                      ORDER BY Inicio 
                    END
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getSPIProjeto(int codigoProjeto)
    {
        string comandoSQL = string.Format(
          @"BEGIN 
	            DECLARE @CodigoProjeto int       
	            SET @CodigoProjeto = {2}
                    
                SELECT 
				        CorGeral = dbo.f_GetCorGeral(@CodigoProjeto)
			        , rp.[PercentualPrevistoRealizacao]
			        , rp.[PercentualRealizacao]
			        , SPI = CAST( 
					        CASE WHEN rp.[PercentualPrevistoRealizacao]>0 
						        THEN rp.[PercentualRealizacao]/rp.[PercentualPrevistoRealizacao]  
						        ELSE NULL
					        END AS Decimal(20,2) )	
                  FROM {0}.{1}.ResumoProjeto rp 
                  WHERE rp.[CodigoProjeto] = @CodigoProjeto;
            END
               ", bancodb, Ownerdb, codigoProjeto);
        return getDataSet(comandoSQL);
    }

    public DataSet getEntregasTarefasCliente(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN 
	                DECLARE @CodigoProjeto int       
        	
	                SET @CodigoProjeto = {2}
                    
                    SELECT CodigoTarefa, NomeTarefa, Termino, TerminoReal, TerminoPrevisto, DesvioPrazo AS Desvio, StatusTarefa AS Status
                      FROM {0}.{1}.f_GetCronogramaProjeto(@CodigoProjeto, GetDate(), -1) f
                      WHERE IniciaisTipoTarefa = 'ENTREGA'
                        {3}
                      ORDER BY Inicio 
                    END
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoDesempenhoFisicoVisaoCliente(DataTable dt, DataTable dtIndicadores, int fonte, string urlImage, string colPercPrevisto, string colPercReal, string titulo)
    {
        StringBuilder xml = new StringBuilder();

        float percentualFisico = 0;
        float percentualPrevisto = 0;
        string real = "";
        string previsto = "";

        try
        {

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
        xml.Append("<chart placeTicksInside=\"1\" valuePadding=\"0\" pointerRadius=\"3\" imageSave=\"1\" imageSaveURL=\"" + urlImage + "\" upperLimit=\"100\" showValue=\"1\" decimals=\"2\" decimalSeparator=\".\" " +
                            "inDecimalSeparator=\",\" baseFontSize=\"" + fonte + "\"  " + exportar +
                            "bgColor=\"FFFFFF\" showBorder=\"0\" chartTopMargin=\"5\" tickMarkDistance=\"0\" " +
                            "chartBottomMargin=\"2\" lowerLimit=\"0\" numberSuffix=\"%\" " +
                            "gaugeFillRatio=\"\" >");
        xml.Append(" <colorRange>");

        if (percentualFisico == 1)
        {
            try
            {
                //define a escala do gráfico
                xml.Append(" <color minValue=\"0\" maxValue=\"" + (int)(float.Parse(dtIndicadores.Rows[0][1].ToString())) + "\" name=\" \" code=\"" + corCritico + "\" />");
                xml.Append(" <color minValue=\"" + (int)(float.Parse(dtIndicadores.Rows[0][1].ToString())) +
                    "\" maxValue=\"" + (int)(float.Parse(dtIndicadores.Rows[1][1].ToString())) + "\" name=\" \" code=\"" + corAtencao + "\" />");
                xml.Append(" <color minValue=\"" + (int)(float.Parse(dtIndicadores.Rows[1][1].ToString())) + "\" maxValue=\"100\" name=\" \" code=\"" + corSatisfatorio + "\" />");
            }
            catch { }
        }
        else
        {
            if ((percentualPrevisto * 100) <= 1 && valorPercentualPrevisto > 0)
            {
                //define a escala do gráfico
                xml.Append(" <color minValue=\"0\" maxValue=\"100\" name=\" \" code=\"" + corSatisfatorio + "\" />");
            }
            else
            {
                if ((percentualPrevisto * 100) <= 2 && valorPercentualPrevisto > 0)
                {
                    //define a escala do gráfico
                    xml.Append(" <color minValue=\"0\" maxValue=\"1\" name=\" \" code=\"" + corCritico + "\" />");
                    xml.Append(" <color minValue=\"1\" maxValue=\"100\" name=\" \" code=\"" + corSatisfatorio + "\" />");
                }
                else
                {
                    if ((percentualPrevisto * 100) == 100 && valorPercentualPrevisto > 0)
                    {
                        //define a escala do gráfico
                        xml.Append(" <color minValue=\"0\" maxValue=\"99\" name=\" \" code=\"" + corCritico + "\" />");
                        xml.Append(" <color minValue=\"99\" maxValue=\"100\" name=\" \" code=\"" + corSatisfatorio + "\" />");
                    }
                    else
                    {
                        try
                        {
                            //define a escala do gráfico
                            xml.Append(" <color minValue=\"0\" maxValue=\"" + (int)(percentualPrevisto * float.Parse(dtIndicadores.Rows[0][1].ToString())) + "\" name=\" \" code=\"" + corCritico + "\" />");
                            xml.Append(" <color minValue=\"" + (int)(percentualPrevisto * float.Parse(dtIndicadores.Rows[0][1].ToString())) +
                                "\" maxValue=\"" + (int)(percentualPrevisto * float.Parse(dtIndicadores.Rows[1][1].ToString())) + "\" name=\" \" code=\"" + corAtencao + "\" />");
                            xml.Append(" <color minValue=\"" + (int)(percentualPrevisto * float.Parse(dtIndicadores.Rows[1][1].ToString())) + "\" maxValue=\"100\" name=\" \" code=\"" + corSatisfatorio + "\" />");
                        }
                        catch { }
                    }
                }
            }
        }

        string percentual = "" + percentualPrevisto * 100;

        xml.Append(" </colorRange>");
        xml.Append(" <value>" + (percentualFisico) + "</value>");

        if (titulo != "")
        {
            xml.Append(string.Format(@"<annotations origW=""890"" origH=""120"" constrainedScale='0'>
                                              <annotationGroup id=""Grp1"" autoScale=""1"">
                                                 <annotation type=""text"" font=""Verdana"" fontSize=""9"" fontColor=""000000"" align=""center"" x=""435"" y=""10"" label=""{0}"" />
                                              </annotationGroup>
                                           </annotations>
                                        ", titulo));
        }
        xml.Append("</chart>");

        return xml.ToString();
    }


    #endregion

    #region PERMISSOES

    /// <summary>
    /// Obtem uma tabela com as permissões indicadas no parâmetro 'permissoesObjetoVinculado'.
    /// Ista tabela posee o nome das columnas iguais a nome das permissoes.
    /// </summary>
    /// <param name="idUsuarioLogado">Usuario que logo no sistema.</param>
    /// <param name="idUnidadeLogada">Unidade/Entidade em a qual o Usuario ta logado.</param>
    /// <param name="idCodigoObjetoVinculado">Objeto alvo do qual vai verificar a permissões.</param>
    /// <param name="idObjetoPaiDoObjetoVinculado">Objeto pai, o do cual depende o objeto alvo.</param>
    /// <param name="iniciaisObjetoVinculado">iniciais que indica o tipo de associasão do objeto alvo.</param>
    /// <param name="permissoesObjetoVinculado">a lista de permissões a obter.</param>
    /// <returns></returns>
    public DataSet getPermissoesDoObjetivoPelaTela(int idUsuarioLogado, int idUnidadeLogada, int idCodigoObjetoVinculado, int idObjetoPaiDoObjetoVinculado, string iniciaisObjetoVinculado, params string[] permissoesObjetoVinculado)
    {
        DataSet ds;
        string declare = @" DECLARE @CodigoUsuario      INT
                                  , @CodigoEntidade     INT
                                  , @CodigoObjetivo     INT
                                  , @CodigoObjetoPai    INT
                                  , @Iniciais           VARCHAR(2)
                                  ";
        string setDeclareIniciais = string.Format(@"
                                  SET @CodigoUsuario  = {0}
                                  SET @CodigoEntidade = {1}
                                  SET @CodigoObjetivo = {2}
                                  SET @CodigoObjetoPai= {3}
                                  SET @Iniciais       = '{4}'
                                ", idUsuarioLogado, idUnidadeLogada, idCodigoObjetoVinculado, idObjetoPaiDoObjetoVinculado, iniciaisObjetoVinculado);
        string selectPermissoes = "";
        string selectTabela = "SELECT 'OK' AS ListaPermissoes ";
        string comandoSQL = "";

        foreach (string permissoe in permissoesObjetoVinculado)
        {
            declare += ", @" + permissoe + @" INT 
                        ";
            selectPermissoes += string.Format(@" SELECT @" + permissoe + @" = {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, @CodigoObjetivo, null, @Iniciais, @CodigoObjetoPai, null, '{2}');
                                               ", getDbName(), getDbOwner(), permissoe);
            selectTabela += ", @" + permissoe + " AS " + permissoe; //@MenuStatusProjeto AS MenuStatusProjeto,
        }

        comandoSQL = declare + setDeclareIniciais + selectPermissoes + selectTabela;
        ds = getDataSet(comandoSQL);

        return ds;
    }

    #endregion

    #region Status Report
    public void publicaStatusReport(int codigoStatusReport, byte[] arquivo)
    {

        SqlCommand Command = new SqlCommand();
        SqlConnection Connection = new SqlConnection(strConn);

        Command.Connection = Connection;
        Command.CommandType = CommandType.Text;
        Command.CommandText = string.Format(@"
UPDATE {0}.{1}.[StatusReport]
   SET [DataPublicacao] = ISNULL([DataPublicacao], GetDate())
      ,[ConteudoStatusReport] = @ConteudoStatusReport
 WHERE CodigoStatusReport = @CodigoStatusReport", bancodb, Ownerdb);
        Command.Parameters.Add("@ConteudoStatusReport", SqlDbType.Image);
        Command.Parameters.Add("@CodigoStatusReport", SqlDbType.Int);

        Command.Parameters["@ConteudoStatusReport"].Value = ((byte[])(arquivo));
        Command.Parameters["@CodigoStatusReport"].Value = ((int)(codigoStatusReport));

        Command.CommandTimeout = CDIS.ClasseDados.TimeOutSqlCommand;
        ConnectionState previousConnectionState = Command.Connection.State;
        if (previousConnectionState != ConnectionState.Open)
        {
            Command.Connection.Open();
        }
        try
        {
            Command.ExecuteNonQuery();
        }
        finally
        {
            Command.Connection.Close();
        }

        /* Quando publicar um Status Report  ( ou boletim ), publica automaticamente todos os anteriores que não estavam publicados */
        int nRegAfetados = 0;
        execSQL(string.Format("exec [dbo].[p_PublicarStatusReportAnterior] @in_codigoStatusReport = {0}", codigoStatusReport), ref nRegAfetados);

    }

    public void registraPdfHistoricoMedicaoApontamento(long codigoMedicaoApontamento, byte[] arquivo)
    {

        SqlCommand Command = new SqlCommand();
        SqlConnection Connection = new SqlConnection(strConn);

        Command.Connection = Connection;
        Command.CommandType = CommandType.Text;
        Command.CommandText = string.Format(@"
UPDATE {0}.{1}.[BoletimApontamentoAtribuicao]
   SET [ConteudoPDFBoletim] = @ConteudoPdfBoletim 
 WHERE CodigoBoletim = @CodigoMedicaoApontamento", bancodb, Ownerdb);
        Command.Parameters.Add("@ConteudoPdfBoletim", SqlDbType.Image);
        Command.Parameters.Add("@CodigoMedicaoApontamento", SqlDbType.Int);

        Command.Parameters["@ConteudoPdfBoletim"].Value = ((byte[])(arquivo));
        Command.Parameters["@CodigoMedicaoApontamento"].Value = ((int)(codigoMedicaoApontamento));

        Command.CommandTimeout = CDIS.ClasseDados.TimeOutSqlCommand;
        ConnectionState previousConnectionState = Command.Connection.State;
        if (previousConnectionState != ConnectionState.Open)
        {
            Command.Connection.Open();
        }
        try
        {
            Command.ExecuteNonQuery();
        }
        finally
        {
            Command.Connection.Close();
        }
    }

    public DataSet enviaStatusReportAosDestinatarios(int codigoEntidade, int codigoStatusReport, int codigoUsuarioEnvioStatusReport)
    {
        #region Comando SQL

        string comandoSql = string.Format(@"
DECLARE @Erro INT,
        @CodigoEntidade INT,
		@CodigoStatusReport INT,
		@CodigoUsuarioResponsavel INT,
		@CodigoObjeto INT,
        @CodigoTipoAssociacaoProjeto INT,
        @CodigoTipoAssociacaoObjeto INT,
		@CodigoModeloStatusReport INT

    SET @Erro = 0
    SET @CodigoEntidade = {2}
    SET @CodigoStatusReport = {3}
    SET @CodigoUsuarioResponsavel = {4}
    SET @CodigoTipoAssociacaoProjeto = {0}.{1}.f_GetCodigoTipoAssociacao('PR')
    

BEGIN TRANSACTION
				
 SELECT @CodigoModeloStatusReport = CodigoModeloStatusReport,
        @CodigoTipoAssociacaoObjeto = CodigoTipoAssociacaoObjeto,
		@CodigoObjeto = CodigoObjeto
   FROM {0}.{1}.StatusReport
  WHERE CodigoStatusReport = @CodigoStatusReport

 UPDATE {0}.{1}.StatusReport
	SET DataEnvioDestinatarios = GETDATE()
  WHERE CodigoStatusReport = @CodigoStatusReport
	
		SET @Erro = @Erro + @@Error

 INSERT INTO {0}.{1}.DestinatarioStatusReport ([CodigoStatusReport], [CodigoUsuarioDestinatario], [DataEnvioStatusReport], [CodigoUsuarioEnvioStatusReport])
     SELECT DISTINCT
		    @CodigoStatusReport,
		    dmsr.CodigoUsuarioDestinatario,
		    GETDATE(),
		    @CodigoUsuarioResponsavel
       FROM {0}.{1}.DestinatarioModeloStatusReport dmsr INNER JOIN
		    {0}.{1}.Usuario u ON u.CodigoUsuario = dmsr.CodigoUsuarioDestinatario INNER JOIN
		    {0}.{1}.UsuarioUnidadeNegocio uun ON uun.CodigoUsuario = u.CodigoUsuario
      WHERE dmsr.CodigoModeloStatusReport = @CodigoModeloStatusReport
        AND dmsr.CodigoTipoAssociacaoObjeto = @CodigoTipoAssociacaoObjeto
	    AND dmsr.CodigoObjeto = @CodigoObjeto
	    AND u.DataExclusao IS NULL
	    AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S'
    UNION
	SELECT DISTINCT 
		@CodigoStatusReport,
		un.CodigoUsuarioGerente,
		GETDATE(),
		@CodigoUsuarioResponsavel
	FROM {0}.{1}.StatusReport sr INNER JOIN
        {0}.{1}.Projeto p ON p.CodigoProjeto = sr.CodigoObjeto INNER JOIN
        {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio 
	WHERE sr.CodigoStatusReport = @CodigoStatusReport
    AND sr.CodigoTipoAssociacaoObjeto = @CodigoTipoAssociacaoProjeto -- só trará alguma linha se o tipo na SR for de projeto
    AND un.CodigoUsuarioGerente IS NOT NULL
		
		SET @Erro = @Erro + @@Error
		
IF(@Erro <> 0)
BEGIN
		ROLLBACK TRANSACTION
		SELECT 'Falha' AS Resultado
END
ELSE
BEGIN
		COMMIT TRANSACTION
		
		 SELECT 'Sucesso' AS Resultado
		
        DECLARE @Destinatarios VARCHAR(2000)						
		DECLARE @DestinatarioTemp VARCHAR(150)

        SET @Destinatarios = ''

		DECLARE cCursor CURSOR FOR
		 SELECT DISTINCT u.Email
           FROM {0}.{1}.DestinatarioModeloStatusReport dmsr INNER JOIN
		        {0}.{1}.Usuario u ON u.CodigoUsuario = dmsr.CodigoUsuarioDestinatario INNER JOIN
		        {0}.{1}.UsuarioUnidadeNegocio uun ON uun.CodigoUsuario = u.CodigoUsuario
          WHERE dmsr.CodigoModeloStatusReport = @CodigoModeloStatusReport
            AND dmsr.CodigoTipoAssociacaoObjeto = @CodigoTipoAssociacaoObjeto
	        AND dmsr.CodigoObjeto = @CodigoObjeto
	        AND u.DataExclusao IS NULL
	        AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S'
        UNION
		 SELECT DISTINCT u.Email
		   FROM {0}.{1}.StatusReport sr INNER JOIN
                {0}.{1}.Projeto p ON p.CodigoProjeto = sr.CodigoObjeto INNER JOIN
                {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio INNER JOIN
				{0}.{1}.Usuario u ON u.CodigoUsuario = un.CodigoUsuarioGerente
		  WHERE sr.CodigoStatusReport = @CodigoStatusReport
            AND sr.CodigoTipoAssociacaoObjeto = @CodigoTipoAssociacaoProjeto -- só trará alguma linha se o tipo na SR for de projeto
				
		OPEN cCursor

		FETCH NEXT FROM cCursor INTO @DestinatarioTemp

		WHILE @@FETCH_STATUS = 0   
		BEGIN   

			SET @Destinatarios = @Destinatarios + ISNULL(@DestinatarioTemp,'')+ ';' 
			FETCH NEXT FROM cCursor INTO @DestinatarioTemp   
		END

		CLOSE cCursor
		DEALLOCATE cCursor
						
		IF (LEN(@Destinatarios)>0)
			SET @Destinatarios = SUBSTRING(@Destinatarios,1,LEN(@Destinatarios)-1);

        SELECT (SELECT dbo.f_GetDescricaoOrigemAssociacaoObjeto(@CodigoEntidade, sr.CodigoTipoAssociacaoObjeto, null, sr.CodigoObjeto,0,null) ) AS DescricaoObjeto, 
				sr.DataInicioPeriodoRelatorio, 
				sr.DataTerminoPeriodoRelatorio,
				@Destinatarios AS Destinatarios,
				sr.ConteudoStatusReport,
				msr.DescricaoModeloStatusReport,
                convert(varchar(30), sr.DataGeracao, 103) AS DataGeracao
		   FROM {0}.{1}.StatusReport sr INNER JOIN
				{0}.{1}.ModeloStatusReport msr ON msr.CodigoModeloStatusReport = sr.CodigoModeloStatusReport
		  WHERE sr.CodigoStatusReport = @CodigoStatusReport
END",
    bancodb, Ownerdb, codigoEntidade, codigoStatusReport, codigoUsuarioEnvioStatusReport);

        #endregion

        return getDataSet(comandoSql);
    }

    public byte[] getConteudoStatusReport(int codigoStatusReport)
    {
        string comandoSql = string.Format(@"SELECT ConteudoStatusReport FROM {0}.{1}.StatusReport WHERE CodigoStatusReport = {2}", bancodb, Ownerdb, codigoStatusReport);
        DataSet ds = getDataSet(comandoSql);

        return ds.Tables[0].Rows[0]["ConteudoStatusReport"] as byte[];
    }

    public byte[] getConteudoPdfMedicaoHistorico(int codigoMedicao)
    {
        string comandoSql = string.Format(@"SELECT ConteudoPDFBoletim FROM {0}.{1}.BoletimApontamentoAtribuicao WHERE CodigoBoletim = {2}", bancodb, Ownerdb, codigoMedicao);
        DataSet ds = getDataSet(comandoSql);

        return ds.Tables[0].Rows[0]["ConteudoPDFBoletim"] as byte[];
    }

    public bool GeraRelatoriosStatusReport(int codigoModeloStatusReport, string iniciaisTipoObjetoRelacionado, int codigoObjetoRelacionado, int codigoUsuarioGeracao, out string mensagem)
    {
        mensagem = string.Empty;
        string comandoSql = string.Format(@"
DECLARE	@return_value int,
		@ou_MensagemProcessamento varchar(max)

EXEC	@return_value = {0}.{1}.[p_geraRelatoriosStatusReport]
		@in_codigoModeloStatusReport = {2},
		@in_iniciaisTipoObjetoRelacionado = '{3}',
		@in_codigoObjetoRelacionado = {4},
		@in_codigoUsuarioGeracao = {5},
		@ou_MensagemProcessamento = @ou_MensagemProcessamento OUTPUT

SELECT @ou_MensagemProcessamento AS [Mensagem], @return_value AS [ValorRetorno]", bancodb, Ownerdb, codigoModeloStatusReport, iniciaisTipoObjetoRelacionado, codigoObjetoRelacionado, codigoUsuarioGeracao);
        DataSet ds = getDataSet(comandoSql);
        if (ds.Tables[0].Rows.Count > 0)
        {
            DataRow row = ds.Tables[0].Rows[0];
            int valorRetorno = (int)row["ValorRetorno"];
            if (valorRetorno == 1)
                mensagem = (string)row["Mensagem"];
        }
        return string.IsNullOrEmpty(mensagem);
    }

    public bool GeraHistoricoMedicaoApontamento(int mes, int ano, int codigoProjeto, int codigoUsuario, int codigoEntidadeContexto, out int retornoProc, out long codigoNovoBoletim)
    {
        retornoProc = 0;
        codigoNovoBoletim = 0;
        
        string comandoSql = string.Format(@"
        DECLARE @nRetProc int , 
                @codigoNovoBoletim bigint;
        EXEC @nRetProc = {0}.{1}.[p_art_geraBoletimMedicao]
            @in_codigoEntidadeContexto = {6} , 
            @in_codigoUsuarioSistema = {5} , 
            @in_codigoProjeto = {4} , 
            @in_mesReferencia = {2} , 
            @in_anoReferencia = {3} , 
            @ou_codigoNovoBoletim = @codigoNovoBoletim OUTPUT

        SELECT @nRetProc AS [Retorno], @codigoNovoBoletim AS [NovoBoletim]", bancodb, Ownerdb, mes, ano, codigoProjeto, codigoUsuario, codigoEntidadeContexto);
        
        DataSet ds = getDataSet(comandoSql);
        if (ds.Tables[0].Rows.Count > 0)
        {
            DataRow row = ds.Tables[0].Rows[0];
            long idNovoBoletim = (long)row["NovoBoletim"];
            int retornoDaProc = (int)row["Retorno"];

            if (retornoDaProc == 0)
                retornoProc = retornoDaProc;
                codigoNovoBoletim = idNovoBoletim;
        }
        return retornoProc == 0;
    }

    public DataSet ObtemModelosStatusReport(int codigoEntidade, int codigoObjeto, string iniciaisTipoObjeto)
    {
        string comandoSql = string.Format(@"
 SELECT DISTINCT
		MSR.CodigoModeloStatusReport,
		MSR.DescricaoModeloStatusReport 
   FROM {0}.{1}.ModeloStatusReport MSR INNER JOIN 
		{0}.{1}.ModeloStatusReportObjeto MSRO ON MSR.CodigoModeloStatusReport = MSRO.CodigoModeloStatusReport
  WHERE MSR.DataExclusao IS NULL 
	AND MSR.CodigoEntidade = {2}
    AND MSRO.CodigoObjeto = {3}
    AND MSRO.CodigoTipoAssociacaoObjeto = {0}.{1}.f_GetCodigoTipoAssociacao('{4}')
    AND MSRO.IndicaModeloAtivoObjeto = 'S'
UNION
 SELECT DISTINCT 
		MSR.CodigoModeloStatusReport,
		MSR.DescricaoModeloStatusReport 
   FROM {0}.{1}.ModeloStatusReport MSR 
		INNER JOIN  {0}.{1}.ModeloStatusReportObjeto MSRO ON 
			(			MSRO.CodigoModeloStatusReport = MSR.CodigoModeloStatusReport
				AND MSRO.CodigoTipoAssociacaoObjeto	= {0}.{1}.f_GetCodigoTipoAssociacao('CP')
				AND MSRO.IndicaModeloAtivoObjeto	= 'S' )

		INNER JOIN {0}.{1}.Carteira	c ON c.CodigoCarteira = MSRO.CodigoObjeto AND c.IndicaCarteiraAtiva = 'S'
		INNER JOIN {0}.{1}.vi_CarteiraProjeto cp ON cp.CodigoCarteira = c.CodigoCarteira AND cp.CodigoProjeto = {3}
  WHERE MSR.DataExclusao IS NULL 
	AND MSR.CodigoEntidade = {2}
	AND MSR.IniciaisModeloControladoSistema = 'SR_PPJ01'
	AND {0}.{1}.f_GetCodigoTipoAssociacao('{4}') = {0}.{1}.f_GetCodigoTipoAssociacao('PR')
ORDER BY
        MSR.DescricaoModeloStatusReport", bancodb, Ownerdb, codigoEntidade, codigoObjeto, iniciaisTipoObjeto);

        DataSet ds = getDataSet(comandoSql);
        return ds;
    }

    public void ExcluiRelatorioStatusReport(int codigoStatusReport, int codigoUsuarioExclusao)
    {
        string comandoSql = string.Format(@"
DECLARE @CodigoStatusReport INT
DECLARE @CodigoUsuarioExclusao INT
DECLARE @DataExclusao DATETIME
		SET @CodigoStatusReport = {2}
		SET @CodigoUsuarioExclusao = {3}
		SET @DataExclusao = GETDATE()

UPDATE {0}.{1}.[StatusReport]
   SET [DataExclusao] = @DataExclusao
      ,[CodigoUsuarioExclusao] = @CodigoUsuarioExclusao
 WHERE CodigoStatusReport = @CodigoStatusReport"
            , bancodb, Ownerdb, codigoStatusReport, codigoUsuarioExclusao);

        int qtdAfetados = 0;
        execSQL(comandoSql, ref qtdAfetados);
    }

    public void ExcluiHistoricoMedicaoApontamento(int codigoBoletimMedicao, int codigoUsuarioExclusao, int idProjeto)
    {
        string comandoSql = string.Format(@"
DECLARE @codigoBoletimMedicao INT
DECLARE @CodigoUsuarioExclusao INT
DECLARE @DataExclusao DATETIME
DECLARE @codigoProjeto INT
		SET @codigoBoletimMedicao = {2}
		SET @CodigoUsuarioExclusao = {3}
		SET @DataExclusao = GETDATE()
        SET @codigoProjeto = {4}

UPDATE {0}.{1}.[BoletimApontamentoAtribuicao]
   SET [DataExclusao] = @DataExclusao
      ,[CodigoUsuarioExclusao] = @CodigoUsuarioExclusao
 WHERE CodigoBoletim = @codigoBoletimMedicao AND CodigoProjeto = @codigoProjeto"
            , bancodb, Ownerdb, codigoBoletimMedicao, codigoUsuarioExclusao, idProjeto);

        int qtdAfetados = 0;
        execSQL(comandoSql, ref qtdAfetados);
    }
    #endregion

    #region DEMANDAS NOVO MODELO

    public DataSet getDemandasEntidade(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT d.CodigoDemanda, d.TituloDemanda AS Titulo, ad.DescricaoAssuntoDemanda AS Assunto
	                  ,s.DescricaoStatus AS Status, td.DescricaoTipoDemanda AS Tipo
	                  ,CASE WHEN Urgencia = 'A' THEN 'Alta'
			                WHEN Urgencia = 'M' THEN 'Média'
			                ELSE 'Baixa' END AS Urgencia
	                  ,DataExpectativaAtendimento AS Previsao 
	                  ,UDem.NomeUsuario AS Demandante
	                  ,UResp.NomeUsuario AS Responsavel
                      ,d.CodigoAssuntoDemanda
                      ,d.CodigoTipoDemanda
                      ,d.CodigoUsuarioDemandante
                      ,d.Urgencia AS UrgenciaDemanda
                      ,d.CodigoCanalAberturaDemanda
                      ,d.DescricaoDetalhadaDemanda
                  FROM {0}.{1}.Demanda d INNER JOIN 
	                   {0}.{1}.Projeto p ON (p.CodigoProjeto = d.CodigoProjeto 
					                 AND p.CodigoEntidade = {2}) INNER JOIN
	                   {0}.{1}.AssuntoDemanda ad ON ad.CodigoAssuntoDemanda = d.CodigoAssuntoDemanda INNER JOIN
	                   {0}.{1}.Status s ON s.CodigoStatus = p.CodigoStatusProjeto INNER JOIN
	                   {0}.{1}.TipoDemanda td ON td.CodigoTipoDemanda = d.CodigoTipoDemanda INNER JOIN
	                   {0}.{1}.Usuario UDem ON UDem.CodigoUsuario = d.CodigoUsuarioDemandante LEFT JOIN
	                   {0}.{1}.Usuario UResp ON UResp.CodigoUsuario = d.CodigoUsuarioResponsavel
                 WHERE d.DataExclusao IS NULL         
                   {3}           
               ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiDemandaEntidade(int codigoEntidade, int codigoUsuarioInclusao, int codigoDemandante, string nomeDemanda, string descricaoDemanda
                                      , int codigoAssunto, string urgencia, int codigoTipoDemanda, int codigoCanalAbertura, ref int codigoDemanda, ref string mensagem)
    {
        try
        {
            comandoSQL = string.Format(
                @"BEGIN
                        DECLARE @CodigoProjeto int,
                                @CodigoEntidade int,
                                @CodigoUsuarioLogado int,
                                @CodigoDemandante int,
                                @CodigoAssunto int,
                                @NomeDemanda VarChar(250),
                                @DescricaoDemanda VarChar(2000),
                                @Prioridade int,
                                @Urgencia Char(1),
                                @CodigoStatus int,
                                @CodigoTipoDemanda int,
                                @CodigoCanalAberturaDemanda int

                        SET @CodigoEntidade = {2}
                        SET @CodigoUsuarioLogado = {3}
                        SET @CodigoDemandante = {4}
                        SET @NomeDemanda = '{5}'
                        SET @DescricaoDemanda = '{6}'
                        SET @CodigoAssunto = {7}
                        SET @Prioridade = 0
                        SET @Urgencia = '{8}'
                        SET @CodigoStatus = 14
                        SET @CodigoTipoDemanda = {9}
                        SET @CodigoCanalAberturaDemanda = {10}
                        

                        INSERT INTO {0}.{1}.Projeto(NomeProjeto, DataInclusao, CodigoUsuarioInclusao, CodigoStatusProjeto, IndicaPrograma, CodigoEntidade, CodigoTipoProjeto)
                                              VALUES(@NomeDemanda, GetDate(), @CodigoUsuarioLogado, @CodigoStatus, 'N', @CodigoEntidade, 7)

                        
                        SELECT @CodigoProjeto = scope_identity()

                        INSERT INTO {0}.{1}.Demanda(CodigoProjeto,TituloDemanda,DescricaoDetalhadaDemanda,DataInclusao,CodigoUsuarioInclusao
                                                    ,CodigoUsuarioDemandante,CodigoAssuntoDemanda,Prioridade,Urgencia,CodigoTipoDemanda,CodigoCanalAberturaDemanda)
                                             VALUES(@CodigoProjeto, @NomeDemanda, @DescricaoDemanda, GetDate(), @CodigoUsuarioLogado
                                                    ,@CodigoDemandante, @CodigoAssunto, @Prioridade, @Urgencia, @CodigoTipoDemanda, @CodigoCanalAberturaDemanda)

                        SELECT scope_identity() AS CodigoDemanda                         

                    END", bancodb, Ownerdb, codigoEntidade, codigoUsuarioInclusao, codigoDemandante, nomeDemanda, descricaoDemanda, codigoAssunto, urgencia, codigoTipoDemanda, codigoCanalAbertura);

            DataSet ds = getDataSet(comandoSQL);
            codigoDemanda = int.Parse(ds.Tables[0].Rows[0]["CodigoDemanda"].ToString());

            return true;
        }
        catch (Exception ex)
        {
            mensagem = ex.Message;
            return false;
        }
    }

    public DataSet getTiposDemandas(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT CodigoTipoDemanda, DescricaoTipoDemanda, IniciaisTipoDemanda
                 FROM {0}.{1}.TipoDemanda
                WHERE CodigoEntidade = {2}
                  {3}
                ORDER BY DescricaoTipoDemanda                     
               ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getTiposCanaisAberturaDemandas(string where)
    {
        string comandoSQL = string.Format(
             @"SELECT CodigoCanalAberturaDemanda, DescricaoCanalAberturaDemanda
                 FROM {0}.{1}.TipoCanalAberturaDemanda
                WHERE 1 = 1
                  {2}
                ORDER BY DescricaoCanalAberturaDemanda                     
               ", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getAssuntosDemandas(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT CodigoAssuntoDemanda, DescricaoAssuntoDemanda, CodigoGerente
                     ,CodigoFluxo, CodigoProjetoAssociado, Comentario
                 FROM {0}.{1}.AssuntoDemanda
                WHERE CodigoEntidade = {2}
                  {3}
                ORDER BY DescricaoAssuntoDemanda                     
               ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiAssuntoDemanda(string descricao, int codigoEntidade, int codigoResponsavel, int codigoFluxo, int codigoProjeto, string comentarios, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";

        try
        {
            comandoSQL = string.Format(@"
                    INSERT INTO {0}.{1}.AssuntoDemanda (DescricaoAssuntoDemanda, CodigoEntidade, CodigoGerente, CodigoFluxo, CodigoProjetoAssociado, Comentario)
                                               VALUES('{2}', {3}, {4}, {5}, {6}, '{7}')                    
                     ", bancodb, Ownerdb, descricao, codigoEntidade, codigoResponsavel, codigoFluxo,
                      (codigoProjeto == -1 ? "NULL" : "'" + codigoProjeto + "'"), comentarios);
            int regAf = 0;

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

    public bool atualizaAssuntoDemanda(int codigoAssunto, string descricao, int codigoResponsavel, int codigoFluxo, int codigoProjeto, string comentarios, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";

        try
        {
            comandoSQL = string.Format(@"
                    UPDATE {0}.{1}.AssuntoDemanda SET DescricaoAssuntoDemanda = '{3}'
                                                    , CodigoGerente = {4}
                                                    , CodigoFluxo = {5}
                                                    , CodigoProjetoAssociado = {6}
                                                    , Comentario = '{7}'
                                               WHERE CodigoAssuntoDemanda = {2}                   
                     ", bancodb, Ownerdb, codigoAssunto, descricao, codigoResponsavel, codigoFluxo,
                      (codigoProjeto == -1 ? "NULL" : "'" + codigoProjeto + "'"), comentarios);
            int regAf = 0;

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

    public bool excluiAssuntoDemanda(int codigoAssunto, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";

        try
        {
            comandoSQL = string.Format(@"
                    SELECT 1 FROM {0}.{1}.Demanda WHERE CodigoAssuntoDemanda = {2}                   
                     ", bancodb, Ownerdb, codigoAssunto);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                msgError = "Erro ao excluir! Existem demandas associadas ao assunto.";
                return false;
            }

            comandoSQL = string.Format(@"
                    DELETE FROM {0}.{1}.AssuntoDemanda 
                     WHERE CodigoAssuntoDemanda = {2}                   
                     ", bancodb, Ownerdb, codigoAssunto);

            int regAf = 0;

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

    public DataSet getListaInformacoesDemandas(int codigoEntidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(@"SELECT CodigoDemanda, TituloDemanda, SiglaOrgaoDemandante, NomeResponsavel, TipoDemanda, StatusDemanda
			                                      ,EtapaAtualProcesso, ValorSolicitado, ValorAprovado, ProjetoPlanoSetorial, ProjetoPDTI
			                                      ,CodigoWorkflow, CodigoInstanciaWf, CodigoEtapaInicial, CodigoEtapaAtual, OcorrenciaAtual
			                                      ,CodigoStatus, TipoProjeto, ProcessoComigo, SequenciaOcorrenciaEtapaWf, CodigoFluxo, NumeroCGTIC, PodeIniciarFluxoAbrirProjeto
                                                  ,CASE WHEN PossuiPlanoAcaoDemanda = 'S' THEN 'Sim' 
												        WHEN PossuiPlanoAcaoDemanda = 'N' THEN 'Não'
														ELSE '' END AS PossuiPlanoAcaoDemanda
                                                  ,SituacaoPlanoAcaoDemanda, ValorExecutado, Ano, ValorSolicitadoAno, ValorAprovadoAno, ValorExecutadoAno, PodeEditarVinculos
                                                  ,OficioJucof, ValorAprovadoJUCOF, AnoOrcamentoJUCOF, ValorAprovadoAnoJUCOF
                                              FROM {0}.{1}.f_pbh_getDemandas({2}, {3})
                                             WHERE 1 = 1
                                               {4}
                                             ORDER BY RIGHT(NumeroCGTIC, 4) DESC, RIGHT('00000' + LEFT(NumeroCGTIC, 4), 5) DESC",
            bancodb, Ownerdb, codigoEntidade, codigoUsuario, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getListaInformacoesPlanosInvestimento(int codigoEntidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(@"SELECT CodigoProjeto, TituloProjeto, SiglaOrgaoDemandante, NomeResponsavel, StatusProjeto
			                                      ,EtapaAtualProcesso, CodigoWorkflow, CodigoInstanciaWf, CodigoEtapaInicial, CodigoEtapaAtual, OcorrenciaAtual
			                                      ,CodigoStatus, SequenciaOcorrenciaEtapaWf, CodigoFluxo, Ano
                                              FROM {0}.{1}.f_pbh_GetPlanosInvestimento({2}, {3})
                                             WHERE 1 = 1
                                               {4}
                                             ORDER BY Ano DESC, TituloProjeto",
            bancodb, Ownerdb, codigoEntidade, codigoUsuario, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getListaVinculosPDTI(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"SELECT p.CodigoProjeto AS Codigo, 
                                                   prog.NomeProjeto AS Plano,
                                                   p.NomeProjeto AS Projeto
                                              FROM {0}.{1}.Projeto AS p INNER JOIN 
                                                   {0}.{1}.LinkProjeto AS l ON (l.CodigoProjetoFilho = p.CodigoProjeto) INNER JOIN 
                                                   {0}.{1}.Projeto AS prog ON (prog.CodigoProjeto = l.CodigoProjetoPai) 
                                             WHERE p.DataExclusao IS NULL 
                                               AND p.CodigoTipoProjeto = 17  
                                               AND p.CodigoEntidade = {2}
                                               {3}
                                             ORDER BY 2,1",
            bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getListaVinculosLOA(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"SELECT p.CodigoProjeto AS Codigo,        
                                                   p.NomeProjeto AS Projeto,
                                                   pinv.Ano AS AnoOrcamento
                                              FROM {0}.{1}.Projeto AS p INNER JOIN        
                                                   {0}.{1}.pbh_PlanoInvestimentoProjeto AS pip ON (pip.CodigoProjeto = p.CodigoProjeto) INNER JOIN
                                                   {0}.{1}.pbh_PlanoInvestimento AS pinv ON (pip.CodigoPlanoInvestimento = pinv.CodigoPlanoInvestimento)
                                             WHERE p.DataExclusao IS NULL 
                                               AND p.CodigoTipoProjeto = 20
                                               AND p.CodigoEntidade = {2}
                                               {3}
                                             ORDER BY 3 DESC,2 ",
            bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getListaVinculosTIC(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"SELECT p.CodigoProjeto AS Codigo,        
                                                   p.NomeProjeto AS Projeto,
                                                   s.DescricaoStatus AS Status
                                              FROM {0}.{1}.Projeto AS p INNER JOIN 
                                                   {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto)
                                             WHERE p.DataExclusao IS NULL 
                                               AND p.CodigoTipoProjeto = 14
                                               AND p.CodigoEntidade = {2} 
                                               {3}
                                             ORDER BY 2",
            bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getVinculosDemanda(int codigoDemanda, string where)
    {
        string comandoSQL = string.Format(@"
                                        SELECT ISNULL(CodigoPDTI, -1) AS CodigoPDTI
			                                  ,ISNULL(CodigoLOA, -1) AS CodigoLOA
			                                  ,ISNULL(CodigoTIC, -1) AS CodigoTIC 
                                              ,ValorSolicitado
                                              ,ValorLiberado
                                              ,ValorSolicitadoAno
                                              ,ValorLiberadoAno
                                         FROM {0}.{1}.pbh_HistoricoDemanda
                                        WHERE CodigoDemandaComoProjeto = {2}
                                          {3}",
            bancodb, Ownerdb, codigoDemanda, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getPlanosInvestimentoTIC(string where)
    {
        string comandoSQL = string.Format(@"
                      SELECT p.CodigoPlanoInvestimento
			                ,p.DescricaoPlanoInvestimento
			                ,p.CodigoStatusPlanoInvestimento
			                ,p.Ano
			                ,p.DataInicioInclusaoProjeto
			                ,p.DataFinalInclusaoProjetos
			                ,s.DescricaoStatusPlanoInvestimento
			                ,s.DescricaoAcaoProximoStatus
			                ,s.CodigoProximoStatusPlanoInvestimento
                       FROM {0}.{1}.pbh_PlanoInvestimento p INNER JOIN
			                {0}.{1}.pbh_StatusPlanoInvestimento s ON s.CodigoStatusPlanoInvestimento = p.CodigoStatusPlanoInvestimento
                      WHERE 1 = 1
                        {2}
                      ORDER BY CodigoPlanoInvestimento DESC",
            bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getProjetosPlanoInvestimentoTIC(int codigoPlanoInvestimento, string where)
    {
        string comandoSQL = string.Format(@"
                      SELECT * 
                        FROM {0}.{1}.f_pbh_GetProjetosPlanoInvestimento({2})
                       WHERE 1 = 1
                         {3}",
            bancodb, Ownerdb, codigoPlanoInvestimento, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getMovimentoProjetoPlanoInvestimentoTIC(int codigoPlanoInvestimento, int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(@"
                      SELECT mpi.CodigoMovimentoPlanoInvestimento, mpi.DataMovimento, mpi.CodigoUsuarioMovimento, mpi.TipoMovimento
			                ,mpi.ComentarioMovimento, spiDe.DescricaoStatusPlanoInvestimento AS StatusDe, u.NomeUsuario
			                ,spiPara.DescricaoStatusPlanoInvestimento AS StatusPara, mpi.ValorAnterior, mpi.NovoValor
			                ,CASE WHEN mpi.TipoMovimento = 'EX' THEN 'Marcado Como Não Selecionado LOA'
						          WHEN mpi.TipoMovimento = 'AS' THEN 'Alteração de Status'
                                  WHEN mpi.TipoMovimento = 'RM' THEN 'Alteração de Valor Remanejado'
						          ELSE CASE WHEN mpi.CodigoStatusDe = 1 THEN 'Alteração de Valor Ajustado CGTIC'
							                ELSE 'Alteração de Valor LOA' END END AS DescricaoMovimento				
                       FROM {0}.{1}.pbh_MovimentoPlanoInvestimento mpi INNER JOIN
			                {0}.{1}.Usuario u ON u.CodigoUsuario = mpi.CodigoUsuarioMovimento LEFT JOIN
			                {0}.{1}.pbh_StatusPlanoInvestimento spiDe ON spiDe.CodigoStatusPlanoInvestimento = mpi.CodigoStatusDe LEFT JOIN
			                {0}.{1}.pbh_StatusPlanoInvestimento spiPara ON spiPara.CodigoStatusPlanoInvestimento = mpi.CodigoStatusPara
                      WHERE CodigoPlanoInvestimento = {2}
                        AND CodigoProjeto = {3}
                        {4}
                      ORDER BY mpi.DataMovimento DESC",
            bancodb, Ownerdb, codigoPlanoInvestimento, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region Visão Objetivos

    public DataSet getObjetivosVisaoCorporativa(int codigoEntidade, int codigoUsuario)
    {
        string comandoSQL = string.Format(@"            
                           SELECT CodigoObjetivo, DescricaoObjetivo, DesempenhoAtual FROM {0}.{1}.f_getObjetivosIniciativas({2}, {3})
            ", bancodb, Ownerdb, codigoUsuario, codigoEntidade);

        return getDataSet(comandoSQL);
    }

    #endregion

    #region Visão de Obras - NE

    public DataSet getContratosComboCurvaS(int codigoEntidade, int codigoCarteira, int codigoUsuario, int ano, int tipoContratacao, int codigoFonte, string where)
    {
        string comandoSQL = string.Format(@"            
                           BEGIN
	                            DECLARE @CodigoEntidade Int
				                            , @CodigoCarteira int
				                            , @CodigoUsuario int
				                            , @Ano SmallInt
				                            , @CodigoTipoServico SmallInt
				                            , @CodigoFonte int
				
	                            SET @CodigoEntidade = {2}
	                            SET @CodigoCarteira = {3}
	                            SET @CodigoUsuario = {4}
	                            SET @CodigoTipoServico = {5}
	                            SET @Ano = {6}
	                            SET @CodigoFonte = {7}

	                            IF @CodigoTipoServico = -1 
			                            SELECT c.CodigoContrato, c.NumeroContrato, c.DescricaoObjetoContrato
				                            FROM {0}.{1}.Contrato AS c LEFT JOIN
				                                 {0}.{1}.Pessoa pe ON pe.CodigoPessoa = c.CodigoPessoaContratada LEFT JOIN 
                                                {0}.{1}.[PessoaEntidade] AS [pen] ON (
			                                    pen.[CodigoPessoa] = c.[CodigoPessoaContratada]
			                                    AND pen.codigoEntidade = c.codigoEntidade
                                                --AND pen.IndicaFornecedor = 'S'
			                                    ) 	 	
				                            WHERE c.CodigoEntidade = @CodigoEntidade
                                                AND YEAR(c.DataTermino) = @Ano
					                            AND (c.CodigoFonteRecursosFinanceiros = @CodigoFonte OR ISNULL(@CodigoFonte, -1) = -1)
                                                {8}
		                             ELSE /* Há filtro por tipo de serviço */	   
				                            SELECT c.CodigoContrato, c.NumeroContrato, c.DescricaoObjetoContrato
				                            FROM {0}.{1}.Obra AS o  INNER JOIN 
						                         {0}.{1}.TipoServico AS ts ON (ts.CodigoTipoServico	= o.CodigoTipoServico		
																	   AND ts.CodigoTipoServico = @CodigoTipoServico) INNER JOIN
						                         {0}.{1}.Contrato AS c ON (c.CodigoContrato = o.CodigoContrato
                                                                           AND c.StatusContrato = 'A')	LEFT JOIN
				                                 {0}.{1}.Pessoa pe ON pe.CodigoPessoa = c.CodigoPessoaContratada LEFT JOIN 
                                                {0}.{1}.[PessoaEntidade] AS [pen] ON (
			                                    pen.[CodigoPessoa] = c.[CodigoPessoaContratada]
			                                    AND pen.codigoEntidade = c.codigoEntidade
                                                --AND pen.IndicaFornecedor = 'S'
			                                    ) 					
				                            WHERE c.CodigoEntidade = @CodigoEntidade					                            
					                            AND o.DataExclusao IS NULL						
					                            AND YEAR(c.DataTermino) = @Ano
					                            AND (c.CodigoFonteRecursosFinanceiros = @CodigoFonte OR ISNULL(@CodigoFonte, -1) = -1)
                                                {8}
                            END
            ", bancodb, Ownerdb, codigoEntidade, codigoCarteira, codigoUsuario, tipoContratacao, ano, codigoFonte, where);


        return getDataSet(comandoSQL);
    }

    public DataSet getContratosPorFaseMunicipio(int codigoEntidade, int codigoCarteira, int codigoUsuario, int ano, string where)
    {
        string comandoSQL = string.Format(@"            
                           SELECT * FROM {0}.{1}.f_GetContratosPorMunicipio({2},{3},{4},{5} )
            ", bancodb, Ownerdb, codigoEntidade, codigoCarteira, codigoUsuario, ano, where);


        return getDataSet(comandoSQL);
    }

    public DataSet getEvolucaoContratosPorFase(int codigoEntidade, int codigoCarteira, int codigoUsuario, int anoContrato, string where)
    {
        string comandoSQL = string.Format(@"            
                           SELECT * FROM {0}.{1}.f_GetContratosPorMes({2},{3},{4}, {5})
            ", bancodb, Ownerdb, codigoEntidade, codigoCarteira, codigoUsuario, anoContrato, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getContratosPorStatusMunicipio(int codigoEntidade, int codigoCarteira, int codigoUsuario, int ano, int tipoContratacao, int fonte, string where)
    {
        string comandoSQL = string.Format(@"            
                           SELECT * FROM {0}.{1}.f_GetContratosPorMunicipioStatus({2},{3},{4},{5},{6},{7} )
            ", bancodb, Ownerdb, codigoEntidade, codigoCarteira, codigoUsuario, ano, tipoContratacao, fonte, where);


        return getDataSet(comandoSQL);
    }

    public DataSet getRealizacaoContratosPorMunicipio(int codigoEntidade, int codigoCarteira, int codigoUsuario, int anoContrato, string indicaObras, int tipoContratacao, int fonte, string where)
    {
        string comandoSQL = string.Format(@"            
                           SELECT * FROM {0}.{1}.f_GetFinanceiroContratoRealizadoPorMunicipio({2},{3},{4},{5},'{6}',{7}, {8} )
            ", bancodb, Ownerdb, codigoEntidade, codigoCarteira, codigoUsuario, anoContrato, indicaObras, tipoContratacao, fonte, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getEvolucaoContratosPorStatus(int codigoEntidade, int codigoCarteira, int codigoUsuario, int anoContrato, int tipoContratacao, int fonte, string where)
    {
        string comandoSQL = string.Format(@"            
                           SELECT * FROM {0}.{1}.f_GetContratosPorMesStatus({2},{3},{4}, {5}, {6}, {7})
            ", bancodb, Ownerdb, codigoEntidade, codigoCarteira, codigoUsuario, anoContrato, tipoContratacao, fonte, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getEvolucaoRealizacaoContratos(int codigoEntidade, int codigoCarteira, int codigoUsuario, int anoContrato, string indicaObras, int tipoContratacao, int fonte, string where)
    {
        string comandoSQL = string.Format(@"            
                           SELECT * FROM {0}.{1}.f_GetFinanceiroContratoRealizadoPorMes({2},{3},{4},{5},'{6}',{7},{8})
            ", bancodb, Ownerdb, codigoEntidade, codigoCarteira, codigoUsuario, anoContrato, indicaObras, tipoContratacao, fonte, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getCurvaSGestaoContratos(int codigoContrato, int codigoEntidade, int codigoCarteira, int codigoUsuario, int ano, int tipoContratacao, int fonte, string where)
    {
        string comandoSQL = string.Format(@"            
                           EXEC {0}.{1}.p_curva_S_GestaoContratos {2},{3},{4},{5},{6},{7},{8}
            ", bancodb, Ownerdb, codigoContrato, codigoEntidade, codigoCarteira, codigoUsuario, ano, tipoContratacao, fonte, where);


        return getDataSet(comandoSQL);
    }

    public DataSet getContratosPorStatus(int codigoEntidade, int codigoCarteira, int codigoUsuario, int ano, string where)
    {
        string comandoSQL = string.Format(@"            
                           SELECT * FROM {0}.{1}.f_GetQuantidadeObrasEntornoPorStatus({2},{5} )
            ", bancodb, Ownerdb, codigoEntidade, codigoCarteira, codigoUsuario, ano, where);


        return getDataSet(comandoSQL);
    }

    public DataSet getContratosPorTipoContratacao(int codigoEntidade, int codigoCarteira, int codigoUsuario, int ano, string where)
    {
        string comandoSQL = string.Format(@"            
                           SELECT * FROM {0}.{1}.f_GetQuantidadeObrasPorTipoObraStatus({2},{5} )
            ", bancodb, Ownerdb, codigoEntidade, codigoCarteira, codigoUsuario, ano, where);


        return getDataSet(comandoSQL);
    }

    public DataSet getContratosPorTipoContrato(int codigoEntidade, int codigoCarteira, int codigoUsuario, int ano, string where)
    {
        string comandoSQL = string.Format(@"            
                           SELECT * FROM {0}.{1}.f_GetValoresContratosPorTipo({2},{5})
            ", bancodb, Ownerdb, codigoEntidade, codigoCarteira, codigoUsuario, ano, where);


        return getDataSet(comandoSQL);
    }


    public DataSet getAnosContratos(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"            
                          SELECT DISTINCT YEAR(c.DataTermino) AS AnoContrato 
                            FROM {0}.{1}.Contrato c LEFT JOIN
				             {0}.{1}.Pessoa pe ON pe.CodigoPessoa = c.CodigoPessoaContratada LEFT JOIN 
                            {0}.{1}.[PessoaEntidade] AS [pen] ON (
			                pen.[CodigoPessoa] = c.[CodigoPessoaContratada]
			                AND pen.codigoEntidade = c.codigoEntidade
                            --AND pen.IndicaFornecedor = 'S'
			                ) 
                           WHERE c.CodigoEntidade = {2}
                             AND DataTermino IS NOT NULL
                            {3}
                           UNION
                           SELECT DISTINCT YEAR(IsNull(TerminoRepactuado,TerminoPrevistoObraPBA))
                              FROM {0}.{1}.Obra o INNER JOIN
			                       {0}.{1}.Projeto p ON (p.CodigoProjeto = o.CodigoProjeto
								             AND p.CodigoEntidade = {2})
                             WHERE IsNull(TerminoRepactuado,TerminoPrevistoObraPBA) IS NOT NULL
                               AND o.DataExclusao IS NULL
                               AND p.DataExclusao IS NULL
                          ORDER BY AnoContrato DESC
            ", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public string getGraficoContratosPorFaseMunicipio(DataTable dt, string titulo, int fonte, bool linkLista)
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
                xmlAux.Append(string.Format(@"<category label=""{0}"" toolText=""{1}"" />", dt.Rows[i]["SiglaMunicipio"]
                    , dt.Rows[i]["NomeMunicipio"]));

            }

            xmlAux.Append("</categories>");

            string link = "";

            xmlAux.Append(string.Format(@"<dataset seriesName=""Concluídos""  color=""27DC79"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&CodigoMunicipio={0}&Situacao={1}"" ", dt.Rows[i]["CodigoMunicipio"], Server.UrlEncode("Concluídos"));

                xmlAux.Append(string.Format(@"<set label=""Concluídos"" value=""{0}"" color=""27DC79"" {1}/>", dt.Rows[i]["Concluidos"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Construção""  color=""FFBE5E"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&CodigoMunicipio={0}&Situacao={1}"" ", dt.Rows[i]["CodigoMunicipio"], Server.UrlEncode("Em Construção"));

                xmlAux.Append(string.Format(@"<set label=""Construção"" value=""{0}"" color=""FFBE5E"" {1}/>", dt.Rows[i]["EmConstrucao"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Contratação""  color=""18C6EB"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&CodigoMunicipio={0}&Situacao={1}"" ", dt.Rows[i]["CodigoMunicipio"], Server.UrlEncode("Em Contratação"));

                xmlAux.Append(string.Format(@"<set label=""Contratação"" value=""{0}"" color=""18C6EB"" {1}/>", dt.Rows[i]["EmContratacao"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Aguardando OS""  color=""FFFFA4"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&CodigoMunicipio={0}&Situacao={1}"" ", dt.Rows[i]["CodigoMunicipio"], Server.UrlEncode("Aguardando OS"));

                xmlAux.Append(string.Format(@"<set label=""Aguardando OS"" value=""{0}"" color=""FFFFA4"" {1}/>", dt.Rows[i]["AguardandoOS"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Elaboração de Projetos""  color=""D5C3FC"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&CodigoMunicipio={0}&Situacao={1}"" ", dt.Rows[i]["CodigoMunicipio"], Server.UrlEncode("Em Elaboração/Aprovação de Projetos"));

                xmlAux.Append(string.Format(@"<set label=""Elaboração de Projetos"" value=""{0}"" color=""D5C3FC"" {1}/>", dt.Rows[i]["EmElaboracao"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Previstos""  color=""7595B5"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&CodigoMunicipio={0}&Situacao={1}"" ", dt.Rows[i]["CodigoMunicipio"], Server.UrlEncode("Previstos"));

                xmlAux.Append(string.Format(@"<set label=""Previstos"" value=""{0}"" color=""7595B5"" {1}/>", dt.Rows[i]["Previstos"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");
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
        xml.Append(string.Format(@"<chart imageSave=""1"" caption=""{0}"" adjustDiv=""1"" numVisiblePlot=""10""  scrollHeight=""12""  showLegend=""1""
                  BgColor=""F7F7F7"" canvasBgColor=""F7F7F7"" showvalues=""0"" decimals=""0"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" 
                  chartTopMargin=""5"" chartRightMargin=""4"" chartBottomMargin=""2"" chartLeftMargin=""4"" canvasLeftMargin=""2"" canvasRightMargin=""2"" legendPadding=""4""
                  showShadow=""0"" {1} showBorder=""0"" baseFontSize=""{2}"" slantLabels=""1"" labelDisplay=""ROTATE""  decimalPrecision=""0"">"
                        , titulo
                        , usarGradiente + usarBordasArredondadas + exportar
                        , fonte));

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

    public string getGraficoContratosPorStatusMunicipio(DataTable dt, string titulo, int fonte, bool linkLista)
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
                xmlAux.Append(string.Format(@"<category label=""{0}"" toolText=""{1}"" />", dt.Rows[i]["SiglaMunicipio"]
                    , dt.Rows[i]["NomeMunicipio"]));

            }

            xmlAux.Append("</categories>");

            string link = "";

            xmlAux.Append(string.Format(@"<dataset seriesName=""Previstos"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../../_Projetos/Administracao/ListaContratosEstendidos.aspx?IndicaVigentesAno=S&CodigoMunicipio={0}&Situacao={1}"" ", dt.Rows[i]["CodigoMunicipio"], Server.UrlEncode("Previsto"));

                xmlAux.Append(string.Format(@"<set label=""Previstos"" value=""{0}"" {1}/>", dt.Rows[i]["EmContratacao"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Ativos"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../../_Projetos/Administracao/ListaContratosEstendidos.aspx?IndicaVigentesAno=S&CodigoMunicipio={0}&Situacao={1}"" ", dt.Rows[i]["CodigoMunicipio"], Server.UrlEncode("Ativo"));

                xmlAux.Append(string.Format(@"<set label=""Ativos"" value=""{0}"" {1}/>", dt.Rows[i]["Ativo"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Encerrados"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../../_Projetos/Administracao/ListaContratosEstendidos.aspx?IndicaVigentesAno=S&CodigoMunicipio={0}&Situacao={1}"" ", dt.Rows[i]["CodigoMunicipio"], Server.UrlEncode("Encerrado"));

                xmlAux.Append(string.Format(@"<set label=""Encerrados"" value=""{0}"" {1}/>", dt.Rows[i]["Encerrado"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");
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
        xml.Append(string.Format(@"<chart imageSave=""1"" caption=""{0}"" adjustDiv=""1"" numVisiblePlot=""10""  scrollHeight=""12""  showLegend=""1""
                  BgColor=""F7F7F7"" canvasBgColor=""F7F7F7"" showvalues=""0"" decimals=""0"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" 
                  chartTopMargin=""5"" chartRightMargin=""4"" chartBottomMargin=""2"" chartLeftMargin=""4"" canvasLeftMargin=""2"" canvasRightMargin=""2"" legendPadding=""4""
                  showShadow=""0"" {1} showBorder=""0"" baseFontSize=""{2}"" slantLabels=""1"" labelDisplay=""ROTATE""  decimalPrecision=""0"">"
                        , titulo
                        , usarGradiente + usarBordasArredondadas + exportar
                        , fonte));

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

    public string getGraficoRealizacaoContratosPorMunicipio(DataTable dt, string titulo, int fonte, bool linkLista, string indicaTelaObras)
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
                xmlAux.Append(string.Format(@"<category label=""{0}"" toolText=""{1}"" />", dt.Rows[i]["SiglaMunicipio"]
                    , dt.Rows[i]["NomeMunicipio"]));
            }

            xmlAux.Append("</categories>");


            xmlAux.Append(string.Format(@"<dataset seriesName=""Índice de Realização Financeira""  color=""{0}"">", corReal));

            string link = "";

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                {
                    if (indicaTelaObras == "S")
                        link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&CodigoMunicipio={0}"" ", dt.Rows[i]["CodigoMunicipio"]);
                    else
                        link = string.Format(@"link=""F-_top-../../_Projetos/Administracao/ListaContratosEstendidos.aspx?IndicaVigentesAno=S&CodigoMunicipio={0}"" ", dt.Rows[i]["CodigoMunicipio"]);
                }

                xmlAux.Append(string.Format(@"<set label=""Índice de Realização Financeira: {0:n2}%"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["PercentualReal"].ToString()
                    , corReal
                    , link));
            }

            xmlAux.Append("</dataset>");
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
        xml.Append(string.Format(@"<chart imageSave=""1"" caption=""{0}"" adjustDiv=""1"" numVisiblePlot=""10""  scrollHeight=""12""  showLegend=""1"" yAxisMaxValue=""100""
                  BgColor=""F7F7F7"" canvasBgColor=""F7F7F7"" showvalues=""1"" decimals=""2"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" 
                  chartTopMargin=""5"" chartRightMargin=""8"" chartBottomMargin=""2"" chartLeftMargin=""4"" inThousandSeparator=""."" thousandSeparator=""."" inDecimalSeparator="","" decimalSeparator="","" 
                  showShadow=""0"" {1} showBorder=""0"" baseFontSize=""{2}"" slantLabels=""1"" labelDisplay=""ROTATE"" numberSuffix=""%"" placeValuesInside=""0"" >"
                        , titulo
                        , usarGradiente + usarBordasArredondadas + exportar
                        , fonte));

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

    public string getGraficoEvolucaoContratosPorFase(DataTable dt, string titulo, int fonte)
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
                xmlAux.Append(string.Format(@"<category label=""{0}""/>", dt.Rows[i]["MesAno"]));

            }

            xmlAux.Append("</categories>");

            string link = "";

            xmlAux.Append(string.Format(@"<dataset seriesName=""Concluídos""  color=""27DC79"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""Concluídos"" value=""{0}"" color=""27DC79"" {1}/>", dt.Rows[i]["Concluidos"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Construção""  color=""FFBE5E"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""Construção"" value=""{0}"" color=""FFBE5E"" {1}/>", dt.Rows[i]["EmConstrucao"].ToString()
                   , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Contratação""  color=""18C6EB"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""Contratação"" value=""{0}"" color=""18C6EB"" {1}/>", dt.Rows[i]["EmContratacao"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Aguardando OS""  color=""FFFFA4"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""Aguardando OS"" value=""{0}"" color=""FFFFA4"" {1}/>", dt.Rows[i]["AguardandoOS"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Elaboração de Projetos""  color=""D5C3FC"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""Elaboração de Projetos"" value=""{0}"" color=""D5C3FC"" {1}/>", dt.Rows[i]["EmElaboracao"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Previstos""  color=""7595B5"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""Previstos"" value=""{0}"" color=""7595B5"" {1}/>", dt.Rows[i]["Previstos"].ToString()
                     , link));
            }

            xmlAux.Append("</dataset>");
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
        xml.Append(string.Format(@"<chart imageSave=""1"" caption=""{0}"" adjustDiv=""1"" numVisiblePlot=""10""  scrollHeight=""12""  showLegend=""1"" minimiseWrappingInLegend=""0""
                  BgColor=""F7F7F7"" canvasBgColor=""F7F7F7"" showvalues=""0"" decimals=""0"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" legendNumColumns=""3"" 
                  chartTopMargin=""5"" chartRightMargin=""0"" chartBottomMargin=""1"" chartLeftMargin=""0"" canvasLeftMargin=""1"" canvasRightMargin=""1"" legendPadding=""0""
                  showShadow=""0"" {1} showBorder=""0"" baseFontSize=""{2}"" slantLabels=""1"" labelDisplay=""ROTATE""  decimalPrecision=""0"">"
                        , titulo
                        , usarGradiente + usarBordasArredondadas + exportar
                        , fonte));

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

    public string getGraficoEvolucaoContratosPorStatus(DataTable dt, string titulo, int fonte)
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
                xmlAux.Append(string.Format(@"<category label=""{0}""/>", dt.Rows[i]["MesAno"]));

            }

            xmlAux.Append("</categories>");

            string link = "";

            xmlAux.Append(string.Format(@"<dataset seriesName=""Previstos"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""Previstos"" value=""{0}"" {1}/>", dt.Rows[i]["EmContratacao"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Ativos"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""Ativos"" value=""{0}"" {1}/>", dt.Rows[i]["Ativo"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Encerrados"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""Encerrados"" value=""{0}"" {1}/>", dt.Rows[i]["Encerrado"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");
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
        xml.Append(string.Format(@"<chart imageSave=""1"" caption=""{0}"" adjustDiv=""1"" numVisiblePlot=""10""  scrollHeight=""12""  showLegend=""1"" minimiseWrappingInLegend=""0""
                  BgColor=""F7F7F7"" canvasBgColor=""F7F7F7"" showvalues=""0"" decimals=""0"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" legendNumColumns=""3"" 
                  chartTopMargin=""5"" chartRightMargin=""0"" chartBottomMargin=""1"" chartLeftMargin=""0"" canvasLeftMargin=""1"" canvasRightMargin=""1"" legendPadding=""0""
                  showShadow=""0"" {1} showBorder=""0"" baseFontSize=""{2}"" slantLabels=""1"" labelDisplay=""ROTATE""  decimalPrecision=""0"">"
                        , titulo
                        , usarGradiente + usarBordasArredondadas + exportar
                        , fonte));

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

    public string getGraficoEvolucaoRealizacaoContratos(DataTable dt, string titulo, int fonte)
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
                xmlAux.Append(string.Format(@"<category label=""{0}""/>", dt.Rows[i]["MesAno"]));

            }

            xmlAux.Append("</categories>");


            xmlAux.Append(string.Format(@"<dataset seriesName=""Índice de Realização Financeira""  color=""{0}"">", corReal));


            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""Índice de Realização Financeira: {0:n2}%"" value=""{0}"" color=""{1}""/>", dt.Rows[i]["PercentualReal"].ToString()
                    , corReal));
            }

            xmlAux.Append("</dataset>");
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
        xml.Append(string.Format(@"<chart imageSave=""1"" caption=""{0}"" adjustDiv=""1"" numVisiblePlot=""10""  scrollHeight=""12""  showLegend=""1"" yAxisMaxValue=""100""
                  BgColor=""F7F7F7"" canvasBgColor=""F7F7F7"" showvalues=""1"" decimals=""2"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" 
                  chartTopMargin=""5"" chartRightMargin=""8"" chartBottomMargin=""2"" chartLeftMargin=""4""  inThousandSeparator=""."" thousandSeparator=""."" inDecimalSeparator="","" decimalSeparator="","" 
                  showShadow=""0"" {1} showBorder=""0"" baseFontSize=""{2}"" slantLabels=""1"" labelDisplay=""ROTATE""  numberSuffix=""%"">"
                        , titulo
                        , usarGradiente + usarBordasArredondadas + exportar
                        , fonte));

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

    public string getGraficoContratosPorStatus(DataTable dt, string titulo, int fonte, bool linkLista)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();
        int i = 0;

        try
        {
            string link = "";

            if (dt.Rows[i]["Concluidos"].ToString() != "0")
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&Situacao={0}"" ", Server.UrlEncode("Concluídos"));

                xmlAux.Append(string.Format(@"<set label=""Concluídos"" value=""{0}"" color=""18C6EB"" {1}/>", dt.Rows[i]["Concluidos"].ToString()
                    , link));
            }

            if (dt.Rows[i]["EmConstrucao"].ToString() != "0")
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&Situacao={0}"" ", Server.UrlEncode("Em Construção"));

                xmlAux.Append(string.Format(@"<set label=""Construção"" value=""{0}"" color=""18C6EB"" {1}/>", dt.Rows[i]["EmConstrucao"].ToString()
                    , link));
            }

            if (dt.Rows[i]["EmContratacao"].ToString() != "0")
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&Situacao={0}"" ", Server.UrlEncode("Em Contratação"));

                xmlAux.Append(string.Format(@"<set label=""Contratação"" value=""{0}"" color=""18C6EB"" {1}/>", dt.Rows[i]["EmContratacao"].ToString()
                    , link));
            }

            if (dt.Rows[i]["AguardandoOS"].ToString() != "0")
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&Situacao={0}"" ", Server.UrlEncode("Aguardando OS"));

                xmlAux.Append(string.Format(@"<set label=""Aguardando OS"" value=""{0}"" color=""18C6EB"" {1}/>", dt.Rows[i]["AguardandoOS"].ToString()
                    , link));
            }

            if (dt.Rows[i]["EmElaboracao"].ToString() != "0")
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&Situacao={0}"" ", Server.UrlEncode("Em Elaboração/Aprovação de Projetos"));

                xmlAux.Append(string.Format(@"<set label=""Elaboração de Projetos"" value=""{0}"" color=""18C6EB"" {1}/>", dt.Rows[i]["EmElaboracao"].ToString()
                    , link));
            }

            if (dt.Rows[i]["Previstos"].ToString() != "0")
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&Situacao={0}"" ", Server.UrlEncode("Previstos"));

                xmlAux.Append(string.Format(@"<set label=""Previstos"" value=""{0}"" color=""18C6EB"" {1}/>", dt.Rows[i]["Previstos"].ToString()
                    , link));
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
        xml.Append(string.Format(@"<chart imageSave=""1"" caption=""{0}"" adjustDiv=""1"" showLegend=""0""
                  BgColor=""F7F7F7"" canvasBgColor=""F7F7F7"" showvalues=""1"" decimals=""0"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" 
                  chartTopMargin=""5"" chartRightMargin=""4"" chartBottomMargin=""2"" chartLeftMargin=""4"" canvasLeftMargin=""2"" canvasRightMargin=""2"" 
                  showShadow=""0"" {1} showBorder=""0"" baseFontSize=""{2}""  decimalPrecision=""0"">"
                        , titulo
                        , usarGradiente + usarBordasArredondadas + exportar
                        , fonte));

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

    public string getGraficoContratosPorValoresTipoContrato(DataTable dt, string titulo, int fonte, bool linkLista)
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
                xmlAux.Append(string.Format(@"<category label=""{0}"" toolText=""{0}"" />", dt.Rows[i]["TipoContrato"]));

            }

            xmlAux.Append("</categories>");

            string link = "";

            DataSet dsTemp = getParametrosSistema("labelPrevistoParcelaContrato");

            string labelValorMedido = "Valor Medido";

            if (DataSetOk(dsTemp) && DataTableOk(dsTemp.Tables[0]))
            {
                if (dsTemp.Tables[0].Rows[0]["labelPrevistoParcelaContrato"].ToString() != "")
                    labelValorMedido = dsTemp.Tables[0].Rows[0]["labelPrevistoParcelaContrato"].ToString();
            }

            xmlAux.Append(string.Format(@"<dataset seriesName=""{0}"" >", labelValorMedido));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""{3}"" toolText=""{3}: {2:n2}"" value=""{0}"" {1}/>", dt.Rows[i]["ValorMedido"].ToString()
                    , link
                    , dt.Rows[i]["ValorMedido"].ToString() != "" ? double.Parse(dt.Rows[i]["ValorMedido"].ToString()) : 0
                    , labelValorMedido));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Saldo Contratual"" >"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""Saldo Contratual"" toolText=""Saldo Contratual: {2:n2}"" value=""{0}"" {1}/>", dt.Rows[i]["SaldoContratual"].ToString()
                    , link
                    , dt.Rows[i]["SaldoContratual"].ToString() != "" ? double.Parse(dt.Rows[i]["SaldoContratual"].ToString()) : 0));
            }

            xmlAux.Append("</dataset>");
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
        xml.Append(string.Format(@"<chart imageSave=""1"" caption=""{0}"" adjustDiv=""1"" numVisiblePlot=""10""  scrollHeight=""12""  showLegend=""1""
                  BgColor=""F7F7F7"" canvasBgColor=""F7F7F7"" showvalues=""0"" decimals=""2"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" 
                  chartTopMargin=""5"" chartRightMargin=""4"" chartBottomMargin=""2"" chartLeftMargin=""4"" canvasLeftMargin=""2"" canvasRightMargin=""2"" legendPadding=""4""
                  showShadow=""0"" {1} showBorder=""0"" baseFontSize=""{2}"" yAxisName=""Valor Contratado""  
                  inThousandSeparator=""."" thousandSeparator=""."" inDecimalSeparator="","" decimalSeparator="","" >"
                        , titulo
                        , usarGradiente + usarBordasArredondadas + exportar
                        , fonte));

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

    public string getGraficoContratosPorTipoContratacao(DataTable dt, string titulo, int fonte, bool linkLista)
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
                xmlAux.Append(string.Format(@"<category label=""{0}"" toolText=""{0}"" />", dt.Rows[i]["TipoObra"]));

            }

            xmlAux.Append("</categories>");

            string link = "";

            xmlAux.Append(string.Format(@"<dataset seriesName=""Concluídos""  color=""27DC79"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&TipoObra={0}&Situacao={1}"" ", Server.UrlEncode(dt.Rows[i]["TipoObra"].ToString()), Server.UrlEncode("Concluídos"));

                xmlAux.Append(string.Format(@"<set label=""Concluídos"" value=""{0}"" color=""27DC79"" {1}/>", dt.Rows[i]["Concluidos"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Construção""  color=""FFBE5E"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&TipoObra={0}&Situacao={1}"" ", Server.UrlEncode(dt.Rows[i]["TipoObra"].ToString()), Server.UrlEncode("Em Construção"));

                xmlAux.Append(string.Format(@"<set label=""Construção"" value=""{0}"" color=""FFBE5E"" {1}/>", dt.Rows[i]["EmConstrucao"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Contratação""  color=""18C6EB"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&TipoObra={0}&Situacao={1}"" ", Server.UrlEncode(dt.Rows[i]["TipoObra"].ToString()), Server.UrlEncode("Em Contratação"));

                xmlAux.Append(string.Format(@"<set label=""Contratação"" value=""{0}"" color=""18C6EB"" {1}/>", dt.Rows[i]["EmContratacao"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Aguardando OS""  color=""FFFFA4"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&TipoObra={0}&Situacao={1}"" ", Server.UrlEncode(dt.Rows[i]["TipoObra"].ToString()), Server.UrlEncode("Aguardando OS"));

                xmlAux.Append(string.Format(@"<set label=""Aguardando OS"" value=""{0}"" color=""FFFFA4"" {1}/>", dt.Rows[i]["AguardandoOS"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Elaboração de Projetos""  color=""D5C3FC"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&TipoObra={0}&Situacao={1}"" ", Server.UrlEncode(dt.Rows[i]["TipoObra"].ToString()), Server.UrlEncode("Em Elaboração/Aprovação de Projetos"));

                xmlAux.Append(string.Format(@"<set label=""Elaboração de Projetos"" value=""{0}"" color=""D5C3FC"" {1}/>", dt.Rows[i]["EmElaboracao"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");

            xmlAux.Append(string.Format(@"<dataset seriesName=""Previstos""  color=""7595B5"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../ListaObras.aspx?IndicaObraServico=Obra&IndicaVigentesAno=S&TipoObra={0}&Situacao={1}"" ", Server.UrlEncode(dt.Rows[i]["TipoObra"].ToString()), Server.UrlEncode("Previstos"));

                xmlAux.Append(string.Format(@"<set label=""Previstos"" value=""{0}"" color=""7595B5"" {1}/>", dt.Rows[i]["Previstos"].ToString()
                    , link));
            }

            xmlAux.Append("</dataset>");
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
        xml.Append(string.Format(@"<chart imageSave=""1"" caption=""{0}"" adjustDiv=""1"" numVisiblePlot=""4""  scrollHeight=""12""  showLegend=""1""
                  BgColor=""F7F7F7"" canvasBgColor=""F7F7F7"" showvalues=""0"" decimals=""0"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" 
                  chartTopMargin=""5"" chartRightMargin=""2"" chartBottomMargin=""2"" chartLeftMargin=""2"" canvasLeftMargin=""2"" canvasRightMargin=""2"" legendPadding=""2""
                  showShadow=""0"" {1} showBorder=""0"" baseFontSize=""{2}""  decimalPrecision=""0"">"
                        , titulo
                        , usarGradiente + usarBordasArredondadas + exportar
                        , fonte));

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


    public DataSet getNumerosContratos(int codigoEntidade, int codigoCarteira, int codigoUsuario, int ano, string where)
    {
        string comandoSQL = string.Format(@"            
                           SELECT * FROM {0}.{1}.f_GetNumerosContratos({2}, {3}, {4}, {5})
            ", bancodb, Ownerdb, codigoEntidade, ano, codigoCarteira, codigoUsuario, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getNumerosObra_Tabela1(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"            
                           BEGIN 
	                            DECLARE @tbResumo TABLE(Descricao VarChar(50),
													                            Quantidade numeric,
													                            Percentual Decimal(18, 2),
													                            IndicaSumario Char(1))
													
	                            DECLARE @QuantidadeTotal numeric,
				                              @CodigoEntidade int
				  
	                            SET @CodigoEntidade = {2}
	
	                            SELECT @QuantidadeTotal = ISNULL(SUM(Quantidade), 0) 
	                              FROM dbo.f_GetQuantidadeTotalObrasPorStatus(@CodigoEntidade)
	
	                            INSERT INTO @tbResumo
		                            SELECT 'Total de Obras', @QuantidadeTotal, CASE WHEN @QuantidadeTotal = 0 THEN 0 ELSE 100 END,'S'
	
	                            INSERT INTO @tbResumo
		                            SELECT Status, Quantidade, CASE WHEN @QuantidadeTotal = 0 THEN 0 ELSE ((Quantidade / @QuantidadeTotal) * 100) END,'N'
		                              FROM dbo.f_GetQuantidadeTotalObrasPorStatus(@CodigoEntidade)
	
	                            SELECT Descricao, ISNULL(Quantidade, 0) AS Quantidade, 
                                       ISNULL(Percentual, 0) AS Percentual, IndicaSumario
                                  FROM @tbResumo
	 
                            END
            ", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getNumerosObra_Tabela2(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"            
                      EXEC {0}.{1}.p_NumerosContratosObras {2}    
            ", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getNumerosObra_Tabela3(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"            
                           SELECT * FROM {0}.{1}.f_GetQuantidadeTotalObrasPorSegmento({2})
            ", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getNumerosContratos_Tabela1(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"            
                      EXEC {0}.{1}.p_NumerosContratos {2}   
            ", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getNumerosContratoMaster(int codigoContratoMaster)
    {
        string comandoSQL = string.Format(@"            
                      EXEC {0}.{1}.p_numerosContratoMaster {2} 
            ", bancodb, Ownerdb, codigoContratoMaster);

        return getDataSet(comandoSQL);
    }

    public DataSet getListaInformacoesContratos(int codigoEntidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(@" SELECT * FROM {0}.{1}.f_ctt_GetDetalhesContratos({3},{2}) WHERE TipoPessoa = '{4}' ",
            bancodb, Ownerdb, codigoEntidade, codigoUsuario, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getListaInformacoesObras(int codigoEntidade, int codigoUsuario, int CodigoCarteira)
    {
        string comandoSQL = string.Format(@"EXEC {0}.{1}.p_obr_GetDetalhesObras {3}, {2}",
            bancodb, Ownerdb, codigoEntidade, codigoUsuario);

        return getDataSet(comandoSQL);
    }

    public DataSet getInformacoesObra(int codigoObra, string where)
    {
        string comandoSQL = string.Format(@"
            SELECT o.CodigoObra, p.NomeProjeto, CodigoMunicipioObra, CodigoSegmentoObra, CodigoTipoServico, ItemAtendidoPBA
                 , InicioPrevistoObraPBA, TerminoPrevistoObraPBA, QuantidadeEntregaPrevistaPBA
                 , NumeroTermoCooperacao, NumeroTermoAnuencia, NumeroOficio, IndicaConstrucao, IndicaReforma, IndicaAmpliacao, PrevistoPBA, ObservacaoObra
			     , InicioRepactuado, TerminoRepactuado
              FROM Obra o INNER JOIN 
	               Projeto p ON p.CodigoProjeto = o.CodigoProjeto
             WHERE o.CodigoObra = {2}
               {3}",
            bancodb, Ownerdb, codigoObra, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getInformacoesServico(int codigoServico, string where)
    {
        string comandoSQL = string.Format(@"
            SELECT o.CodigoObra, p.NomeProjeto, CodigoMunicipioObra, NumeroOS, LocalObra, CoordenadaGPS
                 , InicioPrevistoObraPBA, TerminoPrevistoObraPBA, DetalhamentoObra, DataEmissaoOS
                 , LicencaExigida, ObservacaoObra, ISNULL(lp.CodigoProjetoPai, 0) AS CodigoProjetoAssociado
              FROM {0}.{1}.Obra o INNER JOIN 
	               {0}.{1}.Projeto p ON p.CodigoProjeto = o.CodigoProjeto LEFT JOIN
                   {0}.{1}.LinkProjeto lp ON lp.CodigoProjetoFilho = o.CodigoProjeto
             WHERE o.CodigoObra = {2}
               {3}",
            bancodb, Ownerdb, codigoServico, where);

        return getDataSet(comandoSQL);
    }

    public int getCodigoContrato(int codigoProjeto)
    {
        string comandoSQL = string.Format(@"
            SELECT c.CodigoContrato
              FROM {0}.{1}.Contrato c LEFT JOIN
				    {0}.{1}.Pessoa pe ON pe.CodigoPessoa = c.CodigoPessoaContratada LEFT JOIN 
                {0}.{1}.[PessoaEntidade] AS [pen] ON (
			    pen.[CodigoPessoa] = c.[CodigoPessoaContratada]
			    AND pen.codigoEntidade = c.codigoEntidade
                --AND pen.IndicaFornecedor = 'S'
			    ) 
             WHERE c.CodigoProjeto = {2}",
            bancodb, Ownerdb, codigoProjeto);

        int codigoContrato = -1;

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            codigoContrato = int.Parse(ds.Tables[0].Rows[0]["CodigoContrato"].ToString());

        return codigoContrato;
    }

    public int getCodigoObra(int codigoProjeto)
    {
        string comandoSQL = string.Format(@"
            SELECT o.CodigoObra
              FROM {0}.{1}.Obra o
             WHERE o.CodigoProjeto = {2}",
            bancodb, Ownerdb, codigoProjeto);

        int codigoObra = -1;

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            codigoObra = int.Parse(ds.Tables[0].Rows[0]["CodigoObra"].ToString());

        return codigoObra;
    }

    public bool verificaExistenciaNomeTabelaProjeto(int codigoEntidade, string nomeObra, string where)
    {
        string comandoSQL = string.Format(@"
            SELECT 1
              FROM {0}.{1}.Projeto p
             WHERE p.CodigoEntidade = {2}
               AND p.NomeProjeto = '{3}'
               {4}",
            bancodb, Ownerdb, codigoEntidade, nomeObra, where);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            return true;

        return false;
    }

    public bool incluirObra(int codigoEntidade, int codigoUsuario, int codigoUnidadePadrao, string nomeObra, string previsaoPBA
        , string referenciaPBA, int codigoMunicipio, int codigoSegmento, string numeroTermoCooperacao, string numeroTermoAnuencia
        , string numeroOficio, int quantidadeObras, int codigoTipoObra, string indicaConstrucao, string indicaReforma, string indicaAmpliacao
        , string dataInicio, string dataTermino, string observacoes, int codigoTipoProjeto, string InicioRepactuado, string TerminoRepactuado,
        ref int novoCodigoObra, ref string msgErro)
    {
        DataSet ds;
        bool retorno = true;
        try
        {

            comandoSQL = string.Format(@"
             BEGIN
                DECLARE @CodigoObra int
                       ,@CodigoProjeto int
                       ,@CodigoEntidade int
                       ,@CodigoUsuario int
                       ,@NomeObra VarChar(250)
                       ,@PrevistoPBA VarChar(2000)
                       ,@ItemAtendidoPBA VarChar(2000)
                       ,@CodigoMunicipio int
                       ,@CodigoSegmento int
                       ,@NumeroTermoCooperacao VarChar(25)
                       ,@NumeroTermoAnuencia VarChar(25)
                       ,@NumeroOficio VarChar(25)
                       ,@QuantidadeEntregaPrevistaPBA int
                       ,@CodigoTipoObra int
                       ,@IndicaConstrucao Char(1)
                       ,@IndicaReforma Char(1)
                       ,@IndicaAmpliacao Char(1)
                       ,@DataInicio DateTime
                       ,@DataTermino DateTime
                       ,@Observacoes VarChar(2000)
                       ,@CodigoUnidadePadrao int
                       ,@CodigoTipoProjeto int
                       ,@InicioRepactuado DateTime
                       ,@TerminoRepactuado DateTime

                SET @CodigoEntidade                 =  {2}
                SET @CodigoUsuario                  =  {3}
                SET @NomeObra                       = LEFT('{4}',250)
                SET @PrevistoPBA                    = '{5}'
                SET @ItemAtendidoPBA                = '{6}'
                SET @CodigoMunicipio                =  {7}
                SET @CodigoSegmento                 =  {8}
                SET @NumeroTermoCooperacao          = '{9}'
                SET @NumeroTermoAnuencia            = '{10}'
                SET @NumeroOficio                   = '{11}'
                SET @QuantidadeEntregaPrevistaPBA   =  {12}
                SET @CodigoTipoObra                 =  {13}
                SET @IndicaConstrucao               = '{14}'
                SET @IndicaReforma                  = '{15}'
                SET @IndicaAmpliacao                = '{16}'
                SET @DataInicio                     =  {17}
                SET @DataTermino                    =  {18}
                SET @Observacoes                    = '{19}'
                SET @CodigoUnidadePadrao            =  {20}
                SET @CodigoTipoProjeto              =  {21}
                SET @InicioRepactuado               =  {22}
                SET @TerminoRepactuado              =  {23}
               
                INSERT INTO {0}.{1}.Projeto (NomeProjeto, DataInclusao, CodigoUsuarioInclusao, CodigoStatusProjeto, IndicaPrograma, CodigoEntidade
                                           , CodigoGerenteProjeto, CodigoUnidadeNegocio, CodigoTipoProjeto, IndicaRecursosAtualizamTarefas
                                           , IndicaTipoAtualizacaoTarefas, IndicaAprovadorTarefas)
                     VALUES(@NomeObra, GetDate(), @CodigoUsuario, 1, 'N', @CodigoEntidade, @CodigoUsuario, @CodigoUnidadePadrao, @CodigoTipoProjeto, 'N', 'P', 'GP')

                SELECT @CodigoProjeto = SCOPE_IDENTITY()
                             
                INSERT INTO {0}.{1}.Obra(CodigoProjeto, CodigoMunicipioObra, CodigoSegmentoObra, CodigoTipoServico, ItemAtendidoPBA
                                       , InicioPrevistoObraPBA, TerminoPrevistoObraPBA, QuantidadeEntregaPrevistaPBA
                                       , NumeroTermoCooperacao, NumeroTermoAnuencia, NumeroOficio, IndicaConstrucao, IndicaReforma, IndicaAmpliacao, PrevistoPBA, ObservacaoObra
                                       , InicioRepactuado,TerminoRepactuado )
                     VALUES(@CodigoProjeto, @CodigoMunicipio, @CodigoSegmento, @CodigoTipoObra, @ItemAtendidoPBA, @DataInicio, @DataTermino
                           ,@QuantidadeEntregaPrevistaPBA, @NumeroTermoCooperacao, @NumeroTermoAnuencia, @NumeroOficio, @IndicaConstrucao, @IndicaReforma, @IndicaAmpliacao, @PrevistoPBA, @Observacoes
                            , @InicioRepactuado,@TerminoRepactuado)

                SELECT @CodigoObra = SCOPE_IDENTITY()

                SELECT @CodigoObra AS CodigoObra
              END
				", bancodb, Ownerdb
                 , codigoEntidade, codigoUsuario, nomeObra.Replace("'", "''"), previsaoPBA.Replace("'", "''")
                 , referenciaPBA.Replace("'", "''"), codigoMunicipio, codigoSegmento, numeroTermoCooperacao.Replace("'", "''")
                 , numeroTermoAnuencia.Replace("'", "''"), numeroOficio.Replace("'", "''"), quantidadeObras, codigoTipoObra
                 , indicaConstrucao, indicaReforma, indicaAmpliacao, dataInicio, dataTermino, observacoes.Replace("'", "''"), codigoUnidadePadrao, codigoTipoProjeto,
                  InicioRepactuado, TerminoRepactuado);


            ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigoObra = int.Parse(ds.Tables[0].Rows[0]["CodigoObra"].ToString());

            msgErro = "";
            retorno = true;
        }
        catch (Exception ex)
        {
            msgErro = getErroIncluiRegistro(ex.Message);
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaObra(int codigoObra, int codigoUsuario, string nomeObra, string previsaoPBA, string referenciaPBA
        , int codigoMunicipio, int codigoSegmento, string numeroTermoCooperacao, string numeroTermoAnuencia, string numeroOficio
        , int quantidadeObras, int codigoTipoObra, string indicaConstrucao, string indicaReforma, string indicaAmpliacao, string dataInicio, string dataTermino, string observacoes,
        string InicioRepactuado, string TerminoRepactuado, ref string msgErro)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"
             BEGIN
                DECLARE @CodigoProjeto int
                       ,@CodigoObra int
                       ,@CodigoUsuario int
                       ,@NomeObra VarChar(250)
                       ,@PrevistoPBA VarChar(2000)
                       ,@ItemAtendidoPBA VarChar(2000)
                       ,@CodigoMunicipio int
                       ,@CodigoSegmento int
                       ,@NumeroTermoCooperacao VarChar(25)
                       ,@NumeroTermoAnuencia VarChar(25)
                       ,@NumeroOficio VarChar(25)
                       ,@QuantidadeEntregaPrevistaPBA int
                       ,@CodigoTipoObra int
                       ,@IndicaConstrucao Char(1)
                       ,@IndicaReforma Char(1)
                       ,@IndicaAmpliacao Char(1)
                       ,@DataInicio DateTime
                       ,@DataTermino DateTime
                       ,@Observacoes VarChar(2000)
                       ,@InicioRepactuado DateTime
                       ,@TerminoRepactuado DateTime

                SET @CodigoObra                     =  {2}
                SET @CodigoUsuario                  =  {3}
                SET @NomeObra                       = LEFT('{4}',250)
                SET @PrevistoPBA                    = '{5}'
                SET @ItemAtendidoPBA                = '{6}'
                SET @CodigoMunicipio                =  {7}
                SET @CodigoSegmento                 =  {8}
                SET @NumeroTermoCooperacao          = '{9}'
                SET @NumeroTermoAnuencia            = '{10}'
                SET @NumeroOficio                   = '{11}'
                SET @QuantidadeEntregaPrevistaPBA   =  {12}
                SET @CodigoTipoObra                 =  {13}
                SET @IndicaConstrucao               = '{14}'
                SET @IndicaReforma                  = '{15}'
                SET @IndicaAmpliacao                = '{16}'
                SET @DataInicio                     =  {17}
                SET @DataTermino                    =  {18}
                SET @Observacoes                    = '{19}'
                SET @InicioRepactuado               =  {20}
                SET @TerminoRepactuado              =  {21}
               
                SELECT @CodigoProjeto = CodigoProjeto FROM {0}.{1}.Obra WHERE CodigoObra = @CodigoObra

                UPDATE {0}.{1}.Projeto SET NomeProjeto = @NomeObra
                                         , DataUltimaAlteracao = GetDate()
                                         , CodigoUsuarioUltimaAlteracao = @CodigoUsuario
                 WHERE CodigoProjeto = @CodigoProjeto            
                             
                UPDATE {0}.{1}.Obra SET CodigoMunicipioObra = @CodigoMunicipio
                                      , CodigoSegmentoObra = @CodigoSegmento
                                      , CodigoTipoServico = @CodigoTipoObra
                                      , ItemAtendidoPBA = @ItemAtendidoPBA
                                      , InicioPrevistoObraPBA = @DataInicio
                                      , TerminoPrevistoObraPBA = @DataTermino
                                      , QuantidadeEntregaPrevistaPBA = @QuantidadeEntregaPrevistaPBA
                                      , NumeroTermoCooperacao = @NumeroTermoCooperacao
                                      , NumeroTermoAnuencia = @NumeroTermoAnuencia
                                      , NumeroOficio = @NumeroOficio
                                      , IndicaConstrucao = @IndicaConstrucao
                                      , IndicaReforma = @IndicaReforma
                                      , IndicaAmpliacao = @IndicaAmpliacao
                                      , PrevistoPBA = @PrevistoPBA
                                      , ObservacaoObra = @Observacoes
                                      , InicioRepactuado = @InicioRepactuado
                                      , TerminoRepactuado = @TerminoRepactuado
                WHERE CodigoObra = @CodigoObra
              END
				", bancodb, Ownerdb
                 , codigoObra, codigoUsuario, nomeObra.Replace("'", "''"), previsaoPBA.Replace("'", "''")
                 , referenciaPBA.Replace("'", "''"), codigoMunicipio, codigoSegmento, numeroTermoCooperacao.Replace("'", "''")
                 , numeroTermoAnuencia.Replace("'", "''"), numeroOficio.Replace("'", "''"), quantidadeObras, codigoTipoObra
                 , indicaConstrucao, indicaReforma, indicaAmpliacao, dataInicio, dataTermino, observacoes, InicioRepactuado, TerminoRepactuado);

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

    public bool incluirServico(int codigoEntidade, int codigoUsuario, int codigoUnidadePadrao, string nomeObra, string numeroOS
    , string dataEmissaoOS, int codigoMunicipio, int codigoProjeto, string enderecoCompleto, string coordenadasGPS
    , string dataInicio, string dataTermino, string detalhamento, string licencas, string observacoes, int codigoTipoProjeto
    , ref int novoCodigoObra, ref string msgErro)
    {
        DataSet ds;
        bool retorno = true;
        try
        {
            string insertLinkProjeto = "";

            if (codigoProjeto != 0)
            {
                insertLinkProjeto = string.Format(@"INSERT INTO {0}.{1}.LinkProjeto(CodigoProjetoPai, CodigoProjetoFilho, TipoLink, PesoProjetoFilho, AtualizaTarefaLink)
                                                                VALUES ({2}, @CodigoProjeto, 'PJPJ', 100, 'S')", bancodb, Ownerdb, codigoProjeto);
            }

            comandoSQL = string.Format(@"
             BEGIN
                DECLARE @CodigoObra int
                       ,@CodigoProjeto int
                       ,@CodigoEntidade int
                       ,@CodigoUsuario int
                       ,@NomeObra VarChar(255)
                       ,@NumeroOS VarChar(25)
                       ,@DataEmissaoOS DateTime
                       ,@CodigoMunicipio int
                       ,@EnderecoCompleto VarChar(500)
                       ,@CoordenadasGPS VarChar(100)
                       ,@QuantidadeEntregaPrevistaPBA int
                       ,@CodigoTipoServico int
                       ,@CodigoSegmentoObra int
                       ,@DataInicio DateTime
                       ,@DataTermino DateTime
                       ,@DetalhamentoObra VarChar(4000)
                       ,@LicencaExigida VarChar(250)
                       ,@Observacoes VarChar(2000)
                       ,@CodigoUnidadePadrao int
                       ,@CodigoTipoProjeto int

                SET @CodigoEntidade                 =  {2}
                SET @CodigoUsuario                  =  {3}
                SET @NomeObra                       = LEFT('{4}',250)
                SET @NumeroOS                       = '{5}'
                SET @DataEmissaoOS                  =  {6}
                SET @CodigoMunicipio                =  {7}
                SET @EnderecoCompleto               = '{8}'
                SET @CoordenadasGPS                 = '{9}'
                SET @QuantidadeEntregaPrevistaPBA   = 1
                SET @CodigoTipoServico              = 5
                SET @CodigoSegmentoObra             = 4
                SET @DataInicio                     =  {10}
                SET @DataTermino                    =  {11}
                SET @DetalhamentoObra               = '{12}'
                SET @LicencaExigida                 = '{13}'
                SET @Observacoes                    = '{14}'
                SET @CodigoUnidadePadrao            =  {15}
                SET @CodigoTipoProjeto              =  {16}
               
                INSERT INTO {0}.{1}.Projeto (NomeProjeto, DataInclusao, CodigoUsuarioInclusao, CodigoStatusProjeto, IndicaPrograma, CodigoEntidade
                                           , CodigoGerenteProjeto, CodigoUnidadeNegocio, CodigoTipoProjeto, IndicaRecursosAtualizamTarefas
                                           , IndicaTipoAtualizacaoTarefas, IndicaAprovadorTarefas)
                     VALUES(@NomeObra, GetDate(), @CodigoUsuario, 1, 'N', @CodigoEntidade, @CodigoUsuario, @CodigoUnidadePadrao, @CodigoTipoProjeto, 'N', 'P', 'GP')

                SELECT @CodigoProjeto = SCOPE_IDENTITY()

                {17}
                             
                INSERT INTO {0}.{1}.Obra(CodigoProjeto, CodigoMunicipioObra, CodigoSegmentoObra, CodigoTipoServico, NumeroOS
                                       , InicioPrevistoObraPBA, TerminoPrevistoObraPBA, QuantidadeEntregaPrevistaPBA
                                       , DataEmissaoOS, LocalObra, CoordenadaGPS, DetalhamentoObra, LicencaExigida, ObservacaoObra)
                     VALUES(@CodigoProjeto, @CodigoMunicipio, @CodigoSegmentoObra, @CodigoTipoServico, @NumeroOS, @DataInicio, @DataTermino
                           ,@QuantidadeEntregaPrevistaPBA, @DataEmissaoOS, @EnderecoCompleto, @CoordenadasGPS, @DetalhamentoObra, @LicencaExigida, @Observacoes)

                SELECT @CodigoObra = SCOPE_IDENTITY()

                SELECT @CodigoObra AS CodigoObra
              END
				", bancodb, Ownerdb
                 , codigoEntidade, codigoUsuario, nomeObra.Replace("'", "''"), numeroOS.Replace("'", "''"), dataEmissaoOS
                 , codigoMunicipio, enderecoCompleto.Replace("'", "''"), coordenadasGPS.Replace("'", "''")
                 , dataInicio, dataTermino, detalhamento.Replace("'", "''"), licencas.Replace("'", "''"), observacoes.Replace("'", "''")
                 , codigoUnidadePadrao, codigoTipoProjeto, insertLinkProjeto);


            ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigoObra = int.Parse(ds.Tables[0].Rows[0]["CodigoObra"].ToString());

            msgErro = "";
            retorno = true;
        }
        catch (Exception ex)
        {
            msgErro = getErroIncluiRegistro(ex.Message);
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaServico(int codigoObra, int codigoUsuario, string nomeObra, string numeroOS
        , string dataEmissaoOS, int codigoMunicipio, int codigoProjeto, string enderecoCompleto, string coordenadasGPS
        , string dataInicio, string dataTermino, string detalhamento, string licencas, string observacoes, ref string msgErro)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            string insertLinkProjeto = string.Format(@"DELETE FROM {0}.{1}.LinkProjeto WHERE CodigoProjetoFilho = @CodigoProjeto
                                                       ", bancodb, Ownerdb);

            if (codigoProjeto != 0)
            {
                insertLinkProjeto += string.Format(@"INSERT INTO {0}.{1}.LinkProjeto(CodigoProjetoPai, CodigoProjetoFilho, TipoLink, PesoProjetoFilho, AtualizaTarefaLink)
                                                                VALUES ({2}, @CodigoProjeto, 'PJPJ', 100, 'S')", bancodb, Ownerdb, codigoProjeto);
            }

            comandoSQL = string.Format(@"
                 BEGIN

                    DECLARE @CodigoObra int
                       ,@CodigoProjeto int
                       ,@CodigoEntidade int
                       ,@CodigoUsuario int
                       ,@NomeObra VarChar(250)
                       ,@NumeroOS VarChar(25)
                       ,@DataEmissaoOS DateTime
                       ,@CodigoMunicipio int
                       ,@EnderecoCompleto VarChar(500)
                       ,@CoordenadasGPS VarChar(100)
                       ,@QuantidadeEntregaPrevistaPBA int
                       ,@CodigoTipoServico int
                       ,@CodigoSegmentoObra int
                       ,@DataInicio DateTime
                       ,@DataTermino DateTime
                       ,@DetalhamentoObra VarChar(4000)
                       ,@LicencaExigida VarChar(250)
                       ,@Observacoes VarChar(2000)
                       ,@CodigoUnidadePadrao int
                       ,@CodigoTipoProjeto int

                SET @CodigoObra                     =  {2}
                SET @CodigoUsuario                  =  {3}
                SET @NomeObra                       = LEFT('{4}',250)
                SET @NumeroOS                       = '{5}'
                SET @DataEmissaoOS                  =  {6}
                SET @CodigoMunicipio                =  {7}
                SET @EnderecoCompleto               = '{8}'
                SET @CoordenadasGPS                 = '{9}'
                SET @DataInicio                     =  {10}
                SET @DataTermino                    =  {11}
                SET @DetalhamentoObra               = '{12}'
                SET @LicencaExigida                 = '{13}'
                SET @Observacoes                    = '{14}'
                   
                    SELECT @CodigoProjeto = CodigoProjeto FROM {0}.{1}.Obra WHERE CodigoObra = @CodigoObra
    
                    UPDATE {0}.{1}.Projeto SET NomeProjeto = @NomeObra
                                             , DataUltimaAlteracao = GetDate()
                                             , CodigoUsuarioUltimaAlteracao = @CodigoUsuario
                     WHERE CodigoProjeto = @CodigoProjeto  
                          
                    UPDATE {0}.{1}.Obra SET CodigoMunicipioObra = @CodigoMunicipio
                                          , NumeroOS = @NumeroOS
                                          , InicioPrevistoObraPBA = @DataInicio
                                          , TerminoPrevistoObraPBA = @DataTermino
                                          , DataEmissaoOS = @DataEmissaoOS
                                          , LocalObra = @EnderecoCompleto
                                          , CoordenadaGPS = @CoordenadasGPS
                                          , DetalhamentoObra = @DetalhamentoObra
                                          , LicencaExigida = @LicencaExigida
                                          , ObservacaoObra = @Observacoes
                    WHERE CodigoObra = @CodigoObra

                    {15}                    

                  END
    			 ", bancodb, Ownerdb
                  , codigoObra, codigoUsuario, nomeObra.Replace("'", "''"), numeroOS.Replace("'", "''"), dataEmissaoOS
                  , codigoMunicipio, enderecoCompleto.Replace("'", "''"), coordenadasGPS.Replace("'", "''")
                  , dataInicio, dataTermino, detalhamento.Replace("'", "''"), licencas.Replace("'", "''"), observacoes.Replace("'", "''")
                  , insertLinkProjeto);

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

    public bool excluiObra(int codigoObra, int codigoUsuario, ref string msgErro)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            comandoSQL = string.Format(@"
             BEGIN
                DECLARE @CodigoProjeto int
                       ,@CodigoObra int
                       ,@CodigoUsuario int
                       
                SET @CodigoObra                     =  {2}
                SET @CodigoUsuario                  =  {3}
                               
                SELECT @CodigoProjeto = CodigoProjeto FROM {0}.{1}.Obra WHERE CodigoObra = @CodigoObra

                UPDATE {0}.{1}.Projeto SET DataExclusao = GetDate()
                                         , CodigoUsuarioExclusao = @CodigoUsuario
                 WHERE CodigoProjeto = @CodigoProjeto            
                             
                UPDATE {0}.{1}.Obra SET DataExclusao = GetDate()
                                      , CodigoUsuarioExclusao = @CodigoUsuario
                 WHERE CodigoObra = @CodigoObra
              END
				", bancodb, Ownerdb
                 , codigoObra, codigoUsuario);

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

    #region Visão EPC

    public DataSet getContratoEPC(int codigoContrato, char indicaContratoMaster, char periodicidade)
    {
        string comandoSQL = string.Format(@"            
                           EXEC {0}.{1}.p_curva_S_PagamentosContrato  {2}, '{3}', '{4}'
            ", bancodb, Ownerdb, codigoContrato, indicaContratoMaster, periodicidade);


        return getDataSet(comandoSQL);
    }

    public DataSet getContratoFilhosEPC(int codigoContratoMaster)
    {
        string comandoSQL = string.Format(@"            
                            SELECT DescricaoContratoAgrupado, CodigoContrato
                              FROM {0}.{1}.AgrupamentoContratoMaster
                             WHERE CodigoContratoMaster = {2}
                             ORDER BY DescricaoContratoAgrupado
            ", bancodb, Ownerdb, codigoContratoMaster);


        return getDataSet(comandoSQL);
    }

    public string getGraficoCurvaS_EPC(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot, string periodicidade)
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

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""2"" chartLeftMargin=""5"" chartRightMargin=""1"" chartTopMargin=""8"" canvasBgColor=""F7F7F7"" canvasLeftMargin=""15"" canvasRightMargin=""20"" canvasBottomMargin=""2""
                                    numDivLines=""{4}"" showvalues=""0""  numVisiblePlot=""{5}"" scrollToEnd=""1"" showBorder=""0"" BgColor=""F7F7F7"" slantLabels=""1"" labelDisplay=""NONE"" rotateLabels=""1"" labelPadding=""0""
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
                xml.Append(string.Format(@"<category label=""{0}"" />", dt.Rows[i]["DescricaoPeriodo"].ToString()));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));
        xml.Append(string.Format(@"<dataset seriesName=""Valor Pago"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorPago"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                string dataRef = dt.Rows[i]["DataReferencia"].ToString();

                if (dataRef != "")
                {
                    int anoRef = DateTime.Parse(dataRef).Year;
                    int mesRef = DateTime.Parse(dataRef).Month;
                    int anoAtual = DateTime.Now.Year;
                    int mesAtual = DateTime.Now.Month;

                    bool mostraReg = false;

                    if (periodicidade == "A")
                    {
                        if (anoAtual >= anoRef)
                            mostraReg = true;
                        else
                            mostraReg = false;
                    }
                    else if (periodicidade == "S")
                    {
                        if (anoAtual > anoRef)
                            mostraReg = true;
                        else
                        {
                            int semestreRef = mesRef <= 6 ? 0 : 6;
                            int semestreAtual = mesAtual <= 6 ? 0 : 6;

                            if (anoAtual == anoRef && semestreRef == semestreAtual)
                                mostraReg = true;
                            else
                                mostraReg = false;
                        }
                    }
                    else
                    {
                        if (anoAtual > anoRef)
                            mostraReg = true;
                        else
                        {
                            if (anoAtual == anoRef && mesAtual >= mesRef)
                                mostraReg = true;
                            else
                                mostraReg = false;
                        }
                    }

                    if (mostraReg)
                        xml.Append(string.Format(@"<set value=""{0}"" toolText=""Valor Pago(Acumulado): {1}""/> ", valor == "" ? "0" : valor, displayValue));
                }
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        DataSet dsTemp = getParametrosSistema("labelPrevistoParcelaContrato");

        string labelValorMedido = "Valor Medido";

        if (DataSetOk(dsTemp) && DataTableOk(dsTemp.Tables[0]))
        {
            if (dsTemp.Tables[0].Rows[0]["labelPrevistoParcelaContrato"].ToString() != "")
                labelValorMedido = dsTemp.Tables[0].Rows[0]["labelPrevistoParcelaContrato"].ToString();
        }

        xml.Append(string.Format(@"<dataset seriesName=""{0}"" showValues=""0"">", labelValorMedido));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorPrevisto"].ToString();
                string dataRef = dt.Rows[i]["DataReferencia"].ToString();
                char dashed = '0';

                if (dataRef != "" && DateTime.Parse(dataRef) > DateTime.Now)
                {
                    dashed = '1';
                }

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""{3}(Acumulado): {1}"" dashed=""{2}""/> ", valor == "" ? "0" : valor, displayValue, dashed, labelValorMedido));
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

    public string getGraficoCurvaS_PrevisaoOrcamentaria(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot, string periodicidade)
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

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""2"" chartLeftMargin=""5"" chartRightMargin=""1"" chartTopMargin=""8"" canvasBgColor=""F7F7F7"" canvasLeftMargin=""15"" canvasRightMargin=""20"" canvasBottomMargin=""2""
                                    numDivLines=""{4}"" showvalues=""0""  numVisiblePlot=""{5}"" scrollToEnd=""1"" showBorder=""0"" BgColor=""F7F7F7"" slantLabels=""1"" labelDisplay=""NONE"" rotateLabels=""1"" labelPadding=""0""
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
                xml.Append(string.Format(@"<category label=""{0}"" />", dt.Rows[i]["DescricaoPeriodo"].ToString()));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));
        xml.Append(string.Format(@"<dataset seriesName=""Realizado"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorPago"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                string dataRef = dt.Rows[i]["DataReferencia"].ToString();

                if (dataRef != "")
                {
                    int anoRef = DateTime.Parse(dataRef).Year;
                    int mesRef = DateTime.Parse(dataRef).Month;
                    int anoAtual = DateTime.Now.Year;
                    int mesAtual = DateTime.Now.Month;

                    bool mostraReg = false;

                    if (periodicidade == "A")
                    {
                        if (anoAtual >= anoRef)
                            mostraReg = true;
                        else
                            mostraReg = false;
                    }
                    else if (periodicidade == "S")
                    {
                        if (anoAtual > anoRef)
                            mostraReg = true;
                        else
                        {
                            int semestreRef = mesRef <= 6 ? 0 : 6;
                            int semestreAtual = mesAtual <= 6 ? 0 : 6;

                            if (anoAtual == anoRef && semestreRef == semestreAtual)
                                mostraReg = true;
                            else
                                mostraReg = false;
                        }
                    }
                    else
                    {
                        if (anoAtual > anoRef)
                            mostraReg = true;
                        else
                        {
                            if (anoAtual == anoRef && mesAtual >= mesRef)
                                mostraReg = true;
                            else
                                mostraReg = false;
                        }
                    }

                    if (mostraReg)
                        xml.Append(string.Format(@"<set value=""{0}"" toolText=""Realizado(Acumulado): {1}""/> ", valor == "" ? "0" : valor, displayValue));
                }
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Previsto"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorPrevisto"].ToString();
                string dataRef = dt.Rows[i]["DataReferencia"].ToString();
                char dashed = '0';

                if (dataRef != "" && DateTime.Parse(dataRef) > DateTime.Now)
                {
                    dashed = '1';
                }

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Previsto(Acumulado): {1}"" dashed=""{2}""/> ", valor == "" ? "0" : valor, displayValue, dashed));
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

    public string getGraficoCurvaS_PainelGerenciamento(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot, string periodicidade)
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
                                    numDivLines=""{4}"" showvalues=""0""  numVisiblePlot=""{5}"" scrollToEnd=""1"" showBorder=""0"" BgColor=""F7F7F7"" labelPadding=""0"" numberSuffix=""%""
                                    inDecimalSeparator="","" scrollHeight=""{3}"" showLegend=""1""  inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" slantLabels=""1"" labelDisplay=""ROTATE"" >", usarGradiente + usarBordasArredondadas + exportar
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
        xml.Append(string.Format(@"<dataset seriesName=""Previsto"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["PercentualPrevistoAcum"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                string dataRef = dt.Rows[i]["DataReferencia"].ToString();

                if (dataRef != "")
                {
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

                    xml.Append(string.Format(@"<set value=""{0}"" toolText=""Previsto(Acumulado): {1}%"" dashed=""{2}""/> ", valor == "" ? "" : valor, displayValue, dashed));
                }
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));


        xml.Append(string.Format(@"<dataset seriesName=""Realizado"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["PercentualRealAcum"].ToString();


                string dataRef = dt.Rows[i]["DataReferencia"].ToString();

                if (dataRef != "")
                {
                    int anoRef = DateTime.Parse(dataRef).Year;
                    int mesRef = DateTime.Parse(dataRef).Month;
                    int anoAtual = DateTime.Now.Year;
                    int mesAtual = DateTime.Now.Month;

                    bool mostraReg = false;

                    if (periodicidade == "A")
                    {
                        if (anoAtual >= anoRef)
                            mostraReg = true;
                        else
                            mostraReg = false;
                    }
                    else if (periodicidade == "S")
                    {
                        if (anoAtual > anoRef)
                            mostraReg = true;
                        else
                        {
                            int semestreRef = mesRef <= 6 ? 0 : 6;
                            int semestreAtual = mesAtual <= 6 ? 0 : 6;

                            if (anoAtual == anoRef && semestreRef == semestreAtual)
                                mostraReg = true;
                            else
                                mostraReg = false;
                        }
                    }
                    else
                    {
                        if (anoAtual > anoRef)
                            mostraReg = true;
                        else
                        {
                            if (anoAtual == anoRef && mesAtual >= mesRef)
                                mostraReg = true;
                            else
                                mostraReg = false;
                        }
                    }

                    if (mostraReg)
                    {
                        string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                        xml.Append(string.Format(@"<set value=""{0}"" toolText=""Realizado(Acumulado): {1}%"" /> ", valor == "" ? "" : valor, displayValue));
                    }
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

    public string getGraficoCurvaS_Contratos(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot, string periodicidade)
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
                                    numDivLines=""{4}"" showvalues=""0""  numVisiblePlot=""{5}"" scrollToEnd=""1"" showBorder=""0"" BgColor=""F7F7F7"" labelDisplay=""NONE"" labelPadding=""0""
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
        xml.Append(string.Format(@"<dataset seriesName=""Valor Previsto"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorMedido"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                string dataRef = dt.Rows[i]["DataReferencia"].ToString();

                if (dataRef != "")
                {
                    int anoRef = DateTime.Parse(dataRef).Year;
                    int mesRef = DateTime.Parse(dataRef).Month;
                    int anoAtual = DateTime.Now.Year;
                    int mesAtual = DateTime.Now.Month;

                    bool mostraReg = false;

                    if (periodicidade == "A")
                    {
                        if (anoAtual >= anoRef)
                            mostraReg = true;
                        else
                            mostraReg = false;
                    }
                    else if (periodicidade == "S")
                    {
                        if (anoAtual > anoRef)
                            mostraReg = true;
                        else
                        {
                            int semestreRef = mesRef <= 6 ? 0 : 6;
                            int semestreAtual = mesAtual <= 6 ? 0 : 6;

                            if (anoAtual == anoRef && semestreRef == semestreAtual)
                                mostraReg = true;
                            else
                                mostraReg = false;
                        }
                    }
                    else
                    {
                        if (anoAtual > anoRef)
                            mostraReg = true;
                        else
                        {
                            if (anoAtual == anoRef && mesAtual >= mesRef)
                                mostraReg = true;
                            else
                                mostraReg = false;
                        }
                    }

                    if (mostraReg)
                        xml.Append(string.Format(@"<set value=""{0}"" toolText=""Valor Previsto(Acumulado): {1}""/> ", valor == "" ? "0" : valor, displayValue));
                }
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));



        xml.Append(string.Format(@"<dataset seriesName=""Valor Medido"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorPrevisto"].ToString();
                string dataRef = dt.Rows[i]["DataReferencia"].ToString();
                char dashed = '0';

                if (dataRef != "" && DateTime.Parse(dataRef) > DateTime.Now)
                {
                    dashed = '1';
                }

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Valor Medido(Acumulado): {1}"" dashed=""{2}""/> ", valor == "" ? "0" : valor, displayValue, dashed));
            }
        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Valor Medido Reajustado"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorPrevistoReajustado"].ToString();
                string dataRef = dt.Rows[i]["DataReferencia"].ToString();
                char dashed = '0';

                if (dataRef != "" && DateTime.Parse(dataRef) > DateTime.Now)
                {
                    dashed = '1';
                }

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Valor Medido Reajustado(Acumulado): {1}"" dashed=""{2}""/> ", valor == "" ? "0" : valor, displayValue, dashed));
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

    #region Boletins

    public bool incluiUsuariosBoletinsUnidade(int codigoUnidade, int[] usuarios)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            string comandoInsert = "";

            foreach (int codigoUsuario in usuarios)
            {
                comandoInsert += string.Format(@"INSERT INTO {0}.{1}.Tabela(CodigoUnidade, CodigoUsuario)
                                                        VALUES({2}, {3})", bancodb, Ownerdb, codigoUnidade, codigoUsuario);
            }

            comandoSQL = string.Format(@"
                    DELETE {0}.{1}.Tabela 
                     WHERE CodigoUnidade = {2}
                    {3}
                    
                    ", bancodb, Ownerdb, codigoUnidade, comandoInsert);

            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    #endregion

    #region Auditoria

    public DataSet getListaAuditoria(string where, string expressaoFiltro)
    {
        string comandoSQL = string.Format(@"
               DECLARE @Role VARCHAR(255)

               SET @Role = '{3}'          
                    
               SELECT ta.ID, ta.DATA_OPERACAO, ta.USUARIO, ta.MAQUINA, ta.TABELA, ta.OPERACAO
			            ,CASE ta.OPERACAO WHEN 'S' THEN 'Seleção'
												            WHEN 'I' THEN 'Inclusão'
												            WHEN 'U' THEN 'Atualização' 
												            WHEN 'D' THEN 'Exclusão'
												            ELSE '' END AS TipoOperacao 
	            FROM {0}.{1}.TB_AUDIT ta
             WHERE 1 = 1
               {2}     
             ORDER BY ta.DATA_OPERACAO DESC
            ", bancodb, Ownerdb, where, expressaoFiltro);

        return getDataSet(comandoSQL);
    }

    public DataSet getInformacoesCampoAlteradoAuditoria(int codigo, string where)
    {
        string comandoSQL = string.Format(@"            
               SELECT ta.ID, DATA_OPERACAO AS Data, CONVERT(VarChar(13), ta.DATA_OPERACAO, 103) AS DataT, ta.USUARIO, ta.MAQUINA, ta.TABELA, ta.OPERACAO
			            ,CASE ta.OPERACAO WHEN 'S' THEN 'Seleção'
												            WHEN 'I' THEN 'Inclusão'
												            WHEN 'U' THEN 'Atualização' 
												            WHEN 'D' THEN 'Exclusão'
												            ELSE '' END AS TipoOperacao 
			            ,ta.NEW_DATA, ta.OLD_DATA
	            FROM {0}.{1}.TB_AUDIT ta
             WHERE ta.ID = {2}      
               {3}     
            ", bancodb, Ownerdb, codigo, where);

        return getDataSet(comandoSQL);
    }


    public DataSet getListaAuditoriaTarefaCronograma(string where)
    {
        string comandoSQL = string.Format(@"            
                select top 100
                 ltcp.ID
                ,ltcp.[DataOperacao]
                ,ltcp.[UsuarioOperacao]
                ,CASE ltcp.[Operacao] WHEN 'I' THEN 'Inclusão'
                WHEN 'U' THEN 'Atualização' 
                WHEN 'D' THEN 'Exclusão'
                ELSE '' END AS [Operacao]           
                ,ltcp.[Maquina]
                ,ltcp.[CodigoCronogramaProjeto]
                ,ltcp.[CodigoTarefa]
                ,ltcp.[NomeTarefa]
                ,ltcp.[Duracao]
                ,ltcp.[Inicio]
                ,ltcp.[Termino]
                ,ltcp.[Predecessoras]
                ,ltcp.[PercentualFisicoConcluido]
                ,ltcp.[DuracaoReal]
                ,ltcp.[InicioReal]
                ,ltcp.[TerminoReal]
                ,ltcp.[TrabalhoReal]
                ,ltcp.[CustoReal]
                ,ltcp.[InicioLB]
                ,ltcp.[TerminoLB]
                ,ltcp.[DuracaoLB]
                ,ltcp.[TrabalhoLB]
                ,ltcp.[oldNomeTarefa]
                ,ltcp.[oldDuracao]
                ,ltcp.[oldInicio]
                ,ltcp.[oldTermino]
                ,ltcp.[oldPredecessoras]
                ,ltcp.[oldPercentualFisicoConcluido]
                ,ltcp.[oldDuracaoReal]
                ,ltcp.[oldInicioReal]
                ,ltcp.[oldTerminoReal]
                ,ltcp.[oldTrabalhoReal]
                ,ltcp.[oldCustoReal]
                ,ltcp.[oldInicioLB]
                ,ltcp.[oldTerminoLB]
                ,ltcp.[oldDuracaoLB]
                ,ltcp.[oldTrabalhoLB]
                ,p.NomeProjeto
                from 
                logTarefaCronogramaProjeto ltcp inner join 
                CronogramaProjeto cp on (ltcp.CodigoCronogramaProjeto = cp.CodigoCronogramaProjeto) inner join
                Projeto p on (p.CodigoProjeto = cp.CodigoProjeto)

             WHERE 1 = 1
               {2}     
             order by p.NomeProjeto, ltcp.id

            ", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    #endregion

    #region TAI04
    public DataSet getTrimestreLiberado(int codigoEntidade)
    {
        string comandoSQL = string.Format(@"            
               select * from [dbo].[f_GetTrimestreOrcamentoLiberado]({2})
            ", bancodb, Ownerdb, codigoEntidade);

        return getDataSet(comandoSQL);
    }
    #endregion

    #region Financeiro

    public DataSet getTarefasCronogramaProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
                  @"SELECT CodigoTarefa, NomeTarefa 
                      FROM {0}.{1}.TarefaCronogramaProjeto tcp INNER JOIN
                           {0}.{1}.CronogramaProjeto cp ON (cp.CodigoCronogramaProjeto = tcp.CodigoCronogramaProjeto
														AND cp.CodigoProjeto = {2}
                                                        AND cp.DataExclusao IS NULL)
                     WHERE tcp.IndicaTarefaResumo = 'N'
                       AND tcp.DataExclusao IS NULL
                       {3}
                     ORDER BY NomeTarefa
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getAnotacoesTarefasCronogramaProjeto(string codigoCronograma, string codigoTarefa, string where)
    {
        string comandoSQL = string.Format(
                  @"SELECT CodigoTarefa, NomeTarefa, Anotacoes
                      FROM {0}.{1}.TarefaCronogramaProjeto tcp 
                     WHERE tcp.CodigoCronogramaProjeto = '{2}'
                       AND tcp.CodigoTarefa = {3}
                       {4}
                     ORDER BY NomeTarefa
               ", bancodb, Ownerdb, codigoCronograma, codigoTarefa, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getRecursosTarefaCronogramaProjeto(int codigoProjeto, int codigoTarefa, string where)
    {
        string comandoSQL = string.Format(
                  @"SELECT DISTINCT rcp.CodigoRecursoProjeto, rcp.NomeRecurso
                      FROM {0}.{1}.RecursoCronogramaProjeto AS rcp INNER JOIN
                           {0}.{1}.AtribuicaoRecursoTarefa AS art ON (art.CodigoRecursoProjeto = rcp.CodigoRecursoProjeto
                                                          AND art.CodigoCronogramaProjeto = rcp.CodigoCronogramaProjeto) INNER JOIN
                           CronogramaProjeto AS cp ON (cp.CodigoCronogramaProjeto = rcp.CodigoCronogramaProjeto)
                    WHERE cp.CodigoProjeto = {2}
                       AND art.CodigoTarefa = {3}
                       {4}
               ", bancodb, Ownerdb, codigoProjeto, codigoTarefa, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getContasAnaliticasEntidade(int codigoEntidade, string tipo, string where)
    {
        string comandoSQL = string.Format(
                  @"SELECT pcfc.CodigoConta,
                           pcfc.DescricaoConta,
                           pcfc.CodigoReservadoGrupoConta
                      FROM {0}.{1}.PlanoContasFluxoCaixa AS pcfc
                     WHERE pcfc.CodigoEntidade = {2}
                       AND pcfc.IndicaContaAnalitica = 'S'
                       --AND pcfc.EntradaSaida = '{3}'
                       {4}
                       ORDER BY pcfc.DescricaoConta ASC
               ", bancodb, Ownerdb, codigoEntidade, tipo, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getEmpenhosFinanceirosProjeto(int codigoProjeto, string where, string orderby)
    {
        string comandoSQL = string.Format(@"            
               BEGIN

	                DECLARE @CodigoProjeto int
					
                    SET @CodigoProjeto = {2}

	                SELECT CodigoLancamentoFinanceiro, lf.CodigoProjeto, prj.NomeProjeto, lf.CodigoAtribuicao
				          ,lf.IndicaDespesaReceita, lf.DataEmpenho, lf.ValorEmpenhado, lf.DataPagamentoRecebimento
				          ,lf.ValorPagamentoRecebimento, lf.CodigoPessoaEmitente, p.NomePessoa AS PessoaEmitente
				          ,lf.NumeroDocFiscal, lf.DataEmissaoDocFiscal, lf.CodigoConta, lf.CodigoWorkflow
				          ,lf.CodigoInstanciaWF, lf.IndicaAprovacaoReprovacao, lf.DataPrevistaPagamentoRecebimento
				          ,lf.HistoricoEmpenho, lf.HistoricoPagamento, lf.HistoricoAprovacaoReprovacao 
				          ,art.CodigoTarefa, art.CodigoRecursoProjeto, ui.NomeUsuario AS UsuarioInclusao
                          ,ua.NomeUsuario AS UsuarioAprovacao, CONVERT(VarChar(10), lf.DataInclusao, 103) AS DataInclusao
                          ,CONVERT(VarChar(10), lf.DataAprovacaoReprovacao, 103) AS DataAprovacaoReprovacao
                          ,CONVERT(VarChar(10), lf.DataImportacao, 103) AS DataImportacao 
                          ,pcf.DescricaoConta                  
		             FROM {0}.{1}.LancamentoFinanceiro lf LEFT JOIN
				          {0}.{1}.Pessoa p ON p.CodigoPessoa = lf.CodigoPessoaEmitente INNER JOIN
				          {0}.{1}.Projeto prj ON prj.CodigoProjeto = lf.CodigoProjeto LEFT JOIN
				          {0}.{1}.AtribuicaoRecursoTarefa art ON art.CodigoAtribuicao = lf.CodigoAtribuicao INNER JOIN
                          {0}.{1}.Usuario ui ON ui.CodigoUsuario = lf.CodigoUsuarioInclusao LEFT JOIN
                          {0}.{1}.Usuario ua ON ua.CodigoUsuario = lf.CodigoUsuarioAprovacaoReprovacao LEFT JOIN
                          {0}.{1}.PlanoContasFluxocaixa pcf ON pcf.CodigoConta = lf.CodigoConta
	                WHERE lf.CodigoProjeto = @CodigoProjeto
                      AND lf.DataExclusao IS NULL			
                      {3}	 
	                  {4}
                END
            ", bancodb, Ownerdb, codigoProjeto, where, orderby);

        return getDataSet(comandoSQL);
    }

    public bool incluiEmpenhoFinanceiroProjeto(int codigoProjeto, int codigoRecurso, int codigoTarefa, string indicaDespesaReceita
        , string dataEmpenho, string valorEmpenhado, string codigoPessoaEmitente, string numeroDocFiscal, string dataEmissaoDocFiscal
        , int codigoUsuario, string historicoEmpenho, string historicoPagamento, string dataPrevistaPagamentoRecebimento, string codigoConta
        , string codigoFontePagadora, string dataPagamentoRecebimento, string valorPagamentoRecebimento, string indicaAprovado, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @CodigoTipoAssociacao int,
                                    @CodigoAtribuicao int,
                                    @CodigoLancamento int				
	
	                        SELECT @CodigoTipoAssociacao = ta.CodigoTipoAssociacao 
	                          FROM {0}.{1}.TipoAssociacao ta
	                         WHERE ta.IniciaisTipoAssociacao = 'PR'
    
                            SELECT @CodigoAtribuicao = CodigoAtribuicao 
                              FROM {0}.{1}.AtribuicaoRecursoTarefa art INNER JOIN
			                       {0}.{1}.CronogramaProjeto cp ON (cp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto
                                                                    AND cp.CodigoProjeto = {2})
                             WHERE art.CodigoTarefa = {4}
                               AND art.CodigoRecursoProjeto = {3}

                            INSERT INTO {0}.{1}.LancamentoFinanceiro
                                        (CodigoProjeto        ,CodigoAtribuicao                 ,IndicaDespesaReceita       ,DataEmpenho
                                        ,ValorEmpenhado       ,CodigoPessoaEmitente             ,NumeroDocFiscal            ,DataEmissaoDocFiscal
                                        ,DataInclusao         ,CodigoUsuarioInclusao            ,HistoricoEmpenho           ,CodigoObjetoAssociado
                                        ,CodigoTipoAssociacao ,DataPrevistaPagamentoRecebimento ,CodigoConta                ,DataPagamentoRecebimento
                                        ,HistoricoPagamento   ,ValorPagamentoRecebimento        ,IndicaAprovacaoReprovacao  ,CodigoUsuarioAprovacaoReprovacao   
                                        ,DataAprovacaoReprovacao, CodigoFonteRecursosFinanceiros)
                                 VALUES({2}                   ,@CodigoAtribuicao                ,'{5}'                ,{6}
                                        ,{7}                  ,{8}                              ,'{9}'                ,{10}
                                        ,GetDate()            ,{11}                             ,'{12}'               ,{2}
                                        ,@CodigoTipoAssociacao,{13}                             ,{14}                 ,{15}
                                        ,'{16}'               ,{17}                             ,'{18}'               ,{19}
                                        ,{20}                 ,{21})

                            SELECT @CodigoLancamento = SCOPE_IDENTITY()

                            SELECT @CodigoLancamento AS CodigoLancamento

                          END
                        ", bancodb, Ownerdb
                         /*{2},@CodigoAtribuicao,'{5}',{6}*/
                         , codigoProjeto,/*@CodigoAtribuicao*/ codigoRecurso, codigoTarefa, indicaDespesaReceita, dataEmpenho
                         , valorEmpenhado.Replace(".", "").Replace(",", "."), codigoPessoaEmitente, numeroDocFiscal.Replace("'", "''"), dataEmissaoDocFiscal
                         , /*data inclusao*/ codigoUsuario, historicoEmpenho.Replace("'", "''") /*codigoObjetoAssociado*/
                         ,/*CodigoTipoAssociacao*/dataPrevistaPagamentoRecebimento, codigoConta, dataPagamentoRecebimento
                         , historicoPagamento.Replace("'", "''"), valorPagamentoRecebimento.Replace(".", "").Replace(",", ".")
                         , indicaAprovado, indicaAprovado == "A" ? codigoUsuario.ToString() : "NULL"
                         , indicaAprovado == "A" ? "GETDATE()" : "NULL", codigoFontePagadora);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["CodigoLancamento"].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaEmpenhoFinanceiroProjeto(int codigoLancamentoFinanceiro, int codigoProjeto, int codigoRecurso, int codigoTarefa, string indicaDespesaReceita
        , string dataEmpenho, string valorEmpenhado, string codigoPessoaEmitente, string numeroDocFiscal, string dataEmissaoDocFiscal
        , int codigoUsuario, string historicoEmpenho, string historicoPagamento, string dataPrevistaPagamentoRecebimento, string dataPagamentoRecebimento
        , string codigoConta, string codigoFontePagadora, string valorPagamentoRecebimento)
    {
        bool retorno = false;

        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoAtribuicao int	
                            SELECT @CodigoAtribuicao = CodigoAtribuicao 
                              FROM {0}.{1}.AtribuicaoRecursoTarefa art INNER JOIN
			                       {0}.{1}.CronogramaProjeto cp ON (cp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto
                                                                    AND cp.CodigoProjeto = {2})
                             WHERE art.CodigoTarefa = {4}
                               AND art.CodigoRecursoProjeto = {3}

                            UPDATE {0}.{1}.LancamentoFinanceiro
                               SET CodigoAtribuicao = @CodigoAtribuicao
                                  ,IndicaDespesaReceita = '{5}'
                                  ,DataEmpenho = {6}
                                  ,ValorEmpenhado = {7}
                                  ,CodigoPessoaEmitente = {8}
                                  ,NumeroDocFiscal = '{9}'
                                  ,DataEmissaoDocFiscal = {10}
                                  ,DataUltimaAlteracao = GetDate()
                                  ,CodigoUsuarioUltimaAlteracao = {11}
                                  ,HistoricoEmpenho = '{12}'
                                  ,DataPrevistaPagamentoRecebimento = {13}
                                  ,CodigoConta = {14}
                                  ,DataPagamentoRecebimento = {16}
                                  ,HistoricoPagamento = '{17}'      
                                  ,ValorPagamentoRecebimento = {18}
                                  ,CodigoFonteRecursosFinanceiros = {19}
                             WHERE CodigoLancamentoFinanceiro = {15}
                        END
                        ", bancodb, Ownerdb
                         , codigoProjeto, codigoRecurso, codigoTarefa, indicaDespesaReceita
                         , dataEmpenho, valorEmpenhado.Replace(".", "").Replace(",", "."), codigoPessoaEmitente, numeroDocFiscal.Replace("'", "''"), dataEmissaoDocFiscal
                         , codigoUsuario, historicoEmpenho.Replace("'", "''"), dataPrevistaPagamentoRecebimento, codigoConta, codigoLancamentoFinanceiro, dataPagamentoRecebimento, historicoPagamento.Replace("'", "''")
                         , valorPagamentoRecebimento.Replace(".", "").Replace(",", "."), codigoFontePagadora);

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

    public bool atualizaAprovacaoEmpenhoFinanceiroProjeto(int codigoLancamentoFinanceiro, string indicaAprovadoReprovado, int codigoUsuario, string historicoAprovacao)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            UPDATE {0}.{1}.LancamentoFinanceiro
                               SET IndicaAprovacaoReprovacao = '{3}'
                                  ,HistoricoAprovacaoReprovacao = '{4}'
                                  ,CodigoUsuarioAprovacaoReprovacao = {5}
                                  ,DataAprovacaoReprovacao = GetDate()
                             WHERE CodigoLancamentoFinanceiro = {2}
                        END
                        ", bancodb, Ownerdb
                         , codigoLancamentoFinanceiro, indicaAprovadoReprovado, historicoAprovacao.Replace("'", "''"), codigoUsuario);

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

    public bool excluiEmpenhoFinanceiroProjeto(int codigoLancamentoFinanceiro, int codigoUsuario)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                            UPDATE {0}.{1}.LancamentoFinanceiro
                               SET DataExclusao = GetDate()
                                  ,CodigoUsuarioExclusao = {2}
                             WHERE CodigoLancamentoFinanceiro = {3}
                        ", bancodb, Ownerdb
                         , codigoUsuario
                         , codigoLancamentoFinanceiro);

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

    #region Plano de Trabalho

    public DataSet getPlanoTrabalhoProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
                @"SELECT t1.CodigoAcao AS Codigo, t1.CodigoAcaoSuperior AS CodigoPai, t1.EtapaAcaoAtividade
			            ,CASE WHEN t1.CodigoAcao = t1.CodigoAcaoSuperior THEN CONVERT(VARCHAR, t1.NumeroAcao)
						            ELSE CONVERT(VARCHAR, tSup.NumeroAcao) + '.' + CONVERT(VARCHAR, t1.NumeroAcao) END AS Numero
			            ,t1.NomeAcao AS Descricao, t1.Inicio, t1.Termino
			            ,CASE WHEN t1.CodigoAcao = t1.CodigoAcaoSuperior THEN ' - '
			             ELSE t1.IndicaEventoInstitucional END AS Institucional
                        ,t1.CodigoUsuarioResponsavel
			            ,t1.NomeUsuarioResponsavel AS Responsavel, t1.FonteRecurso,
                        CASE WHEN t1.CodigoAcao = t1.CodigoAcaoSuperior THEN right('0000'+CONVERT(VARCHAR, t1.NumeroAcao),4)
                                        ELSE right('0000'+CONVERT(VARCHAR, tSup.NumeroAcao),4) + '.' + right('0000'+CONVERT(VARCHAR, t1.NumeroAcao),4) 
                        END AS ordem
                        ,t1.CodigoReservado

	               FROM {0}.{1}.tai02_AcoesIniciativa t1 INNER JOIN
			            {0}.{1}.tai02_AcoesIniciativa tSup ON tSup.CodigoAcao = t1.CodigoAcaoSuperior
                  WHERE t1.CodigoProjeto = {2}
                    {3}
                 ORDER BY ordem
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getMetasPlanoTrabalhoProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
                      @" SELECT SequenciaRegistro AS CodigoMeta, CodigoAcao AS Codigo, DescricaoProduto AS Meta
                           FROM {0}.{1}.tai02_ProdutosAcoesIniciativa
                          WHERE CodigoProjeto = {2}
                            {3}
                          ORDER BY DescricaoProduto
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getParceriasPlanoTrabalhoProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
                  @"SELECT p.CodigoAcao AS Codigo, ai.CodigoAcaoSuperior AS CodigoObjetoPai, p.SequenciaRegistro AS CodigoParceria
			              ,p.CodigoParceiro, p.NomeParceiro AS Area, p.ProdutoSolicitado AS Elemento 
                      FROM {0}.{1}.tai02_ParceirosIniciativa p INNER JOIN
			               {0}.{1}.tai02_AcoesIniciativa ai ON ai.CodigoAcao = p.CodigoAcao
                     WHERE p.CodigoProjeto = {2}
                       {3}
                     ORDER BY p.NomeParceiro, p.ProdutoSolicitado
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getMarcosPlanoTrabalhoProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
               @"SELECT m.CodigoAcao AS Codigo, ai.CodigoAcaoSuperior AS CodigoObjetoPai, m.SequenciaRegistro AS CodigoMarco
			          , m.NomeMarco AS Marco, m.DataLimitePrevista AS Data
                      , '<b>'+CONVERT(varchar, m.DataLimitePrevista,103)+'</b> - '+m.NomeMarco As MarcoComData 
                  FROM {0}.{1}.tai02_MarcosAcoesIniciativa m INNER JOIN
			           {0}.{1}.tai02_AcoesIniciativa ai ON ai.CodigoAcao = m.CodigoAcao
                 WHERE m.CodigoProjeto = {2}
                   {3}
                 ORDER BY m.DataLimitePrevista, m.NomeMarco
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiAcaoIniciativa(int codigoProjeto, int numeroAcao, string nomeAcao, int codigoResponsavel, int codigoEntidade
        , string fonteRecursos, string codigoReservado, string etapaAcaoAtividade, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                          begin try
                          begin tran
                            DECLARE @NomeEntidade VarChar(100)
                                   ,@NomeUsuario VarChar(60)
                                   ,@CodigoAcao int

                            SELECT @NomeEntidade = NomeUnidadeNegocio
                              FROM {0}.{1}.UnidadeNegocio un
                             WHERE un.CodigoUnidadeNegocio = {6}

                            SELECT @NomeUsuario = NomeUsuario
                              FROM {0}.{1}.Usuario us
                             WHERE us.CodigoUsuario = {5}

                            UPDATE {0}.{1}.tai02_AcoesIniciativa SET NumeroAcao = (NumeroAcao + 1)
                             WHERE NumeroAcao >= {3}
                               AND CodigoProjeto = {2}
                               AND CodigoAcao = CodigoAcaoSuperior
                            
                            INSERT INTO {0}.{1}.tai02_AcoesIniciativa
                                       (CodigoProjeto
                                       ,NumeroAcao
                                       ,NomeAcao
                                       ,CodigoUsuarioResponsavel
                                       ,NomeUsuarioResponsavel
                                       ,CodigoEntidadeResponsavel
                                       ,NomeEntidadeResponsavel
                                       ,FonteRecurso
                                       ,IndicaSemRecurso
                                       ,CodigoReservado
                                       ,EtapaAcaoAtividade)
                                 VALUES
                                       ({2}
                                       ,{3}
                                       ,'{4}'
                                       ,{5}
                                       ,@NomeUsuario
                                       ,{6}
                                       ,@NomeEntidade
                                       ,'{7}'
                                       ,'{8}'
                                       ,'{9}'
                                       ,'{10}')

                            SELECT @CodigoAcao = SCOPE_IDENTITY()

                            UPDATE {0}.{1}.tai02_AcoesIniciativa SET CodigoAcaoSuperior = @CodigoAcao
                             WHERE CodigoAcao = @CodigoAcao

                            SELECT @CodigoAcao AS CodigoAcao
                           Commit Tran
                        End Try
                        Begin Catch
                           Rollback Tran
                            DECLARE @ErrorMessage NVARCHAR(4000);
                            DECLARE @ErrorSeverity INT;
                            DECLARE @ErrorState INT;

                            SELECT @ErrorMessage = ERROR_MESSAGE();
                            SELECT @ErrorSeverity = ERROR_SEVERITY();
                            SELECT @ErrorState = ERROR_STATE();
    
                            RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);

                        End Catch
                        END
                        ", bancodb, Ownerdb
                         , codigoProjeto, numeroAcao, nomeAcao.Replace("'", "''")
                         , codigoResponsavel, codigoEntidade, fonteRecursos, fonteRecursos == "SR" ? "S" : "N"
                         , codigoReservado
                         , etapaAcaoAtividade);

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

    public bool atualizaAcaoIniciativa(int codigoProjeto, int numeroAcao, string nomeAcao, int codigoResponsavel, string fonteRecursos, int codigoAcao, string codigoReservado)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                        begin try
                          begin tran
                            DECLARE @NumeroAtualAcao int
                                   ,@NomeUsuario VarChar(60)

                            SELECT @NomeUsuario = NomeUsuario
                              FROM {0}.{1}.Usuario us
                             WHERE us.CodigoUsuario = {5}

                            SELECT @NumeroAtualAcao = NumeroAcao
                              FROM {0}.{1}.tai02_AcoesIniciativa
                             WHERE CodigoAcao = {8}
                               AND CodigoProjeto = {2}

                            IF (@NumeroAtualAcao > {3})
                                UPDATE {0}.{1}.tai02_AcoesIniciativa SET NumeroAcao = (NumeroAcao + 1)
                                 WHERE NumeroAcao < @NumeroAtualAcao
                                   AND NumeroAcao >= {3}
                                   AND CodigoProjeto = {2}
                                   AND CodigoAcao = CodigoAcaoSuperior
                            ELSE IF (@NumeroAtualAcao < {3})
                                UPDATE {0}.{1}.tai02_AcoesIniciativa SET NumeroAcao = (NumeroAcao - 1)
                                 WHERE NumeroAcao > @NumeroAtualAcao
                                   AND NumeroAcao <= {3}
                                   AND CodigoProjeto = {2}
                                   AND CodigoAcao = CodigoAcaoSuperior

                            UPDATE {0}.{1}.tai02_AcoesIniciativa 
                               SET NumeroAcao = {3}
                                  ,NomeAcao = '{4}'
                                  ,CodigoUsuarioResponsavel = {5}                                  
                                  ,NomeUsuarioResponsavel = @NomeUsuario
                                  ,FonteRecurso = '{6}'
                                  ,IndicaSemRecurso = '{7}'
                                  ,CodigoReservado = '{9}'
                            WHERE CodigoAcao  = {8} 
                              AND CodigoProjeto = {2}

                            -- alterando atividades da ação 
                                 UPDATE {0}.{1}.tai02_AcoesIniciativa 
                                   SET FonteRecurso = '{6}'
                                     , IndicaSemrecurso = '{7}'
                                 WHERE codigoAcaoSuperior = {8} 
                                   AND CodigoProjeto = {2}
                          
                            IF ('{6}' = 'SR')
                            BEGIN
                                DELETE FROM {0}.{1}.CronogramaOrcamentarioAcao  
                                WHERE CodigoProjeto = {2}  
                                  AND CodigoAcao =  {8} 
                            END    
                            Commit Tran
                        End Try
                        Begin Catch
                           Rollback Tran
                            DECLARE @ErrorMessage NVARCHAR(4000);
                            DECLARE @ErrorSeverity INT;
                            DECLARE @ErrorState INT;

                            SELECT @ErrorMessage = ERROR_MESSAGE();
                            SELECT @ErrorSeverity = ERROR_SEVERITY();
                            SELECT @ErrorState = ERROR_STATE();
    
                            RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);

                        End Catch

                        END
                        ", bancodb, Ownerdb
                         , codigoProjeto, numeroAcao, nomeAcao.Replace("'", "''")
                         , codigoResponsavel, fonteRecursos, fonteRecursos == "SR" ? "S" : "N"
                         , codigoAcao, codigoReservado);

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

    //    public bool excluiAcaoIniciativa(int codigoProjeto, int codigoAcao)
    //    {
    //        bool retorno = false;

    //            comandoSQL = string.Format(@"
    //                      BEGIN
    //                        Begin Try
    //                            Begin Tran
    //                            DECLARE @NumeroAtualAcao int
    //
    //                            SELECT @NumeroAtualAcao = NumeroAcao
    //                              FROM {0}.{1}.tai02_AcoesIniciativa
    //                             WHERE CodigoAcao = {3}
    //                               AND CodigoProjeto = {2}
    //
    //                            --DELETE {0}.{1}.tai02_ProdutosAcoesIniciativa
    //                            --WHERE CodigoAcao = {3}
    //                            --  AND CodigoProjeto = {2} 
    //
    //                            DELETE FROM {0}.{1}.tai02_AcoesIniciativa                                
    //                                  WHERE CodigoAcao = {3}                              
    //                                    AND CodigoProjeto = {2}
    //                            
    //                            UPDATE {0}.{1}.tai02_AcoesIniciativa SET NumeroAcao = (NumeroAcao - 1)
    //                             WHERE NumeroAcao >= @NumeroAtualAcao
    //                               AND CodigoProjeto = {2}
    //                               AND CodigoAcao = CodigoAcaoSuperior
    //                           Commit Tran
    //                        End Try
    //                        Begin Catch
    //                           Rollback Tran
    //                            DECLARE @ErrorMessage NVARCHAR(4000);
    //                            DECLARE @ErrorSeverity INT;
    //                            DECLARE @ErrorState INT;
    //
    //                            SELECT @ErrorMessage = ERROR_MESSAGE();
    //                            SELECT @ErrorSeverity = ERROR_SEVERITY();
    //                            SELECT @ErrorState = ERROR_STATE();
    //    
    //                            RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    //                        End Catch
    //                     END
    //                        ", bancodb, Ownerdb
    //                         , codigoProjeto, codigoAcao);

    //            int regAf = 0;
    //            try
    //            {
    //                retorno = execSQL(comandoSQL, ref regAf);
    //            }
    //            catch (Exception)
    //            {
    //                retorno = false;
    //            }
    //            return retorno;

    //    }

    public bool verificaExclusaoPlanoTrabalho(int codigoAcao)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      SELECT 1 
                        FROM {0}.{1}.tai02_ProdutosAcoesIniciativa
                       WHERE CodigoAcao = {2}
                       UNION
                       SELECT 1 
                        FROM {0}.{1}.tai02_AcoesIniciativa
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

    public DataSet getAtividadeAcaoIniciativa(int codigoAtividade, string where)
    {
        string comandoSQL = string.Format(
                @"SELECT t1.CodigoAcao AS CodigoAtividade, t1.CodigoAcaoSuperior AS CodigoAcao
			            ,t1.NumeroAcao AS NumeroAtividade, tSup.NumeroAcao AS NumeroAcao
                        ,t1.NomeAcao AS Descricao, t1.Inicio, t1.Termino
                        ,t1.IndicaEventoInstitucional AS Institucional, t1.CodigoUsuarioResponsavel
                        ,t1.LocalEvento, t1.IndicaSemRecurso, t1.DetalhesEvento, tSup.NomeAcao AS NomeAcao
                   FROM {0}.{1}.tai02_AcoesIniciativa t1 INNER JOIN
  		                {0}.{1}.tai02_AcoesIniciativa tSup ON tSup.CodigoAcao = t1.CodigoAcaoSuperior
                  WHERE t1.CodigoAcao = {2}
                    {3}
               ", bancodb, Ownerdb, codigoAtividade, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiAtividadeAcaoIniciativa(int codigoProjeto, int codigoAcao, int numeroAtividade, string nomeAtividade, string dataInicio, string dataTermino
                                            , int codigoResponsavel, int codigoEntidade, string indicaInstitucinal, string localEvento, string detalhesEvento, string indicaSemRecursos, string etapaAcaoAtividade, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                         begin try
                          begin tran 
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
                              FROM {0}.{1}.tai02_AcoesIniciativa
                             WHERE codigoAcao = {3}

                            UPDATE {0}.{1}.tai02_AcoesIniciativa SET NumeroAcao = (NumeroAcao + 1)
                             WHERE NumeroAcao >= {4}
                               AND CodigoAcaoSuperior = {3}
                               AND CodigoProjeto = {2}
                               AND CodigoAcao <> {3}
                            
                            INSERT INTO {0}.{1}.tai02_AcoesIniciativa
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
                                       ,IndicaSemRecurso
                                       ,IndicaEventoInstitucional
                                       ,LocalEvento
                                       ,DetalhesEvento
                                       ,EtapaAcaoAtividade)
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
                                       ,'{10}'
                                       ,'{11}'
                                       ,'{12}'
                                       ,'{13}'
                                       ,'{14}')

                            SELECT @CodigoAtividade = SCOPE_IDENTITY()

                            UPDATE {0}.{1}.tai02_AcoesIniciativa 
	                           SET Inicio = (SELECT MIN(t.Inicio) FROM {0}.{1}.tai02_AcoesIniciativa t WHERE t.CodigoAcaoSuperior = {3} and t.CodigoAcaoSuperior <> t.CodigoAcao)
			                      ,Termino = (SELECT MAX(t.Termino) FROM {0}.{1}.tai02_AcoesIniciativa t WHERE t.CodigoAcaoSuperior = {3} and t.CodigoAcaoSuperior <> t.CodigoAcao)
                             WHERE CodigoAcao = {3}

                            SELECT @CodigoAtividade AS CodigoAtividade
                            Commit Tran
                        End Try
                        Begin Catch
                           Rollback Tran
                            DECLARE @ErrorMessage NVARCHAR(4000);
                            DECLARE @ErrorSeverity INT;
                            DECLARE @ErrorState INT;

                            SELECT @ErrorMessage = ERROR_MESSAGE();
                            SELECT @ErrorSeverity = ERROR_SEVERITY();
                            SELECT @ErrorState = ERROR_STATE();
    
                            RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);

                        End Catch
                        END
                        ", bancodb, Ownerdb
                         , codigoProjeto, codigoAcao, numeroAtividade, nomeAtividade.Replace("'", "''")
                         , codigoResponsavel, codigoEntidade, dataInicio, dataTermino, indicaSemRecursos
                         , indicaInstitucinal, localEvento.Replace("'", "''"), detalhesEvento.Replace("'", "''")
                         , etapaAcaoAtividade);

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

    public bool atualizaAtividadeAcaoIniciativa(int codigoAtividade, int codigoProjeto, int codigoAcao, int numeroAtividade, string nomeAtividade, string dataInicio, string dataTermino
                                            , int codigoResponsavel, int codigoEntidade, string indicaInstitucinal, string localEvento, string detalhesEvento, string indicaSemRecursos)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                        begin try
                          begin tran
                            DECLARE @NumeroAtualAcao int
                                   ,@NomeUsuario VarChar(60)
                                   ,@FonteRecurso Char(2)

                            SELECT @NomeUsuario = NomeUsuario
                              FROM {0}.{1}.Usuario us
                             WHERE us.CodigoUsuario = {7}

                            SELECT @FonteRecurso = FonteRecurso
                              FROM {0}.{1}.tai02_AcoesIniciativa
                             WHERE codigoAcao = {4}

                            SELECT @NumeroAtualAcao = NumeroAcao
                              FROM {0}.{1}.tai02_AcoesIniciativa
                             WHERE CodigoAcao = {2}
                               AND CodigoProjeto = {3}
                               AND CodigoAcaoSuperior = {4}

                            IF (@NumeroAtualAcao > {5})
                                UPDATE {0}.{1}.tai02_AcoesIniciativa SET NumeroAcao = (NumeroAcao + 1)
                                 WHERE NumeroAcao < @NumeroAtualAcao
                                   AND NumeroAcao >= {5}
                                   AND CodigoProjeto = {3}
                                   AND CodigoAcaoSuperior = {4}                                   
                                   AND CodigoAcao <> {4}
                            ELSE IF (@NumeroAtualAcao < {5})
                                UPDATE {0}.{1}.tai02_AcoesIniciativa SET NumeroAcao = (NumeroAcao - 1)
                                 WHERE NumeroAcao > @NumeroAtualAcao
                                   AND NumeroAcao <= {5}
                                   AND CodigoProjeto = {3}
                                   AND CodigoAcaoSuperior = {4}                                   
                                   AND CodigoAcao <> {4}

                            UPDATE {0}.{1}.tai02_AcoesIniciativa 
                               SET NumeroAcao = {5}
                                  ,NomeAcao = '{6}'
                                  ,CodigoUsuarioResponsavel = {7}
                                  ,NomeUsuarioResponsavel = @NomeUsuario
                                  ,Inicio = {8}
                                  ,Termino = {9}
                                  ,FonteRecurso = @FonteRecurso
                                  ,IndicaSemRecurso = '{10}'
                                  ,IndicaEventoInstitucional = '{11}'
                                  ,LocalEvento = '{12}'
                                  ,DetalhesEvento = '{13}'
                            WHERE CodigoAcao = {2}
                              AND CodigoProjeto = {3}
                              AND CodigoAcaoSuperior = {4}

                            UPDATE {0}.{1}.tai02_AcoesIniciativa 
	                           SET Inicio = (SELECT MIN(t.Inicio) FROM {0}.{1}.tai02_AcoesIniciativa t WHERE t.CodigoAcaoSuperior = {4} and t.CodigoAcaoSuperior <> t.CodigoAcao)
			                      ,Termino = (SELECT MAX(t.Termino) FROM {0}.{1}.tai02_AcoesIniciativa t WHERE t.CodigoAcaoSuperior = {4} and t.CodigoAcaoSuperior <> t.CodigoAcao)
                             WHERE CodigoAcao = {4}

                            IF ('{10}' = 'S')
                            BEGIN
                                DELETE FROM {0}.{1}.CronogramaOrcamentario  
                                WHERE CodigoProjeto = {3}  
                                  AND CodigoAcao in (select distinct CodigoAcao from {0}.{1}.tai02_AcoesIniciativa where CodigoAcaoSuperior =  {4}  and CodigoAcao = {2} AND CodigoProjeto = {3} )
                            END  
                            Commit Tran
                        End Try
                        Begin Catch
                           Rollback Tran
                            DECLARE @ErrorMessage NVARCHAR(4000);
                            DECLARE @ErrorSeverity INT;
                            DECLARE @ErrorState INT;

                            SELECT @ErrorMessage = ERROR_MESSAGE();
                            SELECT @ErrorSeverity = ERROR_SEVERITY();
                            SELECT @ErrorState = ERROR_STATE();
    
                            RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);

                        End Catch
                        END
                        ", bancodb, Ownerdb
                         , codigoAtividade, codigoProjeto, codigoAcao, numeroAtividade, nomeAtividade.Replace("'", "''")
                         , codigoResponsavel, dataInicio, dataTermino, indicaSemRecursos
                         , indicaInstitucinal, localEvento.Replace("'", "''"), detalhesEvento.Replace("'", "''"));

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

    public bool excluiAtividadeAcaoIniciativa(int codigoProjeto, int codigoAtividade)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                         BEGIN
                         begin try
                          begin tran
                            DECLARE @NumeroAtualAcao int,
                                    @CodigoAcaoSuperior int

                            SELECT @CodigoAcaoSuperior = CodigoAcaoSuperior
                              FROM {0}.{1}.tai02_AcoesIniciativa
                             WHERE CodigoAcao = {3}
                               AND CodigoProjeto = {2}

                            SELECT @NumeroAtualAcao = NumeroAcao
                              FROM {0}.{1}.tai02_AcoesIniciativa
                             WHERE CodigoAcao = {3}
                               AND CodigoProjeto = {2}

                            DELETE {0}.{1}.tai02_ProdutosAcoesIniciativa
                            WHERE CodigoAcao = {3}
                            
                            DELETE  {0}.{1}.tai02_MarcosAcoesIniciativa
                            WHERE CodigoAcao = {3}
                            
                            DELETE  {0}.{1}.tai02_ParceirosIniciativa
                            WHERE CodigoAcao = {3}

                            DELETE FROM {0}.{1}.CronogramaOrcamentarioAcao                                
                                  WHERE CodigoAcao = {3}                              
                                    AND CodigoProjeto = {2}

                            DELETE FROM {0}.{1}.tai02_AcoesIniciativa                                
                                  WHERE CodigoAcao = {3}                              
                                    AND CodigoProjeto = {2}
                            
                            UPDATE {0}.{1}.tai02_AcoesIniciativa SET NumeroAcao = (NumeroAcao - 1)
                             WHERE NumeroAcao >= @NumeroAtualAcao
                               AND CodigoProjeto = {2}
                               AND CodigoAcaoSuperior = @CodigoAcaoSuperior
                               AND CodigoAcao <> @CodigoAcaoSuperior

                            UPDATE {0}.{1}.tai02_AcoesIniciativa 
	                           SET Inicio = (SELECT MIN(t.Inicio) FROM {0}.{1}.tai02_AcoesIniciativa t WHERE t.CodigoAcaoSuperior = @CodigoAcaoSuperior and t.CodigoAcaoSuperior <> t.CodigoAcao)
			                      ,Termino = (SELECT MAX(t.Termino) FROM {0}.{1}.tai02_AcoesIniciativa t WHERE t.CodigoAcaoSuperior = @CodigoAcaoSuperior and t.CodigoAcaoSuperior <> t.CodigoAcao)
                             WHERE CodigoAcao = @CodigoAcaoSuperior
                        
                             Commit Tran
                        End Try
                        Begin Catch
                           Rollback Tran
                            DECLARE @ErrorMessage NVARCHAR(4000);
                            DECLARE @ErrorSeverity INT;
                            DECLARE @ErrorState INT;

                            SELECT @ErrorMessage = ERROR_MESSAGE();
                            SELECT @ErrorSeverity = ERROR_SEVERITY();
                            SELECT @ErrorState = ERROR_STATE();
    
                            RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);

                        End Catch
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

    public bool excluiAllAtividadeAcaoIniciativa(int codigoProjeto, int codigoAcao)
    {
        bool retorno = false;

        comandoSQL = string.Format(@"
                         BEGIN
                        Begin Try
                            Begin Tran

                            DECLARE @NumeroAtualAcao int


                            DELETE {0}.{1}.tai02_ProdutosAcoesIniciativa
                            WHERE CodigoAcao in (SELECT CodigoAcao 
                                                    FROM {0}.{1}.tai02_AcoesIniciativa
                                                    WHERE CodigoAcaoSuperior = {3}
                                                        AND CodigoAcao <> {3}) 
                              AND CodigoProjeto = {2} 
                            
                            DELETE  {0}.{1}.tai02_MarcosAcoesIniciativa
                            WHERE CodigoAcao in (SELECT CodigoAcao 
                                                    FROM {0}.{1}.tai02_AcoesIniciativa
                                                    WHERE CodigoAcaoSuperior = {3}
                                                        AND CodigoAcao <> {3}) 
                              AND CodigoProjeto = {2} 
                            
                            DELETE  {0}.{1}.tai02_ParceirosIniciativa
                            WHERE CodigoAcao in (SELECT CodigoAcao 
                                                    FROM {0}.{1}.tai02_AcoesIniciativa
                                                    WHERE CodigoAcaoSuperior = {3}
                                                        AND CodigoAcao <> {3}) 
                              AND CodigoProjeto = {2} 

                            DELETE  {0}.{1}.CronogramaOrcamentarioAcao
                            WHERE CodigoAcao in (SELECT CodigoAcao 
                                                    FROM {0}.{1}.tai02_AcoesIniciativa
                                                    WHERE CodigoAcaoSuperior = {3}
                                                        AND CodigoAcao <> {3}) 
                              AND CodigoProjeto = {2} 

                            DELETE FROM {0}.{1}.tai02_AcoesIniciativa                                
                                  WHERE CodigoAcao in (SELECT CodigoAcao 
                                                    FROM {0}.{1}.tai02_AcoesIniciativa
                                                    WHERE CodigoAcaoSuperior = {3}
                                                        AND CodigoAcao <> {3})                               
                                    AND CodigoProjeto = {2}

                            SELECT @NumeroAtualAcao = NumeroAcao
                              FROM {0}.{1}.tai02_AcoesIniciativa
                             WHERE CodigoAcao = {3}
                               AND CodigoProjeto = {2}

                            DELETE {0}.{1}.tai02_ProdutosAcoesIniciativa
                            WHERE CodigoAcao = {3}
                              AND CodigoProjeto = {2} 

                            DELETE FROM {0}.{1}.tai02_AcoesIniciativa                                
                                  WHERE CodigoAcao = {3}                              
                                    AND CodigoProjeto = {2}
                            
                            UPDATE {0}.{1}.tai02_AcoesIniciativa SET NumeroAcao = (NumeroAcao - 1)
                             WHERE NumeroAcao >= @NumeroAtualAcao
                               AND CodigoProjeto = {2}
                               AND CodigoAcao = CodigoAcaoSuperior

                           Commit Tran
                        End Try
                        Begin Catch
                           Rollback Tran
                            DECLARE @ErrorMessage NVARCHAR(4000);
                            DECLARE @ErrorSeverity INT;
                            DECLARE @ErrorState INT;

                            SELECT @ErrorMessage = ERROR_MESSAGE();
                            SELECT @ErrorSeverity = ERROR_SEVERITY();
                            SELECT @ErrorState = ERROR_STATE();
    
                            RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);

                        End Catch

                        END
                        ", bancodb, Ownerdb
                     , codigoProjeto, codigoAcao);

        int regAf = 0;
        try
        {
            retorno = execSQL(comandoSQL, ref regAf);
        }
        catch (Exception)
        {
            retorno = false;
        }
        return retorno;
    }

    public bool verificaExclusaoAtividadeAcaoIniciativa(int codigoAtividade)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      SELECT 1 
                        FROM {0}.{1}.tai02_ProdutosAcoesIniciativa
                       WHERE CodigoAcao = {2}
                       UNION
                       SELECT 1 
                        FROM {0}.{1}.tai02_MarcosAcoesIniciativa
                       WHERE CodigoAcao = {2}
                       UNION
                       SELECT 1 
                        FROM {0}.{1}.tai02_ParceirosIniciativa
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

    public bool incluiMetaAcaoIniciativa(int codigoProjeto, int codigoAcao, string meta)
    {
        bool retorno = false;
        int regAf = 0;
        try
        {
            comandoSQL = string.Format(@"
                        INSERT INTO {0}.{1}.tai02_ProdutosAcoesIniciativa(CodigoProjeto, CodigoAcao, DescricaoProduto)
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

    public bool atualizaMetaAcaoIniciativa(int codigoMeta, string meta)
    {
        bool retorno = false;
        int regAf = 0;
        try
        {
            comandoSQL = string.Format(@"
                        UPDATE {0}.{1}.tai02_ProdutosAcoesIniciativa SET DescricaoProduto = '{3}'
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

    public bool excluiMetaAcaoIniciativa(int codigoMeta)
    {
        bool retorno = false;
        int regAf = 0;
        try
        {
            comandoSQL = string.Format(@"
                        DELETE FROM {0}.{1}.tai02_ProdutosAcoesIniciativa 
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

    public bool incluiMarcoAcaoIniciativa(int codigoProjeto, int codigoAcao, string marco, string data)
    {
        bool retorno = false;
        int regAf = 0;
        try
        {
            comandoSQL = string.Format(@"
                        INSERT INTO {0}.{1}.tai02_MarcosAcoesIniciativa(CodigoProjeto, CodigoAcao, NomeMarco, DataLimitePrevista)
                                                                   VALUES({2}, {3}, '{4}', {5})
                        ", bancodb, Ownerdb
                         , codigoProjeto, codigoAcao, marco.Replace("'", "''")
                         , data);

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaMarcoAcaoIniciativa(int codigoMarco, string marco, string data)
    {
        bool retorno = false;
        int regAf = 0;
        try
        {
            comandoSQL = string.Format(@"
                        UPDATE {0}.{1}.tai02_MarcosAcoesIniciativa 
                           SET NomeMarco = '{3}'
                              ,DataLimitePrevista = {4}
                         WHERE SequenciaRegistro = {2}
                        ", bancodb, Ownerdb
                         , codigoMarco, marco.Replace("'", "''")
                         , data);

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool excluiMarcoAcaoIniciativa(int codigoMarco)
    {
        bool retorno = false;
        int regAf = 0;
        try
        {
            comandoSQL = string.Format(@"
                        DELETE FROM {0}.{1}.tai02_MarcosAcoesIniciativa 
                         WHERE SequenciaRegistro = {2}
                        ", bancodb, Ownerdb
                         , codigoMarco);

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool incluiParceriaAcaoIniciativa(int codigoProjeto, int codigoAcao, string codigoParceiro, string produto)
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

                        INSERT INTO {0}.{1}.tai02_ParceirosIniciativa(CodigoProjeto, CodigoAcao, CodigoParceiro, NomeParceiro, ProdutoSolicitado)
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

    public bool atualizaParceriaAcaoIniciativa(int codigoParceria, string codigoParceiro, string produto)
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

                        UPDATE {0}.{1}.tai02_ParceirosIniciativa 
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

    public bool excluiParceriaAcaoIniciativa(int codigoParceria)
    {
        bool retorno = false;
        int regAf = 0;
        try
        {
            comandoSQL = string.Format(@"
                        DELETE FROM {0}.{1}.tai02_ParceirosIniciativa 
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

    #endregion
}
}