using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;
using BriskPtf.GanttHelper;

namespace BriskPtf._Projetos.DadosProjeto
{
public partial class _Projetos_DadosProjeto_Cronograma_tasques : System.Web.UI.Page
{
    dados cDados;

    public int codigoProjeto = 0;
    
    public string alturaGrafico = "", larguraGrafico = "", nenhumGrafico = "";
    public string nomeProjeto = "";
    private int versaoLinhaBase = -1;
    private CdisGanttHelper cdisGanttHelper;


    protected void Page_Init(object sender, EventArgs e)
    {
        DevExpress.Web.ASPxWebControl.SetIECompatibilityModeEdge();
        

        cDados = CdadosUtil.GetCdados(null);
        
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // a pagina não pode ser armazenada no cache
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        
        

        
        if (Request.QueryString["IDProjeto"] != null && Request.QueryString["IDProjeto"].ToString() != "")        {

            string comandoSQL = string.Format(@"
                SELECT  CodigoProjeto 
                FROM    {0}.{1}.CronogramaProjeto 
                WHERE   CodigoCronogramaProjeto = '{2}'
                ", cDados.getDbName(), cDados.getDbOwner(), Request.QueryString["IDProjeto"]);
            DataSet ds = cDados.getDataSet(comandoSQL);

            if(cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
                codigoProjeto = int.Parse(ds.Tables[0].Rows[0]["CodigoProjeto"].ToString());
        } 

        hfCodigoProjeto.Set("CodigoProjeto", codigoProjeto);
        hfCodigoProjeto.Set("NomeProjeto", nomeProjeto);
        
        
        Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"" src=""../../scripts/basicBryNTum/basic.js""></script>"));
        this.TH(this.TS("basic"));
        //Função que gera o gráfico
        geraGrafico();

    }
   
    //Função para geração do gráfico Gantt - Bryntum
    private void geraGrafico()
    {

        bool removerIdentacao = false;

        int codigoEntidade = -1;

        if (Request.QueryString["CodigoEntidade"] != null && Request.QueryString["CodigoEntidade"].ToString() != "")
        {
            codigoEntidade = int.Parse(Request.QueryString["CodigoEntidade"].ToString());
        }

        //Data Set contendo a tabela com os dados a serem carregados no gráfico de Gatt (Bryntum)
        var percentualConcluido = (int?)(null);
        var data = (DateTime?)(null);
        DataSet ds = cDados.getCronogramaGantt(codigoProjeto, "-1", versaoLinhaBase, removerIdentacao, false, false, percentualConcluido, data);


        bool indicaCronogramaTasques = cDados.indicaCronogramaVersaoTasques(codigoProjeto);

        if (cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
        {
            geraGraficoJsonTaskData(removerIdentacao, codigoEntidade);
            geraDependenciasJSON();

        }
        else
        {
            nenhumGrafico = cDados.getGanttVazio((int.Parse(alturaGrafico) - 30).ToString());

            alturaGrafico = "0";
        }

    }

    private void geraGraficoJsonTaskData( bool removerIdentacao, int codigoEntidade)
    {
        cdisGanttHelper = new CdisGanttHelper(cDados);
        var percentualConcluido = (int?)(null);
        var data = (DateTime?)(null);
        string taskStore = cdisGanttHelper.geraGraficoJsonTaskData(codigoProjeto, "-1", versaoLinhaBase, removerIdentacao, false, false, percentualConcluido, data);
        //atribui o valor do caminho do JSON a ser carregado
        string caminhoJSON = "../.." + cdisGanttHelper.PathToBryntumFile;
        string scripts = @"<script type=""text/javascript"">var urlJSON = """ + caminhoJSON + @""";
                                                                                 var dataInicio = " + cdisGanttHelper.DataInicio + @";
                                                                                 var dataTermino = " + cdisGanttHelper.DataTermino + @";
                                                </script>";
        Literal literal = new Literal();
        literal.Text = scripts;
        Header.Controls.Add(literal);
    }

    // Gera as tarefas filhas
    private void geraDependenciasJSON()
    {
        //cria  a variável para armazenar o JSON_BryNTum
        string json = cdisGanttHelper.retornaJSONDependencias(codigoProjeto);
        string caminhoJSON = "../.." + cdisGanttHelper.PathToDependencyFile;
        string scripts = @"<script type=""text/javascript"">var urlJSONDep = """ + caminhoJSON + @""";
                                                </script>";
        Literal literal = new Literal();
        literal.Text = scripts;
        Header.Controls.Add(literal);
    }    
}

}
