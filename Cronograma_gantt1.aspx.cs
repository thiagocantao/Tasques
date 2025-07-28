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
using System.Web.Hosting;
using System.IO;
using System.Xml;
using CDIS;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace BriskPtf._Projetos.DadosProjeto
{
public partial class _Projetos_DadosProjeto_Cronograma_gantt1 : System.Web.UI.Page
{
    dados cDados;

    //pega a data e hora atual do sistema
    string dataHora;

    public int codigoProjeto = 0;

    public string grafico_xml = "";
    public string alturaGrafico = "", larguraGrafico = "", nenhumGrafico = "";
    public string nomeProjeto = "";
    public int codigoEntidadeUsuarioResponsavel = -1;
    int codigoUsuario = -1;
    DataSet dsCrono;
    string tarefasAdicionadas = ";";
    int index = 0;

    protected void Page_Init(object sender, EventArgs e)
    {
        DevExpress.Web.ASPxWebControl.SetIECompatibilityModeEdge();
        System.Collections.Specialized.OrderedDictionary listaParametrosDados = new System.Collections.Specialized.OrderedDictionary();

        listaParametrosDados["RemoteIPUsuario"] = Session["RemoteIPUsuario"] + "";
        listaParametrosDados["NomeUsuario"] = Session["NomeUsuario"] + "";

        cDados = CdadosUtil.GetCdados(listaParametrosDados);
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

        codigoEntidadeUsuarioResponsavel = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());
        codigoUsuario = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // a pagina não pode ser armazenada no cache
        Response.Cache.SetCacheability(HttpCacheability.NoCache);

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

        dataHora = DateTime.Now.ToString().Replace("/", "-").Replace(":", "_") + ".xml";

        if (Request.QueryString["IDProjeto"] != null && Request.QueryString["IDProjeto"].ToString() != "")
        {
            codigoProjeto = int.Parse(Request.QueryString["IDProjeto"].ToString());
        }

        if (Request.QueryString["NP"] != null && Request.QueryString["NP"].ToString() != "")
        {
            nomeProjeto = Request.QueryString["NP"].ToString();
        }

        if (!IsPostBack)
        {
            int codigoUsuario = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());
            int codigoEntidade = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());

            cDados.VerificaAcessoTelaSemMaster(this, codigoUsuario, codigoEntidade, codigoProjeto, "null", "PR", 0, "null", "PR_CnsCrn");
        }

        Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"" src=""../../scripts/basicBryNTum/basic1.js""></script>"));
        this.TH(this.TS("basic1"));
        hfCodigoProjeto.Set("CodigoProjeto", codigoProjeto);
        hfCodigoProjeto.Set("NomeProjeto", nomeProjeto);

        defineLarguraTela();     

        //Função que gera o gráfico
       geraGrafico();
        geraDependencias();
    }

    private void defineLarguraTela()
    {
        string ResolucaoCliente = cDados.getInfoSistema("ResolucaoCliente").ToString().ToString();
        int largura = int.Parse(ResolucaoCliente.Substring(0, ResolucaoCliente.IndexOf('x')));
        int altura = int.Parse(ResolucaoCliente.Substring(ResolucaoCliente.IndexOf('x') + 1));

        alturaGrafico = (altura - 250).ToString();
        larguraGrafico = (largura - 175).ToString();
    }

    //Função para geração do gráfico (Quantidade de Projetos por Desempenho) - PIZZA
    private void geraGrafico()
    {
        //cria  a variável para armazenar o XML_BryNTum
        string xml = "";

        //cria uma variável com o nome e o caminho do XML do gráfico do BryNTum
        string nomeGrafico_bry = @"/ArquivosTemporarios/GanttBryNTum_" + cDados.getInfoSistema("IDUsuarioLogado").ToString() + "_" + dataHora;

        string where = "";

        int versaoLinhaBase = -1;

        //Data Set contendo a tabela com os dados a serem carregados no gráfico de PIZZA
        var percentualConcluido = (int?)(null);
        var data = (DateTime?)(null);
        dsCrono = cDados.getCronogramaGantt(codigoProjeto, "-1", versaoLinhaBase, true, false, false, percentualConcluido, data);
        string dataInicio = "";
        string dataTermino = "";


        xml += "<Tasks>";

        foreach (DataRow dr in dsCrono.Tables[0].Rows)
        {
            if (tarefasAdicionadas.Contains(";" + dr["CodigoTarefa"] + ";") == false || index == 0)
            {
                if (index == 0)
                {
                    dataInicio = string.Format("Sch.util.Date.add(new Date({0:yyyy, M, d}), Sch.util.Date.MONTH, -2)", dr["Inicio"]);
                    dataTermino = string.Format("Sch.util.Date.add(new Date({0:yyyy, M, d}), Sch.util.Date.MONTH, 2)", dr["Termino"]);
                }

                string codigoTarefaIn = dr["CodigoTarefa"].ToString();
                string codigoRealTarefaIn = dr["CodigoRealTarefa"].ToString();
                string sumariaIn = dr["IndicaTarefaSumario"].ToString();
                string nomeIn = dr["NomeTarefa"].ToString();
                string dataInicioIn = string.Format("{0:yyyy-MM-dd}", dr["Inicio"]) + "T00:00:00";
                string dataTerminoIn = string.Format("{0:yyyy-MM-dd}", dr["Termino"]) + "T00:00:00";
                string duracaoIn = string.Format("{0:n0}", dr["Duracao"]);
                string percentualIn = string.Format("{0:n0}", dr["Concluido"]);
                xml += retornaXML(codigoTarefaIn, codigoRealTarefaIn, sumariaIn, nomeIn, dataInicioIn, dataTerminoIn, duracaoIn, percentualIn);
            }
        }

        xml += "</Tasks>";

        //escreve o arquivo xml de quantidade de projetos por entidade
        escreveXML(xml, nomeGrafico_bry);

        //atribui o valor do caminho do XML a ser carregado
        string caminhoXML = "../.." + nomeGrafico_bry;

        Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"">var urlXML = """ + caminhoXML + @""";
                                                                                 var dataInicio = " + dataInicio + @";
                                                                                 var dataTermino = " + dataTermino + @";
                                                </script>"));
    }

    private void geraDependencias()
    {
        //cria  a variável para armazenar o XML_BryNTum
        string xml = "";

        //cria uma variável com o nome e o caminho do XML do gráfico do BryNTum
        string nomeGrafico_bry = @"/ArquivosTemporarios/GanttBryNTumDependencias_" + cDados.getInfoSistema("IDUsuarioLogado").ToString() + "_" + dataHora;

        xml = retornaXMLDependencias();

        //escreve o arquivo xml de quantidade de projetos por entidade
        escreveXML(xml, nomeGrafico_bry);

        //atribui o valor do caminho do XML a ser carregado
        string caminhoXML = "../.." + nomeGrafico_bry;

        Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"">var urlXMLDep = """ + caminhoXML + @""";
                                                </script>"));
    }

    private string escreveXML(string xml, string nome)
    {
        StreamWriter strWriter;

        //cria um novo arquivo xml e abre para escrita
        strWriter = new StreamWriter(HostingEnvironment.ApplicationPhysicalPath + nome, false, System.Text.Encoding.UTF8);
        
        //escrever o corpo do XML no arquivo xml criado
        strWriter.Write(xml);
        //fecha o arquivo criado
        strWriter.Close();

        return nome;
    }

    private string retornaXML(string codigoTarefa, string codigoRealTarefa, string sumaria, string nome,string dataInicio, string dataTermino, string duracao, string percentual)
    {
        tarefasAdicionadas += codigoTarefa + ";";
        index++;

        string xml = string.Format(@"
                    <Task>
                        <Id>{0}</Id>
                        <leaf>{7}</leaf>
                        <Name>{1}</Name>
                        <Duration>{4}</Duration>
                        <PercentDone>{5}</PercentDone>
                        <StartDate>{2}</StartDate>
                        <EndDate>{3}</EndDate>{6}
                        ", codigoTarefa
                         , nome
                         , dataInicio
                         , dataTermino
                         , duracao
                         , percentual
                         , sumaria == "1" ? @"
                         <expanded>1</expanded>" : ""
                         , sumaria == "1" ? "0" :"1");

        if (sumaria == "1")
        {
            xml += "<Tasks>";
            foreach (DataRow dr in dsCrono.Tables[0].Select("TarefaSuperior = '" + codigoRealTarefa + "'"))
            {
                string codigoTarefaIn = dr["CodigoTarefa"].ToString();
                string codigoRealTarefaIn = dr["CodigoRealTarefa"].ToString();
                string sumariaIn = dr["IndicaTarefaSumario"].ToString();
                string nomeIn = dr["NomeTarefa"].ToString();
                string dataInicioIn = string.Format("{0:yyyy-MM-dd}", dr["Inicio"]) + "T00:00:00";
                string dataTerminoIn = string.Format("{0:yyyy-MM-dd}", dr["Termino"]) + "T00:00:00";
                string duracaoIn = string.Format("{0:n0}", dr["Duracao"]);
                string percentualIn = string.Format("{0:n0}", dr["Concluido"]);
                xml += retornaXML(codigoTarefaIn, codigoRealTarefaIn, sumariaIn, nomeIn, dataInicioIn, dataTerminoIn, duracaoIn, percentualIn);
            }
            xml += "</Tasks>";
        }

        xml += @"
                </Task>
                ";

        return xml;
    }

    private string retornaXMLDependencias()
    {
        string xml = "";
        string tipoConector = "";

        DataSet ds = cDados.getDataSet(string.Format(@"
                SELECT tcpp.codigoTarefa AS TarefaTo, tcpp.codigoTarefaPredecessora AS TarefaFrom, tipoLatencia
                  FROM {0}.{1}.[TarefaCronogramaProjetoPredecessoras] tcpp INNER JOIN
			           {0}.{1}.CronogramaProjeto cp ON (cp.CodigoCronogramaProjeto = tcpp.CodigoCronogramaProjeto 
													AND cp.CodigoProjeto = {2})", cDados.getDbName(), cDados.getDbOwner(), codigoProjeto));

        if (cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
        {
            xml += "<Links>";

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                tipoConector = dr["tipoLatencia"].ToString();
                                
                if (tipoConector == "" || tipoConector == "TI")
                    tipoConector = "2";
                else if (tipoConector == "II")
                    tipoConector = "0";
                else if (tipoConector == "TT")
                    tipoConector = "3";
                else if (tipoConector == "IT")
                    tipoConector = "1";

                xml += string.Format(@"
  <Link>
    <From>{0}</From>
    <To>{1}</To>
    <Type>{2}</Type>
  </Link>", dr["TarefaFrom"]
          , dr["TarefaTo"]
          , tipoConector);

            }

            xml += "</Links>";
        }

        return xml;
    }


    protected void callbackSalvar_Callback(object source, CallbackEventArgs e)
    {
        string jsonInseridas = e.Parameter.Split('¥')[0];
        string jsonEditadas = e.Parameter.Split('¥')[1];
        string jsonExcluidas = e.Parameter.Split('¥')[2];

        if (jsonInseridas.Trim() != "")
        {
            DataTable dtInseridas = (DataTable)JsonConvert.DeserializeObject("[" + jsonInseridas + "]", (typeof(DataTable)));
        }

        if (jsonEditadas.Trim() != "")
        {
            DataTable dtEditadas = (DataTable)JsonConvert.DeserializeObject("[" + jsonEditadas + "]", (typeof(DataTable)));
        }

        if (jsonExcluidas.Trim() != "")
        {
            DataTable dtExcluidas = (DataTable)JsonConvert.DeserializeObject("[" + jsonExcluidas+ "]", (typeof(DataTable)));
        }
    }
}

}
