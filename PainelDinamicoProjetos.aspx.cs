using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DevExpress.Web;

namespace BriskPtf._Projetos
{
    public partial class _Projetos_PainelDinamicoProjetos : System.Web.UI.Page
    {
        dados cDados;
        public string alturaTela = "";
        int codigoUsuarioResponsavel;
        int codigoEntidade;

        protected void Page_PreInit(object sender, EventArgs e)
        {
            System.Collections.Specialized.OrderedDictionary listaParametrosDados = new System.Collections.Specialized.OrderedDictionary();

            listaParametrosDados["RemoteIPUsuario"] = Session["RemoteIPUsuario"] + "";
            listaParametrosDados["NomeUsuario"] = Session["NomeUsuario"] + "";

            cDados = CdadosUtil.GetCdados(listaParametrosDados);
            cDados.aplicaTema(this.Page);
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            // =========================== Verifica se a sessão existe INICIO ========================
            try
            {
                if (cDados.getInfoSistema("IDUsuarioLogado") == null)
                    Response.Redirect("~/erros/erroInatividade.aspx");
            }
            catch
            {
                Response.RedirectLocation = cDados.getPathSistema() + "erros/erroInatividade.aspx";
                Response.End();
            }

            codigoUsuarioResponsavel = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());
            codigoEntidade = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());

            if (!IsPostBack)
            {
                cDados.VerificaAcessoTela(this, codigoUsuarioResponsavel, codigoEntidade, codigoEntidade, "NULL", "EN", 0, "NULL", "EN_AcsPMC");
            }

            cDados.defineConfiguracoesComboCarteiras(ddlCarteirasProjetos, IsPostBack, codigoUsuarioResponsavel, codigoEntidade, true);


            if (!IsPostBack)
            {
                //cDados.aplicaEstiloVisual(Page);
                cDados.excluiNiveisAbaixo(0);
                cDados.insereNivel(0, this);
                Master.geraRastroSite();
                Master.verificaAcaoFavoritos(true, lblTituloTela.Text, "PNLPMC", "ENT", -1, "Adicionar Painel aos Favoritos");
            }

            if (!IsPostBack)
                carregaProjetos(ddlCarteirasProjetos.Value.ToString());

            defineLarguraTela();
            this.Title = cDados.getNomeSistema();

            defineLabelCarteira();

            gvDados.SettingsPager.Mode = GridViewPagerMode.ShowAllRecords;
            gvDados.Settings.ShowFilterRow = false;

            if (!IsPostBack)
                verificaParametroTempo();

