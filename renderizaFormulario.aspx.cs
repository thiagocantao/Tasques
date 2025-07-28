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
using CDIS;
using DevExpress.Web;

namespace BriskPtf._Projetos
{
    public partial class _Projetos_renderizaFormulario : System.Web.UI.Page
    {
        dados cDados;

        int codigoModeloFormulario;
        int? codigoProjeto;
        int codigoUsuarioResponsavel;
        int codigoEntidadeUsuarioResponsavel;
        bool readOnly;
        string CssFilePath = "";
        string CssPostfix = "";

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
            if (cDados.getInfoSistema("IDUsuarioLogado") == null)
            {
                Response.Redirect("~/erros/erroInatividade.aspx");
            }

            //if (!IsPostBack)
            //    cDados.aplicaEstiloVisual(Page);

            cDados.getVisual(cDados.getInfoSistema("IDEstiloVisual").ToString(), ref CssFilePath, ref CssPostfix);

            codigoUsuarioResponsavel = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());
            codigoEntidadeUsuarioResponsavel = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());
            codigoModeloFormulario = int.Parse(Request.QueryString["CMF"].ToString());
            codigoProjeto = int.Parse(cDados.getInfoSistema("CodigoProjeto").ToString());
            if (Request.QueryString["RO"] != null)
                readOnly = Request.QueryString["RO"].ToString() == "S";
            renderizaFormulario();
        }

        private void renderizaFormulario()
        {
            if (codigoModeloFormulario > 0)
            {
                // se tem filtro por formularios
                string filtroCodigosFormularios = "";
                if (Request.QueryString["FF"] != null)
                {
                    filtroCodigosFormularios = Request.QueryString["FF"].ToString().Trim();
                }

                ASPxHiddenField hf = new ASPxHiddenField();
                Hashtable parametros = new Hashtable();
                Formulario myForm = new Formulario(cDados.classeDados, codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, codigoModeloFormulario, new Unit("100%"), new Unit("600px"), false, this.Page, parametros, ref hf, false);
                Control gridFormulario = myForm.constroiInterfaceFormulario(true, IsPostBack, null, codigoProjeto, CssFilePath, CssPostfix);

                form1.Controls.Add(gridFormulario);
            }
        }
    }

}
