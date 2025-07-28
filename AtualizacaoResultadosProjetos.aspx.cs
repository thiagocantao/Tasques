using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using DevExpress.Web;

namespace BriskPtf._Projetos
{
    public partial class _Projetos_AtualizacaoResultadosProjetos : System.Web.UI.Page
    {
        dados cDados;

        private int idUsuarioLogado;
        private int codigoEntidade;
        private string resolucaoCliente = "";
        private int alturaPrincipal = 0;

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

            //Get dado do usuario logado, e do qual entidad ele pertenece.        
            idUsuarioLogado = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());
            codigoEntidade = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // a pagina não pode ser armazenada no cache.
            Response.Cache.SetCacheability(HttpCacheability.NoCache); // Ok

            HearderOnTela();

            populaGrid();

            resolucaoCliente = cDados.getInfoSistema("ResolucaoCliente").ToString();
            defineAlturaTela(resolucaoCliente);

            if (!IsPostBack)
            {
                cDados.excluiNiveisAbaixo(1);
                cDados.insereNivel(1, this);
                Master.geraRastroSite();
                Master.verificaAcaoFavoritos(true, "Atualização de Resultados", "ATLRPR", "PRJ", -1, Resources.traducao.adicionar_aos_favoritos);
            }
        }

        //GridPrincipal
        private void populaGrid()
        {
            DataSet ds = cDados.getIndicadoresOperacionaisProjetos(idUsuarioLogado, codigoEntidade, "");

            if (cDados.DataSetOk(ds))
            {
                gvDados.DataSource = ds.Tables[0];
                gvDados.DataBind();
            }
        }

        #region VARIOS

        private void HearderOnTela()
        {
            // inclui o arquivo de scripts desta tela
            Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"">var pastaImagens = ""../imagens"";</script>"));
            Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"" src=""../scripts/AtualizacaoResultadosProjetos.js""></script>"));
            this.TH(this.TS("AtualizacaoResultadosProjetos"));
            //cDados.aplicaEstiloVisual(Page);
        }

        private void defineAlturaTela(string resolucaoCliente)
        {
            // Calcula a altura da tela
            int largura1 = 0;
            int altura1 = 0;

            cDados.getLarguraAlturaTela(resolucaoCliente, out largura1, out altura1);


            alturaPrincipal = altura1;

            int altura = (alturaPrincipal - 135);

            if (altura1 > 0)
                gvDados.Settings.VerticalScrollableHeight = altura1 - 280;

            gvDados.Width = new Unit("100%");
        }

        #endregion


        protected void gvDados_AutoFilterCellEditorInitialize(object sender, ASPxGridViewEditorEventArgs e)
        {

        }
    }

}