            getNumeroPaginasMetas();

            Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"" src=""../scripts/PainelDinamicoProjetos.js""></script>"));
            this.TH(this.TS("PainelDinamicoProjetos"));
        }

        private void verificaParametroTempo()
        {
            string comandoSQL = string.Format(@"SELECT Valor FROM dbo.ParametroConfiguracaoSistema WHERE CodigoEntidade = {0} AND Parametro = 'TempoExecucaoPainelProjetosCarteira'", codigoEntidade);

            DataSet dsParametros = cDados.getDataSet(comandoSQL);

            if (cDados.DataSetOk(dsParametros) && cDados.DataTableOk(dsParametros.Tables[0]))
            {
                txtTempo.Text = dsParametros.Tables[0].Rows[0]["Valor"].ToString();
            }
            else
            {
                comandoSQL = string.Format(@"
            INSERT INTO [dbo].[ParametroConfiguracaoSistema]
              (
                      [CodigoEntidade]
                    , [Parametro]
                    , [Valor]
                    , [DescricaoParametro_PT]
                    , [DescricaoParametro_EN]
                    , [DescricaoParametro_ES]
                    , [IndicaControladoSistema]
                    , [TipoDadoParametro]
                    , [ValorMinimo]
                    , [ValorMaximo]
                    , [CodigoConjuntoOpcaoParametro]
                    , [CodigoModuloSistema]
              )VALUES
              ({0}
              , 'TempoExecucaoPainelProjetosCarteira'
              , '30'
              , 'Tempo de Execução de Cada Painel do Painel de Projetos e Metas por Carteira'
              , 'Tempo de Execução de Cada Painel do Painel de Projetos e Metas por Carteira'
              , 'Tempo de Execução de Cada Painel do Painel de Projetos e Metas por Carteira'
              , 'N'
              , 'NUM'
              , NULL
              , NULL
              , NULL 
              , NULL)", codigoEntidade);

                int regAf = 0;

                cDados.execSQL(comandoSQL, ref regAf);

                txtTempo.Text = "30";
            }
        }

        private void defineLabelCarteira()
        {
            DataSet dsParametro = cDados.getParametrosSistema("labelCarteiras", "labelCarteirasPlural");
            string label = Resources.traducao.PainelDinamicoProjetos_carteira;

            if ((cDados.DataSetOk(dsParametro)) && (cDados.DataTableOk(dsParametro.Tables[0])))
            {
                label = dsParametro.Tables[0].Rows[0]["labelCarteiras"].ToString();
            }

            lblCarteira.Text = label.TrimEnd() + ":";

        }

        private void defineLarguraTela()
        {
            string ResolucaoCliente = cDados.getInfoSistema("ResolucaoCliente").ToString().ToString();
            int altura = int.Parse(ResolucaoCliente.Substring(ResolucaoCliente.IndexOf('x') + 1)); ;

            alturaTela = (altura - 120).ToString() + "px";
        }

        protected void callBack_Callback(object source, CallbackEventArgs e)
        {
            string comandoSQL = string.Format(@"
            UPDATE ParametroConfiguracaoSistema 
               SET Valor = {0}
             WHERE Parametro = 'TempoExecucaoPainelProjetosCarteira'
               AND CodigoEntidade = {1}", txtTempo.Text == "" ? "30" : txtTempo.Text
                   , codigoEntidade);

            int regAf = 0;

            cDados.execSQL(comandoSQL, ref regAf);
        }

        protected void gvDados_CustomCallback(object sender, ASPxGridViewCustomCallbackEventArgs e)
        {
            carregaProjetos(e.Parameters);
        }

        private void carregaProjetos(string codigoCarteira)
        {
            string comandoSQL = string.Format(@"
        SELECT p.NomeProjeto, p.CodigoProjeto
                          FROM dbo.Projeto AS p  INNER JOIN
                               dbo.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                    AND s.IndicaAcompanhamentoExecucao = 'S') INNER JOIN
                               dbo.f_GetProjetosUsuario({0}, {1}, {2}) F on (f.codigoProjeto = p.CodigoProjeto)
                               INNER JOIN CarteiraProjeto cp on (cp.CodigoProjeto = p.CodigoProjeto and cp.CodigoCarteira = {2})
                         WHERE p.DataExclusao IS NULL
                       ORDER BY p.NomeProjeto", codigoUsuarioResponsavel, codigoEntidade, codigoCarteira);

            DataSet ds = cDados.getDataSet(comandoSQL);

            if (cDados.DataSetOk(ds))
            {
                gvDados.DataSource = ds;
                gvDados.DataBind();

                if (cDados.DataTableOk(ds.Tables[0]))
                {
                    gvDados.Selection.SelectRow(0);
                    btnOk.ClientEnabled = true;
                }
                else
                {
                    btnOk.ClientEnabled = false;
                }
            }
        }

        private string getChavePrimaria() // retorna a primary key da tabela
        {
            if (gvDados.GetSelectedFieldValues(gvDados.KeyFieldName).Count > 0)
                return gvDados.GetSelectedFieldValues(gvDados.KeyFieldName)[0].ToString();
            else
                return "-1";
        }

        private void getNumeroPaginasMetas()
        {
            int codigoProjeto = int.Parse(getChavePrimaria());
            string where = "";

            if (codigoProjeto != -1)
            {
                int codigoTipoProjeto = cDados.getCodigoTipoProjeto(codigoProjeto);

                if (codigoTipoProjeto == 2)
                {
                    where += string.Format(@" AND p.CodigoProjeto IN(SELECT lp.CodigoProjetoFilho
                                                                   FROM {0}.{1}.LinkProjeto AS lp
                                                                  WHERE CodigoProjetoPai = {2}
                                                                    AND TipoLink = 'PP'
                                                                 UNION
                                                                 SELECT {2})", cDados.getDbName(), cDados.getDbOwner(), codigoProjeto);
                }
                else
                {
                    where += " AND p.CodigoProjeto = " + codigoProjeto.ToString();
                }
            }

            where += string.Format(" AND p.CodigoProjeto in (SELECT codigoProjeto FROM {0}.{1}.f_getProjetosUsuario({2}, {3}, {4})) ", cDados.getDbName(), cDados.getDbOwner(), codigoUsuarioResponsavel, codigoEntidade
                , ddlCarteirasProjetos.Value);

            DataSet ds = cDados.getQTDMetasVisaoCorporativaProjetos(codigoEntidade, where);

            int quantidadePaginasMetas = 0;

            if (cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
            {
                int quantidadeMetas = int.Parse(ds.Tables[0].Rows[0]["QTD"].ToString());
                quantidadePaginasMetas = quantidadeMetas / 4;
                if (quantidadeMetas % 4 != 0)
                    quantidadePaginasMetas++;
            }

            callBack.JSProperties["cp_NumeroPaginas"] = quantidadePaginasMetas;
        }
    }
}
