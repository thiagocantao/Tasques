using CDIS;
using BriskPtf.DataSources;
using DevExpress.Utils;
using DevExpress.Web;
using DevExpress.Web.ASPxHtmlEditor;
using DevExpress.Web.ASPxPivotGrid;
using DevExpress.Web.ASPxScheduler;
using DevExpress.Web.ASPxTreeList;
using DevExpress.XtraReports.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DevExpress.DashboardWeb;

/// <summary>
/// Summary description for myDados
/// </summary>


namespace BriskPtf
{
    public partial class dados
    {
        #region definição da classe

        public ClasseDados classeDados;
        private string comandoSQL;


        private string PathDB = System.Configuration.ConfigurationManager.AppSettings["pathDB"].ToString();
        private string IDProduto = System.Configuration.ConfigurationManager.AppSettings["IDProduto"].ToString();
        private string tipoBancoDados = System.Configuration.ConfigurationManager.AppSettings["tipoBancoDados"].ToString();
        private string enviarEmailSistema = System.Configuration.ConfigurationManager.AppSettings["enviarEmailSistema"] != null ? System.Configuration.ConfigurationManager.AppSettings["enviarEmailSistema"].ToString() : "S";
        private string bancodb = string.Empty;
        private string Ownerdb = string.Empty;
        public string corSatisfatorio, corAtencao, corCritico, corFundoBullets, usarGradiente, corPrevisto, corPrevistoLB1, corReal, corTendencia, corExcelente, usarBordasArredondadas, corDemandasSimples, corDemandasComplexas, corProjetos, corProcessos, mostraLaranja;
        private int fonte = 8;
        public string corAguardandoAnalista, corAguardandoAprovador, corAprovado, corReprovado;

        private string _key = "#COMANDO#CDIS!";
        private char Delimitador_Erro = '¥';
        string strConn = "";

        #region Informação da versão para cache busting de arquivos css e js
        private static readonly Lazy<string> _dataHoraUltimaVersao =
            new Lazy<string>(() => getDataHoraUltimaVersaoBrisk());

        public static string dataHoraUltimaVersao
        {
            get
            {
                return _dataHoraUltimaVersao.Value;
            }
        }

        #endregion


        #region Campos para funcionalidade personalização da lista

        DsListaProcessos dsLP;
        int codigoLista;

