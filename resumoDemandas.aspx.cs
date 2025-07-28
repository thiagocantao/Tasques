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
    public partial class _Projetos_resumoDemandas : System.Web.UI.Page
    {
        dados cDados;

        private int codigoUsuarioResponsavel;
        private int codigoEntidadeUsuarioResponsavel;
        public string alturaTabela;

        DataSet dsDemandas;

        public string estiloFooter = "dxtlFooter";

        protected void Page_PreInit(object sender, EventArgs e)
        {
            System.Collections.Specialized.OrderedDictionary listaParametrosDados = new System.Collections.Specialized.OrderedDictionary();

            listaParametrosDados["RemoteIPUsuario"] = Session["RemoteIPUsuario"] + "";
            listaParametrosDados["NomeUsuario"] = Session["NomeUsuario"] + "";

            cDados = CdadosUtil.GetCdados(listaParametrosDados);
            cDados.aplicaTema(this.Page);
        }


        protected void Page_Init(object sender, EventArgs e)
        {
            if (cDados.getInfoSistema("ResolucaoCliente") == null)
                Response.Redirect("~/index.aspx");

            codigoUsuarioResponsavel = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());
            codigoEntidadeUsuarioResponsavel = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());

            if (!IsPostBack)
            {
                //cDados.VerificaAcessoTela(this, codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "NULL", "EN", 0, "NULL", "AcsLstDmd");
                bool podeIncluirDemandas = cDados.VerificaAcessoEmAlgumObjeto(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "UN", "UN_IncDem");

                imgNovaDemanda.ClientVisible = podeIncluirDemandas;
                ASPxImage1.ClientVisible = podeIncluirDemandas;
            }

            inicializaCombosFiltros();
            this.Title = cDados.getNomeSistema();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            Header.Controls.Add(cDados.getLiteral(@"<link href=""../estilos/cdisEstilos.css"" rel=""stylesheet"" type=""text/css"" />"));
            Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"" src=""../scripts/ListaProjetos.js""></script>"));
            this.TH(this.TS("ListaProjetos"));
            string cssPostfix = "", cssPath = "";

            //cDados.aplicaEstiloVisual(Page);
            cDados.getVisual(cDados.getInfoSistema("IDEstiloVisual").ToString(), ref cssPath, ref cssPostfix);

            if (cssPostfix != "")
                estiloFooter += "_" + cssPostfix;

            if (!IsPostBack)
            {
                verificaFiltrosParametro();
            }

            defineAlturaTela();

            getDemandas();

            if (!IsPostBack)
            {
                cDados.excluiNiveisAbaixo(1);
                cDados.insereNivel(1, this);
                Master.geraRastroSite();
                Master.verificaAcaoFavoritos(true, lblTituloTela.Text, "RESDEM", "DEM", -1, "Adicionar Lista aos Favoritos");
            }

            if (!IsPostBack)
            {
                DataSet ds = cDados.getDefinicaoUnidade(codigoEntidadeUsuarioResponsavel);
                if (cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
                {
                    lblUnidades.Text = ds.Tables[0].Rows[0]["DescricaoTipoUnidadeNegocio"].ToString() + ":";
                }
            }
        }


        private void verificaFiltrosParametro()
        {
            if (Request.QueryString["CUN"] != null)
                ddlUnidades.Value = Request.QueryString["CUN"].ToString();

            if (Request.QueryString["CodigoGerente"] != null)
                ddlGerentes.Value = Request.QueryString["CodigoGerente"].ToString();
        }

        private void getDemandas()
        {
            string where = "";

            if (ddlGerentes.Value.ToString() != "-1")
                where += " AND p.CodigoGerenteProjeto = " + ddlGerentes.Value;

            if (ddlDemandante.Value.ToString() != "-1")
                where += " AND p.CodigoCliente = " + ddlDemandante.Value;

            if (ddlUnidades.Value.ToString() != "-1")
                where += " AND p.CodigoUnidadeNegocio = " + ddlUnidades.Value;

            if (ddlStatus.Value.ToString() != "-1")
                where += " AND p.CodigoStatusProjeto = " + ddlStatus.Value;

            if (ddlPrioridade.Value.ToString() == "A")
                where += " AND p.Prioridade < 200";
            else if (ddlPrioridade.Value.ToString() == "M")
                where += " AND p.Prioridade > 199 AND p.Prioridade < 400";
            else if (ddlPrioridade.Value.ToString() == "B")
                where += " AND p.Prioridade > 399";

            if (txtDescricao.Text != "")
                where += " AND p.NomeProjeto LIKE '%" + txtDescricao.Text + "%'";


            dsDemandas = cDados.getListaDemandas(codigoEntidadeUsuarioResponsavel, int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString()), where);

            carregaTreeListDemandas(dsDemandas);
        }

        public void carregaTreeListDemandas(DataSet ds)
        {
            tlProjetos.DataSource = ds.Tables[0];
            tlProjetos.DataBind();
            tlProjetos.ExpandToLevel(3);
        }

        private void defineAlturaTela()
        {
            string resolucaoCliente = cDados.getInfoSistema("ResolucaoCliente").ToString();

            // Calcula a altura da tela
            int alturaPrincipal = int.Parse(resolucaoCliente.Substring(resolucaoCliente.IndexOf('x') + 1));
            int largura = int.Parse(resolucaoCliente.Substring(0, resolucaoCliente.IndexOf('x')));
            tlProjetos.Settings.ScrollableHeight = alturaPrincipal - 255;
        }

        #region Combos Filtros

        private void carregaCombo(DataSet dsCombo, string textField, string valueField, ASPxComboBox ddlCombo, string customItem, string valueCustomItem)
        {
            if (cDados.DataSetOk(dsCombo))
            {
                ddlCombo.DataSource = dsCombo;
                ddlCombo.TextField = textField;
                ddlCombo.ValueField = valueField;
            }

            if (textField == "NomeUsuario")
            {
                if (customItem != "")
                {
                    DataRow dr = dsCombo.Tables[0].NewRow();
                    dr["CodigoUsuario"] = int.Parse(valueCustomItem);
                    dr["NomeUsuario"] = customItem;
                    dr["EMail"] = customItem;
                    dsCombo.Tables[0].Rows.InsertAt(dr, 0);
                }
                ddlCombo.Columns[0].FieldName = "NomeUsuario";
                ddlCombo.Columns[1].FieldName = "EMail";
                ddlCombo.TextFormatString = "{0}";
            }

            if (textField == "SiglaUnidadeNegocio")
            {

                if (customItem != "")
                {
                    DataRow dr = dsCombo.Tables[0].NewRow();
                    dr["CodigoUnidadeNegocio"] = int.Parse(valueCustomItem);
                    dr["SiglaUnidadeNegocio"] = customItem;
                    dr["NomeUnidadeNegocio"] = customItem;
                    dsCombo.Tables[0].Rows.InsertAt(dr, 0);
                }
                ddlCombo.Columns[0].FieldName = "SiglaUnidadeNegocio";
                ddlCombo.Columns[1].FieldName = "NomeUnidadeNegocio";
                ddlCombo.TextFormatString = "{0}";
            }
            ddlCombo.DataBind();

            if (!IsPostBack && !IsCallback)
            {
                ddlCombo.SelectedIndex = 0;
            }
        }

        private void inicializaCombosFiltros()
        {
            DataSet dsCombos;

            //Carrega Combo de Gerentes
            dsCombos = cDados.getResponsaveisDemandaProcessosEntidade(codigoEntidadeUsuarioResponsavel, codigoUsuarioResponsavel, 'D', "");
            carregaCombo(dsCombos, "NomeUsuario", "CodigoUsuario", ddlGerentes, "Todos", "-1");

            //Carrega Combo de Unidades
            dsCombos = cDados.getUnidadesNegocioEntidade(codigoEntidadeUsuarioResponsavel, "");
            carregaCombo(dsCombos, "SiglaUnidadeNegocio", "CodigoUnidadeNegocio", ddlUnidades, "Todos", "-1");

            //Carrega Combo de Demandantes
            dsCombos = cDados.getClientesDemandaEntidade(codigoEntidadeUsuarioResponsavel, codigoUsuarioResponsavel, "");
            carregaCombo(dsCombos, "NomeUsuario", "CodigoUsuario", ddlDemandante, "Todos", "-1");

            //Carrega Combo de Status
            dsCombos = cDados.getStatus(" AND TipoStatus = 'DMD'");

            carregaCombo(dsCombos, "DescricaoStatus", "CodigoStatus", ddlStatus, "Todos", "-1");

            DataTable dtComboStatus = dsCombos.Tables[0];

            string where = "IndicaSelecaoPortfolio = 'S' AND IndicaAcompanhamentoPortfolio = 'S' AND IndicaAcompanhamentoExecucao = 'S' AND IndicaAcompanhamentoPosExecucao = 'N'";

            DataRow[] dr = dtComboStatus.Select(where);

            if (!IsPostBack && dr.Length > 0)
            {
                ddlStatus.Value = dr[0]["CodigoStatus"].ToString();
            }

            ddlStatus.JSProperties["cp_ValorDefault"] = ddlStatus.SelectedIndex == -1 ? "-1" : ddlStatus.Value.ToString();
        }

        #endregion

        protected void tlProjetos_CustomCallback(object sender, DevExpress.Web.ASPxTreeList.TreeListCustomCallbackEventArgs e)
        {
            tlProjetos.ExpandAll();
            //getDemandas();
        }

        protected void tlProjetos_HtmlDataCellPrepared(object sender, DevExpress.Web.ASPxTreeList.TreeListHtmlDataCellEventArgs e)
        {
        }

        protected void callback_Callback(object source, DevExpress.Web.CallbackEventArgs e)
        {
            callback.JSProperties["cp_URL"] = "X";

            // removendo variável de sessão caso tenha sido criada durante uma 'interação' com algum fluxo
            // instrução necessária para evitar confusão com o código do projeto do fluxo.
            cDados.setInfoSistema("CodigoProjeto", -1);

            // busca o código do workflow para criar uma nova Demanda
            int codigoFluxoNovaDemanda, codigoWFNovaDemanda;

            cDados.getCodigoWfAtualFluxoNovaDemanda(codigoEntidadeUsuarioResponsavel, out codigoFluxoNovaDemanda, out codigoWFNovaDemanda);

            if ((codigoFluxoNovaDemanda != 0) && (codigoWFNovaDemanda != 0))
                callback.JSProperties["cp_URL"] = "wfEngine.aspx?CF=" + codigoFluxoNovaDemanda.ToString() + "&CW=" + codigoWFNovaDemanda.ToString();
        }

    }

}
