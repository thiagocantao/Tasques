using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BriskPtf._Projetos
{
public partial class _Projetos_ItemMenuDashboard : System.Web.UI.Page
{
    private dados cDados;
    private int codigoCarteira;
    private int codigoEntidade;
    private int codigoProjeto;
    private int codigoUsuarioLogado;

    public Guid DashboardId
    {
        get
        {
            return Guid.Parse(Request.QueryString["id"]);
        }
    }

    protected void Page_Init(object sender, EventArgs e)
    {
        cDados = CdadosUtil.GetCdados(null);

        try
        {
            if (cDados.getInfoSistema("IDUsuarioLogado") == null)
                Response.Redirect("~/erros/erroInatividade.aspx");
        }
        catch
        {
            Response.RedirectLocation = String.Format(
                "{0}erros/erroInatividade.aspx", cDados.getPathSistema());
            Response.End();
        }

        codigoProjeto = int.Parse(Request.QueryString["IDProjeto"]);
        codigoUsuarioLogado = Convert.ToInt32(cDados.getInfoSistema("IDUsuarioLogado"));
        codigoEntidade = Convert.ToInt32(cDados.getInfoSistema("CodigoEntidade"));
        codigoCarteira = Convert.ToInt32(cDados.getInfoSistema("CodigoCarteira"));
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string queryString = string.Format("?id={0}&CodEntidade={1}&CodUsuario={2}&CodCarteira={3}&CodProjeto={4}&origem=RD",
            DashboardId, codigoEntidade, codigoUsuarioLogado, codigoCarteira, codigoProjeto);
        string url = string.Format("../_dashboard/VisualizadorDashboard.aspx{0}", queryString);
        Response.Redirect(url);
    }
}
}