        private string _ConnectionString;
        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_ConnectionString))
                    _ConnectionString = classeDados.getStringConexao();
                return _ConnectionString;
            }
            set { _ConnectionString = value; }
        }

        #endregion

        public dados(int codigoEntidade, OrderedDictionary parametrosDados)
        {
            classeDados = getDados();

            if (parametrosDados != null && parametrosDados.Contains("NomeUsuario"))
            {
                classeDados.nomeUsuario = parametrosDados["NomeUsuario"].ToString();
                classeDados.maquinaUsuario = parametrosDados["RemoteIPUsuario"].ToString();
            }

            bancodb = classeDados.databaseNameCdis;
            Ownerdb = System.Configuration.ConfigurationManager.AppSettings["dbOwner"].ToString();
            strConn = classeDados.getStringConexao() + ";Min Pool Size=2";

            if (parametrosDados == null || !parametrosDados.Contains("WSPortal"))
            {
                //codigoEntidade = 1;
                if (codigoEntidade == -1)
                {
                    codigoEntidade = getInfoSistema("CodigoEntidade") == null ? 1 : int.Parse(getInfoSistema("CodigoEntidade").ToString());
                }

                getVariaveisGlobais(codigoEntidade);
            }
        }

        public dados(OrderedDictionary parametrosDados)
            : this(-1, parametrosDados)
        {
        }

        public string getIdioma()
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            string idioma = cultureInfo.ToString();
            return idioma.Substring(0, 2).ToLower();
        }

        public string converteDataBd(string data)
        {
            string idioma = this.getIdioma();
            if (idioma == "en")
            {
                return data.Substring(6, 4) + "-" + data.Substring(0, 2) + "-" + data.Substring(3, 2);
            }
            else
            {
                return data.Substring(6, 4) + "-" + data.Substring(3, 2) + "-" + data.Substring(0, 2);
            }
        }

        public string converteDataDb(string data)
        {
            return this.converteDataBd(data);
        }

        public string converteDataHoraBd(string dataHora)
        {
            dataHora = dataHora.Trim();
            string data = dataHora.Substring(0, 10);
            string hora = "00:00:00";
            if (dataHora.Length > 10)
            {
                hora = dataHora.Substring(11).Trim();
                if (hora.Length == 5)
                {
                    hora += ":00";
                }
            }
            return this.converteDataBd(data) + " " + hora;
        }

        public string converteDataHoraDb(string dataHora)
        {
            return this.converteDataHoraBd(dataHora);
        }

        public bool validaEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void getVariaveisGlobais(int codigoEntidade)
        {
            // string codigoEntidade = "1";// getInfoSistema("CodigoEntidade") == null ? "1" : getInfoSistema("CodigoEntidade").ToString();

            DataSet ds = getParametrosSistema(codigoEntidade, "corExcelente", "corSatisfatorio", "corAtencao", "corCritico",
                "corPrevisto", "corPrevistoLB1", "corReal", "corTendencia", "corFundoBullets", "usarGradiente", "usarBordasArredondadas", "MostraFisicoLaranja", "monetaryLocale");

            DataTable dt = ds.Tables[0];

            corExcelente = (dt.Rows[0]["corExcelente"] + "" == "") ? "1648DC" : dt.Rows[0]["corExcelente"].ToString().Replace("#", "");
            corSatisfatorio = (dt.Rows[0]["corSatisfatorio"] + "" == "") ? "008844" : dt.Rows[0]["corSatisfatorio"].ToString().Replace("#", "");
            corAtencao = (dt.Rows[0]["corAtencao"] + "" == "") ? "FFFF44" : dt.Rows[0]["corAtencao"].ToString().Replace("#", "");
            corCritico = (dt.Rows[0]["corCritico"] + "" == "") ? "DD0100" : dt.Rows[0]["corCritico"].ToString().Replace("#", "");
            corPrevisto = (dt.Rows[0]["corPrevisto"] + "" == "") ? "B1B3AC" : dt.Rows[0]["corPrevisto"].ToString().Replace("#", "");
            corPrevistoLB1 = (dt.Rows[0]["corPrevistoLB1"] + "" == "") ? "FFFF44" : dt.Rows[0]["corPrevistoLB1"].ToString().Replace("#", "");
            corReal = (dt.Rows[0]["corReal"] + "" == "") ? "486A9D" : dt.Rows[0]["corReal"].ToString().Replace("#", "");
            corTendencia = (dt.Rows[0]["corTendencia"] + "" == "") ? "00CCFF" : dt.Rows[0]["corTendencia"].ToString().Replace("#", "");
            corFundoBullets = (dt.Rows[0]["corFundoBullets"] + "" == "") ? "AAAAA6" : dt.Rows[0]["corFundoBullets"].ToString().Replace("#", "");
            usarGradiente = (dt.Rows[0]["usarGradiente"].ToString() == "S") ? " plotGradientColor=\"FFFFFF\" " : " plotGradientColor=\"\" ";
            usarBordasArredondadas = (dt.Rows[0]["usarBordasArredondadas"].ToString() == "S") ? " useRoundEdges=\"1\" " : "";
            mostraLaranja = (dt.Rows[0]["MostraFisicoLaranja"] + "" == "") ? "N" : dt.Rows[0]["MostraFisicoLaranja"].ToString();

            corDemandasSimples = "8EABDB";
            corDemandasComplexas = "CEBEAA";
            corProjetos = "D8D99B";
            corProcessos = "8FBC8F";

            //cores utilizadas no controle de versões 
            corAguardandoAnalista = (dt.Rows[0]["corAtencao"] + "" == "") ? "FFFF44" : dt.Rows[0]["corAtencao"].ToString();
            corAguardandoAprovador = (dt.Rows[0]["corPrevisto"] + "" == "") ? "B1B3AC" : dt.Rows[0]["corPrevisto"].ToString();
            corAprovado = (dt.Rows[0]["corExcelente"] + "" == "") ? "1648DC" : dt.Rows[0]["corExcelente"].ToString();
            corReprovado = (dt.Rows[0]["corCritico"] + "" == "") ? "DD0100" : dt.Rows[0]["corCritico"].ToString();
            // personalizando o formato de moeda a ser usado no sistema
            string monetaryLocale = (dt.Rows[0]["monetaryLocale"] + "" == "") ? "" : dt.Rows[0]["monetaryLocale"].ToString();
            if (!string.IsNullOrEmpty(monetaryLocale))
            {
                try  // se o usuário informar no parâmetro um valor que não corresponda a uma cultura, ignora a modificação 
                {
                    CultureInfo moneyCult = new CultureInfo(monetaryLocale);
                    CultureInfo CurrentCult = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    CurrentCult.NumberFormat.CurrencyDecimalDigits = moneyCult.NumberFormat.CurrencyDecimalDigits;
                    CurrentCult.NumberFormat.CurrencyDecimalSeparator = moneyCult.NumberFormat.CurrencyDecimalSeparator;
                    CurrentCult.NumberFormat.CurrencyGroupSeparator = moneyCult.NumberFormat.CurrencyGroupSeparator;
                    CurrentCult.NumberFormat.CurrencyGroupSizes = moneyCult.NumberFormat.CurrencyGroupSizes;
                    CurrentCult.NumberFormat.CurrencyNegativePattern = moneyCult.NumberFormat.CurrencyNegativePattern;
                    CurrentCult.NumberFormat.CurrencyPositivePattern = moneyCult.NumberFormat.CurrencyPositivePattern;
                    CurrentCult.NumberFormat.CurrencySymbol = moneyCult.NumberFormat.CurrencySymbol;

                    CultureInfo CurrentUICult = (CultureInfo)CultureInfo.CurrentUICulture.Clone();
                    CurrentUICult.NumberFormat.CurrencyDecimalDigits = moneyCult.NumberFormat.CurrencyDecimalDigits;
                    CurrentUICult.NumberFormat.CurrencyDecimalSeparator = moneyCult.NumberFormat.CurrencyDecimalSeparator;
                    CurrentUICult.NumberFormat.CurrencyGroupSeparator = moneyCult.NumberFormat.CurrencyGroupSeparator;
                    CurrentUICult.NumberFormat.CurrencyGroupSizes = moneyCult.NumberFormat.CurrencyGroupSizes;
                    CurrentUICult.NumberFormat.CurrencyNegativePattern = moneyCult.NumberFormat.CurrencyNegativePattern;
                    CurrentUICult.NumberFormat.CurrencyPositivePattern = moneyCult.NumberFormat.CurrencyPositivePattern;
                    CurrentUICult.NumberFormat.CurrencySymbol = moneyCult.NumberFormat.CurrencySymbol;

                    CultureInfo.DefaultThreadCurrentCulture = CurrentCult;
                    CultureInfo.DefaultThreadCurrentUICulture = CurrentUICult;

                    Thread.CurrentThread.CurrentCulture = CurrentCult;
                    Thread.CurrentThread.CurrentUICulture = CurrentUICult;
                }
                catch
                { }
            }
        }

        private ClasseDados getDados()
        {
            return new ClasseDados(tipoBancoDados, PathDB, IDProduto, Ownerdb, "", 2);
        }

        public string getDbOwner()
        {
            return this.Ownerdb;
        }

        public string getDbName()
        {
            return this.bancodb;
        }

        public string getDbNameSuporte()
        {
            string retorno = "desenv_SuporteCliente"; //se não encontrar aponta para base de desenvolvimento
            DataSet ds = getParametrosSistema("dbNameSuporte");

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]) && ds.Tables[0].Rows[0]["dbNameSuporte"] != null && ds.Tables[0].Rows[0]["dbNameSuporte"].ToString() != "")
            {
                retorno = ds.Tables[0].Rows[0]["dbNameSuporte"].ToString();

            }
            return retorno;
        }

        public int insert(string nomeTabela, ListDictionary dados, bool retornaIdentity)
        {
            try
            {
                int afetados = 0;
                string comandoSQL = classeDados.getInsert(nomeTabela, dados);
                if (retornaIdentity)
                {
                    comandoSQL += '\n' + "Select scope_identity()";
                    DataSet ds = getDataSet(comandoSQL);

                    if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                        return int.Parse(ds.Tables[0].Rows[0][0].ToString());
                    else
                        return -1;
                }
                else
                {
                    execSQL(comandoSQL, ref afetados);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool update(string nomeTabela, ListDictionary dados, string where)
        {
            try
            {
                int afetados = 0;
                string comandoSQL = classeDados.getUpdate(nomeTabela, dados, where);
                execSQL(comandoSQL, ref afetados);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool delete(string nomeTabela, string where)
        {
            try
            {
                int afetados = 0;
                string comandoSQL = classeDados.getDelete(nomeTabela, where);
                execSQL(comandoSQL, ref afetados);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataSet getDataSet(string comandoSQL)
        {
            try
            {
                return classeDados.getDataSet(comandoSQL);
            }
            catch (Exception ex)
            {
                InsereRegistroOcorrencia(ex, comandoSQL);

                throw ex;
            }
        }

        public DataSet getMetadadosTabelaBanco(string nomeDaTabela, string nomeDaColuna)
        {
            string comandoSQL = string.Format(@"
        SELECT o.[name]	AS [Tabela]
             , c.[name]					AS [Coluna]
		     , t.[name]					AS [Type]
		     , c.[max_length]		AS [Tamanho]
		     , c.[precision]		AS [Precisao_camposDecimal]
		     , c.[scale]				AS [Escala_camposDecimal]
        FROM sys.objects o
        INNER JOIN sys.all_columns c on (c.[object_id] = o.[object_id])
        INNER JOIN sys.types t on (t.[system_type_id] = c.[user_type_id])
        INNER JOIN sys.systypes st on (st.[xtype] = t.[system_type_id])
             WHERE o.[name] = '{0}'		-- nome da tabela
               AND c.[name]	= '{1}'     --
               AND o.[type] = 'U'", nomeDaTabela, nomeDaColuna);
            return classeDados.getDataSet(comandoSQL);
        }

        public string InsereRegistroOcorrencia(Exception exception, string dadosSesao)
        {
            var exceptionMessage = exception.Message;

            var sqlCommand = @"
INSERT INTO [dbo].[Ocorrencia]
           ([IdentificadorOcorrencia]
           ,[Mensagem]
           ,[DataHoraOcorrencia]
           ,[CodigoUsuarioLogado]
           ,[RastreamentoDePilha]
           ,[DadosDeSessao])
     VALUES
           (@IdentificadorOcorrencia
           ,@Mensagem
           ,@DataHoraOcorrencia
           ,@CodigoUsuarioLogado
           ,@RastreamentoDePilha
           ,@DadosDeSessao)";

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(sqlCommand, connection))
                {
                    const int maxCharCount = 512;
                    var identificadorOcorrencia = Guid.NewGuid();
                    var mensagem = exceptionMessage.Length > maxCharCount ?
                        exceptionMessage.Substring(0, maxCharCount) :
                        exceptionMessage;
                    var codigoUsuarioLogado = getInfoSistema("IDUsuarioLogado") ?? -1;

                    command.Parameters.AddWithValue("IdentificadorOcorrencia", identificadorOcorrencia);
                    command.Parameters.AddWithValue("Mensagem", mensagem);
                    command.Parameters.AddWithValue("DataHoraOcorrencia", DateTime.Now);
                    command.Parameters.AddWithValue("CodigoUsuarioLogado", codigoUsuarioLogado);
                    command.Parameters.AddWithValue("RastreamentoDePilha", (exception.StackTrace != null) ? exception.StackTrace : "");
                    command.Parameters.AddWithValue("DadosDeSessao", dadosSesao);

                    command.CommandTimeout = CDIS.ClasseDados.TimeOutSqlCommand;

                    var affectedRows = command.ExecuteNonQuery();
                    const int quantidadeRegistrosEsperada = 1;

                    if (affectedRows == quantidadeRegistrosEsperada)
                        return identificadorOcorrencia.ToString();
                }
            }

            return string.Empty;
        }

        public bool execSQL(string comandoSQL, ref int registrosAfetados)
        {
            return classeDados.execSQL(comandoSQL, ref registrosAfetados);
        }

        public bool DataSetOk(DataSet dataSet)
        {
            return (dataSet == null || dataSet.Tables.Count == 0) ? false : true;
        }

        public bool DataTableOk(DataTable dataTable)
        {
            return (dataTable == null || dataTable.Rows.Count == 0) ? false : true;
        }

        public string getSQLComboUsuarios(int idEntidadeLogada, string filtro, string where)
        {
            string whereNomeUsuario = filtro == "" ? "" : "AND NomeUsuario LIKE '%" + filtro + "%'";
            string comandoSQL =
              string.Format(@"
                SELECT  ROW_NUMBER()over(order by IndicaUsuarioAtivoUnidadeNegocio DESC, nomeUsuario) as rn , 
                                us.CodigoUsuario, 
                                us.NomeUsuario, 
                                us.EMail,
                                ISNULL(uun.IndicaUsuarioAtivoUnidadeNegocio, 'S') AS IndicaUsuarioAtivoUnidadeNegocio,
                                CASE WHEN ISNULL(uun.IndicaUsuarioAtivoUnidadeNegocio, 'S') = 'S' THEN '' ELSE '({4})' END AS StatusUsuario
                  FROM Usuario us
                 INNER JOIN UsuarioUnidadeNegocio uun on us.CodigoUsuario = uun.CodigoUsuario
                 WHERE uun.CodigoUnidadeNegocio = {0}
                   AND us.DataExclusao IS NULL
                   {3}
                   {2}
                  ", idEntidadeLogada, filtro, where, whereNomeUsuario, Resources.traducao.inativo);

            return comandoSQL;
        }

        public string getSQLComboUsuariosPorID(int idEntidadeLogada)
        {
            string comandoSQL =
              string.Format(@"
                SELECT us.CodigoUsuario
                      ,us.NomeUsuario
                      ,us.EMail
                      ,ISNULL(uun.IndicaUsuarioAtivoUnidadeNegocio, 'S') AS IndicaUsuarioAtivoUnidadeNegocio
                      ,CASE WHEN ISNULL(uun.IndicaUsuarioAtivoUnidadeNegocio, 'S') = 'S' THEN '' ELSE '(INATIVO)' END AS StatusUsuario
                  FROM Usuario  us INNER JOIN 
                       UsuarioUnidadeNegocio uun on us.CodigoUsuario = uun.CodigoUsuario
                 WHERE uun.CodigoUnidadeNegocio = {0}
                   AND (us.CodigoUsuario = @ID) 
                 ORDER BY ISNULL(uun.IndicaUsuarioAtivoUnidadeNegocio, 'S') DESC, us.NomeUsuario
                  ", idEntidadeLogada);

            return comandoSQL;
        }

        public void populaComboVirtual(SqlDataSource ds, string comandoSQL, ASPxComboBox comboBox, int startIndex, int endIndex)
        {
            ds.SelectCommand =
                   string.Format(@"SELECT *
                                 FROM ({0}) as u
                                WHERE u.rn Between @startIndex and @endIndex
                               ORDER BY IndicaUsuarioAtivoUnidadeNegocio DESC, NomeUsuario", comandoSQL);

            ds.SelectParameters.Clear();

            ds.SelectParameters.Add("startIndex", TypeCode.Int64, (startIndex + 1).ToString());
            ds.SelectParameters.Add("endIndex", TypeCode.Int64, (endIndex + 1).ToString());

            comboBox.Columns.Clear();
            ListBoxColumn lbc1 = new ListBoxColumn("NomeUsuario", Resources.traducao.nome, 200);
            ListBoxColumn lbc2 = new ListBoxColumn("EMail", Resources.traducao.email, 350);
            ListBoxColumn lbc3 = new ListBoxColumn("StatusUsuario", Resources.traducao.status, 80);

            comboBox.Columns.Insert(0, lbc1);
            comboBox.Columns.Insert(1, lbc2);
            comboBox.Columns.Insert(2, lbc3);

            if (string.IsNullOrEmpty(comboBox.DataSourceID))
            {
                comboBox.DataSource = ds;
            }
            comboBox.DataBind();
        }

        public void populaComboVirtualGeral(SqlDataSource ds, string comandoSQL, ASPxComboBox comboBox, int startIndex, int endIndex)
        {
            ds.SelectCommand =
                   string.Format(@"SELECT *
                                 FROM ({0}) as u
                                WHERE u.rn Between @startIndex and @endIndex", comandoSQL);


            ds.SelectParameters.Clear();

            ds.SelectParameters.Add("startIndex", TypeCode.Int64, (startIndex + 1).ToString());
            ds.SelectParameters.Add("endIndex", TypeCode.Int64, (endIndex + 1).ToString());

            if (string.IsNullOrEmpty(comboBox.DataSourceID))
            {
                comboBox.DataSource = ds;
            }

            comboBox.DataBind();
        }

        public void PopulaDropDownASPx(Page page, DataTable dtDados, string campoValor, string campoTexto, string itemTODOS, ref ASPxComboBox dropDown)
        {
            // Limpa o DropDwon
            dropDown.Items.Clear();

            // Verifica se existem dados a serem inseridos no DopDown
            if (dtDados == null)
            {
                return;
            }

            // Verififa se deve ser inserido o item 'TODOS'
            if (itemTODOS != "")
            {
                dropDown.Items.Add(itemTODOS);

            }

            // Insere os ítens no DropDwon
            foreach (DataRow dr in dtDados.Rows)
            {
                //dropDown.Items.Add(new ListItem(dr[campoTexto].ToString(), dr[campoValor].ToString()));
                dropDown.Items.Add(new ListEditItem(dr[campoTexto].ToString(), dr[campoValor].ToString()));
            }

            // seta o primeiro item apenas se não for postback;
            if (!page.IsPostBack)
                dropDown.SelectedIndex = 0;
        }

        #endregion

        #region Informações do sistema - Session

        public string getPathSistema()
        {
            return VirtualPathUtility.ToAbsolute("~/");
        }

        private OrderedDictionary getInfoSistema()
        {
            try
            {
                if (Session["infoSistema"] != null)
                    return (OrderedDictionary)Session["infoSistema"];
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public object getInfoSistema(string chave)
        {
            OrderedDictionary infoSistema = getInfoSistema();

            if (infoSistema == null || (!infoSistema.Contains(chave)))
                return null;
            else
                return infoSistema[chave];
        }

        private void setInfoSistema(OrderedDictionary infoSistema)
        {
            try
            {
                Session["infoSistema"] = infoSistema;
            }
            catch { }
        }

        public void setInfoSistema(string chave, object valor)
        {
            OrderedDictionary infoSistema = getInfoSistema();
            if (infoSistema == null)
                infoSistema = new OrderedDictionary();

            if (!infoSistema.Contains(chave))
                infoSistema.Add(chave, valor);
            else
            {
                if (valor != null)
                    infoSistema[chave] = valor;
                else
                    infoSistema.Remove(chave);
            }
            setInfoSistema(infoSistema);
        }

        public void clearInfoSistema()
        {
            Session["infoSistema"] = null;
            Session.Remove("infoSistema");
        }

        public void clearInfoSistema(string chave)
        {
            setInfoSistema(chave, null);
        }

        public string getNomeSistema()
        {
            string nomeSistema = "";
            try
            {
                nomeSistema = System.Configuration.ConfigurationManager.AppSettings["nomeSistema"].ToString();

                DataSet dsParametros = getParametrosSistema("tituloPaginasWEB");

                if (DataSetOk(dsParametros) && DataTableOk(dsParametros.Tables[0]) && dsParametros.Tables[0].Rows[0]["tituloPaginasWEB"] + "" != "")
                    nomeSistema = dsParametros.Tables[0].Rows[0]["tituloPaginasWEB"].ToString();
            }
            catch (Exception) { } // devolve o que tiver em system.configuration caso dê erro, ou vazio se o erro ocorrer 
            return nomeSistema;
        }




        #endregion

        #region funçoes que não estão relacionadas com banco de dados

        public bool isMobileBrowser()
        {
            //GETS THE CURRENT USER CONTEXT
            HttpContext context = HttpContext.Current;

            //FIRST TRY BUILT IN ASP.NT CHECK
            if (context.Request.Browser.IsMobileDevice)
            {
                return true;
            }
            //THEN TRY CHECKING FOR THE HTTP_X_WAP_PROFILE HEADER
            if (context.Request.ServerVariables["HTTP_X_WAP_PROFILE"] != null)
            {
                return true;
            }
            //THEN TRY CHECKING THAT HTTP_ACCEPT EXISTS AND CONTAINS WAP
            if (context.Request.ServerVariables["HTTP_ACCEPT"] != null &&
                context.Request.ServerVariables["HTTP_ACCEPT"].ToLower().Contains("wap"))
            {
                return true;
            }
            //AND FINALLY CHECK THE HTTP_USER_AGENT 
            //HEADER VARIABLE FOR ANY ONE OF THE FOLLOWING
            if (context.Request.ServerVariables["HTTP_USER_AGENT"] != null)
            {
                //Create a list of all mobile types
                string[] mobiles =
                    new[]
                    {
                    "midp", "j2me", "avant", "docomo",
                    "novarra", "palmos", "palmsource",
                    "240x320", "opwv", "chtml",
                    "pda", "windows ce", "mmp/",
                    "blackberry", "mib/", "symbian",
                    "wireless", "nokia", "hand", "mobi",
                    "phone", "cdm", "up.b", "audio",
                    "SIE-", "SEC-", "samsung", "HTC",
                    "mot-", "mitsu", "sagem", "sony"
                    , "alcatel", "lg", "eric", "vx",
                    "NEC", "philips", "mmm", "xx",
                    "panasonic", "sharp", "wap", "sch",
                    "rover", "pocket", "benq", "java",
                    "pt", "pg", "vox", "amoi",
                    "bird", "compal", "kg", "voda",
                    "sany", "kdd", "dbt", "sendo",
                    "sgh", "gradi", "jb", "dddi",
                    "moto", "iphone"
                    };

                //Loop through each item in the list created above 
                //and check if the header contains that text
                foreach (string s in mobiles)
                {
                    if (context.Request.ServerVariables["HTTP_USER_AGENT"].
                                                        ToLower().Contains(s.ToLower()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public enum TipoObjetoEstrategia
        {
            Mapa, Missao, Visao, Perspectiva1, Perspectiva2, Perspectiva3, Perspectiva4, Objetivo, Tema, CausaEfeito
        };

        static private string getDataHoraUltimaVersaoBrisk()
        {
            System.Diagnostics.Debug.WriteLine("passou em getDataHoraUltimaVersaoBrisk");
            System.Diagnostics.Debug.WriteLine(DateTime.Now);
            return GetAssemblyLastModifiedDate().ToString("yyyy_M_d_H_m_s");
        }

        static private DateTime GetAssemblyLastModifiedDate()
        {
            string filePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return File.GetLastWriteTime(filePath);
        }

        public string getStringDataHoraCompleta()
        {
            return DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second;
        }

        public Literal getLiteral(string texto)
        {
            int indexJS = texto.IndexOf(".js", StringComparison.OrdinalIgnoreCase);

            if (indexJS != -1)
            {
                int indexSrc = texto.IndexOf("<script", StringComparison.OrdinalIgnoreCase);
                if ((indexSrc != -1) && (indexSrc < indexJS))
                {
                    string param = "?V=" + dataHoraUltimaVersao;
                    texto = texto.Insert(indexJS + 3, param);
                }
            }
            else
            {
                int indexCSS = texto.IndexOf(".css", StringComparison.OrdinalIgnoreCase);

                if (indexCSS != -1)
                {
                    int indexLink = texto.IndexOf("<link", StringComparison.OrdinalIgnoreCase);
                    if ((indexLink != -1) && (indexLink < indexCSS))
                    {
                        string param = "?V=" + dataHoraUltimaVersao;
                        texto = texto.Insert(indexCSS + 4, param);
                    }
                }

            }

            Literal myLiteral = new Literal();
            myLiteral.Text = texto;
            return myLiteral;
        }

        public void alerta(Page page, string mensagem)
        {
            string script = "<script type='text/javascript' language='javascript'>";
            script += Environment.NewLine + "alert(\" " + mensagem.Replace(Environment.NewLine, " ").Replace('\"', '\'').Replace('\n', ' ') + " \");";
            script += Environment.NewLine + "</script>";
            //page.ClientScript.RegisterClientScriptBlock(GetType(), "Client", script);
            ScriptManager.RegisterClientScriptBlock(page, GetType(), "client", script, false);
        }
        public void alerta(Page page, string mensagem, string imagem)
        {
            bool indicaApareceBotaoOK = !(imagem == "sucesso");

            string script = "<script type='text/javascript' language='javascript'>";
            script += Environment.NewLine + "window.top.mostraMensagem(\" " + mensagem.Replace(Environment.NewLine, " ").Replace('\"', '\'').Replace('\n', ' ') + " \", '" + imagem.ToLower() + "'," + indicaApareceBotaoOK.ToString().ToLower() + " , false, null);";
            script += Environment.NewLine + "</script>";
            //page.ClientScript.RegisterClientScriptBlock(GetType(), "Client", script);
            ScriptManager.RegisterClientScriptBlock(page, GetType(), "client", script, false);
        }

        // retorna todos os controles a partir de uma referencia (root) - Para obter todos os controles da página, faça root = Page

        public List<Control> getControles(Control root)
        {
            List<Control> listControl = new List<Control>();
            List<Task<List<Control>>> listTaskControl = new List<Task<List<Control>>>();

            listControl.Add(root);
            if (root != null && root.HasControls())
            {
                foreach (Control control in root.Controls)
                {
                    string nomeControle = control.ToString();

                    if (nomeControle.Contains("ASPxWebDocumentViewer") == false)
                    {
                        listTaskControl.Add(Task.Factory.StartNew(() =>
                        {
                            return getControles(control);
                        }));
                    }
                }

                Task.WhenAll(listTaskControl);
                listTaskControl.ForEach(t => listControl.AddRange(t.Result));
                listTaskControl.ForEach(t => t.Dispose());
            }

            return listControl;
        }

        //public Control[] getControles(Control root)
        //{
        //    System.Collections.Generic.List<Control> list = new System.Collections.Generic.List<Control>();
        //    list.Add(root);
        //    if (root != null && root.HasControls())
        //    {
        //        foreach (Control control in root.Controls)
        //        {
        //            list.AddRange(getControles(control));
        //        }
        //    }
        //    return list.ToArray();
        //}

        public void aplicaTemaDashboard(ASPxDashboard dashboard)
        {
            var IDEstiloVisual = getInfoSistema("IDEstiloVisual");
            string temaBrisk = BriskTheme.Light;
            string temaDashboard = ASPxDashboard.ColorSchemeLight;

            if (IDEstiloVisual == null)
            {
                temaBrisk = BriskTheme.Light;
            }
            else
            {
                temaBrisk = IDEstiloVisual.ToString();
            }

            if (temaBrisk == BriskTheme.Dark)
                temaDashboard = ASPxDashboard.ColorSchemeMaterialOrangeDarkCompact;

            dashboard.ColorScheme = temaDashboard;
        }

        public string escreveXML(string xml, string nome)
        {
            StreamWriter strWriter;
            //cabeçalho do XML
            string cabecalho = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n";
            //cria um novo arquivo xml e abre para escrita
            strWriter = new StreamWriter(HostingEnvironment.ApplicationPhysicalPath + nome, false, System.Text.Encoding.UTF8);
            //escrever o cabeçalho no arquivo xml criado
            strWriter.Write(cabecalho);
            //escrever o corpo do XML no arquivo xml criado
            strWriter.Write(xml);
            //fecha o arquivo criado
            strWriter.Close();

            return nome;
        }

        public string escreveTexto(string texto, string nome)
        {
            StreamWriter strWriter;
            //cabeçalho do XML

            //cria um novo arquivo xml e abre para escrita
            strWriter = new StreamWriter(HostingEnvironment.ApplicationPhysicalPath + nome, false, System.Text.Encoding.UTF8);
            //escrever o corpo do Texto no arquivo criado
            strWriter.Write(texto);
            //fecha o arquivo criado
            strWriter.Close();

            return nome;
        }

        public int ObtemCodigoHash(string str)
        {
            /*ATENÇÃO: Qualquer mudança neste método tem que refeito no metodo com mesmo nome que está no arquivo wsTasquesreg.cs*/
            if (str == "")
                str = " ";

            int valorRetorno = 0;
            int acumulador = 0;
            MD5 md5Hasher = MD5.Create();
            char[] caracteres = str.ToCharArray();

            foreach (char caracter in caracteres)
            {
                acumulador += caracter * Array.IndexOf(caracteres, caracter) + caracteres.Length;
            }

            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(str));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            valorRetorno = int.Parse(sBuilder.ToString(0, 8), System.Globalization.NumberStyles.HexNumber) ^ (int.Parse(sBuilder.ToString(8, 8), System.Globalization.NumberStyles.HexNumber) / acumulador) ^ int.MaxValue;
            return (valorRetorno % 2 == 0) ? valorRetorno : -valorRetorno;
        }

        public bool getLarguraAlturaTela(string resolucaoCliente, out int largura, out int altura)
        {
            altura = 0;
            largura = 0;
            try
            {
                largura = int.Parse(resolucaoCliente.Substring(0, resolucaoCliente.IndexOf('x')));
                altura = int.Parse(resolucaoCliente.Substring(resolucaoCliente.IndexOf('x') + 1, resolucaoCliente.Length - resolucaoCliente.IndexOf('x') - 1));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void apagaArquivosTemporarios()
        {
            string[] arquivos = Directory.GetFiles(HostingEnvironment.ApplicationPhysicalPath + "ArquivosTemporarios\\", "*.*", SearchOption.TopDirectoryOnly);

            foreach (string arquivo in arquivos)
            {
                try
                {
                    FileInfo fl = new FileInfo(arquivo);

                    if (fl.Exists && fl.IsReadOnly == false && fl.LastWriteTime.AddHours(1) < DateTime.Now)
                        fl.Delete();
                }
                catch
                {
                }
            }

            // -------------------------------------------------------------------------------------------------
            // ACG: 17/09/2015 - Verifica se é para apagar os arquivos da pasta "Cronogramas" - Chamado: P318468
            // -------------------------------------------------------------------------------------------------
            bool apagarDiretorioCronogramas = false;
            int CodigoEntidade = int.Parse(getInfoSistema("CodigoEntidade") + "");
            DataSet ds = getParametrosSistema(CodigoEntidade, "apagarDiretorioCronogramas");
            if (ds != null && ds.Tables[0].Rows[0]["apagarDiretorioCronogramas"] + "" == "S")
                apagarDiretorioCronogramas = true;

            if (apagarDiretorioCronogramas)
            {
                // Apaga o conteúdo da pasta "Cronogramas" (Web.Config)
                string pastaCronogramas = "";
                // se não existir um local definido no Webconfig, assume o local padrão
                if (System.Configuration.ConfigurationManager.AppSettings["diretorioCronogramas"] != null && System.Configuration.ConfigurationManager.AppSettings["diretorioCronogramas"].ToString() != "")
                    pastaCronogramas = System.Configuration.ConfigurationManager.AppSettings["diretorioCronogramas"] + "";
                else
                    pastaCronogramas = @"C:\CDIS_PortalEstrategia\Cronogramas";

                if (Directory.Exists(pastaCronogramas))
                {
                    arquivos = Directory.GetFiles(pastaCronogramas, "*.*", SearchOption.TopDirectoryOnly);
                    foreach (string arquivo in arquivos)
                    {
                        try
                        {
                            FileInfo fl = new FileInfo(arquivo);

                            // apaga apenas arquivos com mais de 6 meses sem alteração
                            if (fl.Exists && fl.IsReadOnly == false && fl.LastWriteTime.AddMonths(6) < DateTime.Now)
                                fl.Delete();
                        }
                        catch
                        {
                        }

                    }
                }
            }
        }

        #endregion

        #region Tradução - Novas funções

        public string getTextoTraduzido(string nomeResource, string textoDefault)
        {
            try
            {
                return Resources.traducao.ResourceManager.GetString(nomeResource, Thread.CurrentThread.CurrentCulture);
            }
            catch
            {
                return "";
            }
        }

        #endregion

        #region ASPX COMPONENTS

        public void habilitaComponentes(bool habilitar, ASPxCallbackPanel painelCallback)
        {
            for (int i = 0; i < painelCallback.Controls.Count; i++)
            {
                if (painelCallback.Controls[i] is ASPxRoundPanel)
                {
                    habilitaComponentesDoRoundPanel(habilitar, (ASPxRoundPanel)painelCallback.Controls[i]);
                }
                else if (painelCallback.Controls[i] is ASPxTextBox)
                {
                    ((ASPxTextBox)painelCallback.Controls[i]).ClientEnabled = habilitar;
                }
                else if (painelCallback.Controls[i] is ASPxMemo)
                {
                    ((ASPxMemo)painelCallback.Controls[i]).ClientEnabled = habilitar;
                }
                else if (painelCallback.Controls[i] is ASPxComboBox)
                {
                    ((ASPxComboBox)painelCallback.Controls[i]).ClientEnabled = habilitar;
                }
                else if (painelCallback.Controls[i] is ASPxCheckBox)
                {
                    ((ASPxCheckBox)painelCallback.Controls[i]).ClientEnabled = habilitar;
                }
                else if (painelCallback.Controls[i] is ASPxDateEdit)
                {
                    ((ASPxDateEdit)painelCallback.Controls[i]).ClientEnabled = habilitar;
                }
                else if (painelCallback.Controls[i] is ASPxUploadControl)
                {
                    ((ASPxUploadControl)painelCallback.Controls[i]).Enabled = habilitar;
                }
                else if (painelCallback.Controls[i] is ASPxButton)
                {
                    ((ASPxButton)painelCallback.Controls[i]).ClientEnabled = habilitar;
                }

                else if (painelCallback.Controls[i] is ASPxRadioButtonList)
                {
                    ((ASPxRadioButtonList)painelCallback.Controls[i]).ClientEnabled = habilitar;
                }
                else if (painelCallback.Controls[i] is ASPxListBox)
                {
                    ((ASPxListBox)painelCallback.Controls[i]).ClientEnabled = habilitar;
                }
                else if (painelCallback.Controls[i] is ASPxPageControl)
                {
                    for (int j = 0; j < ((ASPxPageControl)painelCallback.Controls[i]).TabPages[0].Controls.Count; j++)
                    {
                        if (((ASPxPageControl)painelCallback.Controls[i]).TabPages[0].Controls[j] is ASPxButton)
                        {
                            ((ASPxButton)((ASPxPageControl)painelCallback.Controls[i]).TabPages[0].Controls[j]).ClientEnabled = habilitar;
                        }
                    }
                }
                if (habilitar == true && i == 0)
                    painelCallback.Controls[0].Focus();
            }
        }

        private void limpaCamposDoRoundPanel(ASPxRoundPanel painelRound1)
        {
            for (int i = 0; i < painelRound1.Controls.Count; i++)
            {
                if (painelRound1.Controls[i] is ASPxTextBox)
                {
                    ((ASPxTextBox)painelRound1.Controls[i]).Text = "";
                }
                else if (painelRound1.Controls[i] is ASPxMemo)
                {
                    ((ASPxMemo)painelRound1.Controls[i]).Text = "";
                }
                else if (painelRound1.Controls[i] is ASPxComboBox)
                {
                    ASPxComboBox cb = ((ASPxComboBox)painelRound1.Controls[i]);
                    if (cb.Items.Count > 0)
                        cb.SelectedIndex = 0;
                }
                else if (painelRound1.Controls[i] is ASPxCheckBox)
                {
                    ((ASPxCheckBox)painelRound1.Controls[i]).Checked = false;
                }
            }
        }

        private void habilitaComponentesDoRoundPanel(bool habilitar, ASPxRoundPanel painelRound1)
        {
            for (int i = 0; i < painelRound1.Controls.Count; i++)
            {
                if (painelRound1.Controls[i] is ASPxTextBox)
                {
                    ((ASPxTextBox)painelRound1.Controls[i]).ClientEnabled = habilitar;
                }
                else if (painelRound1.Controls[i] is ASPxMemo)
                {
                    ((ASPxMemo)painelRound1.Controls[i]).ClientEnabled = habilitar;
                }
                else if (painelRound1.Controls[i] is ASPxComboBox)
                {
                    ((ASPxComboBox)painelRound1.Controls[i]).ClientEnabled = habilitar;
                }
                else if (painelRound1.Controls[i] is ASPxCheckBox)
                {
                    ((ASPxCheckBox)painelRound1.Controls[i]).ClientEnabled = habilitar;
                }
                else if (painelRound1.Controls[i] is ASPxDateEdit)
                {
                    ((ASPxDateEdit)painelRound1.Controls[i]).ClientEnabled = habilitar;
                }
                else if (painelRound1.Controls[i] is ASPxUploadControl)
                {
                    ((ASPxUploadControl)painelRound1.Controls[i]).Enabled = habilitar;
                }
                else if (painelRound1.Controls[i] is ASPxButton)
                {
                    ((ASPxButton)painelRound1.Controls[i]).ClientEnabled = habilitar;
                }
                if (habilitar == true && i == 0)
                    painelRound1.Controls[0].Focus();

            }


        }

        public void limpaCampos(ASPxCallbackPanel painelCallback)
        {
            for (int i = 0; i < painelCallback.Controls.Count; i++)
            {
                if (painelCallback.Controls[i] is ASPxRoundPanel)
                {
                    ASPxRoundPanel painelRound1 = (ASPxRoundPanel)painelCallback.Controls[i];
                    limpaCamposDoRoundPanel(painelRound1);
                }

                if (painelCallback.Controls[i] is ASPxTextBox)
                {
                    ((ASPxTextBox)painelCallback.Controls[i]).Text = "";
                }
                else if (painelCallback.Controls[i] is ASPxMemo)
                {
                    ((ASPxMemo)painelCallback.Controls[i]).Text = "";
                }
                else if (painelCallback.Controls[i] is ASPxComboBox)
                {
                    ASPxComboBox cb = ((ASPxComboBox)painelCallback.Controls[i]);
                    if (cb.Items.Count > 0)
                        cb.SelectedIndex = 0;
                }
                else if (painelCallback.Controls[i] is ASPxCheckBox)
                {
                    ((ASPxCheckBox)painelCallback.Controls[i]).Checked = false;
                }
            }
        }

        #endregion

        #region login

        public string retiraCaracteresSqlInjection(string texto)
        {
            string novoTexto = texto;
            novoTexto = novoTexto.Replace("'", ""); // aspas simples
            novoTexto = novoTexto.Replace("\"", ""); // aspas duplas
            novoTexto = novoTexto.Replace(" ", ""); // espaço em branco
            novoTexto = novoTexto.Replace(";", ""); // ponto e virgula
            novoTexto = novoTexto.Replace("=", ""); // sinal de igualdade
            novoTexto = novoTexto.Replace("<", ""); // sinal menor que
            novoTexto = novoTexto.Replace(">", ""); // sinal maior que
            novoTexto = novoTexto.Replace("!", ""); // exclamação
            novoTexto = novoTexto.Replace("/", ""); // barra
            novoTexto = novoTexto.Replace("--", ""); // dois traços
            novoTexto = novoTexto.Replace("*", ""); // asterisco
            novoTexto = novoTexto.Replace(":", ""); // dois pontos

            return novoTexto;
        }

        public int getAutenticacaoUsuario(string usuario, int? senha, string tipoAutenticacao, out string nomeUsuario, out string IDEstiloVisual, out string alterarSenha)
        {
            string whereTipoAutenticacao = "";
            nomeUsuario = "";
            IDEstiloVisual = "";
            alterarSenha = "";

            if (tipoAutenticacao == "AI")
                whereTipoAutenticacao = string.Format(
                    @"  AND Usuario.tipoAutenticacao = 'AI'
                    AND Usuario.ContaWindows = '{0}' ", usuario);
            else
            {
                string complementoWhere = "AND Usuario.tipoAutenticacao = 'AS'";
                if (usuario[0] == '_' && usuario[usuario.Length - 1] == '_')
                {
                    usuario = usuario.Substring(1);
                    usuario = usuario.Substring(0, usuario.Length - 1);
                    complementoWhere = "";
                }

                whereTipoAutenticacao = string.Format(
                    @"  AND Usuario.EMail = '{0}'
                    AND Usuario.SenhaAcessoAutenticacaoSistema = '{1}'
                    {2}
             ", usuario, senha, complementoWhere);
            }

            comandoSQL = string.Format(
                @"SELECT Usuario.CodigoUsuario, Usuario.NomeUsuario, Usuario.IDEstiloVisual, Usuario.indicaAlterarSenhaProximoLogin
                FROM {0}.{1}.Usuario 
               WHERE Usuario.DataExclusao is null
                  {2} ", bancodb, Ownerdb, whereTipoAutenticacao);

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                alterarSenha = ds.Tables[0].Rows[0]["indicaAlterarSenhaProximoLogin"].ToString();
                nomeUsuario = ds.Tables[0].Rows[0]["NomeUsuario"].ToString();
                IDEstiloVisual = ds.Tables[0].Rows[0]["IDEstiloVisual"].ToString();
                return int.Parse(ds.Tables[0].Rows[0]["CodigoUsuario"].ToString());
            }
            return 0;
        }

        public int getDadosAutenticacaoUsuarioExterno(int codigoUsuario, out string nomeUsuario, out string IDEstiloVisual, out string alterarSenha)
        {
            nomeUsuario = "";
            IDEstiloVisual = "";
            alterarSenha = "";

            comandoSQL = string.Format(
                @"SELECT Usuario.CodigoUsuario, Usuario.NomeUsuario, Usuario.IDEstiloVisual, Usuario.indicaAlterarSenhaProximoLogin
                FROM {0}.{1}.Usuario 
               WHERE Usuario.DataExclusao is null
                 AND CodigoUsuario = {2}", bancodb, Ownerdb, codigoUsuario);

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                alterarSenha = ds.Tables[0].Rows[0]["indicaAlterarSenhaProximoLogin"].ToString();
                nomeUsuario = ds.Tables[0].Rows[0]["NomeUsuario"].ToString();
                IDEstiloVisual = ds.Tables[0].Rows[0]["IDEstiloVisual"].ToString();
                return codigoUsuario;
            }
            return 0;
        }


        /*public DataSet getUsuarioSenha(string usuario, int senha, string where, ref string mensagem)
        {
            comandoSQL = string.Format(
                @"BEGIN
                      DECLARE @Acesso TABLE (CodigoUsuario Int,
                            NomeUsuario Varchar(255),
                            EMail Varchar(255),
                            IDEstiloVisual varchar(20),
                            CodigoTipoUnidadeNegocio SmallInt,
                            NomeEntidade VarChar(255),
                            CodigoUnidadeNegocio Int,
                            CodigoEntidade Int)

          INSERT INTO @Acesso
          SELECT DISTINCT 
                 [u].[CodigoUsuario], 
                 [u].[NomeUsuario], 
                 [u].[email], 
                 [u].[IDEstiloVisual],
                 [e].[CodigoTipoUnidadeNegocio], 
                 [e].[NomeUnidadeNegocio],
                 [e].[CodigoUnidadeNegocio],
                 [e].[CodigoEntidade]
            FROM 
                [{0}].[{1}].[Usuario]                               AS [u] 
                    INNER JOIN [{0}].[{1}].[UsuarioUnidadeNegocio]  AS [uun] 
                        ON ( [uun].[CodigoUsuario] = [u].[CodigoUsuario] ) 
                    INNER JOIN [{0}].[{1}].[UnidadeNegocio]         AS [un] 
                        ON ( [uun].[CodigoUnidadeNegocio] = [un].[CodigoUnidadeNegocio] )
                    INNER JOIN [{0}].[{1}].[UnidadeNegocio]         AS [e] 
                        ON ( [e].[CodigoEntidade]= [un].[CodigoEntidade] ) 
                    INNER JOIN [{0}].[{1}].[TipoUnidadeNegocio]      AS [tun] 
                        ON ( [e].[CodigoTipoUnidadeNegocio] = [tun].[CodigoTipoUnidadeNegocio] )
           WHERE 
                    [u].[email]                             = '{2}'
                AND [u].[SenhaAcessoAutenticacaoSistema]    = '{3}'
                AND [u].[DataExclusao]                      IS NULL
                AND [uun].[IndicaUsuarioAtivoUnidadeNegocio]= 'S'
                AND [un].[IndicaUnidadeNegocioAtiva]        = 'S'
                AND [un].[DataExclusao]                     IS NULL 
                AND [e].[IndicaUnidadeNegocioAtiva]         = 'S'
                AND [e].[DataExclusao]                      IS NULL 
                AND [tun].IndicaEntidade                    = 'S' {4}

          INSERT INTO @Acesso
                SELECT [u].[CodigoUsuario], 
                       [u].[NomeUsuario], 
                       [u].[email], 
                       [u].[IDEstiloVisual],
                       min([e].[CodigoTipoUnidadeNegocio])      AS [CodigoTipoUnidadeNegocio], 
                 min([e].[NomeUnidadeNegocio])                  AS [NomeUnidadeNegocio],
                 min([e].[CodigoUnidadeNegocio])                AS [CodigoUnidadeNegocio], 
                 min([e].[CodigoEntidade])                      AS [CodigoEntidade]
            FROM [{0}].[{1}].[Usuario]                              AS [u] 
                    INNER JOIN [{0}].[{1}].[UnidadeNegocio]         AS [e]
                        ON ( [e].[CodigoSuperUsuario] = [u].[CodigoUsuario]) 
                    INNER JOIN [{0}].[{1}].[TipoUnidadeNegocio]     AS [tun]
                        ON ( [tun].[CodigoTipoUnidadeNegocio] = [e].[CodigoTipoUnidadeNegocio] )
           WHERE 
                    [u].[email]                             = '{2}'
                AND [u].[SenhaAcessoAutenticacaoSistema]    = '{3}'
                AND [u].[DataExclusao]                      IS NULL
                AND [e].[IndicaUnidadeNegocioAtiva]         = 'S'
                AND [e].[DataExclusao]                      IS NULL 
                AND [tun].IndicaEntidade                    = 'S' {4}
           GROUP BY [u].[CodigoUsuario], 
                    [u].[NomeUsuario], 
                    [u].[email],
                    [u].[IDEstiloVisual]

          SELECT CodigoUsuario, 
                       NomeUsuario, 
                       EMail, 
                       IDEstiloVisual,
                 Count(Distinct CodigoEntidade) AS QtdeEntidades,
                       min(CodigoTipoUnidadeNegocio) CodigoTipoUnidadeNegocio, 
                 min(NomeEntidade) NomeUnidadeNegocio,
                 min(CodigoUnidadeNegocio) AS CodigoUnidadeNegocio, 
                 min(CodigoEntidade) AS CodigoEntidade
            FROM @Acesso
           GROUP BY CodigoUsuario, 
                          NomeUsuario, 
                          EMail,
                          IDEstiloVisual
    END", bancodb, Ownerdb, usuario, senha, where);
            return getDataSet(comandoSQL);
        }*/

        public DataSet getUsuarios(string where)
        {
            string comandoSQLNovo = string.Format(
            @"SELECT u.[CodigoUsuario]
                ,u.[NomeUsuario]
                ,u.[ContaWindows]
                ,u.[TipoAutenticacao]
                ,u.[SenhaAcessoAutenticacaoSistema]
                ,u.[EMail]
                ,u.[ResourceUID]
                ,u.[TelefoneContato1]
                ,u.[TelefoneContato2]
                ,u.[Observacoes]
                --,u.[IndicaUsuarioAtivo]
                --,u.[CodigoGerenteFuncional]
                ,u.[DataInclusao]
                ,u.[CodigoUsuarioInclusao]
                ,u.[DataUltimaAlteracao]
                ,u.[CodigoUsuarioUltimaAlteracao]
                ,u.[DataExclusao]
                ,u.[CodigoUsuarioExclusao]
                , ( SELECT top 1 CodigoUnidadeNegocio 
					  FROM {0}.{1}.UsuarioUnidadeNegocio
 					 WHERE DataAtivacaoPerfilUnidadeNegocio IN(SELECT MIN(DataAtivacaoPerfilUnidadeNegocio) 
                                                                 FROM {0}.{1}.UsuarioUnidadeNegocio
                                                                WHERE CodigoUsuario = u.CodigoUsuario  
                                                                  AND IndicaUsuarioAtivoUnidadeNegocio = 'S')   ) as CodigoUnidadePrimeiraInclusao
              ,(SELECT TOP 1 CodigoUnidadeNegocio 
                  FROM {0}.{1}.UsuarioUnidadeNegocio 
                 WHERE CodigoUsuario = u.[CodigoUsuario]) as listaPrimeiraUnidadeUsuario
                             
              ,(SELECT TOP 1 IndicaUsuarioAtivoUnidadeNegocio                              
                  FROM {0}.{1}.UsuarioUnidadeNegocio 
                 WHERE CodigoUsuario = u.[CodigoUsuario]) as usuarioAtivo
              
               ,(SELECT TOP 1 case when isnull( CodigoUsuarioGerente,0) > 0 then 'N' else 'S' END 
                   FROM {0}.{1}.UnidadeNegocio un1
             INNER JOIN {0}.{1}.UsuarioUnidadeNegocio uun on (uun.CodigoUnidadeNegocio = un1.CodigoUnidadeNegocio)                             
                  WHERE uun.CodigoUsuario = u.[CodigoUsuario]) as gerente
               ,(SELECT TOP 1 CodigoEntidade from
                   {0}.{1}.UnidadeNegocio un1
             INNER JOIN {0}.{1}.UsuarioUnidadeNegocio uun on (uun.CodigoUnidadeNegocio = un1.CodigoUnidadeNegocio)                             
                  WHERE uun.CodigoUsuario = u.[CodigoUsuario]) as CodigoEntidade
               ,u.UrlTelaInicialMobileUsuario
              FROM {0}.{1}.[Usuario] u
             WHERE EXISTS (SELECT 1
                             FROM {0}.{1}.UsuarioUnidadeNegocio AS uun INNER JOIN
                                  {0}.{1}.UnidadeNegocio AS un ON (uun.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio)
                            WHERE uun.CodigoUsuario = u.CodigoUsuario and uun.IndicaUsuarioAtivoUnidadeNegocio = 'S')
               AND u.DataExclusao is null {2} order by u.[NomeUsuario] asc ", bancodb, Ownerdb, where);

            DataSet ds = getDataSet(comandoSQLNovo);
            return ds;
        }

        public DataSet getMapaDefaultUsuario(int codigoUsuario, string where)
        {
            string comandoSQLNovo = string.Format(
            @"BEGIN
            DECLARE @vinculaMetasMapas CHAR(1),
                    @CodigoUnidade INT,
                    @codigoUsuario INT,
                    @codigoMapaContexto INT,
                    @codigoEntidadeMapa INT

            SET @CodigoUnidade = {2}
            SET @CodigoUsuario = {3}

            SET @vinculaMetasMapas		= 'N';
	            SELECT @vinculaMetasMapas = CAST(pcs.[Valor] AS CHAR(1)) FROM {0}.{1}.[ParametroConfiguracaoSistema] AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoUnidade AND pcs.[Parametro] = 'vinculaMetasEstrategicasAMapasEmPaineis'

	            SET @codigoMapaContexto = NULL;
	            IF ( @vinculaMetasMapas = 'S' )
	            BEGIN
		            SELECT @codigoMapaContexto = u.[CodigoMapaEstrategicoPadrao] FROM {0}.{1}.[Usuario] AS [u] WHERE u.[CodigoUsuario] = @codigoUsuario;
		
		            IF (@codigoMapaContexto IS NULL) BEGIN
			            SELECT @codigoMapaContexto = CAST(pcs.[Valor] AS Int) FROM [dbo].[ParametroConfiguracaoSistema] AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoUnidade AND pcs.[Parametro] = 'CodigoMapaDefault' AND ISNUMERIC(pcs.[Valor]) = 1;
		            END -- IF (@codigoMapaContexto IS NULL) 
		            ELSE BEGIN
			            SET @codigoEntidadeMapa = dbo.f_GetCodigoEntidadeObjeto(@codigoMapaContexto, NULL, 'ME', 0);
			
			            IF ( (@codigoEntidadeMapa IS NULL) OR (@codigoEntidadeMapa != @CodigoUnidade) ) BEGIN
				            SET @codigoMapaContexto = NULL;
				            SELECT @codigoMapaContexto = CAST(pcs.[Valor] AS Int) FROM [dbo].[ParametroConfiguracaoSistema] AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoUnidade AND pcs.[Parametro] = 'CodigoMapaDefault' AND ISNUMERIC(pcs.[Valor]) = 1;
			            END -- IF ( (@codigoEntidadeMapa IS NULL) OR ...
		            END -- ELSE (@codigoMapaContexto IS NULL) 
	            END -- IF ( @vinculaMetasMapas = 'S' )

            SELECT CodigoMapaEstrategico AS CodigoMapaEstrategicoPadrao, mp.TituloMapaEstrategico                       
                          FROM {0}.{1}.MapaEstrategico mp
                         WHERE CodigoMapaEstrategico = @codigoMapaContexto

        END", bancodb, Ownerdb, getInfoSistema("CodigoEntidade"), codigoUsuario);

            DataSet ds = getDataSet(comandoSQLNovo);
            return ds;
        }

        public DataSet getCodigoMapaEstrategicoPadraoUsuario(int codigoUsuario)
        {
            string comandoSQLNovo = string.Format(
            @"SELECT CodigoMapaEstrategicoPadrao                       
                          FROM {0}.{1}.Usuario u
                         WHERE CodigoUsuario = {3}
                        --AND CodigoEntidadeAcessoPadrao = {2}

        ", bancodb, Ownerdb, getInfoSistema("CodigoEntidade"), codigoUsuario);

            DataSet ds = getDataSet(comandoSQLNovo);
            return ds;
        }

        public bool setNovaSenha(int usuario, int senha, string where, ref string mensagem)
        {
            try
            {
                comandoSQL = string.Format(@"UPDATE {0}.{1}.[Usuario]
            SET [SenhaAcessoAutenticacaoSistema] = {2}
              , [indicaAlterarSenhaProximoLogin] = 'N'
            WHERE CodigoUsuario = {3} ", bancodb, Ownerdb, senha, usuario);
                int regAfetados = 0;
                execSQL(comandoSQL, ref regAfetados);
                //mensagem = "Senha alterada com sucesso!";
                return true;
            }
            catch (Exception ex)
            {
                mensagem = ex.Message;
                return false;
            }

        }

        public XmlDocument getDadosAutenticacaoExterna(string xmlEntrada, string parametroUrlRequisicao)
        {
            XmlDocument doc = new XmlDocument();
            System.Net.HttpWebRequest req = null;
            HttpWebResponse resp = null;
            StreamReader strReader = null;

            DataSet dsAutenticacaoExterna = getParametrosSistema(-1, parametroUrlRequisicao);

            string uri = dsAutenticacaoExterna.Tables[0].Rows[0][parametroUrlRequisicao] + "xml=" + xmlEntrada;

            req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
            req.Method = "POST";        // Post method
            req.ContentType = "text/xml";     // content type
            req.KeepAlive = false;
            //req.ProtocolVersion = HttpVersion.Version10;

            // ignorando erros de certificados inválidos
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
                strReader = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                doc.LoadXml(strReader.ReadToEnd());
            }
            finally
            {

                if (resp != null)
                    resp.Close();

                if (strReader != null)
                    strReader.Close();
            }

            return doc;
        }

        public bool sincronizaUsuarioExterno(string login, string senha, string nome, string email, string perfis, ref int codigoUsuario)
        {
            try
            {
                comandoSQL = string.Format(@"
                  BEGIN
                    DECLARE @CodigoUsuario int,
                            @CodigoRetorno int

                    EXEC {0}.{1}.p_autExt_SincronizaUserInfo '{2}', '{3}', '{4}', '{5}', '{6}', @CodigoUsuario output, @CodigoRetorno output

                    SELECT ISNULL(@CodigoUsuario, 0) AS CodigoUsuario
                  END
               ", bancodb, Ownerdb, login, senha, nome, email, perfis);

                DataSet ds = getDataSet(comandoSQL);

                if (DataSetOk(ds) && DataTableOk(ds.Tables[0]) && ds.Tables[0].Rows[0]["CodigoUsuario"].ToString() != "")
                    codigoUsuario = int.Parse(ds.Tables[0].Rows[0]["CodigoUsuario"].ToString());

                return true;
            }
            catch
            {
                return false;
            }

        }

        public string geraTokenAutorizacaoNewBrisk()
        {
            var retorno = "";
            string comandoSQL = "";

            Guid tokenGuid = Guid.NewGuid();
            //x.ToString()
            //Gerar o token, Acionar a proc para gravar o token e ao final gravar o token na variavel de sessão.
            //            string comandoSQL = @"
            int idUsuarioLogado = int.Parse(getInfoSistema("IDUsuarioLogado").ToString());

            comandoSQL = string.Format(@"
            DECLARE @RC int
            DECLARE @siglaFederacaoIdentidade varchar(30)
            DECLARE @codigoAutorizacaoAcesso varchar(500)
            DECLARE @tokenAcesso varchar(4000)
            DECLARE @CodigoUsuario int
            DECLARE @codigoControleToken bigint

           set @siglaFederacaoIdentidade = 'brisk_em'
           set @codigoAutorizacaoAcesso = '{0}'
           set @tokenAcesso = null
           set @CodigoUsuario = {1}

           EXECUTE @RC = [dbo].[p_brk_rw_registraTokenUsuarioFederacaoIdentidade]
               @siglaFederacaoIdentidade
              ,@codigoAutorizacaoAcesso
              ,@tokenAcesso
              ,@CodigoUsuario
              ,@codigoControleToken OUTPUT
          
        SELECT @codigoAutorizacaoAcesso as codigoAutorizacaoAcesso, @codigoControleToken as codigoControleToken", tokenGuid.ToString(), idUsuarioLogado);

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                retorno = ds.Tables[0].Rows[0]["codigoAutorizacaoAcesso"].ToString();
            }

            return retorno;
        }

        #endregion

        #region seleccionarOpcao.aspx

        public DataSet obtemEntidadPadraoUsuarioLogado(string codigoUsuario)
        {
            comandoSQL = string.Format(@"
            SELECT us.CodigoEntidadeAcessoPadrao, un.SiglaUnidadeNegocio
            FROM {0}.{1}.Usuario us
                INNER JOIN {0}.{1}.UsuarioUnidadeNegocio uun
                    ON us.CodigoUsuario = uun.CodigoUsuario and uun.CodigoUnidadeNegocio = us.CodigoEntidadeAcessoPadrao
                INNER JOIN {0}.{1}.UnidadeNegocio as un
                    ON uun.CodigoUnidadeNegocio = un.CodigoEntidade
            WHERE us.CodigoUsuario = {2} AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S'

            ", bancodb, Ownerdb, codigoUsuario);

            return getDataSet(comandoSQL);
        }

        #endregion

        #region Aparencia da tela + Tradução do Geter

        public void getVisual(string estilo, ref string CssFilePath, ref string CssPostfix)
        {
            if (estilo != BriskTheme.Light && estilo != BriskTheme.Dark)
            {
                estilo = BriskTheme.Light;
            }

            CssFilePath = "~/App_Themes/" + estilo + "/{0}/styles.css";
            CssPostfix = estilo;
        }

        public void setVisual(string estilo, bool isMaster, bool isCalendar = false, params object[] controles)
        {
            string CssFilePath = "";
            string CssPostfix = "";
            getVisual(estilo, ref CssFilePath, ref CssPostfix);

            Color corFundoDesabilitado = Color.FromName("#EBEBEB");
            Color corFonteDesabilitado = Color.Black;

            //DevExpress.Web.ASPxWebControl 
            foreach (object controle in controles)
            {
                if (controle is ASPxMenu && ((ASPxMenu)controle).ID == "menuPrincipalMasterPage")
                {

                }
                else
                {
                    if (controle is ASPxCallbackPanel)
                    {
                        (controle as ASPxCallbackPanel).SettingsLoadingPanel.Text = "";
                        (controle as ASPxCallbackPanel).Images.LoadingPanel.Url = "~/imagens/carregando.gif";
                        //(controle as ASPxCallbackPanel).LoadingPanelStyle.HorizontalAlign = HorizontalAlign.Center;
                        //(controle as ASPxCallbackPanel).LoadingPanelStyle.VerticalAlign = VerticalAlign.Middle;
                        (controle as ASPxCallbackPanel).SettingsLoadingPanel.Enabled = false;
                        (controle as ASPxCallbackPanel).SettingsLoadingPanel.ShowImage = false;

                        (controle as ASPxCallbackPanel).Styles.LoadingPanel.HorizontalAlign = HorizontalAlign.Center;
                        (controle as ASPxCallbackPanel).Styles.LoadingPanel.VerticalAlign = VerticalAlign.Middle;

                    }

                   (controle as DevExpress.Web.ASPxWebControl).CssFilePath = CssFilePath;
                    (controle as DevExpress.Web.ASPxWebControl).CssPostfix = CssPostfix;

                    string CssSprite = CssFilePath.Replace("styles", "sprite");
                    if (controle is DevExpress.Web.ASPxComboBox)
                    {
                        (controle as DevExpress.Web.ASPxComboBox).ValidationSettings.ErrorDisplayMode = ErrorDisplayMode.ImageWithTooltip;
                        (controle as DevExpress.Web.ASPxComboBox).ValidationSettings.Display = Display.Dynamic;
                        (controle as DevExpress.Web.ASPxComboBox).IncrementalFilteringMode = IncrementalFilteringMode.Contains;
                        (controle as DevExpress.Web.ASPxComboBox).SpriteCssFilePath = CssSprite;
                        (controle as DevExpress.Web.ASPxComboBox).DisabledStyle.BackColor = corFundoDesabilitado;
                        (controle as DevExpress.Web.ASPxComboBox).DisabledStyle.ForeColor = corFonteDesabilitado;
                        (controle as DevExpress.Web.ASPxComboBox).Height = 20;
                        (controle as DevExpress.Web.ASPxComboBox).SettingsLoadingPanel.Text = "";
                    }
                    else if (controle is DevExpress.Web.ASPxListBox)
                    {
                        //(controle as DevExpress.Web.ASPxListBox).SelectionMode = ListEditSelectionMode.CheckColumn;
                        (controle as DevExpress.Web.ASPxListBox).EnableCallbackMode = true;
                        (controle as DevExpress.Web.ASPxListBox).CallbackPageSize = 50;
                        (controle as DevExpress.Web.ASPxListBox).ItemStyle.Wrap = DefaultBoolean.True;

                        //(controle as DevExpress.Web.ASPxListBox).FilteringSettings.ShowSearchUI = true;
                        //(controle as DevExpress.Web.ASPxListBox).EnableSelectAll = false;
                        (controle as DevExpress.Web.ASPxListBox).FilteringSettings.EditorNullText = Resources.traducao.digite_o_texto_para_filtrar;
                    }
                    else if (controle is DevExpress.Web.ASPxMemo)
                    {
                        (controle as DevExpress.Web.ASPxMemo).ValidationSettings.ErrorDisplayMode = ErrorDisplayMode.ImageWithTooltip;
                        (controle as DevExpress.Web.ASPxMemo).ValidationSettings.Display = Display.Dynamic;
                        (controle as DevExpress.Web.ASPxMemo).SpriteCssFilePath = CssSprite;
                        (controle as DevExpress.Web.ASPxMemo).DisabledStyle.BackColor = corFundoDesabilitado;
                        (controle as DevExpress.Web.ASPxMemo).DisabledStyle.ForeColor = corFonteDesabilitado;
                        (controle as DevExpress.Web.ASPxMemo).Border.BorderWidth = new Unit("1px");
                    }
                    else if (controle is DevExpress.Web.ASPxTextBox)
                    {
                        (controle as DevExpress.Web.ASPxTextBox).ValidationSettings.ErrorDisplayMode = ErrorDisplayMode.ImageWithTooltip;
                        (controle as DevExpress.Web.ASPxTextBox).ValidationSettings.Display = Display.Dynamic;
                        (controle as DevExpress.Web.ASPxTextBox).SpriteCssFilePath = CssSprite;
                        (controle as DevExpress.Web.ASPxTextBox).DisabledStyle.BackColor = corFundoDesabilitado;
                        (controle as DevExpress.Web.ASPxTextBox).DisabledStyle.ForeColor = corFonteDesabilitado;
                        (controle as DevExpress.Web.ASPxTextBox).Height = 20;
                        (controle as DevExpress.Web.ASPxTextBox).AutoCompleteType = AutoCompleteType.Disabled;
                    }
                    else if (controle is DevExpress.Web.ASPxButton)
                    {
                        if ((controle as DevExpress.Web.ASPxButton).SpriteCssFilePath == "SF")
                        {
                            (controle as DevExpress.Web.ASPxButton).CssClass = "buttonWithoutImage";
                            (controle as DevExpress.Web.ASPxButton).Style.Add(HtmlTextWriterStyle.BackgroundImage, "none");
                        }

                        (controle as DevExpress.Web.ASPxButton).SpriteCssFilePath = CssSprite;
                        (controle as DevExpress.Web.ASPxButton).Paddings.Padding = 0;
                        (controle as DevExpress.Web.ASPxButton).Height = 22;
                    }
                    else if (controle is DevExpress.Web.ASPxDateEdit)
                    {
                        (controle as DevExpress.Web.ASPxDateEdit).PopupVerticalAlign = PopupVerticalAlign.TopSides;

                        (controle as DevExpress.Web.ASPxDateEdit).SpriteCssFilePath = CssSprite;
                        (controle as DevExpress.Web.ASPxDateEdit).DisabledStyle.BackColor = corFundoDesabilitado;
                        (controle as DevExpress.Web.ASPxDateEdit).DisabledStyle.ForeColor = corFonteDesabilitado;
                        (controle as DevExpress.Web.ASPxDateEdit).Height = 20;
                    }
                    else if (controle is DevExpress.Web.ASPxEdit)
                        (controle as DevExpress.Web.ASPxEdit).SpriteCssFilePath = CssSprite;
                    else if (controle is DevExpress.Web.ASPxMenuBase)
                    {
                        (controle as DevExpress.Web.ASPxMenuBase).SpriteCssFilePath = CssSprite;
                        (controle as DevExpress.Web.ASPxMenuBase).ItemSpacing = 2;
                    }
                    else if (controle is DevExpress.Web.ASPxGridView && !isCalendar)
                    {
                        ASPxGridView gv = (controle as DevExpress.Web.ASPxGridView);
                        gv.Templates.EditForm = new EditFormTemplate();


                        gv.EditFormLayoutProperties.Styles.Style.Paddings.Padding = 0;
                        gv.StylesPopup.EditForm.MainArea.Paddings.Padding = 0;
                        gv.StylesPopup.EditForm.Header.Paddings.PaddingBottom = 0;
                        gv.StylesPopup.EditForm.Header.Paddings.PaddingTop = 0;
                        gv.SettingsEditing.BatchEditSettings.KeepChangesOnCallbacks = DefaultBoolean.False;

                        foreach (GridViewColumn column in gv.Columns)
                        {
                            if (column is GridViewCommandColumn)
                            {
                                // Espaço para outras situações de colunas serem usadas.
                            }
                            else
                            {
                                if (column is GridViewEditDataColumn)
                                {
                                    if (column is GridViewDataDateColumn)
                                    {
                                        ((GridViewDataDateColumn)column).PropertiesDateEdit.PopupVerticalAlign = PopupVerticalAlign.TopSides;
                                    }
                                }
                            }
                        }

                        if (!isMaster)
                            padronizaGridView(gv, true, 50, true);

                        gv.Images.SpriteCssFilePath = CssSprite;


                        /* Experimento Bootstrap */
                        /* Teste de responsividade nos grids */
                        /*
                        gv.Width = System.Web.UI.WebControls.Unit.Percentage(100);
                        gv.SettingsAdaptivity.AdaptivityMode = GridViewAdaptivityMode.HideDataCells;
                        gv.SettingsAdaptivity.AllowOnlyOneAdaptiveDetailExpanded = true;
                        gv.SettingsAdaptivity.AdaptiveDetailColumnCount = 2;
                        gv.SettingsAdaptivity.HideDataCellsAtWindowInnerWidth = 900;
                        gv.EnableRowsCache = false;
                        gv.Settings.UseFixedTableLayout = true;
                        gv.SettingsAdaptivity.AdaptiveDetailLayoutProperties.ColCount = 2;
                        gv.EditFormLayoutProperties.SettingsAdaptivity.AdaptivityMode = FormLayoutAdaptivityMode.SingleColumnWindowLimit;
                        gv.EditFormLayoutProperties.SettingsAdaptivity.SwitchToSingleColumnAtWindowInnerWidth = 600;
                        gv.Styles.Cell.Wrap = DefaultBoolean.True;
                        gv.SettingsBehavior.AllowEllipsisInText = true;
                        */
                    }
                    else if (controle is DevExpress.Web.ASPxTreeList.ASPxTreeList)
                        (controle as DevExpress.Web.ASPxTreeList.ASPxTreeList).Images.SpriteCssFilePath = CssSprite;
                    else if (controle is DevExpress.Web.ASPxPivotGrid.ASPxPivotGrid)
                    {
                        DevExpress.Web.ASPxPivotGrid.ASPxPivotGrid pivot = (controle as DevExpress.Web.ASPxPivotGrid.ASPxPivotGrid);
                        pivot.Images.SpriteCssFilePath = CssSprite;
                        //pivot.StylesPrint.Cell.Font = new Font("Verdana", fonte);
                        //pivot.StylesPrint.CustomTotalCell.Font = new Font("Verdana", fonte);
                        //pivot.StylesPrint.FieldHeader.Font = new Font("Verdana", fonte + 1, FontStyle.Bold);
                        //pivot.StylesPrint.FieldValue.Font = new Font("Verdana", fonte);
                        //pivot.StylesPrint.FieldValueGrandTotal.Font = new Font("Verdana", fonte);
                        //pivot.StylesPrint.FieldValueTotal.Font = new Font("Verdana", fonte);
                        //pivot.StylesPrint.GrandTotalCell.Font = new Font("Verdana", fonte);
                        //pivot.StylesPrint.Lines.Font = new Font("Verdana", fonte);
                        //pivot.StylesPrint.TotalCell.Font = new Font("Verdana", fonte + 1);
                        pivot.OptionsPager.Position = PagerPosition.Bottom;
                        pivot.OptionsFilter.ShowOnlyAvailableItems = true;

                        pivot.OptionsView.HorizontalScrollBarMode = ScrollBarMode.Visible;
                        pivot.OptionsView.VerticalScrollBarMode = ScrollBarMode.Visible;
                    }
                    else if (controle is DevExpress.Web.ASPxNavBar)
                    {
                        (controle as DevExpress.Web.ASPxNavBar).SpriteCssFilePath = CssSprite;
                        (controle as DevExpress.Web.ASPxNavBar).ItemStyle.Wrap = DefaultBoolean.True;
                        (controle as DevExpress.Web.ASPxNavBar).GroupHeaderStyle.Wrap = DefaultBoolean.True;
                    }
                    else if (controle is DevExpress.Web.ASPxScheduler.ASPxScheduler)
                        (controle as DevExpress.Web.ASPxScheduler.ASPxScheduler).Images.SpriteCssFilePath = CssSprite;
                    else if (controle is DevExpress.Web.ASPxRoundPanel)
                    {
                        ASPxRoundPanel painel = (controle as DevExpress.Web.ASPxRoundPanel);
                        painel.SpriteCssFilePath = CssSprite;
                        painel.Border.BorderWidth = Unit.Empty;

                        /*
                        painel.BottomLeftCorner.Height = 2;
                        painel.BottomLeftCorner.Width = 2;

                        painel.BottomRightCorner.Height = 2;
                        painel.BottomRightCorner.Width = 2;

                        painel.NoHeaderTopLeftCorner.Height = 2;
                        painel.NoHeaderTopLeftCorner.Width = 2;

                        painel.NoHeaderTopRightCorner.Height = 2;
                        painel.NoHeaderTopRightCorner.Width = 2;

                        painel.TopLeftCorner.Height = 2;
                        painel.TopLeftCorner.Width = 2;

                        painel.TopRightCorner.Height = 2;
                        painel.TopRightCorner.Width = 2;
                        */



                        painel.CornerRadius = new Unit("1px");


                        if (painel.ShowHeader == false)
                        {
                            painel.HeaderStyle.Border.BorderStyle = BorderStyle.None;
                            painel.BorderTop.BorderStyle = BorderStyle.None;
                        }

                    }
                    else if (controle is DevExpress.Web.ASPxPopupControl)
                        (controle as DevExpress.Web.ASPxPopupControl).SpriteCssFilePath = CssSprite;
                    else if (controle is DevExpress.Web.ASPxHtmlEditor.ASPxHtmlEditor)
                    {
                        (controle as DevExpress.Web.ASPxHtmlEditor.ASPxHtmlEditor).Styles.CssPostfix = CssPostfix;
                        (controle as DevExpress.Web.ASPxHtmlEditor.ASPxHtmlEditor).Styles.CssFilePath = CssFilePath;
                        /*
                        (controle as DevExpress.Web.ASPxHtmlEditor.ASPxHtmlEditor).Images.SpriteCssFilePath = CssFilePath;
                        (controle as DevExpress.Web.ASPxHtmlEditor.ASPxHtmlEditor).ImagesEditors.SpriteCssFilePath = CssFilePath;
                        (controle as DevExpress.Web.ASPxHtmlEditor.ASPxHtmlEditor).StylesButton.CssPostfix = CssPostfix;
                        (controle as DevExpress.Web.ASPxHtmlEditor.ASPxHtmlEditor).StylesButton.CssFilePath = CssFilePath;
                        */
                       //controle as DevExpress.Web.ASPxHtmlEditor.ASPxHtmlEditor).Theme = CssPostfix;
                    }
                    else if (controle is DevExpress.Web.ASPxTabControlBase)
                        (controle as DevExpress.Web.ASPxTabControlBase).SpriteCssFilePath = CssSprite;
                    else if (controle is DevExpress.Web.ASPxSpellChecker.ASPxSpellChecker)
                        (controle as DevExpress.Web.ASPxSpellChecker.ASPxSpellChecker).SpriteCssFilePath = CssSprite;
                    else if (controle is DevExpress.XtraReports.Web.ReportToolbar)
                        (controle as DevExpress.XtraReports.Web.ReportToolbar).Images.SpriteCssFilePath = CssSprite;
                    else if (controle is DevExpress.Web.ASPxLoadingPanel)
                    {
                        (controle as DevExpress.Web.ASPxLoadingPanel).Modal = true;
                        (controle as DevExpress.Web.ASPxLoadingPanel).Text = "";
                        (controle as DevExpress.Web.ASPxLoadingPanel).Image.Url = "~/imagens/carregando.gif";
                    }
                    else if (controle is DevExpress.Web.ASPxDropDownEdit)
                    {
                        (controle as DevExpress.Web.ASPxDropDownEdit).CssFilePath = CssFilePath;
                        (controle as DevExpress.Web.ASPxDropDownEdit).CssPostfix = CssPostfix;
                    }

                    //try
                    //{
                    //    (controle as DevExpress.Web.ASPxWebControl).Font.Name = "Verdana";
                    //    (controle as DevExpress.Web.ASPxWebControl).Font.Size = new FontUnit(fonte + "pt");
                    //}
                    //catch
                    //{
                    //}
                }
            }
        }

        public bool salvarVisual(string estilo, int codigoUsuarioLogado)
        {
            int regAfetados = 0;
            try
            {
                comandoSQL = string.Format(
                    @"UPDATE {0}.{1}.Usuario
                     SET IDEstiloVisual = '{2}'      
                   WHERE CodigoUsuario = {3}",
                    bancodb, Ownerdb, estilo, codigoUsuarioLogado);
                execSQL(comandoSQL, ref regAfetados);

                setInfoSistema("IDEstiloVisual", estilo);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int aplicaEstiloVisual(Control root, bool isCalendar = false)
        {
            var idVisual = getInfoSistema("IDEstiloVisual");
            if (idVisual == null)
                return aplicaEstiloVisual(root, BriskTheme.Light, isCalendar);
            else
                return aplicaEstiloVisual(root, idVisual.ToString(), isCalendar);
        }

        public void aplicaTema(Page pagina, string nomeTema = "")
        {
            //Se o nome do tema não veio por parâmetro eu busco na variável de sessão
            if(nomeTema == "")
            {
                var IDEstiloVisual = getInfoSistema("IDEstiloVisual");

                if (IDEstiloVisual != null)
                {
                    nomeTema = IDEstiloVisual.ToString();
                }
            }
            if (nomeTema == BriskTheme.Light || nomeTema == BriskTheme.Dark)
            {
                pagina.Theme = nomeTema;
            }
            else
            {
                pagina.Theme = BriskTheme.Light;
            }     
        }

        public int aplicaEstiloVisualComFonte(Control root, int fonteZise)
        {
            fonte = fonteZise;
            return aplicaEstiloVisual(root, getInfoSistema("IDEstiloVisual").ToString());
        }

        public Control GetCustomStyle()
        {
            return new LiteralControl { Text = "" };
        }

        public int aplicaEstiloVisual(Control root, string estilo, bool isCalendar = false)
        {
            int qtde = 0;
            List<Control> controles = getControles(root);
            System.Collections.Generic.List<object> objControlesAspX = new System.Collections.Generic.List<object>();
            foreach (Control controle in controles)
            {
                if (controle is DevExpress.Web.ASPxWebControl)
                {

                    objControlesAspX.Add(controle);
                    qtde++;
                }
            }
            setVisual(estilo, root is MasterPage, isCalendar, objControlesAspX.ToArray());
            if ((root is System.Web.UI.Page) && (System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName != "pt"))
            {

                //coletaTraducao(root, controles);
                traduzControles(root, controles);
            }
            try
            {
                if (root != null && root.Page != null && root.Page.Header != null && root.Page.Header.Controls != null)
                {
                    root.Page.Header.Controls.Add(GetCustomStyle());
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }

            return qtde;
        }

        public void traduzControles(Control root, Control[] controles)
        {
            traduzControles(root, controles.ToList());
        }

        public void traduzControles(Control root)
        {
            List<Control> controles = getControles(root);
            if ((root is System.Web.UI.Page) && (System.Threading.Thread.CurrentThread.CurrentCulture.Name != "pt-BR"))
            {
                traduzControles(root, controles);
            }
        }

        public void traduzControles(Control root, List<Control> controles)
        {

            foreach (Control controle in controles)
            {
                if (controle is WebControl)
                {
                    (controle as WebControl).ToolTip = traduzExpressao((controle as WebControl).ToolTip);
                }
                // se é um controle devExpress
                if (controle is ASPxWebControlBase)
                {

                    if (controle is ASPxButton)
                    {
                        (controle as ASPxButton).Text = traduzExpressao((controle as ASPxButton).Text);
                    }
                    else if (controle is ASPxButtonEdit)
                    {
                        ASPxButtonEdit ctrl = controle as ASPxButtonEdit;
                        ctrl.NullText = traduzExpressao(ctrl.NullText);
                        foreach (DevExpress.Web.EditButton item in ctrl.Buttons)
                        {
                            item.Text = traduzExpressao(item.Text);
                            item.ToolTip = traduzExpressao(item.ToolTip);
                        }
                    }
                    else if (controle is ASPxCallbackPanel)
                    {
                        (controle as ASPxCallbackPanel).SettingsLoadingPanel.Text = traduzExpressao((controle as ASPxCallbackPanel).SettingsLoadingPanel.Text);
                        traduzControles(controle);
                    }
                    else if (controle is ASPxCheckBox)
                    {
                        (controle as ASPxCheckBox).Text = traduzExpressao((controle as ASPxCheckBox).Text);
                    }

                    else if (controle is ASPxComboBox)
                    {
                        ASPxComboBox comboBox = (controle as ASPxComboBox);
                        foreach (DevExpress.Web.ListBoxColumn item in comboBox.Columns)
                        {
                            item.Caption = traduzExpressao(item.Caption);
                        }
                        if (string.IsNullOrEmpty(comboBox.DataSourceID) && comboBox.DataSource == null)
                        {
                            foreach (DevExpress.Web.ListEditItem item in comboBox.Items)
                            {
                                item.Text = traduzExpressao(item.Text);
                            }
                        }
                        comboBox.NullText = traduzExpressao(comboBox.NullText);
                    }

                    else if (controle is ASPxGridView)
                    {
                        ASPxGridView gv = (controle as DevExpress.Web.ASPxGridView);

                        gv.SettingsCommandButton.ClearFilterButton.Text = traduzExpressao(gv.SettingsCommandButton.ClearFilterButton.Text);
                        gv.SettingsCommandButton.ClearFilterButton.Image.ToolTip = traduzExpressao(gv.SettingsCommandButton.ClearFilterButton.Image.ToolTip);

                        gv.SettingsCommandButton.NewButton.Text = traduzExpressao(gv.SettingsCommandButton.NewButton.Text);
                        gv.SettingsCommandButton.NewButton.Image.ToolTip = traduzExpressao(gv.SettingsCommandButton.NewButton.Image.ToolTip);

                        gv.SettingsCommandButton.EditButton.Text = traduzExpressao(gv.SettingsCommandButton.EditButton.Text);
                        gv.SettingsCommandButton.EditButton.Image.ToolTip = traduzExpressao(gv.SettingsCommandButton.EditButton.Image.ToolTip);

                        gv.SettingsCommandButton.DeleteButton.Text = traduzExpressao(gv.SettingsCommandButton.DeleteButton.Text);
                        gv.SettingsCommandButton.DeleteButton.Image.ToolTip = traduzExpressao(gv.SettingsCommandButton.DeleteButton.Image.ToolTip);

                        gv.SettingsCommandButton.CancelButton.Text = traduzExpressao(gv.SettingsCommandButton.CancelButton.Text);
                        gv.SettingsCommandButton.CancelButton.Image.ToolTip = traduzExpressao(gv.SettingsCommandButton.CancelButton.Image.ToolTip);

                        gv.SettingsCommandButton.UpdateButton.Text = traduzExpressao(gv.SettingsCommandButton.UpdateButton.Text);
                        gv.SettingsCommandButton.UpdateButton.Image.ToolTip = traduzExpressao(gv.SettingsCommandButton.UpdateButton.Image.ToolTip);

                        gv.SettingsText.ConfirmDelete = traduzExpressao(gv.SettingsText.ConfirmDelete);
                        gv.SettingsText.PopupEditFormCaption = traduzExpressao(gv.SettingsText.PopupEditFormCaption);
                        gv.SettingsText.GroupPanel = traduzExpressao(gv.SettingsText.GroupPanel);
                        gv.SettingsText.EmptyDataRow = traduzExpressao(gv.SettingsText.EmptyDataRow);
                        foreach (LayoutItemBase item in gv.EditFormLayoutProperties.Items)
                        {
                            item.Caption = traduzExpressao(item.Caption);
                            item.TabImage.ToolTip = traduzExpressao(item.TabImage.ToolTip);
                        }

                        foreach (GridViewColumn column in gv.Columns)
                        {
                            if (column is GridViewCommandColumn)
                            {
                                foreach (GridViewCommandColumnCustomButton btn in ((GridViewCommandColumn)column).CustomButtons)
                                {
                                    btn.Text = traduzExpressao(btn.Text);
                                    btn.Image.ToolTip = traduzExpressao(btn.Image.ToolTip);
                                    btn.Image.AlternateText = traduzExpressao(btn.Image.AlternateText);

                                }
                                column.Caption = traduzExpressao(column.Caption);
                            }
                            else
                            {
                                column.Caption = traduzExpressao(column.Caption);
                                column.ToolTip = traduzExpressao(column.ToolTip);
                                if (column is GridViewEditDataColumn)
                                {
                                    ((GridViewEditDataColumn)column).EditFormSettings.Caption = traduzExpressao(column.Caption);
                                    if (column is GridViewDataComboBoxColumn)
                                    {
                                        ((GridViewDataComboBoxColumn)column).PropertiesComboBox.ValidationSettings.RequiredField.ErrorText = traduzExpressao(((GridViewDataComboBoxColumn)column).PropertiesComboBox.ValidationSettings.RequiredField.ErrorText);
                                        foreach (ListEditItem item in ((GridViewDataComboBoxColumn)column).PropertiesComboBox.Items)
                                        {
                                            item.Text = traduzExpressao(item.Text);
                                        }
                                    }
                                    else if (column is GridViewDataTextColumn)
                                    {
                                        ((GridViewDataTextColumn)column).PropertiesTextEdit.ValidationSettings.RequiredField.ErrorText = traduzExpressao(((GridViewDataTextColumn)column).PropertiesTextEdit.ValidationSettings.RequiredField.ErrorText);
                                    }
                                    else if (column is GridViewDataDateColumn)
                                    {
                                        ((GridViewDataDateColumn)column).PropertiesDateEdit.ValidationSettings.RequiredField.ErrorText = traduzExpressao(((GridViewDataDateColumn)column).PropertiesDateEdit.ValidationSettings.RequiredField.ErrorText);
                                    }
                                    else if (column is GridViewDataMemoColumn)
                                    {
                                        ((GridViewDataMemoColumn)column).PropertiesMemoEdit.ValidationSettings.RequiredField.ErrorText = traduzExpressao(((GridViewDataMemoColumn)column).PropertiesMemoEdit.ValidationSettings.RequiredField.ErrorText);
                                    }
                                    else if (column is GridViewDataSpinEditColumn)
                                    {
                                        ((GridViewDataSpinEditColumn)column).PropertiesSpinEdit.ValidationSettings.RequiredField.ErrorText = traduzExpressao(((GridViewDataSpinEditColumn)column).PropertiesSpinEdit.ValidationSettings.RequiredField.ErrorText);
                                    }
                                    else if (column is GridViewDataCheckColumn)
                                    {
                                        ((GridViewDataCheckColumn)column).PropertiesCheckEdit.ValidationSettings.RequiredField.ErrorText = traduzExpressao(((GridViewDataCheckColumn)column).PropertiesCheckEdit.ValidationSettings.RequiredField.ErrorText);
                                    }
                                }
                            }

                        }
                    }

                    else if (controle is ASPxHyperLink)
                    {
                        (controle as ASPxHyperLink).Text = traduzExpressao((controle as ASPxHyperLink).Text);
                    }

                    else if (controle is ASPxLabel)
                    {
                        (controle as ASPxLabel).Text = traduzExpressao((controle as ASPxLabel).Text);
                    }

                    else if (controle is ASPxMenu)
                    {
                        traduzItensMenu((controle as ASPxMenu).Items);
                    }

                    else if (controle is ASPxPageControl)
                    {
                        foreach (TabPage pg in (controle as ASPxPageControl).TabPages)
                        {
                            pg.Text = traduzExpressao(pg.Text);
                            pg.ToolTip = traduzExpressao(pg.ToolTip);
                        }
                    }

                    else if (controle is ASPxPopupControl)
                    {
                        ASPxPopupControl popupControl = controle as ASPxPopupControl;
                        popupControl.HeaderText = traduzExpressao(popupControl.HeaderText);
                        foreach (PopupWindow window in popupControl.Windows)
                        {
                            window.HeaderText = traduzExpressao(window.HeaderText);
                        }
                    }

                    else if (controle is ASPxRadioButtonList)
                    {
                        foreach (DevExpress.Web.ListEditItem item in (controle as ASPxRadioButtonList).Items)
                        {
                            item.Text = traduzExpressao(item.Text);
                        }
                    }

                    else if (controle is ASPxRoundPanel)
                    {
                        (controle as ASPxRoundPanel).HeaderText = traduzExpressao((controle as ASPxRoundPanel).HeaderText);
                    }

                    else if (controle is ASPxTabControl)
                    {
                        foreach (Tab pg in (controle as ASPxTabControl).Tabs)
                        {
                            pg.Text = traduzExpressao(pg.Text);
                            pg.ToolTip = traduzExpressao(pg.ToolTip);
                            pg.ActiveTabImage.AlternateText = traduzExpressao(pg.ActiveTabImage.AlternateText);
                            pg.ActiveTabImage.ToolTip = traduzExpressao(pg.ActiveTabImage.ToolTip);
                        }
                    }

                    else if (controle is ASPxTextBox)
                    {
                        (controle as ASPxTextBox).NullText = traduzExpressao((controle as ASPxTextBox).NullText);
                    }

                    else if (controle is ASPxTreeList)
                    {
                        ASPxTreeList tl = (controle as ASPxTreeList);
                        foreach (TreeListColumn column in tl.Columns)
                        {
                            column.Caption = traduzExpressao(column.Caption);
                            column.ToolTip = traduzExpressao(column.ToolTip);
                        }
                    }

                    else if (controle is ASPxNavBar)
                    {
                        ASPxNavBar navBar = controle as ASPxNavBar;
                        foreach (NavBarGroup grp in navBar.Groups)
                        {
                            grp.Text = traduzExpressao(grp.Text);
                            foreach (NavBarItem item in grp.Items)
                            {
                                item.Text = traduzExpressao(item.Text);
                            }
                        }
                    }

                    else if (controle is ASPxImage)
                    {
                        (controle as ASPxImage).ToolTip = traduzExpressao((controle as ASPxImage).ToolTip);
                    }

                    else if (controle is DevExpress.Web.ASPxPivotGrid.ASPxPivotGrid)
                    {
                        ASPxPivotGrid pivotGrid = controle as ASPxPivotGrid;
                        foreach (DevExpress.Web.ASPxPivotGrid.PivotGridField field in pivotGrid.Fields)
                        {
                            field.Caption = traduzExpressao(field.Caption);
                        }
                    }

                    else if (controle is ASPxFileManager)
                    {
                        ASPxFileManager fileManager = controle as ASPxFileManager;
                        foreach (FileManagerDetailsColumn column in fileManager.SettingsFileList.DetailsViewSettings.Columns)
                        {
                            column.Caption = traduzExpressao(column.Caption);
                        }
                        var itens = fileManager.SettingsToolbar.Items.AsEnumerable().Concat(fileManager.SettingsContextMenu.Items);
                        foreach (var item in itens)
                        {
                            item.Text = traduzExpressao(item.Text);
                            item.ToolTip = traduzExpressao(item.ToolTip);
                            item.Image.ToolTip = traduzExpressao(item.Image.ToolTip);
                        }
                    }

                    else if (controle is ASPxDateEdit)
                    {
                        ASPxDateEdit dateEdit = controle as ASPxDateEdit;
                        dateEdit.CalendarProperties.ClearButtonText = traduzExpressao(dateEdit.CalendarProperties.ClearButtonText);
                        dateEdit.CalendarProperties.TodayButtonText = traduzExpressao(dateEdit.CalendarProperties.TodayButtonText);
                        dateEdit.NullText = traduzExpressao(dateEdit.NullText);
                    }

                    else if (controle is ASPxHtmlEditor)
                    {
                        ASPxHtmlEditor htmlEditor = controle as ASPxHtmlEditor;
                        foreach (HtmlEditorToolbar toolbar in htmlEditor.Toolbars)
                        {
                            foreach (HtmlEditorToolbarItem item in toolbar.Items)
                            {
                                ToolbarListEditItemCollection toolbarListEditItemCollection;
                                //ToolbarListEditItem
                                //ToolbarParagraphFormattingEdit
                                //ToolbarFontSizeEdit
                                if (item is ToolbarParagraphFormattingEdit)
                                    toolbarListEditItemCollection = (item as ToolbarParagraphFormattingEdit).Items;
                                else if (item is ToolbarFontSizeEdit)
                                    toolbarListEditItemCollection = (item as ToolbarFontSizeEdit).Items;
                                else
                                    continue;

                                foreach (ToolbarListEditItem toolbarListEditItem in toolbarListEditItemCollection)
                                {
                                    toolbarListEditItem.Text = traduzExpressao(toolbarListEditItem.Text);
                                }
                            }
                        }
                    }

                    else if (controle is ASPxScheduler)
                    {
                        ASPxScheduler scheduler = controle as ASPxScheduler;
                        foreach (var label in scheduler.Storage.Appointments.Labels)
                        {
                            label.DisplayName = traduzExpressao(label.DisplayName);
                            label.MenuCaption = traduzExpressao(label.MenuCaption);
                        }
                        foreach (var status in scheduler.Storage.Appointments.Statuses)
                        {
                            status.DisplayName = traduzExpressao(status.DisplayName);
                            status.MenuCaption = traduzExpressao(status.MenuCaption);
                        }
                        var timeSlots = scheduler.Views.DayView.TimeSlots
                            .Concat(scheduler.Views.FullWeekView.TimeSlots)
                            .Concat(scheduler.Views.WorkWeekView.TimeSlots).ToList();
                        foreach (var timeSlot in timeSlots)
                        {
                            timeSlot.DisplayName = traduzExpressao(timeSlot.DisplayName);
                            timeSlot.MenuCaption = traduzExpressao(timeSlot.MenuCaption);
                        }
                        scheduler.Views.DayView.ShortDisplayName = traduzExpressao(scheduler.Views.DayView.ShortDisplayName);
                        scheduler.Views.DayView.DisplayName = traduzExpressao(scheduler.Views.DayView.DisplayName);
                        scheduler.Views.DayView.MenuCaption = traduzExpressao(scheduler.Views.DayView.MenuCaption);
                        scheduler.Views.MonthView.DisplayName = traduzExpressao(scheduler.Views.MonthView.DisplayName);
                        scheduler.Views.MonthView.MenuCaption = traduzExpressao(scheduler.Views.MonthView.MenuCaption);
                        scheduler.Views.MonthView.ShortDisplayName = traduzExpressao(scheduler.Views.MonthView.ShortDisplayName);
                        scheduler.Views.TimelineView.DisplayName = traduzExpressao(scheduler.Views.TimelineView.DisplayName);
                        scheduler.Views.TimelineView.MenuCaption = traduzExpressao(scheduler.Views.TimelineView.MenuCaption);
                        scheduler.Views.TimelineView.ShortDisplayName = traduzExpressao(scheduler.Views.TimelineView.ShortDisplayName);
                        scheduler.Views.WeekView.DisplayName = traduzExpressao(scheduler.Views.WeekView.DisplayName);
                        scheduler.Views.WeekView.MenuCaption = traduzExpressao(scheduler.Views.WeekView.MenuCaption);
                        scheduler.Views.WeekView.ShortDisplayName = traduzExpressao(scheduler.Views.WeekView.ShortDisplayName);
                        scheduler.Views.WorkWeekView.DisplayName = traduzExpressao(scheduler.Views.WorkWeekView.DisplayName);
                        scheduler.Views.WorkWeekView.MenuCaption = traduzExpressao(scheduler.Views.WorkWeekView.MenuCaption);
                        scheduler.Views.WorkWeekView.ShortDisplayName = traduzExpressao(scheduler.Views.WorkWeekView.ShortDisplayName);

                        scheduler.OptionsLoadingPanel.Text = traduzExpressao(scheduler.OptionsLoadingPanel.Text);
                        scheduler.OptionsView.NavigationButtons.NextCaption = traduzExpressao(scheduler.OptionsView.NavigationButtons.NextCaption);
                        scheduler.OptionsView.NavigationButtons.PrevCaption = traduzExpressao(scheduler.OptionsView.NavigationButtons.PrevCaption);
                    }

                    else if (controle is ReportToolbar)
                    {
                        var reportToolbar = controle as ReportToolbar;
                        foreach (ReportToolbarButton item in reportToolbar.Items.OfType<ReportToolbarButton>())
                        {
                            item.Text = traduzExpressao(item.Text);
                            item.ToolTip = traduzExpressao(item.ToolTip);
                        }
                        foreach (ReportToolbarLabel item in reportToolbar.Items.OfType<ReportToolbarLabel>())
                        {
                            item.Text = traduzExpressao(item.Text);
                        }
                    }

                    else if (controle is ASPxListBox)
                    {
                        ASPxListBox listBox = controle as ASPxListBox;
                        listBox.FilteringSettings.EditorNullText = traduzExpressao(listBox.FilteringSettings.EditorNullText);
                    }

                    if (controle is ASPxEdit)
                    {
                        ASPxEdit edit = controle as ASPxEdit;
                        edit.ValidationSettings.RequiredField.ErrorText = traduzExpressao(edit.ValidationSettings.RequiredField.ErrorText);
                    }

                    if (controle is ASPxSpinEdit)
                    {
                        ASPxSpinEdit edit = controle as ASPxSpinEdit;
                        edit.ValidationSettings.RequiredField.ErrorText = traduzExpressao(edit.ValidationSettings.RequiredField.ErrorText);
                        edit.ToolTip = traduzExpressao(edit.ToolTip);
                        edit.NullText = traduzExpressao(edit.NullText);
                    }
                }
            }
        }

        private void traduzItensMenu(DevExpress.Web.MenuItemCollection items)
        {
            foreach (DevExpress.Web.MenuItem item in items)
            {
                item.Text = traduzExpressao(item.Text);
                item.ToolTip = traduzExpressao(item.ToolTip);
                DevExpress.Web.MenuItemCollection itensFilhos = item.Items;
                if (itensFilhos != null && itensFilhos.Count > 0)
                    traduzItensMenu(itensFilhos);
            }
        }

        private string traduzExpressao(string expressao)
        {
            string strToReturn;
            if (string.IsNullOrWhiteSpace(expressao))
            {
                strToReturn = expressao;
            }
            else
            {
                char[] arrExpressao = expressao.ToArray();
                char c;

                for (int i = 0; i < arrExpressao.Length; i++)
                {
                    c = arrExpressao[i];
                    if (i == 0)
                    {
                        if ((95 == c) || ((c >= 65) && (c <= 90)) || ((c >= 97) && (c <= 122)))
                        {
                            arrExpressao[i] = char.ToLower(c);
                        }
                        else
                        {
                            arrExpressao[i] = '_';
                        }
                    }
                    else
                    {
                        if ((95 == c) || ((c >= 48) && (c <= 57)) || ((c >= 65) && (c <= 90)) || ((c >= 97) && (c <= 122)))
                        {
                            arrExpressao[i] = char.ToLower(c);
                        }
                        else
                        {
                            arrExpressao[i] = '_';
                        }
                    }
                }

                string strToSearch = new string(arrExpressao);
                //Verifica se não é uma string vazia ou composta apenas por '_'. Caso seja retorna a própria expressão
                if (string.IsNullOrWhiteSpace(strToSearch) || strToSearch == new string('_', strToSearch.Length))
                    return expressao;

                try
                {
                    //System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                    string strTraduzida = getTextoTraduzido(strToSearch, "");
                    if (string.IsNullOrWhiteSpace(strTraduzida))
                    {
                        strToReturn = expressao;
                    }
                    else
                    {
                        strToReturn = strTraduzida;
                    }
                }
                catch (Exception)
                {
                    strToReturn = expressao;
                }
            }
            return strToReturn;
        }

        /// <summary>
        /// Rotina para coletar as 'strings' de tradução do sistema.
        /// </summary>
        /// <remarks>
        /// A rotina assume que as telas passam pela rotina aplicaEstiloVisual que retira textos desnecessários como ASPxCallbackPanel.SettingsLoadingPanel.Text e outros;
        /// </remarks>
        /// <param name="root"></param>
        public void coletaTraducao(Control root)
        {
            List<Control> controles = getControles(root);
            coletaTraducao(root, controles);
        }

        /// <summary>
        /// Rotina para coletar as 'strings' de tradução do sistema.
        /// </summary>
        /// <remarks>
        /// A rotina assume que as telas passam pela rotina aplicaEstiloVisual que retira textos desnecessários como ASPxCallbackPanel.SettingsLoadingPanel.Text e outros;
        /// </remarks>
        /// <param name="root"></param>
        public void coletaTraducao(Control root, List<Control> controles)
        {
            string msgBco = "", contexto = "";

            if (root is System.Web.UI.Page)
            {
                contexto = (root as System.Web.UI.Page).AppRelativeVirtualPath;
            }
            else if (root is System.Web.UI.MasterPage)
            {
                contexto = (root as System.Web.UI.MasterPage).AppRelativeVirtualPath;
            }

            foreach (Control controle in controles)
            {

                if (controle is System.Web.UI.WebControls.WebControl)
                {
                    saveControlTextToTranslate(contexto, (controle as System.Web.UI.WebControls.WebControl).ToolTip, ref msgBco);
                }
                // se é um controle devExpress
                if (controle is ASPxWebControlBase)
                {

                    if (controle is ASPxButton)
                    {
                        saveControlTextToTranslate(contexto, (controle as ASPxButton).Text, ref msgBco);
                    }
                    else if (controle is ASPxButtonEdit)
                    {
                        ASPxButtonEdit ctrl = controle as ASPxButtonEdit;
                        foreach (DevExpress.Web.EditButton item in ctrl.Buttons)
                        {
                            saveControlTextToTranslate(contexto, item.Text, ref msgBco);
                        }
                    }
                    else if (controle is ASPxCallbackPanel)
                    {
                        saveControlTextToTranslate(contexto, (controle as ASPxCallbackPanel).SettingsLoadingPanel.Text, ref msgBco);
                    }
                    else if (controle is ASPxCheckBox)
                    {
                        saveControlTextToTranslate(contexto, (controle as ASPxCheckBox).Text, ref msgBco);
                    }
                    if (controle is ASPxButton)
                    {
                        saveControlTextToTranslate(contexto, (controle as ASPxButton).Text, ref msgBco);
                    }
                    else if (controle is ASPxGridView)
                    {
                        ASPxGridView gv = (controle as DevExpress.Web.ASPxGridView);

                        saveControlTextToTranslate(contexto, gv.SettingsCommandButton.ClearFilterButton.Text, ref msgBco);
                        saveControlTextToTranslate(contexto, gv.SettingsCommandButton.ClearFilterButton.Image.ToolTip, ref msgBco);

                        saveControlTextToTranslate(contexto, gv.SettingsCommandButton.NewButton.Text, ref msgBco);
                        saveControlTextToTranslate(contexto, gv.SettingsCommandButton.NewButton.Image.ToolTip, ref msgBco);

                        saveControlTextToTranslate(contexto, gv.SettingsCommandButton.EditButton.Text, ref msgBco);
                        saveControlTextToTranslate(contexto, gv.SettingsCommandButton.EditButton.Image.ToolTip, ref msgBco);

                        saveControlTextToTranslate(contexto, gv.SettingsCommandButton.DeleteButton.Text, ref msgBco);
                        saveControlTextToTranslate(contexto, gv.SettingsCommandButton.DeleteButton.Image.ToolTip, ref msgBco);

                        saveControlTextToTranslate(contexto, gv.SettingsCommandButton.CancelButton.Text, ref msgBco);
                        saveControlTextToTranslate(contexto, gv.SettingsCommandButton.CancelButton.Image.ToolTip, ref msgBco);

                        saveControlTextToTranslate(contexto, gv.SettingsCommandButton.UpdateButton.Text, ref msgBco);
                        saveControlTextToTranslate(contexto, gv.SettingsCommandButton.UpdateButton.Image.ToolTip, ref msgBco);

                        foreach (LayoutItemBase item in gv.EditFormLayoutProperties.Items)
                        {
                            saveControlTextToTranslate(contexto, item.Caption, ref msgBco);
                            saveControlTextToTranslate(contexto, item.TabImage.ToolTip, ref msgBco);
                        }

                        foreach (GridViewColumn column in gv.Columns)
                        {
                            if (column is GridViewCommandColumn)
                            {
                                foreach (GridViewCommandColumnCustomButton btn in ((GridViewCommandColumn)column).CustomButtons)
                                {
                                    saveControlTextToTranslate(contexto, btn.Text, ref msgBco);
                                    saveControlTextToTranslate(contexto, btn.Image.ToolTip, ref msgBco);
                                }
                            }
                            else
                            {
                                saveControlTextToTranslate(contexto, column.Caption, ref msgBco);
                                saveControlTextToTranslate(contexto, column.ToolTip, ref msgBco);
                                if (column is GridViewEditDataColumn)
                                {
                                    if (column is GridViewDataComboBoxColumn)
                                    {
                                        saveControlTextToTranslate(contexto, ((GridViewDataComboBoxColumn)column).PropertiesComboBox.ValidationSettings.RequiredField.ErrorText, ref msgBco);
                                    }
                                    else if (column is GridViewDataTextColumn)
                                    {
                                        saveControlTextToTranslate(contexto, ((GridViewDataTextColumn)column).PropertiesTextEdit.ValidationSettings.RequiredField.ErrorText, ref msgBco);
                                    }
                                    else if (column is GridViewDataDateColumn)
                                    {
                                        saveControlTextToTranslate(contexto, ((GridViewDataDateColumn)column).PropertiesDateEdit.ValidationSettings.RequiredField.ErrorText, ref msgBco);
                                    }
                                    else if (column is GridViewDataMemoColumn)
                                    {
                                        saveControlTextToTranslate(contexto, ((GridViewDataMemoColumn)column).PropertiesMemoEdit.ValidationSettings.RequiredField.ErrorText, ref msgBco);
                                    }
                                    else if (column is GridViewDataSpinEditColumn)
                                    {
                                        saveControlTextToTranslate(contexto, ((GridViewDataSpinEditColumn)column).PropertiesSpinEdit.ValidationSettings.RequiredField.ErrorText, ref msgBco);
                                    }
                                    else if (column is GridViewDataCheckColumn)
                                    {
                                        saveControlTextToTranslate(contexto, ((GridViewDataCheckColumn)column).PropertiesCheckEdit.ValidationSettings.RequiredField.ErrorText, ref msgBco);
                                    }
                                }
                                else if (column is GridViewDataColumn)
                                {
                                    ((GridViewDataColumn)column).Settings.AllowHeaderFilter = DefaultBoolean.False;
                                }
                            }

                        }
                    }

                    else if (controle is ASPxHyperLink)
                    {
                        saveControlTextToTranslate(contexto, (controle as ASPxHyperLink).Text, ref msgBco);
                    }

                    else if (controle is ASPxLabel)
                    {
                        saveControlTextToTranslate(contexto, (controle as ASPxLabel).Text, ref msgBco);
                    }

                    else if (controle is ASPxMenu)
                    {
                        foreach (DevExpress.Web.MenuItem item in (controle as ASPxMenu).Items)
                        {
                            saveControlTextToTranslate(contexto, item.Text, ref msgBco);
                        }
                    }
                    else if (controle is ASPxPageControl)
                    {
                        foreach (TabPage pg in (controle as ASPxPageControl).TabPages)
                        {
                            saveControlTextToTranslate(contexto, pg.Text, ref msgBco);
                            saveControlTextToTranslate(contexto, pg.ToolTip, ref msgBco);
                        }
                    }

                    else if (controle is ASPxTreeList)
                    {
                        ASPxTreeList tl = (controle as ASPxTreeList);
                        foreach (TreeListColumn column in tl.Columns)
                        {
                            saveControlTextToTranslate(contexto, column.Caption, ref msgBco);
                            saveControlTextToTranslate(contexto, column.ToolTip, ref msgBco);
                        }
                    }

                }
            }
        }

        private bool saveControlTextToTranslate(string IDContextoTraducao, string IDExpressaoTraducao, ref string mensagem)
        {
            if (!string.IsNullOrWhiteSpace(IDExpressaoTraducao))
            {
                int regAfetados = 0;
                try
                {
                    comandoSQL = string.Format(@" 
                BEGIN 
                    DECLARE @CodigoExpressao bigint;

                    EXEC {0}.{1}.[p_incluiExpressaoTraducao] 
                          @in_IDContextoExpressaoTraducao   = '{2}'
                        , @in_IDExpressaoTraducao           = '{3}'
                        , @ou_CodigoExpressaoTraducao       = @CodigoExpressao OUTPUT;
                END 
                    ", bancodb, Ownerdb, IDContextoTraducao.Trim().Replace("'", "''"), IDExpressaoTraducao.Trim().Replace("'", "''"));
                    execSQL(comandoSQL, ref regAfetados);
                }
                catch (Exception ex)
                {
                    mensagem = ex.Message;
                    return false;
                }
            }
            return true;
        }

        public Dictionary<string, string> getDicionarioExpressoesTraduzidas(params string[] grupos)
        {
            var dicionarioRetorno = new Dictionary<string, string>();
            var resourcesSet = Resources.traducao.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            string strKey = "";
            
            foreach (DictionaryEntry entry in resourcesSet)
            {
                strKey = entry.Key.ToString();
                if(grupos.Any(grp => strKey.StartsWith(string.Format("{0}_", grp), StringComparison.InvariantCultureIgnoreCase)))
                {
                    dicionarioRetorno.Add(strKey, entry.Value.ToString());
                }
            }
            return dicionarioRetorno;
        }

        public string getJsonExpressoesTraduzidas(params string[] grupos)
        {
            var serializer = new JavaScriptSerializer();
            var data = getDicionarioExpressoesTraduzidas(grupos);
            Dictionary<string, string> newData = new Dictionary<string, string>() { };
            foreach (KeyValuePair<string, string> item in data)
            {
                /*
                 * The following characters are reserved in JSON and must be properly escaped to be used in strings:
                 * Backspace is replaced with \b
                 * Form feed is replaced with \f
                 * Newline is replaced with \n
                 * Carriage return is replaced with \r
                 * Tab is replaced with \t
                 * Double quote is replaced with \"
                 * Backslash is replaced with \\
                 */
                newData.Add(item.Key, item.Value
                    .Replace("\\", "\\\\")
                    .Replace("\b", "\\\b")
                    .Replace("\f", "\\\f")
                    .Replace("\n", "\\\n")
                    .Replace("\r", "\\\r")
                    .Replace("\t", "\\\t")
                    .Replace("\"", "\\\""));
            }
            var json = serializer.Serialize(newData);
            return json;
        }

        #endregion

        #region Funções de banco de dados Compartilhadas

        public int getCodigoTipoAssociacao(string iniciaisTipoAssociacao)
        {
            string comandoSQL =
                @"SELECT CodigoTipoAssociacao
                    FROM TipoAssociacao
                   WHERE IniciaisTipoAssociacao = '" + iniciaisTipoAssociacao + "' ";

            DataSet ds = getDataSet(comandoSQL);
            return int.Parse(ds.Tables[0].Rows[0]["CodigoTipoAssociacao"].ToString());
        }

        public DataSet getTarefasRiscoQuestoes(int CodigoTipoAssociacao, int? CodigoObjetoAssociado)
        {
            string where = (CodigoObjetoAssociado.HasValue) ? "AND TL.CodigoObjetoAssociado = " + CodigoObjetoAssociado.Value : "";
            string comandoSQL = string.Format(
                        @"SELECT TL.CodigoToDoList, TTL.CodigoTarefa, TTL.DescricaoTarefa, TTL.InicioPrevisto, TTL.TerminoPrevisto, TTL.InicioReal, TTL.TerminoReal, 
                             TTL.CodigoUsuarioResponsavelTarefa, TTL.PercentualConcluido, TTL.Anotacoes, TTL.Prioridade, TTL.CodigoStatusTarefa, TTL.EsforcoPrevisto, TTL.EsforcoReal, 
                             TTL.CustoPrevisto, TTL.CustoReal, TTL.CodigoProjeto, U.NomeUsuario AS NomeUsuarioResponsavelTarefa, P.NomeProjeto, ST.DescricaoStatusTarefa
                        FROM {0}.{1}.ToDoList AS TL INNER JOIN
                             {0}.{1}.TarefaToDoList AS TTL ON TL.CodigoToDoList = TTL.CodigoToDoList INNER JOIN
							 {0}.{1}.RiscoQuestao rq ON rq.CodigoRiscoQuestao = TL.CodigoObjetoAssociado INNER JOIN
                             {0}.{1}.Usuario AS U ON TTL.CodigoUsuarioResponsavelTarefa = U.CodigoUsuario INNER JOIN
                             {0}.{1}.Projeto AS P ON P.CodigoProjeto = rq.CodigoProjeto INNER JOIN
                             {0}.{1}.StatusTarefa AS ST ON TTL.CodigoStatusTarefa = ST.CodigoStatusTarefa
                       WHERE TL.DataExclusao IS NULL 
                         AND TL.CodigoTipoAssociacao = {2}
                         {3} ", bancodb, Ownerdb, CodigoTipoAssociacao, where);

            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getCoresApresentacao()
        {
            string comandoSQL = string.Format(
                 @"SELECT CodigoCorApresentacao, CorApresentacao, _CorHexaDecimal, _CorImagem, LegendaCorEstrategia
			          ,'~/imagens/' + CorApresentacao + '.gif' AS urlCor
                 FROM {0}.{1}.CorApresentacao
               ", bancodb, Ownerdb);
            return getDataSet(comandoSQL);
        }

        public decimal? getValorInicioFaixaCorFisico(string Cor)
        {
            decimal? valInicioFaixa;

            string comandoSQL = string.Format(
                @"SELECT pmt.[ValorInicial] AS [ValorInicialFaixa]
			    FROM
				    {0}.{1}.[ParametroIndicadores]						AS [pmt]
					
					    INNER JOIN {0}.{1}.[TipoStatusAnalise]	AS [tsa]		ON 
						    (tsa.[CodigoTipoStatus]		= pmt.[CodigoTipoStatus])
						
					    INNER JOIN {0}.{1}.[CorApresentacao]		AS [ca]			ON 
						    (ca.[CodigoCorApresentacao] = tsa.[CodigoCorApresentacao])
			    WHERE
						    pmt.[TipoIndicador]	= 'FIS'
				    AND ca.[CorApresentacao] = '{2}'; 
            ", bancodb, Ownerdb, Cor);

            DataSet ds = getDataSet(comandoSQL);

            valInicioFaixa = null;

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                valInicioFaixa = (decimal?)ds.Tables[0].Rows[0]["ValorInicialFaixa"];

            return valInicioFaixa;
        }


        #endregion

        #region Critério Seleção

        public DataSet getCriteriosSelecao(string where)
        {
            comandoSQL = string.Format(
                 @"SELECT * 
                FROM {0}.{1}.CriterioSelecao
               WHERE (DataExclusao is null) {2}  order by DescricaoCriterioSelecao", bancodb, Ownerdb, where);
            return classeDados.getDataSet(comandoSQL);
        }

        public bool atualizaOpcoesCriteriosSelecao(int CodigoCriterioSelecao, string ItemCriterioSelecao
                                                , string OpcaoCriterioSelecao, int ValorOpcaoCriterioSelecao
                                                , string DescricaoCriterioSelecao, ref string msg)
        {
            int regAfetados = 0;
            try
            {
                comandoSQL = string.Format(@"
                        UPDATE {0}.{1}.OpcaoCriterioSelecao
                        SET DescricaoOpcaoCriterioSelecao   = '{5}'
                        ,   ValorOpcaoCriterioSelecao       = {3}
                        ,   DescricaoEstendidaOpcao         = {6}
                        WHERE CodigoCriterioSelecao = {4}
                        AND CodigoOpcaoCriterioSelecao = '{2}'"
               , bancodb, Ownerdb
               , ItemCriterioSelecao
               , ValorOpcaoCriterioSelecao, CodigoCriterioSelecao
               , OpcaoCriterioSelecao.Trim().Replace("'", "''")
               , DescricaoCriterioSelecao.Equals("") ? "NULL" : string.Format("'{0}'", DescricaoCriterioSelecao.Trim().Replace("'", "''")));
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public bool atualizaApenasOpcoesCriteriosSelecao(int CodigoCriterioSelecao, char CodigoOpcaoCriterioSelecao, char NovoCodigoOpcaoCriterioSelecao, ref string msg)
        {
            int regAfetados = 0;
            try
            {
                comandoSQL = string.Format(@"UPDATE {0}.{1}.OpcaoCriterioSelecao
                                            SET CodigoOpcaoCriterioSelecao = '{2}'
                                          WHERE CodigoCriterioSelecao = {3}
                                            AND CodigoOpcaoCriterioSelecao = '{4}'"
               , bancodb, Ownerdb, NovoCodigoOpcaoCriterioSelecao, CodigoCriterioSelecao, CodigoOpcaoCriterioSelecao);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public bool excluiOpcoesCriteriosSelecao(int CodigoCriterioSelecao, string CodigoOpcaoCriterioSelecao, ref string msg)
        {
            int regAfetados = 0;
            try
            {
                string where = "";
                if (CodigoOpcaoCriterioSelecao != "")
                    where = " AND CodigoOpcaoCriterioSelecao = '" + CodigoOpcaoCriterioSelecao + "'";

                comandoSQL = string.Format(
                    @"DELETE 
                    FROM {0}.{1}.OpcaoCriterioSelecao
                   WHERE CodigoCriterioSelecao = {2} {3}", bancodb, Ownerdb, CodigoCriterioSelecao, where);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }

        }

        #endregion

        #region Controle de Acesso

        public bool VerificaPermissaoUsuario(int codigoUsuario, int codigoEntidade, string iniciaisPermissao)
        {
            bool bPermissao = false;

            string comandoSQL = string.Format(@"SELECT {0}.{1}.f_VerificaAcessoConcedido({2}, {3}, {3}, null, 'EN', 0, null, '{4}') AS Permissao", bancodb, Ownerdb, codigoUsuario, codigoEntidade, iniciaisPermissao);

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds))
                if (ds.Tables[0].Rows.Count > 0)
                    bPermissao = (bool)ds.Tables[0].Rows[0]["Permissao"];

            return bPermissao;
        }

        public bool verificaAcessoStatusProjeto(int codigoProjeto)
        {
            string comandoSQL = string.Format(@"SELECT 1 
                                              FROM {0}.{1}.Projeto p 
                                             WHERE p.CodigoProjeto = {2}
                                               AND p.CodigoStatusProjeto IN (4,5,6)", bancodb, Ownerdb, codigoProjeto);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                return false;

            return true;
        }

        //          VerificaPermissaoUsuario(codigoUsuario    , codigoEntidadeUsuarioResponsavel, codigoProjeto   , "null"                 , "PR"                     , 0                  , "null"                , "PR_DesBlq");        
        public bool VerificaPermissaoUsuario(int codigoUsuario, int codigoEntidade, int codigoObjeto, string codigoTipoObjeto, string iniciaisTipoObjeto, int codigoObjetoPai, string codigoPermissao, string iniciaisPermissao)
        {
            bool bPermissao = false;

            if (iniciaisTipoObjeto == "PR")
            {

                int codigoTipoProjeto = int.Parse(getTipoObjetoPorCodigo(codigoObjeto) == "" ? "-1" : getTipoObjetoPorCodigo(codigoObjeto));

                switch (codigoTipoProjeto)
                {

                    case 3:
                        iniciaisTipoObjeto = "PC";
                        break;
                    case 4:
                        iniciaisTipoObjeto = "DC";
                        break;
                    case 5:
                        iniciaisTipoObjeto = "DS";
                        break;
                    default:
                        iniciaisTipoObjeto = "PR";
                        break;

                }

                iniciaisPermissao = iniciaisPermissao.Replace("PR_", iniciaisTipoObjeto + "_");
            }

            string comandoSQL = string.Format(@"SELECT {0}.{1}.f_VerificaAcessoConcedido({2}, {3}, {4}, {5}, '{6}', {7}, {8}, '{9}') AS Permissao"
                , bancodb //{0}
                , Ownerdb //{1}
                , codigoUsuario //{2}
                , codigoEntidade //{3}
                , codigoObjeto //{4}
                , codigoTipoObjeto //{5}
                , iniciaisTipoObjeto //{6}
                , codigoObjetoPai //{7}
                , codigoPermissao //{8}
                , iniciaisPermissao); //{9}

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds))
                if (ds.Tables[0].Rows.Count > 0)
                    bPermissao = (bool)ds.Tables[0].Rows[0]["Permissao"];

            return bPermissao;
        }

        public void verificaPermissaoProjetoInativo(int codigoProjeto, ref bool podeIncluir, ref bool podeEditar, ref bool podeExcluir)
        {
            int codigoStatusProjeto = getStatusProjeto(codigoProjeto);

            bool indicaProjetoInativo = codigoStatusProjeto == 4 || codigoStatusProjeto == 5 || codigoStatusProjeto == 6
              || codigoStatusProjeto == 17 || codigoStatusProjeto == 18 || codigoStatusProjeto == 19
              || codigoStatusProjeto == 22 || codigoStatusProjeto == 23 || codigoStatusProjeto == 24;

            bool possuiPermissaoEditar = VerificaPermissaoUsuario(int.Parse(getInfoSistema("IDUsuarioLogado").ToString()), int.Parse(getInfoSistema("CodigoEntidade").ToString()), "EN_AltPrjEsc");

            if (indicaProjetoInativo && !possuiPermissaoEditar)
            {
                podeIncluir = false;
                podeEditar = false;
                podeExcluir = false;
            }
        }

        /// <summary>
        /// Redireciona o sistema para devolver a página "Sem Acesso"
        /// </summary>
        /// <param name="page">Objeto página sendo montado no momento.</param>
        public void RedirecionaParaTelaSemAcesso(Page page)
        {
            try
            {

                page.Response.Redirect("~/erros/SemAcesso.aspx");
            }
            catch
            {
                page.Response.RedirectLocation = getPathSistema() + "erros/SemAcesso.aspx";
                page.Response.End();
            }
        }

        /// <summary>
        /// Redireciona o sistema para devolver a página "Sem Acesso"
        /// </summary>
        /// <param name="page">Objeto página sendo montado no momento.</param>
        public void RedirecionaParaTelaSemAcessoSemMasterPage(Page page)
        {
            try
            {

                page.Response.Redirect("~/erros/SemAcessoNoMaster.aspx");
            }
            catch
            {
                page.Response.RedirectLocation = getPathSistema() + "erros/SemAcessoNoMaster.aspx";
                page.Response.End();
            }
        }

        public void VerificaAcessoTela(Page page, int codigoUsuario, int codigoEntidade, int codigoObjeto, string codigoTipoObjeto, string iniciaisTipoObjeto, int codigoObjetoPai, string codigoPermissao, string iniciaisPermissao)
        {
            if (VerificaPermissaoUsuario(codigoUsuario, codigoEntidade, codigoObjeto, codigoTipoObjeto, iniciaisTipoObjeto, codigoObjetoPai, codigoPermissao, iniciaisPermissao) == false)
            {
                RedirecionaParaTelaSemAcesso(page);
            }
        }

        public void VerificaAcessoTelaSemMaster(Page page, int codigoUsuario, int codigoEntidade, int codigoObjeto, string codigoTipoObjeto, string iniciaisTipoObjeto, int codigoObjetoPai, string codigoPermissao, string iniciaisPermissao)
        {
            if (VerificaPermissaoUsuario(codigoUsuario, codigoEntidade, codigoObjeto, codigoTipoObjeto, iniciaisTipoObjeto, codigoObjetoPai, codigoPermissao, iniciaisPermissao) == false)
            {
                try
                {

                    page.Response.Redirect("~/erros/SemAcessoNoMaster.aspx");
                }
                catch
                {
                    page.Response.RedirectLocation = getPathSistema() + "erros/SemAcessoNoMaster.aspx";
                    page.Response.End();
                }
            }
        }
        /// <summary>
        /// Verifica se o usuário tem acesso à tela - Forma 2.
        /// </summary>
        /// <remarks>
        /// Esta 2ª forma da função verifica o acesso do usuário sem levar em conta um objeto específico, mas apenas o tipo 
        /// do objeto. Se o usuário tiver acesso a pelos menos 1 objeto do tipo especificado, a função nada fará. 
        /// Se o usuário não tiver acesso a nenhum objeto, ele será redirecionado para a página "sem acesso".
        /// </remarks>
        /// <param name="page">Referência da página em que está sendo verificado o acesso.</param>
        /// <param name="codigoUsuario">Código do usuário para o qual está sendo verificado o acesso.</param>
        /// <param name="codigoEntidade">Código da entidade dos objetos que serão verificados.</param>
        /// <param name="iniciaisTipoObjeto">Iniciais que irão determinar o tipo de objetos a verificar.</param>
        /// <param name="iniciaisPermissao">Iniciais que irão determinar a permissão a verificar.</param>
        public void VerificaAcessoTela(Page page, int codigoUsuario, int codigoEntidade, string iniciaisTipoObjeto, string iniciaisPermissao)
        {
            if (VerificaAcessoEmAlgumObjeto(codigoUsuario, codigoEntidade, iniciaisTipoObjeto, iniciaisPermissao) == false)
            {
                RedirecionaParaTelaSemAcesso(page);
            }
        }

        /// <summary>
        /// Verifica se o usuário tem acesso a algum objeto do tipo mencionado para a permissão indicada.
        /// </summary>
        /// <remarks>
        /// Esta função devolve o valor 0 ou 1 indicando se determinado usuário tem a permissão pretendida em 
        /// pelo menos 1 objeto do tipo indicado.
        /// </remarks>
        /// <param name="codigoUsuario">Código do usuário para o qual está sendo verificado o acesso.</param>
        /// <param name="codigoEntidade">Código da entidade dos objetos que serão verificados.</param>
        /// <param name="iniciaisTipoObjeto">Iniciais que irão determinar o tipo de objetos a verificar.</param>
        /// <param name="iniciaisPermissao">Iniciais que irão determinar a permissão a verificar.</param>
        /// <returns>Retorna 1 se o usuário tem a permissão em pelo menos 1 objeto. Caso contrário, retorna o valor 0.</returns>
        public bool VerificaAcessoEmAlgumObjeto(int codigoUsuario, int codigoEntidade, string iniciaisTipoObjeto, string iniciaisPermissao)
        {
            bool bPermissao = false;

            string comandoSQL = string.Format(@"SELECT {0}.{1}.f_VerificaAcessoEmAlgumObjeto({2}, NULL, '{4}', {3}, '{5}') AS Permissao"
                , bancodb //{0}
                , Ownerdb //{1}
                , codigoUsuario //{2}
                , codigoEntidade //{3}
                , iniciaisTipoObjeto //{4}
                , iniciaisPermissao); //{5}

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds))
                if (ds.Tables[0].Rows.Count > 0)
                    bPermissao = (bool)ds.Tables[0].Rows[0]["Permissao"];

            return bPermissao;
        }


        /// <summary>
        /// Retorna verdadeiro caso o usuário tenha perfil de administrador na entidade em questão
        /// </summary>
        /// <param name="codigoUsuario"></param>
        /// <param name="CodigoEntidade"></param>
        /// <returns></returns>
        public bool PerfilAdministrador(int codigoUsuario, int CodigoEntidade)
        {
            int idUsuarioLogado = int.Parse(getInfoSistema("IDUsuarioLogado").ToString());

            // se o codigo do usuário em questão for o mesmo do usuário logado, verifica se já está registrado 
            // na variável de sessão que é administrador 

            if (codigoUsuario == idUsuarioLogado)
            {
                object objAux = getInfoSistema("PerfilAdministrador");

                // se o codigo do usuário parâmetro for o mesmo do usuário logado e tiver perfil administrador
                // retorna true para todas as permissões
                if ((objAux != null) && objAux.ToString().Equals("1"))
                    return true;
            }

            bool bRetorno = false;

            string comandoSQL = string.Format(@"
            SELECT TOP 1 1 FROM {0}.{1}.f_GetPerfisUsuario({2}, {3}) WHERE [IniciaisPerfil] = 'ADM'
            ", bancodb, Ownerdb, codigoUsuario, CodigoEntidade);

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds))
                if (ds.Tables[0].Rows.Count > 0)
                    bRetorno = true;

            return bRetorno;
        }

        #endregion

        #region Riscos Padroes

        public DataSet getRiscosPadroes(string where)
        {
            comandoSQL = string.Format(
                @"SELECT * 
                FROM {0}.{1}.RiscoPadrao
               WHERE (DataExclusao is null) {2} order by  DescricaoRiscoPadrao", bancodb, Ownerdb, where);
            return classeDados.getDataSet(comandoSQL);
        }


        #endregion

        #region Tipo Projeto

        public DataSet getTipoProjeto(int codigoEmpresa)
        {
            // busca o nome do banco de dados
            comandoSQL = string.Format(
                @"SELECT NomeBDProjeto 
                FROM {0}.{1}.Empresa 
               WHERE CodigoEmpresa = {2}", bancodb, Ownerdb, codigoEmpresa);
            DataSet ds = classeDados.getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                string NomeBDProjeto = ds.Tables[0].Rows[0]["NomeBDProjeto"].ToString();

                comandoSQL = string.Format(
                    @"SELECT CodigoTipoProjeto, TipoProjeto
                    FROM {0}.{1}.TipoProjeto
                   ORDER BY TipoProjeto ", NomeBDProjeto, Ownerdb);

                return classeDados.getDataSet(comandoSQL);
            }
            else
                return null;
        }

        public string getNomeTipoProjeto(int codigoProjeto)
        {
            string nomeTipoProjeto = "";

            comandoSQL = string.Format(
                    @"SELECT tp.TipoProjeto 
                    FROM {0}.{1}.Projeto p INNER JOIN
                         {0}.{1}.TipoProjeto tp ON tp.CodigoTipoProjeto = p.CodigoTipoProjeto
                   WHERE p.CodigoProjeto = {2}", bancodb, Ownerdb, codigoProjeto);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                nomeTipoProjeto = ds.Tables[0].Rows[0]["TipoProjeto"].ToString();

            return nomeTipoProjeto;
        }

        public string getIniciaisTipoProjeto(int codigoProjeto)
        {
            string nomeTipoProjeto = "";

            comandoSQL = string.Format(
                    @"SELECT tp.IndicaTipoProjeto 
                    FROM {0}.{1}.Projeto p INNER JOIN
                         {0}.{1}.TipoProjeto tp ON tp.CodigoTipoProjeto = p.CodigoTipoProjeto
                   WHERE p.CodigoProjeto = {2}", bancodb, Ownerdb, codigoProjeto);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                nomeTipoProjeto = ds.Tables[0].Rows[0]["IndicaTipoProjeto"].ToString();

            if (nomeTipoProjeto.ToUpper() == "SPT")
            {
                nomeTipoProjeto = "PR";
            }

            return nomeTipoProjeto;
        }

        public string getIniciaisTipoAssociacaoProjeto(int codigoProjeto)
        {
            string IniciaisTipoAssociacao = "";

            comandoSQL = string.Format(
                    @"SELECT ta.IniciaisTipoAssociacao
                    FROM {0}.{1}.Projeto p INNER JOIN
                         {0}.{1}.TipoProjeto tp ON tp.CodigoTipoProjeto = p.CodigoTipoProjeto INNER JOIN
                         {0}.{1}.TipoAssociacao ta ON ta.CodigoTipoAssociacao = tp.CodigoTipoAssociacao
                   WHERE p.CodigoProjeto = {2}", bancodb, Ownerdb, codigoProjeto);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                IniciaisTipoAssociacao = ds.Tables[0].Rows[0]["IniciaisTipoAssociacao"].ToString();

            return IniciaisTipoAssociacao;
        }

        public int getCodigoTipoProjeto(int codigoProjeto)
        {
            int codigoTipoProjeto = -1;

            comandoSQL = string.Format(
                    @"SELECT p.CodigoTipoProjeto
                    FROM {0}.{1}.Projeto p 
                   WHERE p.CodigoProjeto = {2}", bancodb, Ownerdb, codigoProjeto);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                codigoTipoProjeto = int.Parse(ds.Tables[0].Rows[0]["CodigoTipoProjeto"].ToString());

            return codigoTipoProjeto;
        }

        #endregion

        #region Administracao - Usuarios

        public string getSelect_Usuario(int? codigoEntidade, string where)
        {
            string comandoSQL = string.Format(@"
               SELECT us.CodigoUsuario
                ,   us.NomeUsuario
                ,   us.ContaWindows
                ,   us.TipoAutenticacao
                ,   us.EMail
                ,   us.ResourceUID
                ,   us.TelefoneContato1
                ,   us.TelefoneContato2
                ,   us.Observacoes
                ,   us.DataInclusao
                ,   us.CodigoUsuarioInclusao
                ,   us.DataUltimaAlteracao
                ,   us.CodigoUsuarioUltimaAlteracao
                ,   us.DataExclusao
                ,   us.CodigoUsuarioExclusao
                ,   us.IDEstiloVisual
                ,   us.CodigoEntidadeResponsavelUsuario
                ,   uun.IndicaUsuarioAtivoUnidadeNegocio
                ,   us.CPF 
               -- ,   {0}.{1}.f_VerificaAcessoConcedido(us.CodigoUsuario, {2}, {2}, NULL, 'ME', 0, NULL, 'ME_AdmPrs')  *   16      -- 16	Compartilhar
               --             AS [Permissoes]

                FROM        {0}.{1}.Usuario AS us 
                INNER JOIN  {0}.{1}.UsuarioUnidadeNegocio AS uun ON (uun.CodigoUsuario = us.CodigoUsuario )

				WHERE Uun.CodigoUnidadeNegocio = {2}
                    {3}

                ORDER BY us.NomeUsuario
                ", bancodb, Ownerdb, codigoEntidade, where);
            return comandoSQL;
        }

        public DataSet getPerfisDisponivelUsuario(int idEntidadeLogada, string iniciaisPermissao, int idObjetoAssociado, int idObjetoPai)
        {
            string comandoSQL = string.Format(@"
                            EXEC {0}.{1}.p_perm_obtemPerfisDisponiveisUsuario  {2}, NULL, '{3}', {4}, {5}, -1
                            ", bancodb, Ownerdb, idObjetoAssociado, iniciaisPermissao, idObjetoPai
                                 , idEntidadeLogada);

            return getDataSet(comandoSQL);
        }

        /// <summary>
        /// Retorna os tipos de autenticações disponíveis
        /// </summary>
        /// <returns></returns>
        public DataSet getTipoAutenticacaoSistema()
        {
            // ACG em 04/11/2014 - Atender pedido de PBH
            string comandoSQL = string.Format(@"
                            SELECT * 
                              FROM TipoAutenticacao
                             WHERE IndicaTipoAtivo = 'S'
                             ORDER BY NomeTipoAutenticacao");

            return getDataSet(comandoSQL);
        }


        /// <summary>
        /// Função para trazer informações principais de um usuário, como código, nome e outras.
        /// </summary>
        /// <remarks>
        /// Indicada para uso quando se deseja saber se já existe na base um usuário 
        /// com email X, conta windows Y, etc.
        /// Como a [UsuarioUnidadeNegocio] é incluída no select, pode-se usá-la também para saber
        /// se determinado usuário está na unidade A com status x.
        /// </remarks>
        /// <param name="where">filtro a ser usado na seleção dos dados.</param>
        /// <returns></returns>
        public DataSet getDadosResumidosUsuario(string where)
        {
            comandoSQL = string.Format(@"  
               SELECT us.[CodigoUsuario], us.[NomeUsuario], us.[ContaWindows], us.[TipoAutenticacao], us.[EMail], us.[ResourceUID]
                , us.[DataInclusao], us.[CodigoUsuarioInclusao], us.[DataUltimaAlteracao], us.[CodigoUsuarioUltimaAlteracao]
                , us.[DataExclusao], us.[CodigoUsuarioExclusao], us.[CodigoEntidadeResponsavelUsuario],us.CPF
                , uun.[CodigoUnidadeNegocio], un.[NomeUnidadeNegocio], uun.[IndicaUsuarioAtivoUnidadeNegocio]
                
               FROM 
                    {0}.{1}.[Usuario]                                   AS [us]

                        INNER JOIN {0}.{1}.[UsuarioUnidadeNegocio]      AS [uun]
						    ON (    uun.[CodigoUsuario]         = us.[CodigoUsuario] )

                        INNER JOIN {0}.{1}.[UnidadeNegocio]				AS [un]
						    ON (    un.[CodigoUnidadeNegocio]	= uun.[CodigoUnidadeNegocio] )

			    WHERE 1=1 {2}
               ORDER BY us.NomeUsuario ", bancodb, Ownerdb, where);
            return getDataSet(comandoSQL);
        }

        public bool excluiUsuario(int codigoUsuario, int codigoUnidadeNegocio, ref int regAfetados, ref string msg)
        {
            try
            {
                comandoSQL = string.Format(@"UPDATE {0}.{1}.[UsuarioUnidadeNegocio]
                                            SET [IndicaUsuarioAtivoUnidadeNegocio] = 'N'
                                          WHERE (CodigoUsuario = {2} AND 
                                                 CodigoUnidadeNegocio = {3});", bancodb, Ownerdb, codigoUsuario, codigoUnidadeNegocio);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public bool atualizaCronogramaAcessoUsuario(int codigoUsuario, string chaveAcesso, ref int regAfetados, ref string msg)
        {
            try
            {
                comandoSQL = string.Format(@"UPDATE {0}.{1}.[Usuario]
                                           SET ChaveAutenticacao = '{3}'                                             
                                         WHERE CodigoUsuario = {2}",
                                               bancodb,
                                               Ownerdb,
                                               codigoUsuario,
                                               chaveAcesso);

                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        //22
        //Função para bloquear o usuário caso alguém já esteja editando o cronograma.
        public bool BloqueiaCronogramaAcessoUsuario(int codigoUsuario, string idProjeto, ref int regAfetados, ref string msg)
        {
            try
            {
                comandoSQL = string.Format(@"UPDATE {0}.{1}.[CronogramaProjeto]
                                           SET DataCheckoutCronograma = GETDATE(), CodigoUsuarioCheckoutCronograma = {2}                                             
                                         WHERE DataExclusao IS NULL and CodigoProjeto = '{3}'",
                                               bancodb,
                                               Ownerdb,
                                               codigoUsuario,
                                               idProjeto);

                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public bool incluiUsuarioExistenteUnidade(int idUsuarioLogado, int codigoUnidadeSuperior, ref int regAfetados, ref string msg)
        {
            try
            {
                comandoSQL = string.Format(@"
            BEGIN 
            INSERT INTO {0}.{1}.[UsuarioUnidadeNegocio]([CodigoUsuario],[CodigoUnidadeNegocio],[IndicaUsuarioAtivoUnidadeNegocio],[DataAtivacaoPerfilUnidadeNegocio],[IndicaSuperUsuario])
                                                 VALUES(            {2},                  {3},                               'S',                         getDate(),'N')        
            END", bancodb, Ownerdb, idUsuarioLogado, codigoUnidadeSuperior);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public DataSet getGerentesProjetosEntidade(int codigoEntidade, int codigoUsuario, int codigoCarteira, string where)
        {
            comandoSQL = string.Format(@"SELECT distinct u.NomeUsuario, u.CodigoUsuario, u.EMail 
                                      FROM {0}.{1}.Usuario u INNER JOIN 
                                           {0}.{1}.Projeto p ON (p.CodigoGerenteProjeto = u.CodigoUsuario
										                                    AND p.CodigoEntidade = {2})
                                     WHERE u.DataExclusao IS NULL {5}
                                       AND u.CodigoUsuario IN (SELECT p.CodigoGerenteProjeto 
                                                                 FROM {0}.{1}.Projeto p 
                                                                WHERE p.DataExclusao IS NULL)                                        
                                       order by u.NomeUsuario"
                                            , bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoCarteira, where);
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getResponsaveisDemandaProcessosEntidade(int codigoEntidade, int codigoUsuario, char indicaDemandaProcesso, string where)
        {
            string funcaoUsuario = "";

            if (indicaDemandaProcesso == 'D')
                funcaoUsuario = string.Format(@"{0}.{1}.f_GetDemandasUsuario({3}, {2})", bancodb, Ownerdb, codigoEntidade, codigoUsuario);
            else
                funcaoUsuario = string.Format(@"{0}.{1}.f_GetProcessosUsuario({3}, {2})", bancodb, Ownerdb, codigoEntidade, codigoUsuario);

            comandoSQL = string.Format(@"SELECT distinct u.NomeUsuario, u.CodigoUsuario,u.EMail 
                                      FROM {0}.{1}.Usuario u INNER JOIN 
                                           {0}.{1}.Projeto p ON (p.CodigoGerenteProjeto = u.CodigoUsuario
										                                    AND p.CodigoEntidade = {2})
                                     WHERE u.DataExclusao IS NULL {4}
                                       AND u.CodigoUsuario IN (SELECT p.CodigoGerenteProjeto 
                                                                 FROM {0}.{1}.Projeto p 
                                                                WHERE p.DataExclusao IS NULL) 
                                       AND p.CodigoProjeto IN (SELECT f.CodigoProjeto 
                                                                 FROM {3} F)
                                       order by u.NomeUsuario"
                                            , bancodb, Ownerdb, codigoEntidade, funcaoUsuario, where);
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getClientesDemandaEntidade(int codigoEntidade, int codigoUsuario, string where)
        {
            comandoSQL = string.Format(@"SELECT distinct u.NomeUsuario, u.CodigoUsuario,u.EMail 
                                      FROM {0}.{1}.Usuario u INNER JOIN 
                                           {0}.{1}.Projeto p ON (p.CodigoCliente = u.CodigoUsuario
										                                    AND p.CodigoEntidade = {2})
                                     WHERE u.DataExclusao IS NULL {4}
                                       AND u.CodigoUsuario IN (SELECT p.CodigoCliente 
                                                                 FROM {0}.{1}.Projeto p 
                                                                WHERE p.DataExclusao IS NULL) 
                                       AND p.CodigoProjeto IN (SELECT f.CodigoProjeto 
                                                                 FROM {0}.{1}.f_GetDemandasUsuario({3}, {2}) F)
                                       order by u.NomeUsuario"
                                            , bancodb, Ownerdb, codigoEntidade, codigoUsuario, where);
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getPermissoesDisponiveisUsuario(int codigoEntidade, int codigoUsuario, int codigoTipoAssociacao)
        {
            comandoSQL = string.Format(@"
        SELECT DescricaoPermissao, CodigoPermissao, 'S' AS Disponivel 
          FROM {0}.{1}.PermissaoSistema 
         WHERE CodigoPermissao 
        NOT IN (SELECT CodigoPermissao 
                  FROM PermissaoUsuarioSistema 
                 WHERE CodigoUsuario = {2} 
                   AND CodigoTipoAssociacao = {3}
                   AND CodigoEntidade = {4}) 
         
         UNION 
        
        SELECT DescricaoPermissao, CodigoPermissao, 'N' as Disponivel 
          FROM {0}.{1}.PermissaoSistema 
         WHERE CodigoPermissao  
            IN (SELECT CodigoPermissao 
                  FROM PermissaoUsuarioSistema 
                 WHERE CodigoUsuario = {2} 
                   AND CodigoTipoAssociacao = {3}
                   AND CodigoEntidade = {4})
     ORDER BY  Disponivel desc ,DescricaoPermissao", bancodb, Ownerdb, codigoUsuario, codigoTipoAssociacao, codigoEntidade);
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public string atualizaPermissoesUsuario(int[] codPermissoes, int codUsuario, int codigoEntidade, int codObjetoAssociado, int codTipoAssociacao)
        {
            string retorno = "";
            string deleteTodasPermissoes = classeDados.getDelete(bancodb, Ownerdb, "PermissaoUsuarioSistema", string.Format("CodigoUsuario = {0} and CodigoEntidade = {1}", codUsuario, codigoEntidade));
            string inserePermissoes = "";
            for (int i = 0; i < codPermissoes.Length; i++)
            {
                ListDictionary dadosPermissoes = new ListDictionary();
                dadosPermissoes.Add("CodigoUsuario", codUsuario);
                dadosPermissoes.Add("CodigoPermissao", codPermissoes[i]);
                dadosPermissoes.Add("CodigoObjetoAssociado", codObjetoAssociado);
                dadosPermissoes.Add("CodigoTipoAssociacao", codTipoAssociacao);
                dadosPermissoes.Add("CodigoEntidade", codigoEntidade);
                inserePermissoes += classeDados.getInsert("PermissaoUsuarioSistema", dadosPermissoes);
            }
            try
            {
                int regAfetadosDel = 0;
                int regAfetadosIns = 0;

                execSQL(deleteTodasPermissoes, ref regAfetadosDel);
                execSQL(inserePermissoes, ref regAfetadosIns);
            }
            catch (Exception ex)
            {
                retorno = ex.Message;
            }
            return retorno;
        }

        #region tela CadastroUsuarioNovo

        public DataSet atualizaUsuarioNovo(string nomeUsuario, string contaWin, string tipoAutenticacao, string EMail, string telefonoContacto1,
                                        string telefonoContato2, string Observacoes, string indicaActivo, int codigoUsuarioOperacao, int codigoEntidadResponsavelUsuario,
                                        int codigoUsuario, string cpf)
        {
            string comandoSQL = string.Format(
                @"BEGIN	
		            DECLARE @retornoProc	int,
                            @dataAtual datetime,
                            @resourceUID VarChar(64)
            
                    SELECT @resourceUID = ResourceUID 
                      FROM {0}.{1}.Usuario
                     WHERE CodigoUsuario = {12}
            		
                    SET @dataAtual = GETDATE()

                    

		            EXEC @retornoProc = {0}.{1}.p_AlteraUsuario '{2}', '{3}', '{4}', '{5}', @resourceUID, '{6}', '{7}', '{8}', '{9}', @dataAtual, {10}, {11}, {12}, '{13}'
            		
		            SELECT @retornoProc AS Retorno
            		
	          END", bancodb, Ownerdb, nomeUsuario.Replace("'", "''"),
                                          contaWin.Replace("'", "''"), tipoAutenticacao,
                                          EMail.Replace("'", "''"), telefonoContacto1, telefonoContato2,
                                          Observacoes.Replace("'", "''"), indicaActivo, codigoUsuarioOperacao,
                 codigoEntidadResponsavelUsuario, codigoUsuario, cpf);

            return getDataSet(comandoSQL);
        }

        public DataSet incluiUsuarioNovo(string nomeUsuario, string contaWin, string tipoAutenticacao, string senha, string EMail, string telefonoContacto1,
                                 string telefonoContato2, string Observacoes, string indicaActivo, int codigoUsuarioOperacao, int codigoEntidadResponsavelUsuario, string cpf)
        {
            string iniciaisDashBoard = string.Empty;
            DataSet dsParam = getParametrosSistema(codigoEntidadResponsavelUsuario, "dashBoardNovoUsuario");

            if (DataSetOk(dsParam) && DataTableOk(dsParam.Tables[0]))
            {
                iniciaisDashBoard = dsParam.Tables[0].Rows[0]["dashBoardNovoUsuario"] + "";
            }
            iniciaisDashBoard = iniciaisDashBoard == "" ? "NULL" : string.Format("'{0}'", iniciaisDashBoard);

            string comandoSQL = string.Format(@"
                BEGIN	
		            DECLARE @codigoUsuario 	int,
				            @retornoProc	int,
                            @dashBoardPadrao Varchar(6),
                            @dataAtual datetime
            		
                    SET @dataAtual = GETDATE()
                    SET @dashBoardPadrao = {13}

		            EXEC @retornoProc = {0}.{1}.p_IncluiUsuario '{2}', '{3}', '{4}', '{5}', '{6}', null, '{7}', '{8}', '{9}', '{10}', @dataAtual, {11}, {12}, @codigoUsuario OUTPUT, '{14}'
            		
                    --Atualizar entidade de acceso e dashboard padrão do novo usuário.
                    IF(@dashBoardPadrao IS NULL) BEGIN
                        UPDATE {0}.{1}.usuario
                           SET [CodigoEntidadeAcessoPadrao] = {12}
                         WHERE CodigoUsuario = @codigoUsuario
                    END -- 
                    ELSE BEGIN
                        UPDATE {0}.{1}.usuario
                           SET [CodigoEntidadeAcessoPadrao] = {12}, [IndicaDashboardPadrao] = @dashBoardPadrao
                         WHERE CodigoUsuario = @codigoUsuario
                    END

		            SELECT @retornoProc AS Retorno, @codigoUsuario AS CodigoUsuario
            		
	            END
                ", bancodb, Ownerdb
                     , nomeUsuario.Replace("'", "''")
                     , contaWin.Replace("'", "''")
                     , tipoAutenticacao, senha
                     , EMail.Replace("'", "''")
                     , telefonoContacto1, telefonoContato2
                     , Observacoes.Replace("'", "''")
                     , indicaActivo, codigoUsuarioOperacao, codigoEntidadResponsavelUsuario, iniciaisDashBoard, cpf);

            return getDataSet(comandoSQL);
        }

        public bool atualizaSenhaUsuario(int codigoUsuario, int idUsuarioLogado, string senha, ref int regAfetados, ref string msg)
        {
            try
            {
                //string senha = "12345678";
                comandoSQL = string.Format(@"
            UPDATE {0}.{1}.Usuario SET SenhaAcessoAutenticacaoSistema = '{3}',
                                       DataUltimaAlteracao = GetDate(),
                                       CodigoUsuarioUltimaAlteracao = {4}
             WHERE CodigoUsuario = {2}", bancodb, Ownerdb, codigoUsuario, ObtemCodigoHash(senha), idUsuarioLogado);
                execSQL(comandoSQL, ref regAfetados);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public DataSet getEntidadeResponsavelUsuario(int codigoUsuario)
        {
            string where = string.Format(@" AND CodigoUnidadeNegocio = ( SELECT TOP 1 u.CodigoEntidadeResponsavelUsuario
                FROM {0}.{1}.Usuario AS u
               WHERE u.CodigoUsuario = {2}) ", bancodb, Ownerdb, codigoUsuario);

            return getUnidade(where);
        }

        public DataSet incluiUsuarioNovoEntidadeAtual(string parametroSQL)
        {
            string comandoSQL = string.Format(
                @"BEGIN	
		            DECLARE @retornoProc int

                    EXEC @retornoProc = {0}.{1}.p_AtualizaAssociacaoUsuarioEntidade {2}
            		
		            SELECT @retornoProc AS Retorno
            		
	          END", bancodb, Ownerdb, parametroSQL);

            return getDataSet(comandoSQL);
        }

        //todo: 04/11/2010 - alejandro :: Definir la consulta para informar si o usuario pode ser excluido.
        public DataSet getPodeExcluirUsuarioEntidadAtual(int codigoUsuario, int idEntidade)
        {
            DataSet ds = null;

            string commandoSQL = string.Format(@"
            BEGIN
                DECLARE @CodigoUsuario  INT,
                        @CodigoEntidade INT

                SET @CodigoUsuario      = {2}
                SET @CodigoEntidade     = {3}

                ---------------------------------------------------------------	0
                SELECT un.NomeUnidadeNegocio
                FROM {0}.{1}.UnidadeNegocio AS un

                WHERE (un.CodigoSuperUsuario = @CodigoUsuario OR	un.CodigoUsuarioGerente = @CodigoUsuario)
                AND		UN.DataExclusao         IS NULL

                ---------------------------------------------------------------	1
                SELECT pr.NomeProjeto
                FROM {0}.{1}.Projeto AS pr
                            
                    INNER JOIN {0}.{1}.TipoProjeto      AS tpr
                            ON pr.CodigoTipoProjeto = tpr.CodigoTipoProjeto
					INNER JOIN {0}.{1}.UnidadeNegocio   AS un
					        ON pr.CodigoEntidade = un.CodigoUnidadeNegocio

                WHERE PR.CodigoGerenteProjeto   = @CodigoUsuario
                  AND tpr.CodigoTipoProjeto     IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ' OR tp.IndicaTipoProjeto = 'PRG')
                  AND pr.DataExclusao           IS NULL
                  AND un.DataExclusao           IS NULL

                ---------------------------------------------------------------- 2
                SELECT indOpe.NomeIndicador
                FROM {0}.{1}.IndicadorOperacional AS indOpe
                            
				    INNER JOIN {0}.{1}.UnidadeNegocio AS UN
					        ON indOpe.CodigoEntidade = UN.CodigoUnidadeNegocio

                WHERE indOpe.CodigoUsuarioResponsavel   = @CodigoUsuario
                  AND indOpe.DataExclusao               IS NULL
                  AND UN.DataExclusao                   IS NULL

                ---------------------------------------------------------------- 3
                SELECT ind.NomeIndicador
                FROM {0}.{1}.Indicador AS ind

                    INNER JOIN {0}.{1}.IndicadorUnidade AS indUni
                            ON ind.CodigoIndicador = indUni.CodigoIndicador
					INNER JOIN {0}.{1}.UnidadeNegocio   AS un
					        ON indUni.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio                            

                WHERE ind.CodigoUsuarioResponsavel  = @CodigoUsuario
				  AND indUni.DataExclusao           IS NULL
				  AND un.DataExclusao               IS NULL

                ---------------------------------------------------------------	4
                SELECT pr.NomeProjeto
                FROM {0}.{1}.Projeto AS pr
                
                    INNER JOIN {0}.{1}.TipoProjeto      AS tpr
                            ON pr.CodigoTipoProjeto = tpr.CodigoTipoProjeto
					INNER JOIN {0}.{1}.UnidadeNegocio   AS un
					        ON pr.CodigoEntidade = un.CodigoUnidadeNegocio

                WHERE PR.CodigoGerenteProjeto   = @CodigoUsuario
                  AND tpr.CodigoTipoProjeto     IN (4, 5)
                  AND pr.DataExclusao           IS NULL
                  AND un.DataExclusao           IS NULL

                 --------------------------------------------------------------- 5
                    SELECT pr.NomeProjeto
                    FROM {0}.{1}.Projeto AS pr
                    
                        INNER JOIN {0}.{1}.TipoProjeto      AS tpr
                                ON pr.CodigoTipoProjeto = tpr.CodigoTipoProjeto
					    INNER JOIN {0}.{1}.UnidadeNegocio   AS un
					            ON pr.CodigoEntidade = un.CodigoUnidadeNegocio

                    WHERE PR.CodigoGerenteProjeto   = @CodigoUsuario
                      AND tpr.CodigoTipoProjeto = 3
                      AND pr.DataExclusao           IS NULL
                      AND un.DataExclusao           IS NULL

                 --------------------------------------------------------------- 6
                SELECT 'OK' AS EsSuperUsuario
                FROM {0}.{1}.UnidadeNegocio AS un

                WHERE   un.CodigoSuperUsuario   = @CodigoUsuario
                  AND   un.CodigoUnidadeNegocio = @CodigoEntidade
                  AND   un.DataExclusao         IS NULL

                 --------------------------------------------------------------- 7
                SELECT DISTINCT 'OK' AS EsRecursoCorporativo 
                FROM {0}.{1}.RecursoCorporativo AS rc
                WHERE   rc.CodigoUsuario        = @CodigoUsuario 
                  AND   rc.CodigoEntidade       = @CodigoEntidade
                  and   rc.IndicaRecursoAtivo = 'S'

                ---------------------------------------------------------------- 8
               SELECT td.DescricaoTarefa
                 FROM {0}.{1}.tarefatodolist  td
                WHERE td.codigousuarioresponsaveltarefa = @CodigoUsuario
                  AND td.DataExclusao IS NULL
                  
            END
        ", bancodb, Ownerdb, codigoUsuario, idEntidade);

            ds = getDataSet(commandoSQL);
            return ds;
        }

        //todo: 08/11/2010 - alejandro :: Definir la consulta para informar si o usuario pode ser excluido.
        public DataSet getPodeExcluirUsuarioOutraEntidad(int codigoUsuario, int codigoUnidadeNegocio)
        {
            DataSet ds = null;

            string commandoSQL = string.Format(@"
                BEGIN
                    DECLARE @CodigoUsuario          INT,
                            @CodigoUnidadeNegocio   INT

                    SET @CodigoUsuario          = {2}
                    SET @CodigoUnidadeNegocio   = {3}

                    SELECT un.NomeUnidadeNegocio
                    FROM {0}.{1}.UnidadeNegocio AS un

                    WHERE   ( un.CodigoSuperUsuario = @CodigoUsuario OR	un.CodigoUsuarioGerente = @CodigoUsuario )
                      AND   un.CodigoUnidadeNegocio = @CodigoUnidadeNegocio
                      AND   un.DataExclusao IS NULL

                    ---------------------------------------------------------------			
                    SELECT pr.NomeProjeto
                    FROM {0}.{1}.Projeto      AS pr
                                
                        INNER JOIN {0}.{1}.TipoProjeto  AS tpr
                                ON pr.CodigoTipoProjeto = tpr.CodigoTipoProjeto 

                    WHERE PR.CodigoGerenteProjeto   = @CodigoUsuario
                      AND tpr.CodigoTipoProjeto     IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ' OR tp.IndicaTipoProjeto = 'PRG')
                      AND pr.CodigoUnidadeNegocio   = @CodigoUnidadeNegocio
                      AND pr.DataExclusao           IS NULL

                    ----------------------------------------------------------------
                    SELECT indOpe.NomeIndicador
                    FROM {0}.{1}.IndicadorOperacional AS indOpe

                    WHERE indOpe.CodigoUsuarioResponsavel   = @CodigoUsuario
                      AND indOpe.CodigoEntidade             = @CodigoUnidadeNegocio
                      AND indOpe.DataExclusao               IS NULL

                    ----------------------------------------------------------------
                    SELECT ind.NomeIndicador
                    FROM {0}.{1}.Indicador        AS ind
                                
                        INNER JOIN {0}.{1}.IndicadorUnidade AS indUni
                                ON ind.CodigoIndicador = indUni.CodigoIndicador 
                        INNER JOIN {0}.{1}.UnidadeNegocio un 
                                ON un.CodigoUnidadeNegocio = indUni.CodigoUnidadeNegocio AND un.CodigoEntidade = @CodigoUnidadeNegocio

                    WHERE ind.CodigoUsuarioResponsavel  = @CodigoUsuario
                      AND indUni.DataExclusao           IS NULL

                    ---------------------------------------------------------------			
                    SELECT pr.NomeProjeto
                    FROM {0}.{1}.Projeto AS pr
                                
                        INNER JOIN {0}.{1}.TipoProjeto AS tpr
                                ON pr.CodigoTipoProjeto = tpr.CodigoTipoProjeto 

                    WHERE PR.CodigoGerenteProjeto   = @CodigoUsuario
                      AND tpr.CodigoTipoProjeto     IN (4, 5)
                      AND pr.CodigoUnidadeNegocio   = @CodigoUnidadeNegocio
                      AND pr.DataExclusao           IS NULL

                    ---------------------------------------------------------------			
                    SELECT pr.NomeProjeto
                    FROM {0}.{1}.Projeto AS pr
                                
                        INNER JOIN {0}.{1}.TipoProjeto AS tpr
                                ON pr.CodigoTipoProjeto = tpr.CodigoTipoProjeto 

                    WHERE PR.CodigoGerenteProjeto   = @CodigoUsuario
                      AND tpr.CodigoTipoProjeto = 3
                      AND pr.CodigoUnidadeNegocio   = @CodigoUnidadeNegocio
                      AND pr.DataExclusao           IS NULL
                END
        ", bancodb, Ownerdb, codigoUsuario, codigoUnidadeNegocio);

            ds = getDataSet(commandoSQL);
            return ds;
        }

        //todo: 08/11/2010 - alejandro :: Definir la consulta para informar si o usuario pode ser Inactivo.
        public DataSet getPodeDesativarUsuario(int codigoUsuario, int codigoUnidadeNegocio)
        {
            DataSet ds = null;

            string commandoSQL = string.Format(@"
        BEGIN
            DECLARE @CodigoUsuario  INT,
                    @CodigoEntidade int

            SET @CodigoUsuario          = {2}
            SET @CodigoEntidade          = {3}

            SELECT un.NomeUnidadeNegocio
            FROM {0}.{1}.UnidadeNegocio AS un

            WHERE   ( un.CodigoSuperUsuario = @CodigoUsuario OR	un.CodigoUsuarioGerente = @CodigoUsuario )
              AND   un.DataExclusao     IS NULL
              AND un.CodigoEntidade = @CodigoEntidade

            ---------------------------------------------------------------	1		
            SELECT pr.NomeProjeto 
            FROM {0}.{1}.Projeto AS pr
            
                INNER JOIN {0}.{1}.Status           AS sta
                        ON pr.CodigoStatusProjeto = sta.CodigoStatus
                INNER JOIN {0}.{1}.TipoProjeto      AS tpr
                        ON pr.CodigoTipoProjeto = tpr.CodigoTipoProjeto
				INNER JOIN {0}.{1}.UnidadeNegocio   AS un
				        ON pr.CodigoEntidade = un.CodigoUnidadeNegocio

            WHERE pr.DataExclusao                           IS NULL
              AND tpr.CodigoTipoProjeto                     IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ' OR tp.IndicaTipoProjeto = 'PRG')
              AND pr.CodigoGerenteProjeto                   = @CodigoUsuario
              AND (     sta.IndicaAcompanhamentoPortfolio   = 'S'
                    OR  sta.IndicaAcompanhamentoExecucao    = 'S'
                    OR  sta.IndicaAcompanhamentoPosExecucao = 'S' )
              AND sta.IndicaSelecaoPortfolio          = 'S'
              AND un.DataExclusao                           IS NULL
              and un.CodigoEntidade = @CodigoEntidade  --->>>>

            ----------------------------------------------------------------2
            SELECT indOpe.NomeIndicador
            FROM {0}.{1}.IndicadorOperacional AS indOpe
                        
			    INNER JOIN {0}.{1}.UnidadeNegocio AS UN
				        ON indOpe.CodigoEntidade = UN.CodigoUnidadeNegocio

            WHERE indOpe.CodigoUsuarioResponsavel   = @CodigoUsuario
              AND indOpe.DataExclusao               IS NULL
              AND UN.DataExclusao                   IS NULL
              AND un.CodigoEntidade = @CodigoEntidade ----->>>>>
            ----------------------------------------------------------------3
            SELECT ind.NomeIndicador
            FROM {0}.{1}.Indicador        AS ind
                        
                INNER JOIN {0}.{1}.IndicadorUnidade AS indUni
                        ON ind.CodigoIndicador = indUni.CodigoIndicador
                INNER JOIN {0}.{1}.UnidadeNegocio   AS un
				        ON indUni.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio
														
            WHERE ind.CodigoUsuarioResponsavel  = @CodigoUsuario
              AND indUni.DataExclusao           IS NULL
              AND un.DataExclusao               IS NULL
              AND un.CodigoEntidade = @CodigoEntidade ---->>>>
            ---------------------------------------------------------------	4		
            SELECT pr.NomeProjeto 
            FROM {0}.{1}.Projeto AS pr

                INNER JOIN {0}.{1}.Status           AS sta
                        ON pr.CodigoStatusProjeto = sta.CodigoStatus
                INNER JOIN {0}.{1}.TipoProjeto      AS tpr
                        ON pr.CodigoTipoProjeto = tpr.CodigoTipoProjeto
				INNER JOIN {0}.{1}.UnidadeNegocio   AS un
				        ON pr.CodigoEntidade = un.CodigoUnidadeNegocio
												
            WHERE pr.DataExclusao                           IS NULL
              AND tpr.CodigoTipoProjeto                     IN (4, 5)
              AND pr.CodigoGerenteProjeto                   = @CodigoUsuario
              AND (     sta.IndicaAcompanhamentoPortfolio   = 'S'
                    OR  sta.IndicaAcompanhamentoExecucao    = 'S'
                    OR  sta.IndicaAcompanhamentoPosExecucao = 'S' )
              AND sta.IndicaSelecaoPortfolio          = 'S'
              AND un.DataExclusao                           IS NULL
              AND un.CodigoEntidade = @CodigoEntidade ---->>>>
          ---------------------------------------------------------------5			
            SELECT td.DescricaoTarefa from {0}.{1}.tarefatodolist  td
             WHERE td.codigousuarioresponsaveltarefa = @CodigoUsuario
               AND td.DataExclusao IS NULL
               AND td.TerminoReal IS NULL
			   AND td.CodigoStatusTarefa IN (1,4)
               AND td.CodigoToDoList IN (SELECT tdl.CodigoToDoList 
											FROM ToDoList AS tdl 
											WHERE tdl.CodigoEntidade = @CodigoEntidade
											 AND tdl.DataExclusao IS NULL) --->>>
          --------------------------------------------------------------- 6
            SELECT 'OK' AS EsSuperUsuario
            FROM {0}.{1}.UnidadeNegocio AS un
            WHERE   un.CodigoSuperUsuario   = @CodigoUsuario
                AND   un.CodigoUnidadeNegocio = @CodigoEntidade
                AND   un.DataExclusao         IS NULL

          --------------------------------------------------------------- 7
            SELECT DISTINCT 'OK' AS EsRecursoCorporativo 
            FROM {0}.{1}.RecursoCorporativo AS rc
            WHERE   rc.CodigoUsuario        = @CodigoUsuario
              AND rc.CodigoEntidade = @CodigoEntidade
              and rc.IndicaRecursoAtivo = 'S'


        END
        ", bancodb, Ownerdb, codigoUsuario, codigoUnidadeNegocio);

            ds = getDataSet(commandoSQL);
            return ds;
        }

        public DataSet getEstadoUsuarioNaUnidadeNoBanco(int codigoUsuario, int codigoUnidade)
        {
            DataSet ds = null;

            string commandoSQL = string.Format(@"
            SELECT uun.IndicaUsuarioAtivoUnidadeNegocio 
            FROM {0}.{1}.Usuario AS us

                INNER JOIN {0}.{1}.UsuarioUnidadeNegocio AS uun
                        ON US.CodigoUsuario = uun.CodigoUsuario

            WHERE us.CodigoUsuario          = {2}
              AND uun.CodigoUnidadeNegocio  = {3}
            ", bancodb, Ownerdb, codigoUsuario, codigoUnidade);

            ds = getDataSet(commandoSQL);
            return ds;
        }

        public DataSet getCodigoUnidadeNegocioDoUsuario(int codigoUsuario)
        {
            DataSet ds = null;

            string commandoSQL = string.Format(@"
            BEGIN
                SELECT CodigoUnidadeNegocio 
                FROM {0}.{1}.UsuarioUnidadeNegocio
                WHERE IndicaUsuarioAtivoUnidadeNegocio = 'S'
                  AND CodigoUsuario = {2}
                  AND indUni.DataExclusao IS NULL
            END
            ", bancodb, Ownerdb, codigoUsuario);

            ds = getDataSet(commandoSQL);
            return ds;
        }

        public DataSet getCodigoUnidadeNegocioResponsavelDoUsuario(int codigoUsuario)
        {
            DataSet ds = null;

            string commandoSQL = string.Format(@"
            BEGIN
                SELECT CodigoEntidadeResponsavelUsuario 
                FROM {0}.{1}.Usuario
                WHERE CodigoUsuario = {2}
            END
            ", bancodb, Ownerdb, codigoUsuario);

            ds = getDataSet(commandoSQL);
            return ds;
        }

        #endregion

        public DataSet getVerificarEmailUsuarioCadastro(string emailUsuario, string entidaLogada)
        {
            comandoSQL = string.Format(@"
                    SELECT
                            usu.CodigoUsuario,
                            usu.DataExclusao,
                            uun.CodigoUnidadeNegocio,
                            CASE WHEN uun.CodigoUnidadeNegocio IS NULL 
                                THEN {3} 
                                ELSE uun.CodigoUnidadeNegocio 
                                END AS CodigoUnidadeNegocio2,
                            usu.NomeUsuario,
                            usu.EMail,
                            usu.TelefoneContato1, 
                            usu.TelefoneContato2,
                            usu.ContaWindows,
                            usu.TipoAutenticacao,
                            usu.Observacoes,
                            usu.CPF

                    FROM {0}.{1}.Usuario AS usu 
                            LEFT JOIN
                            {0}.{1}.UsuarioUnidadeNegocio AS uun
                                ON usu.CodigoUsuario = uun.CodigoUsuario AND uun.CodigoUnidadeNegocio = {3}
                    WHERE usu.EMail = '{2}'
                    ", bancodb, Ownerdb, emailUsuario.Replace("'", ""), entidaLogada);
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        //3ra Altaração: 02/11/2010 : comentar a alteração de senhas do usuario
        public bool atualizaEntidadResponsavel(int codigoUsuario, int codUnidade,
                                               string NomeUsuario, string cpf, string Login, string TipoAutentica,
                                               string Email, string TelefoneContato1, string TelefoneContato2,
                                               string memObservacoes, string Ativo, string idUsuarioLogado,
                                               string tipoAtualiza, string senhaNova)
        {
            int regAfetados = 0;

            if ("AU" == tipoAtualiza) //AU: Adiçionar a Unidade
            {
                comandoSQL = string.Format(@"
            IF NOT EXISTS(SELECT 1 FROM {0}.{1}.UsuarioUnidadeNegocio AS uun WHERE uun.CodigoUsuario = {2} AND uun.CodigoUnidadeNegocio = {3})
                INSERT INTO {0}.{1}.UsuarioUnidadeNegocio(CodigoUsuario, CodigoUnidadeNegocio, IndicaUsuarioAtivoUnidadeNegocio, DataAtivacaoPerfilUnidadeNegocio)
                VALUES({2}, {3}, '{5}', GETDATE())
            ELSE
                UPDATE {0}.{1}.UsuarioUnidadeNegocio
                SET IndicaUsuarioAtivoUnidadeNegocio = '{5}'
                WHERE CodigoUsuario = {2} AND CodigoUnidadeNegocio = {3}
            ", bancodb, Ownerdb, codigoUsuario, codUnidade, idUsuarioLogado, Ativo);
            }
            else if ("OE" == tipoAtualiza) //OE: Excluido De Outra Entidade
            {
                comandoSQL = string.Format(@"
            UPDATE {0}.{1}.Usuario
            SET 
                CodigoEntidadeResponsavelUsuario = {3},
                DataExclusao = NULL,
                DataUltimaAlteracao = GETDATE(),
                SenhaAcessoAutenticacaoSistema = {6},
                CodigoUsuarioUltimaAlteracao = {4}
            WHERE CodigoUsuario = {2}

            IF NOT EXISTS(SELECT 1 FROM {0}.{1}.UsuarioUnidadeNegocio AS uun WHERE uun.CodigoUsuario = {2} AND uun.CodigoUnidadeNegocio = {3})
                INSERT INTO {0}.{1}.UsuarioUnidadeNegocio(CodigoUsuario, CodigoUnidadeNegocio, IndicaUsuarioAtivoUnidadeNegocio, DataAtivacaoPerfilUnidadeNegocio)
                VALUES({2}, {3}, '{5}', GETDATE())
            ELSE
                UPDATE {0}.{1}.UsuarioUnidadeNegocio
                SET IndicaUsuarioAtivoUnidadeNegocio = '{5}'
                WHERE CodigoUsuario = {2} AND CodigoUnidadeNegocio = {3}
            ", bancodb, Ownerdb, codigoUsuario, codUnidade, idUsuarioLogado, Ativo, senhaNova);
            }
            else if ("NE" == tipoAtualiza)
            {
                comandoSQL = string.Format(@"
            UPDATE {0}.{1}.Usuario
            SET 
                CodigoEntidadeResponsavelUsuario = {3},
                DataExclusao = NULL,
                NomeUsuario = '{4}',
                CPF = '{13}' , 
                SenhaAcessoAutenticacaoSistema = {12},
                TelefoneContato1 = '{5}', 
                TelefoneContato2 = '{6}',
                ContaWindows = '{7}',
                TipoAutenticacao = '{8}',
                Observacoes = '{9}',
                DataUltimaAlteracao = GETDATE(),
                CodigoUsuarioUltimaAlteracao = {10}
            WHERE CodigoUsuario = {2}

            IF NOT EXISTS(SELECT 1 FROM {0}.{1}.UsuarioUnidadeNegocio AS uun WHERE uun.CodigoUsuario = {2} AND uun.CodigoUnidadeNegocio = {3})
                INSERT INTO {0}.{1}.UsuarioUnidadeNegocio(CodigoUsuario, CodigoUnidadeNegocio, IndicaUsuarioAtivoUnidadeNegocio, DataAtivacaoPerfilUnidadeNegocio)
                VALUES({2}, {3}, '{11}', GETDATE())
            ELSE
                UPDATE {0}.{1}.UsuarioUnidadeNegocio
                SET IndicaUsuarioAtivoUnidadeNegocio = '{11}'
                WHERE CodigoUsuario = {2} AND CodigoUnidadeNegocio = {3}
            ", bancodb, Ownerdb, codigoUsuario, codUnidade, NomeUsuario, TelefoneContato1,
                   TelefoneContato2, Login, TipoAutentica, memObservacoes, idUsuarioLogado, Ativo, senhaNova, cpf);
            }

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }

        /// <summary>
        /// 4ta Altaração 02/11/2010 : ao excluir usuario o qual não pertenece a entidade logada, apagar so sua referencia
        ///                            da tabela [UsuarioUnidadeNegocio].
        ///                             Para isso, realiço uma pesquiça da entidad responsavel do usuario, cadastrado no momento
        ///                             de inserir o usuario, e comparao com a unidade logada. si e igual, so altero o campo
        ///                             data de exclução, caso contrario, excluo da tabela [UsuarioUnidadeNegocio] o registro.
        /// 5ta Alteração 09/11/2010 : Adicionar o parametro [origemUsuario] 
        ///                             > EA : Entidade Atual, nesse caso, será atribuída a data atual à coluna [Usuario].[DataExclusao]
        ///                             > OE : Outra Entidade, nesse caso, a coluna [Usuario].[DataExclusao] permanecerá NULA. Somente será retirado o registro da [UsuarioUnidadeNegocio]
        ///                            
        /// </summary>
        /// <param name="codigoUsuario"></param>
        /// <param name="codigoUsuarioLogado"></param>
        /// <param name="codigoUnidadLogado"></param>
        /// <param name="origemUsuario"></param>
        /// <param name="msgRetorno"></param>
        /// <returns></returns>
        public bool excluiUsuarioSistema(string codigoUsuario, string codigoUsuarioLogado, int codigoUnidadLogado, string origemUsuario, ref string msgRetorno)
        {
            DataSet ds;
            int registrosAfetados = 0;
            bool retorno = false;
            bool entidadResponsavel = false;
            try
            {
                if (origemUsuario.Equals("EA"))
                {
                    //Verificar si o usuario a Excluir, posee a unidade logada como sua responsavel.
                    comandoSQL = string.Format(@"
                    SELECT CodigoEntidadeResponsavelUsuario, CodigoUsuario 
                      FROM {0}.{1}.Usuario
                     WHERE CodigoUsuario = {2}
                    ", bancodb, Ownerdb, codigoUsuario);
                    ds = getDataSet(comandoSQL);
                    if (DataTableOk(ds.Tables[0]))
                    {
                        int codigoEntidadResponsavel = int.Parse(ds.Tables[0].Rows[0]["CodigoEntidadeResponsavelUsuario"].ToString());
                        if (codigoUnidadLogado == codigoEntidadResponsavel)
                            entidadResponsavel = true;
                    }

                    //Verificar sim este usuário consta como gerente de uma unidade ativa, não sendo possível excluí-lo da base de dados.
                    comandoSQL = string.Format(@"
                    SELECT 'SIM' AS naoPodeEliminar
	                    FROM {0}.{1}.UnidadeNegocio  AS un 
	                    WHERE ( un.CodigoUsuarioGerente = {2} OR un.CodigoSuperUsuario = {2})
                          AND un.IndicaUnidadeNegocioAtiva = 'S'
                          AND un.DataExclusao IS NULL
                          AND un.CodigoUnidadeNegocio = {3} --alterado by ale: 02/11/2010, ver si la unidade logada costa como gerente.
                     ", bancodb, Ownerdb, codigoUsuario, codigoUnidadLogado);
                    ds = getDataSet(comandoSQL);

                    if (DataSetOk(ds))
                    {
                        if (DataTableOk(ds.Tables[0]))
                            msgRetorno = "Atenção! Este usuário consta como gerente de uma unidade ativa, não sendo possível excluí-lo da base de dados.";
                        else
                        {
                            comandoSQL = string.Format(@"
                        UPDATE {0}.{1}.InteressadoObjetoPermissao
                        SET     StatusRegistro              = 'D'
                            ,   DataDesativacaoRegistro     = GETDATE()
	                        ,   CodigoUsuarioDesativacao    = {4}
                        WHERE CodigoUsuario     = {2}
                          AND CodigoEntidade    = {3}
                          AND StatusRegistro    = 'A'

                        UPDATE {0}.{1}.InteressadoObjetoPerfil
                        SET     StatusRegistro              = 'D'
	                        ,   DataDesativacaoRegistro        = GETDATE()
	                        ,   CodigoUsuarioDesativacao    = {4}
                        FROM        {0}.{1}.InteressadoObjetoPerfil AS iop
                        INNER JOIN  {0}.{1}.Perfil AS p on iop.CodigoPerfil = p.CodigoPerfil
                        WHERE iop.CodigoUsuario = {2}
                          AND p.CodigoEntidade  = {3}
                          AND StatusRegistro    = 'A'

                        UPDATE {0}.{1}.InteressadoObjeto
                        SET     StatusRegistro              = 'D'
	                        ,   DataDesativacaoRegistro        = GETDATE()
	                        ,   CodigoUsuarioDesativacao    = {4}

                        WHERE CodigoUsuario     = {2}
                          AND CodigoEntidade    = {3}
                          AND StatusRegistro    = 'A'

                           DELETE FROM {0}.{1}.UsuarioUnidadeNegocio 
                            WHERE CodigoUsuario = {2} 
                              AND CodigoUnidadeNegocio = {3}

                             ", bancodb, Ownerdb, codigoUsuario, codigoUnidadLogado, codigoUsuarioLogado);

                            //if (entidadResponsavel) //Si usuario pertenece a unidade logada, entao indico [DataExclusap] da tabela [Usuario].
                            //{
                            //    comandoSQL += string.Format(@"

                            //    UPDATE {0}.{1}.Usuario
                            //    SET
                            //        CodigoUsuarioExclusao = {3},
                            //        DataExclusao = GETDATE()
                            //    WHERE CodigoUsuario = {2}
                            //    ", bancodb, Ownerdb, codigoUsuario, codigoUsuarioLogado);
                            //}

                            comandoSQL += string.Format(@"
IF NOT EXISTS(
             SELECT 1 
               FROM UsuarioUnidadeNegocio AS uun
              WHERE uun.CodigoUsuario = {2}
                AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S')
                            UPDATE {0}.{1}.Usuario
                            SET
                                CodigoUsuarioExclusao = {3},
                                DataExclusao = GETDATE()
                            WHERE CodigoUsuario = {2}
ELSE
BEGIN
    IF NOT EXISTS(
                 SELECT 1
                   FROM Usuario AS u INNER JOIN
                        UsuarioUnidadeNegocio AS uun ON u.CodigoUsuario = uun.CodigoUsuario
                                                    AND u.CodigoEntidadeAcessoPadrao = uun.CodigoUnidadeNegocio
                  WHERE u.CodigoUsuario = {2}
                    AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S')
    BEGIN
     UPDATE Usuario
        SET CodigoEntidadeAcessoPadrao = (SELECT TOP 1 uun.CodigoUnidadeNegocio FROM UsuarioUnidadeNegocio AS uun WHERE uun.CodigoUsuario = {2} AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S' order by uun.DataAtivacaoPerfilUnidadeNegocio),
            CodigoEntidadeResponsavelUsuario = (SELECT TOP 1 uun.CodigoUnidadeNegocio FROM UsuarioUnidadeNegocio AS uun WHERE uun.CodigoUsuario = {2} AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S' order by uun.DataAtivacaoPerfilUnidadeNegocio)
      WHERE CodigoUsuario = {2}
    END
END
                             ", bancodb, Ownerdb, codigoUsuario, codigoUsuarioLogado, codigoUnidadLogado);

                            execSQL(comandoSQL, ref registrosAfetados);
                            retorno = true;
                        }
                    }

                } //... if(origemUsuario.Equals("EA")) ...
                else if (origemUsuario.Equals("OE"))
                {
                    comandoSQL = string.Format(@"
                        UPDATE {0}.{1}.InteressadoObjetoPermissao
                        SET     StatusRegistro              = 'D'
                            ,   DataDesativacaoRegistro        = GETDATE()
	                        ,   CodigoUsuarioDesativacao    = {4}
                        WHERE CodigoUsuario     = {2}
                          AND CodigoEntidade    = {3}
                          AND StatusRegistro    = 'A'

                        UPDATE {0}.{1}.InteressadoObjetoPerfil
                        SET     StatusRegistro              = 'D'
	                        ,   DataDesativacaoRegistro        = GETDATE()
	                        ,   CodigoUsuarioDesativacao    = {4}
                        FROM        {0}.{1}.InteressadoObjetoPerfil AS iop
                        INNER JOIN  {0}.{1}.Perfil AS p on iop.CodigoPerfil = p.CodigoPerfil
                        WHERE iop.CodigoUsuario = {2}
                          AND p.CodigoEntidade  = {3}
                          AND StatusRegistro    = 'A'

                        UPDATE {0}.{1}.InteressadoObjeto
                        SET     StatusRegistro              = 'D'
	                        ,   DataDesativacaoRegistro        = GETDATE()
	                        ,   CodigoUsuarioDesativacao    = {4}

                        WHERE CodigoUsuario     = {2}
                          AND CodigoEntidade    = {3}
                          AND StatusRegistro    = 'A'

                       DELETE FROM {0}.{1}.UsuarioUnidadeNegocio
                        WHERE CodigoUsuario = {2}
                          AND CodigoUnidadeNegocio = {3}
                             ", bancodb, Ownerdb, codigoUsuario, codigoUnidadLogado, codigoUsuarioLogado);

                    comandoSQL += string.Format(@"
IF NOT EXISTS(SELECT 1 
               FROM UsuarioUnidadeNegocio AS uun
              WHERE uun.CodigoUsuario = {2}
                AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S')
                            UPDATE {0}.{1}.Usuario
                            SET
                                CodigoUsuarioExclusao = {3},
                                DataExclusao = GETDATE()
                            WHERE CodigoUsuario = {2}
ELSE
BEGIN
    IF NOT EXISTS(
                 SELECT 1
                   FROM Usuario AS u INNER JOIN
                        UsuarioUnidadeNegocio AS uun ON u.CodigoUsuario = uun.CodigoUsuario
                                                    AND u.CodigoEntidadeAcessoPadrao = uun.CodigoUnidadeNegocio
                  WHERE u.CodigoUsuario = {2}
                    AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S')
    BEGIN
     UPDATE Usuario
        SET CodigoEntidadeAcessoPadrao = (SELECT TOP 1 uun.CodigoUnidadeNegocio FROM UsuarioUnidadeNegocio AS uun WHERE uun.CodigoUsuario = {2} AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S' order by uun.DataAtivacaoPerfilUnidadeNegocio),
            CodigoEntidadeResponsavelUsuario = (SELECT TOP 1 uun.CodigoUnidadeNegocio FROM UsuarioUnidadeNegocio AS uun WHERE uun.CodigoUsuario = {2} AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S' order by uun.DataAtivacaoPerfilUnidadeNegocio)
      WHERE CodigoUsuario = {2}
    END
END
                             ", bancodb, Ownerdb, codigoUsuario, codigoUsuarioLogado, codigoUnidadLogado);

                    execSQL(comandoSQL, ref registrosAfetados);
                    retorno = true;
                }
            }
            catch (Exception ex)
            {
                msgRetorno = ex.Message + Environment.NewLine + comandoSQL;
                retorno = false;
            }
            return retorno;
        }

        #endregion

        #region Proposta e Projeto

        /// <summary>
        /// Cria o link para a chamada ao portal da Estratégia Desktop
        /// </summary>
        /// <param name="Request_Url">Request.Url</param>
        /// <param name="codigoEntidade">Codigo da entidade do usuário logado</param>
        /// <param name="codigoUsuario">Código do usuário logado</param>
        /// <param name="codigoProjeto">Código do projeto (null) para criar um novo</param>
        /// <returns>Link para acesso ao portal da estratégia Desktop</returns>
        public string getLinkApoioDesktop(Uri Request_Url, int codigoEntidade, int codigoUsuario, int? codigoProjeto, int codigoOrigem)
        {
            if (codigoProjeto == null)
                codigoProjeto = -1;

            //string urlBase = Request_Url.AbsoluteUri;
            //urlBase = urlBase.Substring(0, urlBase.LastIndexOf('/'));
            string urlBase = Request_Url.Scheme + "://" + Request_Url.Authority + "/" + Request_Url.Segments[1].ToString();

            //int controleLocal = Math.Abs((((codigoUsuario * codigoProjeto * codigoEntidade) + (codigoEntidade - codigoUsuario)) + "CDIS").GetHashCode());
            int controleLocal = Math.Abs(ObtemCodigoHash(((codigoUsuario * codigoProjeto * codigoEntidade * codigoOrigem) + (codigoEntidade - codigoUsuario)) + "CDIS"));
            string link = string.Format("ApoioDesktop/Consultas.application?En={0}&Us={1}&Pr={2}&Or={3}&Ct={4}", codigoEntidade, codigoUsuario, codigoProjeto, codigoOrigem, controleLocal);
            link = urlBase + link;
            return link;
        }

        /// <summary>
        /// Cria o link para a chamada ao portal da Estratégia Desktop
        /// </summary>
        /// <param name="Request_Url">Request.Url</param>
        /// <param name="codigoEntidade">Codigo da entidade do usuário logado</param>
        /// <param name="codigoUsuario">Código do usuário logado</param>
        /// <param name="codigoProjeto">Código do projeto (null) para criar um novo</param>
        /// <returns>Link para acesso ao portal da estratégia Desktop</returns>
        public string getLinkPortalDesktop(Uri Request_Url, int codigoEntidade, int codigoUsuario, int? codigoProjeto, string urlBase)
        {
            if (codigoProjeto == null)
                codigoProjeto = -1;
            int registroAfetados = 0;

            //int controleLocal = Math.Abs((((codigoUsuario * codigoProjeto * codigoEntidade) + (codigoEntidade - codigoUsuario)) + "CDIS").GetHashCode());
            int controleLocal = Math.Abs(ObtemCodigoHash(((codigoUsuario * codigoProjeto * codigoEntidade) + (codigoEntidade - codigoUsuario)) + "CDIS"));
            string link = string.Format("portalDesktop/portalDesktop.application?En={0}&Us={1}&Pr={2}&Ct={3}", codigoEntidade, codigoUsuario, codigoProjeto, controleLocal);
            link = urlBase + link;

            // usado pelo tasques
            // -----------------------------------------------------------------------------------------------------
            string macUsuario = "00 E0 4C 10 C9 5A";
            string sDataBancoDeDados = classeDados.getDateDB(); // dd/MM/yyyy HH:mm:ss
            DateTime DataBancoDeDados = DateTime.ParseExact(sDataBancoDeDados, "dd/MM/yyyy HH:mm:ss", null);
            string chaveAutenticacao = codigoUsuario.ToString() + DataBancoDeDados.ToString("yyyyMMdd") + macUsuario;
            chaveAutenticacao = ObtemCodigoHash(chaveAutenticacao).ToString();
            execSQL(string.Format("UPDATE Usuario SET ChaveAutenticacao = '{1}' WHERE codigoUsuario = {0} ", codigoUsuario, chaveAutenticacao), ref registroAfetados);

            DataSet dsParam = getParametrosSistema(codigoEntidade, "utilizarTasques");
            if (DataSetOk(dsParam) && DataTableOk(dsParam.Tables[0]))
            {
                if (dsParam.Tables[0].Rows[0]["utilizarTasques"].ToString() == "S")
                {
                    link = string.Format("tasques/tasques.application?En={0}&Us={1}&Pr={2}&Ct={3}", codigoEntidade, codigoUsuario, codigoProjeto, controleLocal);
                    link = urlBase + link;
                }
            }
            // -----------------------------------------------------------------------------------------------------

            return link;
        }

        public DataSet getProposta(string where, int codigoProjeto)
        {
            comandoSQL = string.Format(
                                       @"SELECT p.CodigoProjeto
                                           ,p.NomeProjeto
                                           ,p.CodigoCategoria
                                           ,p.CodigoStatusProjeto
                                           ,p.CodigoGerenteProjeto
                                           ,p.CodigoUnidadeNegocio
			                               ,p.DescricaoProposta
                                           ,un.NomeUnidadeNegocio
                                      FROM {0}.{1}.Projeto AS p	LEFT JOIN
                                            {0}.{1}.UnidadeNegocio AS un ON un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio 
                            WHERE p.CodigoProjeto = {2} {3}", bancodb, Ownerdb, codigoProjeto, where);
            return getDataSet(comandoSQL);
        }

        public DataSet getPropostas(int idUsuario, int idEntidade, string where)
        {
            comandoSQL = string.Format(
               @"SELECT Projeto.CodigoProjeto, Projeto.NomeProjeto, Projeto.CodigoCategoria, Projeto.CodigoStatusProjeto, 
                     Categoria.DescricaoCategoria, Status.DescricaoStatus, {2} as codigoUsuarioLogado, {3} as codigoEntidadeLogada
                FROM {0}.{1}.Projeto INNER JOIN
                     {0}.{1}.Categoria ON (Projeto.CodigoCategoria = Categoria.CodigoCategoria 
                                       AND Categoria.DataExclusao IS NULL) INNER JOIN
                     {0}.{1}.Status ON Projeto.CodigoStatusProjeto = Status.CodigoStatus
               WHERE (Projeto.DataExclusao IS NULL)
                 {4}", bancodb, Ownerdb, idUsuario, idEntidade, where);
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        //Obrigações contratatada
        public DataSet getObrigacoesContratada(string codigoContrato)
        {
            string comandoSQL = string.Format(@"
            SELECT ob.[CodigoContrato]
                    ,ob.[CodigoObrigacoesContratada]
                    ,ob.[Ocorrencia]
                    ,tob.[DescricaoTipoObrigacoesContratada]
                FROM {0}.{1}.[ObrigacoesContratada] AS [ob] INNER JOIN
                     {0}.{1}.[TipoObrigacoesContratada]	AS [tob]  ON ( ob.CodigoObrigacoesContratada = tob.CodigoTipoObrigacoesContratada)
                WHERE ob.CodigoContrato = {2} 
                ORDER BY ob.CodigoObrigacoesContratada
                ", bancodb, Ownerdb, codigoContrato);

            return getDataSet(comandoSQL);
        }

        public DataSet getListaTiposObrigacaoContratada(string where)
        {
            string comandoSQL = string.Format(@"
      SELECT
          ob.[CodigoTipoObrigacoesContratada] 
         ,ob.[DescricaoTipoObrigacoesContratada]
	FROM
		{0}.{1}.[TipoObrigacoesContratada]	AS [ob] 
    WHERE 1=1 {2}
    ORDER BY ob.[DescricaoTipoObrigacoesContratada]", bancodb, Ownerdb, where);
            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds))
                ds.Tables[0].TableName = "ObrigacaoContratada";

            return ds;
        }

        public bool incluiObrigacoesContratada(int codigoContrato, int CodigoObrigacoesContratada, string Ocorrencia)
        {
            bool retorno = true;
            try
            {

                comandoSQL = string.Format(@"
                                INSERT INTO {0}.{1}.[ObrigacoesContratada]
                                                    ([CodigoContrato]
                                                    ,[CodigoObrigacoesContratada]
                                                    ,[Ocorrencia])
                                VALUES({2}, {3}, '{4}');

                                ", bancodb, Ownerdb
                                     , codigoContrato
                                     , CodigoObrigacoesContratada
                                     , Ocorrencia
                                     );

                int registrosAfetados = 0;

                execSQL(comandoSQL, ref registrosAfetados);

                retorno = true;
            }
            catch (Exception ex)
            {
                retorno = false;
                if (getErroIncluiRegistro(ex.Message).Contains("PRIMARY KEY"))
                {
                    throw new Exception("Não é possível inserir registro duplicado.");
                }
                else
                {
                    throw new Exception(getErroIncluiRegistro(ex.Message));
                }
            }
            return retorno;
        }

        public bool atualizaObrigacoesContratada(int codigoContrato, int CodigoObrigacoesContratada, string Ocorrencia)
        {
            bool retorno = true;
            try
            {

                comandoSQL = string.Format(@"
                                UPDATE {0}.{1}.[ObrigacoesContratada]
                                   SET [Ocorrencia] = '{4}'

                                WHERE   CodigoContrato          = {2}
                                  AND   CodigoObrigacoesContratada           = {3}

                                ", bancodb, Ownerdb
                                     , codigoContrato
                                     , CodigoObrigacoesContratada
                                     , Ocorrencia
                                     );

                int registrosAfetados = 0;

                execSQL(comandoSQL, ref registrosAfetados);

                retorno = true;
            }
            catch (Exception ex)
            {
                retorno = false;
                throw new Exception(getErroAtualizaRegistro(ex.Message));
            }
            return retorno;
        }

        public bool excluiObrigacoesContratada(int codigoContrato, int CodigoObrigacoesContratada)
        {
            bool retorno = true;
            try
            {

                comandoSQL = string.Format(@"
                                DELETE FROM {0}.{1}.[ObrigacoesContratada]
                                WHERE   CodigoContrato          = {2}
                                  AND   CodigoObrigacoesContratada           = {3}

                                ", bancodb, Ownerdb
                                     , codigoContrato
                                     , CodigoObrigacoesContratada
                                     );

                int registrosAfetados = 0;

                execSQL(comandoSQL, ref registrosAfetados);

                retorno = true;
            }
            catch (Exception ex)
            {
                retorno = false;
                throw new Exception(getErroExcluiRegistro(ex.Message));
            }
            return retorno;
        }


        //fim obrigações contratada

        //Sub contratatada

        public bool verificaSubContratacao(int codigoContrato)
        {
            bool retorno = false;
            try
            {
                comandoSQL = string.Format(@"
                    SELECT c.SubContratacao
                      FROM {0}.{1}.Contrato AS c
                     WHERE CodigoContrato = {2}
                    ", bancodb, Ownerdb, codigoContrato);

                DataSet ds = getDataSet(comandoSQL);

                if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                {
                    retorno = ds.Tables[0].Rows[0]["SubContratacao"].ToString() == "S";
                }
            }
            catch (Exception ex)
            {
                retorno = false;
                throw new Exception(getErroAtualizaRegistro(ex.Message));
            }
            return retorno;
        }



        public DataSet getSubContratacao(string codigoContrato)
        {
            string comandoSQL = string.Format(@"
            SELECT sb.[CodigoContrato]
                    ,sb.[RazaoSocial]
                    ,sb.[Classificacao]
                    ,sb.[TipoPessoa]
                    ,CASE WHEN sb.TipoPessoa = 'J' 
                    THEN 
                    SUBSTRING(sb.numeroCNPJCPF,1,2)
                    +'.'+SUBSTRING(sb.numeroCNPJCPF,3,3)
                    +'.'+SUBSTRING(sb.numeroCNPJCPF,6,3)
                    +'/'+SUBSTRING(sb.numeroCNPJCPF,9,4)
                    +'-'+SUBSTRING(sb.numeroCNPJCPF,13,2) 
                    WHEN sb.TipoPessoa = 'F' then SUBSTRING(sb.numeroCNPJCPF,1,3)
                    +'.'+SUBSTRING(sb.numeroCNPJCPF,4,3)
                    +'.'+SUBSTRING(sb.numeroCNPJCPF,7,3)
                    +'-'+SUBSTRING(sb.numeroCNPJCPF,10,2)
                    else sb.numeroCNPJCPF
                    END AS NumeroCNPJCPF
                FROM {0}.{1}.[SubContratada] AS [sb]
                WHERE sb.CodigoContrato = {2} 
                ORDER BY sb.RazaoSocial
                ", bancodb, Ownerdb, codigoContrato);

            return getDataSet(comandoSQL);
        }



        public bool incluiSubContratada(int codigoContrato, string NumeroCNPJCPF, string Razao, string Classificacao, string TipoPessoa)
        {
            bool retorno = true;

            try
            {

                comandoSQL = string.Format(@"
                                INSERT INTO {0}.{1}.[SubContratada]
                                                    ([CodigoContrato]
                                                    ,[NumeroCNPJCPF]
                                                    ,[RazaoSocial]
                                                    ,[Classificacao]
                                                    ,[tipoPessoa])
                                VALUES({2}, '{3}', '{4}', '{5}', '{6}')
                                ", bancodb, Ownerdb
                                     , codigoContrato
                                     , NumeroCNPJCPF.Replace(".", "").Replace("/", "").Replace("-", "")
                                     , Razao
                                     , Classificacao
                                     , TipoPessoa
                                     );

                int registrosAfetados = 0;

                execSQL(comandoSQL, ref registrosAfetados);

                retorno = true;
            }
            catch (Exception ex)
            {
                retorno = false;

                if (getErroIncluiRegistro(ex.Message).Contains("PRIMARY KEY"))
                {
                    throw new Exception("Não é possível inserir o mesmo CNPJ/CPF mais de uma vez.");
                }
                else if (getErroIncluiRegistro(ex.Message).Contains("UNIQUE KEY"))
                {
                    throw new Exception("Já existe um cadastro com esta Razão Social.");
                }
                else
                {
                    throw new Exception(getErroIncluiRegistro(ex.Message));
                }
            }
            return retorno;
        }

        public bool atualizaSubContratada(int codigoContrato, string NumeroCNPJCPF, string Razao, string Classificacao, string TipoPessoa)
        {
            bool retorno = true;
            try
            {

                comandoSQL = string.Format(@"
                                UPDATE {0}.{1}.[SubContratada]
                                   SET [RazaoSocial] = '{4}',
                                       [Classificacao] = '{5}',
                                       [tipoPessoa] = '{6}'
                                WHERE   CodigoContrato          = {2}
                                  AND   NumeroCNPJCPF          = '{3}'
                                ", bancodb, Ownerdb
                                     , codigoContrato
                                     , NumeroCNPJCPF.Replace(".", "").Replace("/", "").Replace("-", "")
                                     , Razao
                                     , Classificacao
                                     , TipoPessoa
                                     );

                int registrosAfetados = 0;

                execSQL(comandoSQL, ref registrosAfetados);

                retorno = true;
            }
            catch (Exception ex)
            {
                retorno = false;
                if (getErroAtualizaRegistro(ex.Message).Contains("UNIQUE KEY"))
                {
                    throw new Exception("Já existe um cadastro com esta Razão Social.");
                }
                else if (getErroAtualizaRegistro(ex.Message).Contains("PRIMARY KEY"))
                {
                    throw new Exception("Não é possível inserir o mesmo CNPJ/CPF mais de uma vez.");
                }
                else
                {
                    throw new Exception(getErroAtualizaRegistro(ex.Message));
                }


            }
            return retorno;
        }

        public bool excluiSubContratada(int codigoContrato, string NumeroCNPJCPF)
        {
            bool retorno = true;
            try
            {

                comandoSQL = string.Format(@"
                                DELETE FROM {0}.{1}.[SubContratada]
                                WHERE   CodigoContrato          = {2}
                                  AND   NumeroCNPJCPF          = '{3}'
                                ", bancodb, Ownerdb
                                     , codigoContrato
                                     , NumeroCNPJCPF.Replace(".", "").Replace("/", "").Replace("-", "")
                                     );

                int registrosAfetados = 0;

                execSQL(comandoSQL, ref registrosAfetados);

                retorno = true;
            }
            catch (Exception ex)
            {
                retorno = false;
                throw new Exception(getErroExcluiRegistro(ex.Message));
            }
            return retorno;
        }


        //fim sub contratada


        public DataSet getProjetosContratos(int idUsuario, int idEntidade, int codigoProjeto, string where)
        {
            comandoSQL = string.Format(
               @"SELECT Projeto.CodigoProjeto, Projeto.NomeProjeto, Projeto.CodigoStatusProjeto, 
                     Status.DescricaoStatus, {2} as codigoUsuarioLogado, {3} as codigoEntidadeLogada
                FROM {0}.{1}.Projeto INNER JOIN
                     {0}.{1}.Status ON Projeto.CodigoStatusProjeto = Status.CodigoStatus
               WHERE (Projeto.DataExclusao IS NULL) 
                 AND Projeto.CodigoProjeto not in (SELECT CodigoProjeto
													  FROM {0}.{1}.Obra ob
													 WHERE ob.CodigoProjeto = Projeto.CodigoProjeto
													   AND (ob.codigoContrato is not null)
                                                       AND ob.CodigoProjeto <> {4})
                 {5}", bancodb, Ownerdb, idUsuario, idEntidade, codigoProjeto, where);
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getProjetosServico(int idUsuario, int idEntidade, string where)
        {
            comandoSQL = string.Format(
                @"SELECT Projeto.CodigoProjeto, Projeto.NomeProjeto, Projeto.CodigoStatusProjeto, 
                     Status.DescricaoStatus, {2} as codigoUsuarioLogado, {3} as codigoEntidadeLogada
                FROM {0}.{1}.Projeto INNER JOIN
                     {0}.{1}.Status ON Projeto.CodigoStatusProjeto = Status.CodigoStatus
               WHERE (Projeto.DataExclusao IS NULL) 
                 AND Projeto.CodigoEntidade = {3}
                 AND Projeto.CodigoProjeto NOT IN(SELECT CodigoProjetoFilho FROM {0}.{1}.LinkProjeto)
                 {4}
               ORDER BY Projeto.NomeProjeto", bancodb, Ownerdb, idUsuario, idEntidade, where);
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public string getNomeProjeto(int codigoProjeto, string where)
        {
            string nomeProjeto = "";
            comandoSQL = string.Format(@"
            SELECT Projeto.NomeProjeto
              FROM {0}.{1}.Projeto
             WHERE Projeto.CodigoProjeto = {2}
               {3}
            ", bancodb, Ownerdb, codigoProjeto, where);
            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                nomeProjeto = ds.Tables[0].Rows[0]["NomeProjeto"].ToString();

            return nomeProjeto;
        }

        public DataSet getProjetos(string where)
        {
            comandoSQL = string.Format(@"
                SELECT      P.NomeProjeto
                        ,   U.NomeUsuario as Gerente
                        ,   S.DescricaoStatus
                        ,   VP.Inicio
                        ,   VP.Termino
                        ,   C.DescricaoCategoria
                        ,   P.CodigoProjeto
                        ,   S.CodigoStatus
                        ,   C.CodigoCategoria
                        ,   P.CodigoGerenteProjeto
                        ,   CASE WHEN P.IndicaPrograma = 'S' THEN '~/imagens/Programa.png'
                            ELSE '~/imagens/Projeto.png' END AS ImagemTipoProjeto
                        ,   P.CodigoUnidadeNegocio
                        ,   P.InicioProposta
                FROM        {0}.{1}.Projeto     AS P 
                INNER JOIN  {0}.{1}.Status      AS S    ON P.CodigoStatusProjeto    = S.CodigoStatus
                INNER JOIN  {0}.{1}.Usuario     AS U    ON P.CodigoGerenteProjeto   = U.CodigoUsuario
                LEFT JOIN  {0}.{1}.vi_Projeto  AS VP   ON P.CodigoProjeto          = VP.CodigoProjeto
                LEFT  JOIN  {0}.{1}.Categoria   AS C    ON (P.CodigoCategoria       = C.CodigoCategoria AND c.DataExclusao IS NULL)

                WHERE 1=1 and p.DataExclusao is null
                   {2}

                ORDER BY P.IndicaPrograma DESC, P.NomeProjeto
                ", bancodb, Ownerdb, where);
            return getDataSet(comandoSQL);
        }

        public DataSet getProjetosExecucaoEntidade(int codigoEntidade, string where)
        {
            comandoSQL = string.Format(@"
                SELECT P.NomeProjeto
                      ,P.CodigoProjeto
                      ,P.CodigoGerenteProjeto
                      ,P.CodigoStatusProjeto
                 FROM {0}.{1}.Projeto AS P INNER JOIN
                      {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                         AND  s.IniciaisStatus = 'EM_EXECUCACAO')       
                WHERE P.CodigoEntidade = {2}
                  AND P.DataExclusao IS NULL
                   {3}
                ORDER BY P.NomeProjeto

                ", bancodb, Ownerdb, codigoEntidade, where);
            return getDataSet(comandoSQL);
        }

        public bool atualizaCategoriaProjeto(int codigoProjeto, int codigoCategoria)
        {
            bool retorno = true;
            try
            {

                comandoSQL = string.Format(@"
                                UPDATE {0}.{1}.[Projeto]
                                   SET CodigoCategoria = {3}
                                 WHERE CodigoProjeto = {2}
                                ", bancodb, Ownerdb
                                     , codigoProjeto
                                     , codigoCategoria
                                     );

                int registrosAfetados = 0;

                execSQL(comandoSQL, ref registrosAfetados);

                retorno = true;
            }
            catch
            {
                retorno = false;
            }
            return retorno;
        }

        public int getStatusProjeto(int codigoProjeto)
        {
            comandoSQL = string.Format(@"
                SELECT P.CodigoStatusProjeto
                  FROM {0}.{1}.Projeto AS P 
                 WHERE CodigoProjeto = {2}
                ", bancodb, Ownerdb, codigoProjeto);

            DataSet ds = getDataSet(comandoSQL);

            int codigoStatus = -1;

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                codigoStatus = int.Parse(ds.Tables[0].Rows[0]["CodigoStatusProjeto"].ToString());
            }

            return codigoStatus;
        }

        public DataSet getTipoContrato(int codigoEntidad, string where)
        {
            comandoSQL = string.Format(@"
            SELECT CodigoTipoContrato, DescricaoTipoContrato
              FROM {0}.{1}.TipoContrato
             WHERE CodigoEntidade = {2}
               {3}
            ", bancodb, Ownerdb, codigoEntidad, where);
            return getDataSet(comandoSQL);
        }

        public DataSet getProjetosCronogramaRecurso(int codigoEntidade, int codigoRecurso, string where)
        {
            comandoSQL = string.Format(
                @"BEGIN
	            DECLARE @VersaoMSProject Varchar(10)
    
                SELECT @VersaoMSProject = Valor
                  FROM {0}.{1}.ParametroConfiguracaoSistema
                 WHERE CodigoEntidade = {2}
                   AND Parametro = 'VersaoMSProject' 
                
                IF @VersaoMSProject = '2007' OR @VersaoMSProject = '2003' 
                    BEGIN
                        SELECT DISTINCT vp.[NomeProjeto], vp.[CodigoProjeto]  
	                    FROM 
		                    {0}.{1}.[vi_Atribuicao]							AS [a]
		
			                    INNER JOIN {0}.{1}.[vi_RecursoCorporativo]	AS [rc]		ON 
				                    (			rc.[CodigoRecursoMSProject]	= a.[CodigoRecurso]
					                    and rc.[CodigoRecurso]					= {3})
				
			                    INNER JOIN {0}.{1}.[vi_Projeto]				AS [vp]		ON 
				                    (vp.[CodigoProjeto]				= a.[CodigoProjeto])
				
			                    INNER JOIN {0}.{1}.[Projeto]				AS [p]		ON 
				                    (			p.[CodigoProjeto]   = vp.[CodigoProjeto]
					                    AND p.[DataExclusao]		IS NULL
					                    AND p.[CodigoEntidade]		= {2}					
                                        AND p.[CodigoStatusProjeto] = 3 -- apenas projetos em execução )		
                                    )
                        WHERE 1=1 {4} 
                        ORDER BY vp.NomeProjeto
                    END
               ELSE
                    BEGIN
                
                      SELECT DISTINCT cp.CodigoProjeto, cp.NomeProjeto				 
			            FROM TarefaCronogramaProjeto AS t INNER JOIN
				               CronogramaProjeto AS cp ON (cp.CodigoCronogramaProjeto = t.CodigoCronogramaProjeto) INNER JOIN
				               AtribuicaoRecursoTarefa AS a ON (a.CodigoCronogramaProjeto = t.CodigoCronogramaProjeto
											                              AND a.CodigoTarefa = t.CodigoTarefa) INNER JOIN											
				               Projeto AS p ON (p.CodigoProjeto = cp.CodigoProjeto) INNER JOIN 
				               RecursoCronogramaProjeto AS rcp ON (rcp.CodigoCronogramaProjeto = a.CodigoCronogramaProjeto
												                               AND rcp.CodigoRecursoProjeto = a.CodigoRecursoProjeto) INNER JOIN                                           
				               RecursoCorporativo AS rc ON (rc.CodigoRecursoCorporativo = rcp.CodigoRecursoCorporativo) 
		             WHERE rc.CodigoUsuario = {3}
			             AND cp.CodigoEntidade = {2}
			             AND t.DataExclusao IS NULL
			             AND a.TerminoReal IS NULL
			             AND t.TerminoReal IS NULL
			             AND t.IndicaTarefaResumo = 'N'
			             AND (t.[IndicaMarco] = 'S' OR a.Trabalho > 0)
			             AND p.CodigoStatusProjeto IN (3,16,21)
			             AND p.DataExclusao IS NULL
                        ORDER BY cp.NomeProjeto

                    END
            END", bancodb, Ownerdb, codigoEntidade, codigoRecurso, where);
            return getDataSet(comandoSQL);
        }

        public DataSet getUnidades(string where)
        {
            comandoSQL = string.Format(
                @"SELECT CodigoProjeto
                  ,PeriodoMensuracao
                  ,DuracaoEstimada
                  ,CustoEstimado
                  ,ReceitaEstimada
                  ,PeriodicidadeFluxoCaixa
                  ,DescricaoProposta
                  ,NomeProjeto
              FROM {0}.{1}.Projeto
             WHERE 1 = 1 {2}", bancodb, Ownerdb, where);
            return classeDados.getDataSet(comandoSQL);
        }

        public DataSet getStatus(string where)
        {
            comandoSQL = string.Format(
                                     @"SELECT *
                                   FROM {0}.{1}.Status 
                            WHERE 1 = 1 {2}", bancodb, Ownerdb, where);
            return getDataSet(comandoSQL);
        }

        public DataSet getListaPossiveisStatusFluxosProjeto(int codigoProjeto, int codigoFluxo, string where)
        {
            comandoSQL = string.Format(
                                     @"
                                    BEGIN
		                                DECLARE @IniciaisTipoProjeto VarChar(20),
						                        @CodigoProjeto int,
                                                @CodigoFluxo int
						
		                                DECLARE @tbResumo Table(CodigoStatus int,
														        Descricao VarChar(50),
														        IndicaSelecionado Char(1))
														
		
		                                SET @CodigoProjeto = {2}
                                        SET @CodigoFluxo = {3}
		
		                                SELECT @IniciaisTipoProjeto = s.TipoStatus
			                              FROM {0}.{1}.Projeto p INNER JOIN
					                           {0}.{1}.Status s ON s.CodigoStatus = p.CodigoStatusProjeto
		                                 WHERE p.CodigoProjeto = @CodigoProjeto
	 
	                                 INSERT INTO @tbResumo	
		                                SELECT s.CodigoStatus, s.DescricaoStatus, 'N'
			                              FROM {0}.{1}.Status s
		                                 WHERE s.TipoStatus = @IniciaisTipoProjeto
                                           {4}
	 
	                                 UPDATE @tbResumo SET IndicaSelecionado = 'S'
		                              WHERE CodigoStatus IN(SELECT fsp.CodigoStatus 
														      FROM {0}.{1}.FluxosStatusProjeto fsp
													          WHERE fsp.CodigoProjeto = @CodigoProjeto
                                                                AND fsp.CodigoFluxo = @CodigoFluxo)
	 
	                                 SELECT * FROM @tbResumo
  
                                  END", bancodb, Ownerdb, codigoProjeto, codigoFluxo, where);
            return getDataSet(comandoSQL);
        }

        public DataSet getCategoria(string where)
        {
            comandoSQL = string.Format(
                                 @"SELECT  CodigoCategoria
                                      ,SiglaCategoria
                                      ,DescricaoCategoria
                                  FROM {0}.{1}.Categoria
                            WHERE DataExclusao IS NULL 
                              {2}
                            ORDER BY SiglaCategoria", bancodb, Ownerdb, where);
            return getDataSet(comandoSQL);
        }

        public DataSet getTipoProjeto(string where)
        {
            comandoSQL = string.Format(
                                 @"SELECT  CodigoTipoProjeto
                                      ,TipoProjeto
                                  FROM {0}.{1}.TipoProjeto
                            WHERE ( (IndicaTipoProjeto = 'PRJ') OR (IndicaTipoProjeto='PRC' AND CodigoTipoAssociacao=26) )
                              {2}
                            ORDER BY TipoProjeto", bancodb, Ownerdb, where);
            return getDataSet(comandoSQL);
        }

        public DataSet getFatoresEntidade(int codigoEntidade, string where)
        {
            comandoSQL = string.Format(
                             @"SELECT CodigoFatorPortfolio
                                 ,NomeFatorPortfolio
                             FROM {0}.{1}.FatorPortfolio
                            WHERE DataExclusao IS NULL 
                              AND CodigoEntidade = {2}
                              {3}
                            ORDER BY NomeFatorPortfolio", bancodb, Ownerdb, codigoEntidade, where);
            return getDataSet(comandoSQL);
        }

        public DataSet getGruposCategoria(int codigoCategoria, int codigoEntidade, string where)
        {
            comandoSQL = string.Format(
                             @"SELECT CodigoGrupoCriterio
                                 ,NomeGrupo
                             FROM {0}.{1}.GrupoCriterioSelecao
                            WHERE DataExclusao IS NULL 
                              AND CodigoEntidade = {2}
                              AND CodigoCategoria = {3}
                              {4}
                            ORDER BY NomeGrupo", bancodb, Ownerdb, codigoEntidade, codigoCategoria, where);
            return getDataSet(comandoSQL);
        }

        public DataSet getUnidade(string where)
        {
            comandoSQL = string.Format(@"
                    SELECT  CodigoUnidadeNegocio
                            ,NomeUnidadeNegocio
                            ,SiglaUnidadeNegocio
                            ,CodigoUnidadeNegocioSuperior
                            ,SiglaUF
                    FROM {0}.{1}.UnidadeNegocio
                    WHERE 1 = 1 {2}
                    ORDER BY SiglaUnidadeNegocio
                    ", bancodb, Ownerdb, where);
            return getDataSet(comandoSQL);
        }

        public bool atualizaProposta(string nomeProjeto, string descricao, int codigoUsuario, int codigoCategoria, int codigoStatus, int codigoGerente, int codigoUnidade, int codigoProjeto, ref int registrosAfetados)
        {
            try
            {
                comandoSQL = string.Format(@"
                                BEGIN
                                     UPDATE {0}.{1}.Projeto SET
                                               NomeProjeto = '{3}'
                                               ,DataUltimaAlteracao = getdate()
                                               ,CodigoUsuarioUltimaAlteracao = {4}
                                               ,CodigoCategoria = {5}
                                               ,CodigoStatusProjeto = {6}
                                               ,CodigoGerenteProjeto = {7}
                                               ,CodigoUnidadeNegocio = {8}
                                               ,DescricaoProposta = '{9}'
                                         WHERE CodigoProjeto = {2}                                     
                                END        
                                               ", bancodb, Ownerdb, codigoProjeto, nomeProjeto, codigoUsuario, codigoCategoria, codigoStatus, codigoGerente, codigoUnidade, descricao);

                execSQL(comandoSQL, ref registrosAfetados);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public DataSet getAnoRecursosHumanos(int idProjeto)
        {


            string comandoSQL = string.Format(
                 @" select distinct YEAR(Data) as ano from {0}.{1}.vi_AtribuicaoDiariaRecurso
                        where CodigoProjeto = {2}", bancodb, Ownerdb, idProjeto);


            DataSet ds = getDataSet(comandoSQL);

            return ds;



        }

        public DataSet getListaProjetos(int codigoEntidade, int idUsuario, int codigoCarteira, int codigoCarteiraListaProcessos, string where)
        {
            string comandoSQL = string.Format(
                @"BEGIN
                  DECLARE @CodigoEntidade int,
						  @CodigoUsuario int,
                          @CodigoCarteira int,
                          @codigoCarteiraListaProcessos         int,
                          @IniciaisCarteiraControladaSistema	varchar(20)
                  
                  SET @CodigoEntidade = {2}
                  SET @CodigoUsuario = {3} 
                  SET @CodigoCarteira = {4} 
                  SET @codigoCarteiraListaProcessos = {6}
                
                SELECT @IniciaisCarteiraControladaSistema = c.[IniciaisCarteiraControladaSistema]
                FROM {0}.{1}.[Carteira] c
                WHERE c.[CodigoCarteira] = @CodigoCarteira;

                IF (ISNULL(@IniciaisCarteiraControladaSistema, '') <> 'PR')
                    SET @codigoCarteiraListaProcessos = NULL;
                  
                EXEC [dbo].[p_SincronizaResumoProjeto];
                  
                  DECLARE @tbResumo TABLE(Descricao VarChar(255),
                                                     Codigo VarChar(10),
                                                     SiglaUnidadeUnidade VarChar(10),
                                                     CorGeral VarChar(10),
                                                     CorRisco VarChar(10),
                                                     CorFisico VarChar(10),
                                                     CorFinanceiro VarChar(10),
                                                     CorEscopo VarChar(10),
                                                     CorReceita VarChar(10),
                                                     CorTrabalho VarChar(10),
                                                     CorMeta VarChar(10),
                                                     TipoProjeto VarChar(60),
                                                     StatusProjeto VarChar(50),
                                                     CodigoGerenteProjeto int,
                                                     CodigoStatusProjeto int,
                                                     CodigoUnidade int,
                                                     GerenteProjeto VarChar(60),
                                                     CodigoProjetoPai VarChar(10),
                                                     CodigoEntidade int,
                                                     IndicaProjeto Char(1),
                                        CodigoProjeto int,
                                        IndicaPrograma Char(1),
                                        Permissao bit,
                                        NomeUnidadeAtendimento varchar(100),
                                        CorIAM VarChar(10),
                                        CorConvenio VarChar(10),
                                        UltimaAtualizacao DateTime,
                                        CR varchar(300),
                                        IniciaisTipoProjeto     varchar(10),
                                        IndicaProjetoAgil       char(1),
                                        IniciaisTipoControladoSistema varchar(18))
                     /* 
                      Insere os projetos
                     */
                   INSERT INTO @tbResumo
                     (
                    Descricao ,
                        Codigo ,
                        SiglaUnidadeUnidade ,
                        CorGeral ,
                        CorRisco ,
                        CorFisico ,
                        CorFinanceiro ,
                        CorEscopo ,
                        CorReceita ,
                        CorTrabalho ,
                        CorMeta ,
                        TipoProjeto ,
                        StatusProjeto ,
                        CodigoGerenteProjeto ,
                        CodigoStatusProjeto ,
                        CodigoUnidade ,
                        GerenteProjeto ,
                        CodigoProjetoPai ,
                        CodigoEntidade ,
                        IndicaProjeto ,
                    CodigoProjeto ,
                    IndicaPrograma ,
                    Permissao,
                    NomeUnidadeAtendimento,
                    CorIAM,
                    CorConvenio,
                    UltimaAtualizacao,
                    IniciaisTipoProjeto,
                    IndicaProjetoAgil,
                    IniciaisTipoControladoSistema
                     )
                            SELECT p.NomeProjeto AS Descricao,
                               Right('00000000' + Convert(Varchar,p.CodigoProjeto),8) AS Codigo,
                               un.SiglaUnidadeNegocio AS SiglaUnidadeUnidade,
                               RTRIM(rp.CorGeral) AS CorGeral,
                               {0}.{1}.f_GetCorRisco(p.CodigoProjeto) AS CorRisco,
                               {0}.{1}.f_GetCorFisico(p.CodigoProjeto) AS CorFisico,
                               {0}.{1}.f_GetCorFinanceiro(p.CodigoProjeto) AS CorFinanceiro,
                               {0}.{1}.f_GetCorEscopo(p.CodigoProjeto) AS CorEscopo,
                               {0}.{1}.f_GetCorReceita(p.CodigoProjeto) AS CorReceita,
                               {0}.{1}.f_GetCorTrabalho(p.CodigoProjeto) AS CorTrabalho,
                               {0}.{1}.f_GetCorMeta(p.CodigoProjeto) AS CorMeta,
                               c.DescricaoCategoria AS TipoProjeto,
                               s.DescricaoStatus AS StatusProjeto,
                               p.CodigoGerenteProjeto AS CodigoGerenteProjeto,
                               p.CodigoStatusProjeto AS CodigoStatusProjeto,
                               un.CodigoUnidadeNegocio AS CodigoUnidade,
                               g.NomeUsuario AS GerenteProjeto,
                               {0}.{1}.f_GetCodigoPaiProjeto(p.CodigoProjeto) AS CodigoProjetoPai,
                               un.CodigoEntidade, 'S',
                               p.CodigoProjeto,
                               p.IndicaPrograma,
                               {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, p.CodigoProjeto, NULL, 'PR', 0, NULL, 'PR_Acs'),
                               un2.NomeUnidadeNegocio,
                               {0}.{1}.f_GetCorIAM(p.CodigoProjeto) AS CorIAM,
                               {0}.{1}.f_GetCorGestaoConvenio(p.CodigoProjeto) AS CorConvenio,
                               p.DataUltimaAtualizacao,
                               tp.IndicaTipoProjeto,
                               tp.IndicaProjetoAgil,
                               tp.IniciaisTipoControladoSistema
                          FROM {0}.{1}.Projeto AS p  INNER JOIN 
                               {0}.{1}.TipoProjeto  AS tp ON (tp.CodigoTipoProjeto = p.CodigoTipoProjeto) INNER JOIN 
                               {0}.{1}.ResumoProjeto rp ON (rp.CodigoProjeto = p.CodigoProjeto) LEFT JOIN
                               {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria
                                                      AND c.DataExclusao IS NULL) LEFT JOIN
                               {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio) LEFT JOIN
                               {0}.{1}.UnidadeNegocio AS un2 ON (un2.CodigoUnidadeNegocio = p.CodigoUnidadeAtendimento) LEFT JOIN
                               {0}.{1}.Usuario AS g ON (g.CodigoUsuario = p.CodigoGerenteProjeto) INNER JOIN
                               {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                    AND s.IndicaAcompanhamentoExecucao = 'S')  
                               INNER JOIN
                               ( SELECT CodigoProjeto
                                 FROM 
                                        {0}.{1}.f_GetProjetosUsuario(@CodigoUsuario, @CodigoEntidade, @CodigoCarteira) prj 
                                 UNION
                                 SELECT CodigoProjeto
                                 FROM 
                                        {0}.{1}.f_GetProjetosUsuario(@CodigoUsuario, @CodigoEntidade, @codigoCarteiraListaProcessos) prc
                               ) AS sub on (sub.codigoProjeto = p.CodigoProjeto)
                         WHERE p.DataExclusao IS NULL AND p.CodigoEntidade = @CodigoEntidade
                           {5}
                           
                       
                       IF( EXISTS( SELECT 1 FROM {0}.{1}.ParametroConfiguracaoSistema AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoEntidade AND pcs.[Parametro] = 'MostraCR' AND pcs.[Valor] = 'S' ) )
                       BEGIN
                         UPDATE @tbResumo
                           SET [CR] = {0}.{1}.f_GetDescricaoCrPrincipal(CodigoProjeto);
                       END -- IF( EXISTS( SELECT 1 FROM {0}.{1}.ParametroConfiguracaoSistema

                       /* 
                        Insere as unidades de negócio
                       */ 
                       INSERT INTO @tbResumo 
                          (
                    Descricao ,
                        Codigo ,
                        SiglaUnidadeUnidade ,
                        CorGeral ,
                        CorRisco ,
                        CorFisico ,
                        CorFinanceiro ,
                        CorEscopo ,
                        CorReceita ,
                        CorTrabalho ,
                        CorMeta ,
                        TipoProjeto ,
                        StatusProjeto ,
                        CodigoGerenteProjeto ,
                        CodigoStatusProjeto ,
                        CodigoUnidade ,
                        GerenteProjeto ,
                        CodigoProjetoPai ,
                        CodigoEntidade ,
                        IndicaProjeto ,
                    CodigoProjeto ,
                    IndicaPrograma,
                    Permissao,
                    NomeUnidadeAtendimento,
                    CorIAM,
                    CorConvenio,
                    UltimaAtualizacao
                     )
                          SELECT un.NomeUnidadeNegocio AS Descricao,
                               'UN' +  Right('00000000' + Convert(Varchar,un.CodigoUnidadeNegocio),8) AS Codigo,
                               un.SiglaUnidadeNegocio AS SiglaUnidadeUnidade,
                               Null,
                               Null,
                               Null,
                               Null,
                               Null,
                               Null,
                               Null,
                               Null,
                               Null,
                               Null,
                               un.CodigoUsuarioGerente,
                               Null,
                               un.CodigoUnidadeNegocio AS CodigoUnidade,
                               null,
                               'UN' + Right('00000000' + Convert(Varchar,un.CodigoUnidadeNegocioSuperior),8),
                               un.CodigoEntidade, 
                               'N',
                               null,
                               null,
                               {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, un.CodigoUnidadeNegocio, NULL, 'UN', 0, NULL, 'UN_CnsReuTec'),
                               null,
                               null,
                               null,
                               null

                          FROM {0}.{1}.UnidadeNegocio AS un  LEFT JOIN
                               {0}.{1}.Usuario AS g ON (g.CodigoUsuario = un.CodigoUsuarioGerente) 
                         WHERE Exists(SELECT 1
                                            FROM {0}.{1}.Projeto AS p INNER JOIN
                                             @tbResumo tr ON tr.CodigoProjeto = p.CodigoProjeto
                                         WHERE p.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio)
                             AND un.CodigoUnidadeNegocio IN(SELECT tb.CodigoUnidade FROM @tbResumo tb)
                        
                        
                                                               
                        DECLARE @CodigoUnidade Int,
                                @CodigoUnidadeSuperior Int,
                                @CodigoUnidadeAux Int
                                                                             
                        /* Cursor para percorrer as unidades de negócio e trazer seus respectivos pais */                                                                         
                        DECLARE cCursor CURSOR LOCAL STATIC FOR
                        SELECT DISTINCT CodigoUnidade
                          FROM @tbResumo
                         WHERE CodigoProjeto IS NULL
                        
                        OPEN cCursor
                        
                        FETCH NEXT FROM cCursor INTO @CodigoUnidade
                        
                        SET @CodigoUnidadeAux = @CodigoUnidade
                                                                  
                      SELECT @CodigoUnidadeSuperior = CodigoUnidadeNegocioSuperior
                        FROM {0}.{1}.UnidadeNegocio
                       WHERE CodigoUnidadeNegocio = @CodigoUnidadeAux
                                                        
                        WHILE @@FETCH_STATUS = 0
                          BEGIN     
                            WHILE @CodigoUnidadeSuperior IS NOT NULL
                              BEGIN   
                                
                                INSERT INTO @tbResumo 
                                                SELECT un.NomeUnidadeNegocio AS Descricao,
                                                        'UN' +  Right('00000000' + Convert(Varchar,un.CodigoUnidadeNegocio),8) AS Codigo,
                                                        un.SiglaUnidadeNegocio AS SiglaUnidadeUnidade,
                                                        Null,
                                                        Null,
                                                        Null,
                                                        Null,
                                                        Null,
                                                        Null,
                                                       Null,
                                                        Null,
                                                        Null,
                                                        Null,
                                                        un.CodigoUsuarioGerente,
                                                        Null,
                                                        un.CodigoUnidadeNegocio AS CodigoUnidade,
                                                        null,
                                                        'UN' + Right('00000000' + Convert(Varchar,un.CodigoUnidadeNegocioSuperior),8),
                                                        un.CodigoEntidade, 
                                                        'N',
                                                        null,
                                                        null,
                                       {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidade, un.CodigoUnidadeNegocio, NULL, 'UN', 0, NULL, 'UN_CnsReuTec'),
                                                        null,
                                                        null,
                                                        null,
                                                        null,
                                                        null,
                                                        null,
                                                        null,
                                                        null
                                                 FROM {0}.{1}.UnidadeNegocio AS un  LEFT JOIN
                                                        {0}.{1}.Usuario AS g ON (g.CodigoUsuario = un.CodigoUsuarioGerente) 
                                                WHERE un.CodigoUnidadeNegocio = @CodigoUnidadeSuperior
                                                  AND un.CodigoUnidadeNegocio NOT IN (SELECT CodigoUnidade FROM @tbResumo)
                                                  
                                                SET @CodigoUnidadeAux = @CodigoUnidadeSuperior
                                               SET @CodigoUnidadeSuperior = NULL
                                                  
                                     SELECT @CodigoUnidadeSuperior = CodigoUnidadeNegocioSuperior
                                       FROM {0}.{1}.UnidadeNegocio
                                      WHERE CodigoUnidadeNegocio = @CodigoUnidadeAux

                                     IF NOT EXISTS(SELECT 1
                                                     FROM {0}.{1}.UnidadeNegocio
                                                    WHERE CodigoUnidadeNegocioSuperior = @CodigoUnidadeSuperior)
                                        SET @CodigoUnidadeSuperior = NULL               
                                  END 
                                   
                                                               
                            FETCH NEXT FROM cCursor INTO @CodigoUnidade
                            
                             SET @CodigoUnidadeAux = @CodigoUnidade
                             SELECT @CodigoUnidadeSuperior = CodigoUnidadeNegocioSuperior
                                     FROM {0}.{1}.UnidadeNegocio
                                    WHERE CodigoUnidadeNegocio = @CodigoUnidade
                            
                          END
                        
                        CLOSE cCursor
                        DEALLOCATE cCursor
                        
                        /* 
                           Se há algum projeto associado a um programa que o usuário
                           não possui acesso, mostra como superior a a unidade de negócio
                           onde ele está lotado
                        */
                        UPDATE @tbResumo
                           SET CodigoProjetoPai = 'UN' + Right('00000000' + Convert(Varchar,CodigoUnidade),8)
                         WHERE CodigoProjetoPai NOT IN (SELECT Codigo
                                                          FROM @tbResumo)    
                                                             
                        
                        /*
                         Exclui eventuais projetos sem pai
                        */
                        DELETE FROM @tbResumo
                              WHERE CodigoProjetoPai NOT IN (SELECT Codigo
                                                               FROM @tbResumo)     

                   
                    
                          
                          SELECT t.* 
                          FROM @tbResumo AS t 
                             WHERE (Codigo IN (SELECT CodigoProjetoPai
                                            FROM @tbResumo
                                                  WHERE Codigo <> CodigoProjetoPai)
                            OR t.CodigoProjeto  IS NOT NULL)
                          ORDER BY Descricao
                           
             END
            ", bancodb, Ownerdb, codigoEntidade, idUsuario, codigoCarteira, where, codigoCarteiraListaProcessos);

            return getDataSet(comandoSQL);
        }

        public DataSet getListaDemandas(int codigoEntidade, int idUsuario, string where)
        {
            string comandoSQL = string.Format(
                @"BEGIN
	            DECLARE @tbResumo TABLE(Descricao VarChar(255),
							            Codigo VarChar(10),
							            SiglaUnidadeUnidade VarChar(10),
							            StatusProjeto VarChar(50),
                                        Prioridade VarChar(5),
							            CodigoGerenteProjeto int,
							            CodigoStatusProjeto int,
							            CodigoUnidade int,
							            GerenteProjeto VarChar(60),
							            CodigoProjetoPai VarChar(10),
							            CodigoEntidade int,
                                        CodigoProjeto int,
                                        IndicaProjeto Char(1),
                                        Demandante VarChar(60),
                                        TipoDemanda Char(1),
                                        InicioProposta DateTime,
                                        TerminoProposta DateTime)
            	   /* 
            	    Insere os projetos
            	   */
                   INSERT INTO @tbResumo
                     (
                    Descricao ,
		            Codigo ,
		            SiglaUnidadeUnidade ,
		            StatusProjeto ,
                    Prioridade,
		            CodigoGerenteProjeto ,
		            CodigoStatusProjeto ,
		            CodigoUnidade ,
		            GerenteProjeto ,
		            CodigoProjetoPai ,
		            CodigoEntidade ,
                    CodigoProjeto,
                    IndicaProjeto,
                    Demandante,
                    TipoDemanda,
                    InicioProposta,
                    TerminoProposta
                     )
		                SELECT p.NomeProjeto AS Descricao,
                               Right('00000000' + Convert(Varchar,p.CodigoProjeto),8) AS Codigo,
                               un.SiglaUnidadeNegocio AS SiglaUnidadeUnidade,
                               s.DescricaoStatus AS StatusProjeto,
                               CASE 
                                WHEN Prioridade < 200 THEN 'Alta' 
                                WHEN Prioridade > 199 AND Prioridade < 400 THEN 'Média'
                                WHEN Prioridade >= 400 THEN 'Baixa'
                                ELSE '-'
                               END,
                               p.CodigoGerenteProjeto AS CodigoGerenteProjeto,
                               p.CodigoStatusProjeto AS CodigoStatusProjeto,
                               un.CodigoUnidadeNegocio AS CodigoUnidade,
                               (Case When (Charindex(' ', Ltrim(Rtrim(g.NomeUsuario))) - 1) > 0 Then LEFT(g.NomeUsuario, CHARINDEX (' ', g.NomeUsuario) - 1) 
		                            + ' ' + RIGHT(g.NomeUsuario, CHARINDEX (' ', REVERSE (g.NomeUsuario)) - 1) ELSE g.NomeUsuario END) AS GerenteProjeto,
                               {0}.{1}.f_GetCodigoPaiProjeto(p.CodigoProjeto) AS CodigoProjetoPai,
                               un.CodigoEntidade, 
                               p.CodigoProjeto, 
                               'S',
                               (Case When (Charindex(' ', Ltrim(Rtrim(dem.NomeUsuario))) - 1) > 0 Then LEFT(dem.NomeUsuario, CHARINDEX (' ', dem.NomeUsuario) - 1) 
		                            + ' ' + RIGHT(dem.NomeUsuario, CHARINDEX (' ', REVERSE (dem.NomeUsuario)) - 1) ELSE dem.NomeUsuario END),
                               CASE WHEN p.CodigoTipoProjeto = 4 THEN 'C' ELSE 'S' END,
                               p.InicioProposta,
                               p.TerminoProposta
                          FROM {0}.{1}.Projeto AS p INNER JOIN
                               {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio) INNER JOIN
                               {0}.{1}.Usuario AS g ON (g.CodigoUsuario = p.CodigoGerenteProjeto) LEFT JOIN
                               {0}.{1}.Usuario AS dem ON (dem.CodigoUsuario = p.CodigoCliente) INNER JOIN
                               {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto) INNER JOIN
                               {0}.{1}.f_GetDemandasUsuario({3}, {2}) F on (f.codigoProjeto = p.CodigoProjeto)
                         WHERE p.CodigoEntidade = {2} {4}
                           
                       
                       /* 
                        Insere as unidades de negócio
                       */ 
                       INSERT INTO @tbResumo 
                          (
                    Descricao ,
		            Codigo ,
		            SiglaUnidadeUnidade ,
		            StatusProjeto ,
		            CodigoGerenteProjeto ,
		            CodigoStatusProjeto ,
		            CodigoUnidade ,
		            GerenteProjeto ,
		            CodigoProjetoPai ,
		            CodigoEntidade ,
                    CodigoProjeto,
                    IndicaProjeto
                     )
                          SELECT un.SiglaUnidadeNegocio AS Descricao,
                               'UN' +  Right('00000000' + Convert(Varchar,un.CodigoUnidadeNegocio),8) AS Codigo,
                               un.SiglaUnidadeNegocio AS SiglaUnidadeUnidade,
                               Null,
                               un.CodigoUsuarioGerente,
                               Null,
                               un.CodigoUnidadeNegocio AS CodigoUnidade,
                               null,
                               'UN' + Right('00000000' + Convert(Varchar,un.CodigoUnidadeNegocioSuperior),8),
                               un.CodigoEntidade, 
                               null,
                               'N'
                          FROM {0}.{1}.UnidadeNegocio AS un  LEFT JOIN
                               {0}.{1}.Usuario AS g ON (g.CodigoUsuario = un.CodigoUsuarioGerente) 
                         WHERE Exists(SELECT 1
		                                FROM {0}.{1}.Projeto AS p INNER JOIN
                                             {0}.{1}.f_GetDemandasUsuario({3}, {2}) F on (f.codigoProjeto = p.CodigoProjeto)
	                                   WHERE p.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio)
	                       AND un.CodigoUnidadeNegocio IN(SELECT tb.CodigoUnidade FROM @tbResumo tb)
            		
            		
            		                                       
            		DECLARE @CodigoUnidade Int,
            		        @CodigoUnidadeSuperior Int,
            		        @CodigoUnidadeAux Int
            		                                               	
            		/* Cursor para percorrer as unidades de negócio e trazer seus respectivos pais */                                       	                                  
		            DECLARE cCursor CURSOR LOCAL STATIC FOR
		            SELECT DISTINCT CodigoUnidade
		              FROM @tbResumo
		             WHERE CodigoProjeto IS NULL
		            
		            OPEN cCursor
		            
		            FETCH NEXT FROM cCursor INTO @CodigoUnidade
		            
		            SET @CodigoUnidadeAux = @CodigoUnidade
		            		            	            
	                SELECT @CodigoUnidadeSuperior = CodigoUnidadeNegocioSuperior
	                  FROM {0}.{1}.UnidadeNegocio
	                 WHERE CodigoUnidadeNegocio = @CodigoUnidadeAux
	                   AND CodigoEntidade = {2} -- Parâmetro CodigoEntidade!!!!
	            	                                
		            WHILE @@FETCH_STATUS = 0
		              BEGIN	
		                WHILE @CodigoUnidadeSuperior IS NOT NULL
		                  BEGIN	  
		                    	                             
		                    INSERT INTO @tbResumo 
								 SELECT un.SiglaUnidadeNegocio AS Descricao,
									   'UN' +  Right('00000000' + Convert(Varchar,un.CodigoUnidadeNegocio),8) AS Codigo,
									   un.SiglaUnidadeNegocio AS SiglaUnidadeUnidade,
									   Null,
                                       null,
									   un.CodigoUsuarioGerente,
									   Null,
									   un.CodigoUnidadeNegocio AS CodigoUnidade,
									   null,
									   'UN' + Right('00000000' + Convert(Varchar,un.CodigoUnidadeNegocioSuperior),8),
									   un.CodigoEntidade, 
									   null,
                                       'N',
                                       null,
                                       null,
                                       null,
                                       null
								  FROM {0}.{1}.UnidadeNegocio AS un  LEFT JOIN
									   {0}.{1}.Usuario AS g ON (g.CodigoUsuario = un.CodigoUsuarioGerente) 
								 WHERE un.CodigoUnidadeNegocio = @CodigoUnidadeSuperior
								   AND un.CodigoUnidadeNegocio NOT IN (SELECT CodigoUnidade FROM @tbResumo)
								   AND un.CodigoEntidade = {2} --> Parâmetro!!!!
								   
								 SET @CodigoUnidadeAux = @CodigoUnidadeSuperior
								 SET @CodigoUnidadeSuperior = NULL
								   
								 SELECT @CodigoUnidadeSuperior = CodigoUnidadeNegocioSuperior
		                           FROM {0}.{1}.UnidadeNegocio
		                          WHERE CodigoUnidadeNegocio = @CodigoUnidadeAux
		                            AND CodigoEntidade = {2} -- Parâmetro CodigoEntidade!!!!  
		                      END  
		                 
		                FETCH NEXT FROM cCursor INTO @CodigoUnidade
		              END
		            
		            CLOSE cCursor
		            DEALLOCATE cCursor
		            
		            /* 
            		   Se há algum projeto associado a um programa que o usuário
            		   não possui acesso, mostra como superior a a unidade de negócio
            		   onde ele está lotado
            		*/
            		UPDATE @tbResumo
            		   SET CodigoProjetoPai = 'UN' + Right('00000000' + Convert(Varchar,CodigoUnidade),8)
            		 WHERE CodigoProjetoPai NOT IN (SELECT Codigo
            		                                  FROM @tbResumo)	
            		                                     
            		
            		/*
            		 Exclui eventuais projetos sem pai
            		*/
            		DELETE FROM @tbResumo
            		      WHERE CodigoProjetoPai NOT IN (SELECT Codigo
            		                                       FROM @tbResumo)	
		              
		            SELECT * FROM @tbResumo
             END", bancodb, Ownerdb, codigoEntidade, idUsuario, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getListaProcessos(int codigoEntidade, int idUsuario, string where)
        {
            string comandoSQL = string.Format(
                @"BEGIN
	            DECLARE @tbResumo TABLE(Descricao VarChar(255),
							            Codigo VarChar(10),
							            SiglaUnidadeUnidade VarChar(10),
							            StatusProjeto VarChar(50),
							            CodigoGerenteProjeto int,
							            CodigoStatusProjeto int,
							            CodigoUnidade int,
							            GerenteProjeto VarChar(60),
							            CodigoProjetoPai VarChar(10),
							            CodigoEntidade int,
                                        CodigoProjeto int,
                                        IndicaProjeto Char(1))
            	   /* 
            	    Insere os projetos
            	   */
                   INSERT INTO @tbResumo
                     (
                    Descricao ,
		            Codigo ,
		            SiglaUnidadeUnidade ,
		            StatusProjeto ,
		            CodigoGerenteProjeto ,
		            CodigoStatusProjeto ,
		            CodigoUnidade ,
		            GerenteProjeto ,
		            CodigoProjetoPai ,
		            CodigoEntidade ,
                    CodigoProjeto,
                    IndicaProjeto
                     )
		                SELECT p.NomeProjeto AS Descricao,
                               Right('00000000' + Convert(Varchar,p.CodigoProjeto),8) AS Codigo,
                               un.SiglaUnidadeNegocio AS SiglaUnidadeUnidade,
                               s.DescricaoStatus AS StatusProjeto,
                               p.CodigoGerenteProjeto AS CodigoGerenteProjeto,
                               p.CodigoStatusProjeto AS CodigoStatusProjeto,
                               un.CodigoUnidadeNegocio AS CodigoUnidade,
                               (Case When (Charindex(' ', Ltrim(Rtrim(g.NomeUsuario))) - 1) > 0 Then LEFT(g.NomeUsuario, CHARINDEX (' ', g.NomeUsuario) - 1) 
		                            + ' ' + RIGHT(g.NomeUsuario, CHARINDEX (' ', REVERSE (g.NomeUsuario)) - 1) ELSE g.NomeUsuario END) AS GerenteProjeto,
                               {0}.{1}.f_GetCodigoPaiProjeto(p.CodigoProjeto) AS CodigoProjetoPai,
                               un.CodigoEntidade, 
                               p.CodigoProjeto, 
                               'S'
                          FROM {0}.{1}.Projeto AS p LEFT JOIN
                               {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio) LEFT JOIN
                               {0}.{1}.Usuario AS g ON (g.CodigoUsuario = p.CodigoGerenteProjeto) LEFT JOIN
                               {0}.{1}.Usuario AS dem ON (dem.CodigoUsuario = p.CodigoCliente) INNER JOIN
                               {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto) INNER JOIN
                               {0}.{1}.f_GetProcessosUsuario({3}, {2}) F on (f.codigoProjeto = p.CodigoProjeto)
                         WHERE p.CodigoEntidade = {2} {4}
                           
                       
                       /* 
                        Insere as unidades de negócio
                       */ 
                       INSERT INTO @tbResumo 
                          (
                    Descricao ,
		            Codigo ,
		            SiglaUnidadeUnidade ,
		            StatusProjeto ,
		            CodigoGerenteProjeto ,
		            CodigoStatusProjeto ,
		            CodigoUnidade ,
		            GerenteProjeto ,
		            CodigoProjetoPai ,
		            CodigoEntidade ,
                    CodigoProjeto,
                    IndicaProjeto
                     )
                          SELECT un.SiglaUnidadeNegocio AS Descricao,
                               'UN' +  Right('00000000' + Convert(Varchar,un.CodigoUnidadeNegocio),8) AS Codigo,
                               un.SiglaUnidadeNegocio AS SiglaUnidadeUnidade,
                               Null,
                               un.CodigoUsuarioGerente,
                               Null,
                               un.CodigoUnidadeNegocio AS CodigoUnidade,
                               null,
                               'UN' + Right('00000000' + Convert(Varchar,un.CodigoUnidadeNegocioSuperior),8),
                               un.CodigoEntidade, 
                               null,
                               'N'
                          FROM {0}.{1}.UnidadeNegocio AS un  LEFT JOIN
                               {0}.{1}.Usuario AS g ON (g.CodigoUsuario = un.CodigoUsuarioGerente) 
                         WHERE Exists(SELECT 1
		                                FROM {0}.{1}.Projeto AS p INNER JOIN
                                             {0}.{1}.f_GetProcessosUsuario({3}, {2}) F on (f.codigoProjeto = p.CodigoProjeto)
	                                   WHERE p.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio)
	                       AND un.CodigoUnidadeNegocio IN(SELECT tb.CodigoUnidade FROM @tbResumo tb)
            		
            		
            		                                       
            		DECLARE @CodigoUnidade Int,
            		        @CodigoUnidadeSuperior Int,
            		        @CodigoUnidadeAux Int
            		                                               	
            		/* Cursor para percorrer as unidades de negócio e trazer seus respectivos pais */                                       	                                  
		            DECLARE cCursor CURSOR LOCAL STATIC FOR
		            SELECT DISTINCT CodigoUnidade
		              FROM @tbResumo
		             WHERE CodigoProjeto IS NULL
		            
		            OPEN cCursor
		            
		            FETCH NEXT FROM cCursor INTO @CodigoUnidade
		            
		            SET @CodigoUnidadeAux = @CodigoUnidade
		            		            	            
	                SELECT @CodigoUnidadeSuperior = CodigoUnidadeNegocioSuperior
	                  FROM {0}.{1}.UnidadeNegocio
	                 WHERE CodigoUnidadeNegocio = @CodigoUnidadeAux
	                   AND CodigoEntidade = {2} -- Parâmetro CodigoEntidade!!!!
	            	                                
		            WHILE @@FETCH_STATUS = 0
		              BEGIN	
		                WHILE @CodigoUnidadeSuperior IS NOT NULL
		                  BEGIN	  
		                    	                             
		                    INSERT INTO @tbResumo 
								 SELECT un.SiglaUnidadeNegocio AS Descricao,
									   'UN' +  Right('00000000' + Convert(Varchar,un.CodigoUnidadeNegocio),8) AS Codigo,
									   un.SiglaUnidadeNegocio AS SiglaUnidadeUnidade,
                                       null,
									   un.CodigoUsuarioGerente,
									   Null,
									   un.CodigoUnidadeNegocio AS CodigoUnidade,
									   null,
									   'UN' + Right('00000000' + Convert(Varchar,un.CodigoUnidadeNegocioSuperior),8),
									   un.CodigoEntidade, 
									   null,
                                       'N'
								  FROM {0}.{1}.UnidadeNegocio AS un  LEFT JOIN
									   {0}.{1}.Usuario AS g ON (g.CodigoUsuario = un.CodigoUsuarioGerente) 
								 WHERE un.CodigoUnidadeNegocio = @CodigoUnidadeSuperior
								   AND un.CodigoUnidadeNegocio NOT IN (SELECT CodigoUnidade FROM @tbResumo)
								   AND un.CodigoEntidade = {2} --> Parâmetro!!!!
								   
								 SET @CodigoUnidadeAux = @CodigoUnidadeSuperior
								 SET @CodigoUnidadeSuperior = NULL
								   
								 SELECT @CodigoUnidadeSuperior = CodigoUnidadeNegocioSuperior
		                           FROM {0}.{1}.UnidadeNegocio
		                          WHERE CodigoUnidadeNegocio = @CodigoUnidadeAux
		                            AND CodigoEntidade = {2} -- Parâmetro CodigoEntidade!!!!  
		                      END  
		                 
		                FETCH NEXT FROM cCursor INTO @CodigoUnidade
		              END
		            
		            CLOSE cCursor
		            DEALLOCATE cCursor
		            
		            /* 
            		   Se há algum projeto associado a um programa que o usuário
            		   não possui acesso, mostra como superior a a unidade de negócio
            		   onde ele está lotado
            		*/
            		UPDATE @tbResumo
            		   SET CodigoProjetoPai = 'UN' + Right('00000000' + Convert(Varchar,CodigoUnidade),8)
            		 WHERE CodigoProjetoPai NOT IN (SELECT Codigo
            		                                  FROM @tbResumo)	
            		                                     
            		
            		/*
            		 Exclui eventuais projetos sem pai
            		*/
            		DELETE FROM @tbResumo
            		      WHERE CodigoProjetoPai NOT IN (SELECT Codigo
            		                                       FROM @tbResumo)	
		              
		            SELECT * FROM @tbResumo
             END", bancodb, Ownerdb, codigoEntidade, idUsuario, where);

            return getDataSet(comandoSQL);
        }

        public DataSet getDadosDemanda(int codigoDemanda, string where)
        {
            string comandoSQL = string.Format(
                  @"SELECT p.NomeProjeto AS Descricao,
                       p.CodigoProjeto,
                       CASE 
                        WHEN Prioridade < 200 THEN 'A' 
                        WHEN Prioridade > 199 AND Prioridade < 400 THEN 'M'
                        WHEN Prioridade >= 400 THEN 'B'
                        ELSE '-'
                       END AS Prioridade,
                       p.CodigoGerenteProjeto AS CodigoGerenteProjeto,
                       p.CodigoStatusProjeto AS CodigoStatusProjeto,
                       p.CodigoUnidadeNegocio AS CodigoUnidade,
                       p.CodigoProjeto,                
                       CONVERT(Char(10), p.InicioProposta, 103) AS InicioProposta,
                       CONVERT(Char(10), p.TerminoProposta, 103) AS TerminoProposta,
                       g.NomeUsuario AS NomeGerente,
                       dem.NomeUsuario AS NomeDemandante,
                       p.CodigoCliente AS CodigoDemandante,
                       DescricaoProposta,
                       p.Comentario
                  FROM {0}.{1}.Projeto AS p  LEFT JOIN
                       {0}.{1}.Usuario AS g ON (g.CodigoUsuario = p.CodigoGerenteProjeto) LEFT JOIN
                       {0}.{1}.Usuario AS dem ON (dem.CodigoUsuario = p.CodigoCliente)
                 WHERE p.CodigoProjeto = {2}
                   {3}", bancodb, Ownerdb, codigoDemanda, where);

            return getDataSet(comandoSQL);
        }
        public bool envioEmailRecursoCorporativo(string in_CodigoItem, string in_TituloItem, string in_DataMovimento, string in_NomeUsuarioMovimento, string in_CodigoNovoResponsavel)
        {
            try
            {
                comandoSQL = string.Format(@"                  
                    EXEC {0}.{1}.p_Agil_NotificaNovoResponsavelItem '{2}', '{3}', '{4}', '{5}', '{6}'", bancodb, Ownerdb, in_CodigoItem, in_TituloItem, in_DataMovimento, in_NomeUsuarioMovimento, in_CodigoNovoResponsavel);

                DataSet ds = getDataSet(comandoSQL);

                if (DataSetOk(ds))
                    return true;
                return false;
            }
            catch
            {
                return false;
            }

        }

        #endregion

        #region Relatórios

        public DataSet getDadosPlanilhaProposta(int ano, int codigoEntidade, string planilha, int? statusEtapa, int? trimestre)
        {

            comandoSQL = string.Format(@"EXEC {0}.{1}.p_sescoop_relMinisterio {2}, '{3}', {4}, '{5}', '{6}'"
                                        , bancodb, Ownerdb, codigoEntidade, ano, planilha, statusEtapa, trimestre);

            return getDataSet(comandoSQL);
        }

        public DataSet getAnosOrcamento(int codigoEntidade, int codigoUsuario)
        {
            string comandoSQL = string.Format(@"SELECT [Ano] FROM [dbo].[f_GetAnosOrcamentoProjeto]({0}, {1}) ORDER BY 1 DESC", codigoEntidade, codigoUsuario);

            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }


        public DataSet getStatusPrevisaoFluxoCaixaMTE()
        {
            string comandoSQL = string.Format(@"SELECT f.[Codigo], f.[Descricao] FROM [dbo].[f_getStatusPrevisaoFluxoCaixaMTE]() AS f");

            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }

        public DataSet getInformacoesPlanilhaMte(int codigoEntidade, int codigoUsuario, string planilha)
        {
            try
            {
                string comandoSQL = string.Format(@"EXEC {0}.{1}.p_GetAnexoPlanilhaSescoop {2}, {3}, {4}
               ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, planilha);

                DataSet ds = getDataSet(comandoSQL);
                return ds;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}