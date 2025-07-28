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

namespace BriskPtf._Projetos
{
    public partial class _Projetos_visaoDemandas : System.Web.UI.Page
    {
        dados cDados;
        public string alturaTela = "";

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
            // =========================== Verifica se a sessão existe FIM ========================


            //if (!IsPostBack)
            //    cDados.aplicaEstiloVisual(Page);

            defineLarguraTela();
        }

        private void defineLarguraTela()
        {
            string ResolucaoCliente = cDados.getInfoSistema("ResolucaoCliente").ToString().ToString();
            int altura = int.Parse(ResolucaoCliente.Substring(ResolucaoCliente.IndexOf('x') + 1)); ;

            alturaTela = (altura - 150).ToString() + "px";
        }
    }

}
