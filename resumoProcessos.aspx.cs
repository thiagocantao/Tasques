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
    public partial class _Projetos_resumoProcessos : System.Web.UI.Page
    {
        dados cDados;

        private int codigoUsuarioResponsavel;
        private int codigoEntidadeUsuarioResponsavel;
        public string alturaTabela;

        DataSet dsProcessos;
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

            inicializaCombosFiltros();
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

                cDados.excluiNiveisAbaixo(1);
                cDados.insereNivel(1, this);
                Master.geraRastroSite();
                Master.verificaAcaoFavoritos(true, lblTituloTela.Text, "LPROC", "PROC", -1, "Adicionar Lista aos Favoritos");
            }

            defineAlturaTela();

            getProcessos();
        }


        private void verificaFiltrosParametro()
        {
            if (Request.QueryString["CUN"] != null)
                ddlUnidades.Value = Request.QueryString["CUN"].ToString();

            if (Request.QueryString["CodigoGerente"] != null)
                ddlGerentes.Value = Request.QueryString["CodigoGerente"].ToString();
        }

        private void getProcessos()
        {
            string where = "";

            if (ddlGerentes.Value.ToString() != "-1")
                where += " AND p.CodigoGerenteProjeto = " + ddlGerentes.Value;

            if (ddlUnidades.Value.ToString() != "-1")
                where += " AND p.CodigoUnidadeNegocio = " + ddlUnidades.Value;

            if (ddlStatus.Value.ToString() != "-1")
                where += " AND p.CodigoStatusProjeto = " + ddlStatus.Value;

            if (txtDescricao.Text != "")
                where += " AND p.NomeProjeto LIKE '%" + txtDescricao.Text + "%'";


            dsProcessos = cDados.getListaProcessos(codigoEntidadeUsuarioResponsavel, int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString()), where);

            carregaTreeListProcessos(dsProcessos);
        }

        public void carregaTreeListProcessos(DataSet ds)
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
                ddlCombo.DataBind();


            }

            if (customItem != "")
            {
                ListEditItem lei = new ListEditItem(customItem, valueCustomItem);
                ddlCombo.Items.Insert(0, lei);
            }

            if (!IsPostBack && !IsCallback)
            {
                ddlCombo.SelectedIndex = 0;
            }
        }

        private void inicializaCombosFiltros()
        {
            DataSet dsCombos;

            //Carrega Combo de Gerentes
            dsCombos = cDados.getResponsaveisDemandaProcessosEntidade(codigoEntidadeUsuarioResponsavel, codigoUsuarioResponsavel, 'P', "");
            carregaCombo(dsCombos, "NomeUsuario", "CodigoUsuario", ddlGerentes, "Todos", "-1");

            //Carrega Combo de Unidades
            dsCombos = cDados.getUnidadesNegocioEntidade(codigoEntidadeUsuarioResponsavel, "");
            carregaCombo(dsCombos, "SiglaUnidadeNegocio", "CodigoUnidadeNegocio", ddlUnidades, "Todos", "-1");

            //Carrega Combo de Status
            dsCombos = cDados.getStatus(" AND TipoStatus = 'PRC'");

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
            getProcessos();
            cDados.setInfoSistema("CodigoStatusProcesso", ddlStatus.Items.Count > 0 ? ddlStatus.Value.ToString() : "-1");
        }

        protected void tlProjetos_HtmlDataCellPrepared(object sender, DevExpress.Web.ASPxTreeList.TreeListHtmlDataCellEventArgs e)
        {
        }
    }

}
