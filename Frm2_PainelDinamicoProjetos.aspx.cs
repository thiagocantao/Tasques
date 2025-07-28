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
    public partial class _Projetos_Frm2_PainelDinamicoProjetos : System.Web.UI.Page
    {
        dados cDados;
        int codigoEntidade, codigoUsuario, largura;
        int codigoProjeto = -1;

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

            codigoEntidade = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());
            codigoUsuario = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());

            if (Request.QueryString["CP"] != null && Request.QueryString["CP"].ToString() != "")
                codigoProjeto = int.Parse(Request.QueryString["CP"].ToString());

            string ResolucaoCliente = cDados.getInfoSistema("ResolucaoCliente").ToString().ToString();
            largura = int.Parse(ResolucaoCliente.Substring(0, ResolucaoCliente.IndexOf('x'))) / 2;

            //cDados.aplicaEstiloVisual(this);
            carregaItens();
            lblNomeProjeto.Font.Size = new FontUnit("15pt");
            lblNomeProjeto.Text = cDados.getNomeProjeto(codigoProjeto, "");
        }

        private void carregaItens()
        {
            string where = "";

            string ResolucaoCliente = cDados.getInfoSistema("ResolucaoCliente").ToString().ToString();
            //int altura = int.Parse(ResolucaoCliente.Substring(ResolucaoCliente.IndexOf('x') + 1)); ;
            int largura = 0, altura = 0;

            cDados.getLarguraAlturaTela(ResolucaoCliente, out largura, out altura);

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

            where += string.Format(" AND p.CodigoProjeto in (SELECT codigoProjeto FROM {0}.{1}.f_getProjetosUsuario({2}, {3}, {4})) ", cDados.getDbName(), cDados.getDbOwner(), codigoUsuario, codigoEntidade
                , Request.QueryString["CC"]);

            DataSet ds = cDados.getMetasVisaoCorporativaProjetos(codigoEntidade, where);

            nb01.Groups.Clear();
            nb2.Groups.Clear();

            int count = 0;
            int pagina = int.Parse(Request.QueryString["Page"].ToString());
            if (cDados.DataSetOk(ds))
            {
                int ultimoItemPagina = (pagina * 4) > ds.Tables[0].Rows.Count ? ds.Tables[0].Rows.Count : (pagina * 4);
                for (int i = 4 * pagina - 4; i < ultimoItemPagina; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    string textoGrupo = string.Format("<table><tr><td><img src='" + cDados.getPathSistema() + "imagens/{0}Menor.gif' /></td><td>{1}</td></tr></table>",
                        dr["Desempenho"].ToString(), dr["Meta"].ToString());

                    string urlGrafico = cDados.getPathSistema() + "_Portfolios/VisaoMetas/mt_004.aspx?CM=" + dr["CodigoMetaOperacional"] + "&CI=" + dr["CodigoIndicador"] + "&UM=" + dr["SiglaUnidadeMedida"] + "&CD=" + dr["CasasDecimais"] + "&Largura=" + ((largura - 100) / 2) + "&Altura=" + ((altura - 360) / 2);

                    string textoItem = string.Format(@"<table style='width:100%;'><tr><td height='5px'></td></tr><tr><td>Indicador: {2}</td></tr><tr><td height='5px'></td></tr><tr><td><table style='width:100%;'><tr><td><iframe id=""frm2_{1}"" frameborder=""0"" height=""{4}"" scrolling=""no"" src=""{0}"" width=""{3}""></iframe></td></tr></table></td></tr></table>"
                                                                , urlGrafico
                                                                , dr["CodigoMetaOperacional"].ToString()
                                                                , dr["NomeIndicador"].ToString()
                                                                , (largura - 100) / 2 + "px"
                                                                , (altura - 360) / 2 + "px");

                    NavBarGroup nb;

                    if (count % 2 == 0)
                    {
                        nb = nb01.Groups.Add(textoGrupo, dr["CodigoMetaOperacional"].ToString());
                    }
                    else
                    {
                        nb = nb2.Groups.Add(textoGrupo, dr["CodigoMetaOperacional"].ToString());
                    }

                    nb.Expanded = true;

                    count++;

                    NavBarItem nbi = nb.Items.Add(textoItem);

                    nbi.ClientEnabled = false;
                }
            }

            if (count == 0)
            {
                popUpStatusTela.ShowOnPageLoad = true;
            }
        }
    }

}
