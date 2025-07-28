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
    #region PPA
    public bool podeEditarPPA(int codigoPlano, int codigoEntidade, int codigoUsuario)
    {
        bool retorno = false;
        string comandoSQL = string.Format(@"
        SELECT dbo.f_pln_GetEditabilidadePlano({0}, {1}, {2}) AS Editavel"
        , codigoPlano
        , codigoEntidade
        , codigoUsuario);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            retorno = ds.Tables[0].Rows[0]["Editavel"].ToString() == "S";

        return retorno;
    }
    #endregion

    #region OperacaoCritica

    public long RegistraOperacaoCritica(string iniciaisTipoOperacao, int codigoUsuarioOperacao, int? codigoEntidade = null, long? codigoObjetoAssociado = null, string iniciaisTipoAssociacao = null, long? codigoObjetoAssociadoPai = null, string contexto = null)
    {
        long codigoOperacao = -1;
        StringBuilder comandoSql = new StringBuilder();

        #region Comando SQL

        comandoSql.AppendFormat(@"
DECLARE @CodigoUsuarioOperacao int,
        @CodigoTipoOperacao smallint,
        @CodigoEntidade int,
        @CodigoTipoAssociacao smallint,
        @CodigoObjetoAssociado bigint,
        @CodigoObjetoAssociadoPai bigint,
        @ContextoOperacao varchar(2000)

SELECT @CodigoTipoOperacao = CodigoTipoOperacao FROM TipoOperacaoCritica WHERE IniciaisTipoOperacao = '{0}'

SET @CodigoUsuarioOperacao = {1}", iniciaisTipoOperacao, codigoUsuarioOperacao);

        if (codigoEntidade.HasValue)
        {
            comandoSql.AppendLine();
            comandoSql.AppendFormat(@"SET @CodigoEntidade = {0}", codigoEntidade.Value);
        }

        if (codigoObjetoAssociado.HasValue)
        {
            comandoSql.AppendLine();
            comandoSql.AppendFormat(@"SET @CodigoObjetoAssociado = {0}", codigoObjetoAssociado.Value);
        }

        if (codigoObjetoAssociadoPai.HasValue)
        {
            comandoSql.AppendLine();
            comandoSql.AppendFormat(@"SET @CodigoObjetoAssociadoPai = {0}", codigoObjetoAssociadoPai.Value);
        }

        if (!string.IsNullOrWhiteSpace(iniciaisTipoAssociacao))
        {
            comandoSql.AppendLine();
            comandoSql.AppendFormat(@"SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao FROM TipoAssociacao WHERE IniciaisTipoAssociacao = '{0}'", iniciaisTipoAssociacao);
        }

        if (!string.IsNullOrWhiteSpace(contexto))
        {
            comandoSql.AppendLine();
            comandoSql.AppendFormat(@"SET @ContextoOperacao = '{0}'", contexto);
        }

        comandoSql.Append(@"
INSERT INTO OperacaoCritica
           (DataInicioOperacao
           ,CodigoUsuarioOperacao
           ,CodigoTipoOperacao
           ,CodigoEntidade
           ,CodigoTipoAssociacao
           ,CodigoObjetoAssociado
           ,CodigoObjetoAssociadoPai
           ,ContextoOperacao)
     VALUES
           (GETDATE()
           ,@CodigoUsuarioOperacao
           ,@CodigoTipoOperacao
           ,@CodigoEntidade
           ,@CodigoTipoAssociacao
           ,@CodigoObjetoAssociado
           ,@CodigoObjetoAssociadoPai
           ,@ContextoOperacao)
           
SELECT Convert(bigint,ISNULL(SCOPE_IDENTITY(), -1)) AS CodigoOperacao");

        #endregion

        DataSet ds = getDataSet(comandoSql.ToString());
        if (ds.Tables[0].Rows.Count > 0)
            codigoOperacao = ds.Tables[0].Rows[0].Field<long>("CodigoOperacao");

        return codigoOperacao;
    }

    public long RegistraPassoOperacaoCritica(long codigoOperacao, string descricao, string contexto)
    {
        long codigoPassoOperacao = -1;
        string comandoSql;

        #region Comando SQL

        comandoSql = string.Format(@"
DECLARE @CodigoOperacao bigint,
        @DescricaoPassoOperacao varchar(500),
        @ContextoPassoOperacao varchar(8000)
        
SET @CodigoOperacao = {0}
SET @DescricaoPassoOperacao = '{1}'
SET @ContextoPassoOperacao = '{2}'

INSERT INTO PassoOperacaoCritica
           (CodigoOperacao
           ,DataPassoOperacao
           ,DescricaoPassoOperacao
           ,ContextoPassoOperacao)
     VALUES
           (@CodigoOperacao
           ,GETDATE()
           ,@DescricaoPassoOperacao
           ,@ContextoPassoOperacao)
           
SELECT Convert(bigint,ISNULL(SCOPE_IDENTITY(), -1)) AS CodigoPassoOperacao", codigoOperacao, descricao, contexto);

        #endregion

        DataSet ds = getDataSet(comandoSql);
        if (ds.Tables[0].Rows.Count > 0)
            codigoPassoOperacao = ds.Tables[0].Rows[0].Field<long>("CodigoPassoOperacao");

        return codigoPassoOperacao;
    }

    public void RegistraFalhaOperacaoCritica(long codigoOperacao, string codigoErroOperacao, string descricaoErroOperacao)
    {
        string comandoSql;

        #region Comando SQL

        comandoSql = string.Format(@"
DECLARE @CodigoOperacao bigint

SET @CodigoOperacao = {0}

UPDATE OperacaoCritica
   SET DataTerminoOperacao = GETDATE()
      ,CodigoErroOperacao = '{1}'
      ,DescricaoErroOperacao = '{2}'
 WHERE CodigoOperacao = @CodigoOperacao", codigoOperacao, codigoErroOperacao, descricaoErroOperacao);

        #endregion

        int registrosAfetados = 0;
        execSQL(comandoSql, ref registrosAfetados);
    }

    public void FinalizaOperacaoCritica(long codigoOperacao)
    {
        string comandoSql;

        #region Comando SQL

        comandoSql = string.Format(@"
DECLARE @CodigoOperacao bigint

SET @CodigoOperacao = {0}

UPDATE OperacaoCritica
   SET DataTerminoOperacao = GETDATE()
 WHERE CodigoOperacao = @CodigoOperacao", codigoOperacao);

        #endregion

        int registrosAfetados = 0;
        execSQL(comandoSql, ref registrosAfetados);
    }


    public bool ProcessaAcaoWorkflow(string usuario, string workflow, string instanciaWf, string seqEtapa, string etapa, bool efetivarAssinaturaDigitalFormulario, out string mensagemErro)
    {
        bool bRet;
        StringBuilder comandoSql = new StringBuilder();
        try
        {

            //comandoSQL = string.Format(
            //    @"EXEC dbo.p_efetivaAssinaturaDigitalFormulario {0}, {1}, {2}, {3}", workflow, instanciaWf, seqEtapa, etapa);

            comandoSql.AppendFormat(@"
            BEGIN
                DECLARE @RC                             int 
                DECLARE @in_codigoWorkFlow              int 
                DECLARE @in_codigoInstanciaWf           bigint 
                DECLARE @in_SequenciaOcorrenciaEtapaWf  int 
                DECLARE @in_codigoEtapaWf               int 
                DECLARE @in_codigoAcaoWf                int 
                DECLARE @in_identificadorUsuarioAcao    varchar(50)
                DECLARE @in_opcoes                      int 
                DECLARE @ou_resultado                   int 
                DECLARE @ou_codigoRetorno               int 
                DECLARE @ou_mensagemErro                nvarchar(2048) 

                SET @in_codigoWorkFlow              = {2} 
                SET @in_codigoInstanciaWf           = {3} 
                SET @in_SequenciaOcorrenciaEtapaWf  = {4} 
                SET @in_codigoEtapaWf               = {5} 
                SELECT @in_codigoAcaoWf = dbo.f_pbh_GetProximaAcaoAssinaturaOficio(@in_codigoWorkFlow, @in_codigoEtapaWf)
                SET @in_identificadorUsuarioAcao    = '{6}'
                SET @in_opcoes						= {7} "
                           , ""
                           , ""
                           , workflow, instanciaWf, seqEtapa, etapa, usuario, 0);

            if (efetivarAssinaturaDigitalFormulario)
                comandoSql.Append(@"
            EXEC dbo.p_efetivaAssinaturaDigitalFormulario 
               @in_codigoWorkFlow               = @in_codigoWorkFlow 
              ,@in_codigoInstanciaWf            = @in_codigoInstanciaWf 
              ,@in_SequenciaOcorrenciaEtapaWf   = @in_SequenciaOcorrenciaEtapaWf 
              ,@in_codigoEtapaWf                = @in_codigoEtapaWf ");

            comandoSql.Append(@"
            EXECUTE @RC = dbo.[p_processaAcaoWorkflow] 
                   @in_codigoWorkFlow               = @in_codigoWorkFlow 
                  ,@in_codigoInstanciaWf            = @in_codigoInstanciaWf 
                  ,@in_SequenciaOcorrenciaEtapaWf   = @in_SequenciaOcorrenciaEtapaWf 
                  ,@in_codigoEtapaWf                = @in_codigoEtapaWf 
                  ,@in_codigoAcaoWf                 = @in_codigoAcaoWf 
                  ,@in_identificadorUsuarioAcao     = @in_identificadorUsuarioAcao
                  ,@in_opcoes                       = @in_opcoes 
                  ,@ou_resultado                    = @ou_resultado                 OUTPUT 
                  ,@ou_codigoRetorno                = @ou_codigoRetorno             OUTPUT 
                  ,@ou_mensagemErro                 = @ou_mensagemErro              OUTPUT 

                SELECT 
                        @RC									AS RetornoProc,  
                        @ou_resultado				AS Resultado, 
                        @ou_codigoRetorno		AS CodigoRetorno, 
                        @ou_mensagemErro		AS MensagemErro
        END");

            DataSet ds = getDataSet(comandoSql.ToString());

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
                    msgWhere = "na definição da próxima etapa do processo";
                else if (((4 & resultado) > 0) || ((8 & resultado) > 0))
                    msgWhere = "na execução de ações automáticas da etapa do processo";
                else if (((1 & resultado) > 0) || ((2 & resultado) > 0))
                    msgWhere = "no envio de notificações";
                else if ((1024 & resultado) > 0)
                    msgWhere = "no acionamento de API's externas configuradas na etapa";

                switch (codigoRetorno)
                {
                    case 1:
                    case 2:
                        msgWhat = @"As informações na base de dados encontram-se inconsistentes.";
                        break;

                    case 3:
                    case 4:
                        msgWhat = @"A configuração dos parâmetros para execução de procedimentos no servidor ficou incorreta.";
                        break;
                    case 5:
                        msgWhat = @"A etapa do processo foi alterada por outro usuário. Acesse o processo novamente para ver as informações atualizadas";
                        break;
                    case 6:
                        msgWhat = @"A ação executada não produziu efeito e não foi possível determinar a causa.";
                        break;
                    case 7:
                        msgWhat = @"O usuário não possui permissão para executar a ação.";
                        break;
                    case 8:
                        msgWhat = @"Houve um erro durante o processamento e não foi possível determinar a causa.";
                        break;
                    default:
                        msgWhat = string.Format(@"Falha na execução do procedimento no servidor de banco de dados. (errorcode: {0}:{1})", codigoRetorno, retornoProc);
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
        catch (Exception ex)
        {
            mensagemErro = string.Format(@"Falha ao executar procedimento no servidor. Mensagem original do erro: {0}", ex.Message);
            bRet = false;
        }

        return bRet;
    }
    #endregion

    #region BreadCrumps 
    public DataSet GetNomeTelaUrlPadraoUsuarioIdioma(string codigoUsuario, string codigoEntidade)
    {
        DataSet retorno;
        string comandoSQL = string.Format(@"
        SELECT * from dbo.f_GetNomeTelaUrlPadraoUsuarioIdioma({0}, {1}) AS GetNomeTelaUrlPadraoUsuarioIdioma"
       , codigoUsuario
       , codigoEntidade);


        return retorno = getDataSet(comandoSQL);
    }
    #endregion
}
}