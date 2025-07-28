using Cdis.Brisk.Application;
using Cdis.Brisk.DataTransfer.Usuario;
using Crypto;
using System;
using System.Collections.Specialized;

/// <summary>
/// 
/// </summary>


namespace BriskPtf
{
    public class BasePageBrisk : System.Web.UI.Page
    {
        private OrderedDictionary _infoSistema;
        private OrderedDictionary InfoSistema
        {
            get
            {
                if (_infoSistema == null && Session["infoSistema"] != null)
                {
                    _infoSistema = (OrderedDictionary)Session["infoSistema"];
                }
                return _infoSistema;
            }
        }
        private UserDataTransfer _usuarioLogado { get; set; }
        public UserDataTransfer UsuarioLogado
        {
            get
            {
                if (InfoSistema == null)
                {
                    return new UserDataTransfer
                    {
                        Id = 123,
                        Nome = "Usuário Teste",
                        CodigoEntidade = 111,
                        CodigoPortfolio = 51,
                        CodigoCarteira = 1696,
                        Browser = GetUserBrowser()
                    };
                }

                if (_usuarioLogado == null)
                {
                    var nomeUsuario = GetItemInfoSistema("NomeUsuarioLogado");
                    _usuarioLogado = new UserDataTransfer
                    {
                        Id = int.Parse(GetItemInfoSistema("IDUsuarioLogado").ToString()),
                        Nome = nomeUsuario == null ? string.Empty : nomeUsuario.ToString(),
                        Browser = GetUserBrowser()
                    };
                }
                var codigoPortfolio = GetItemInfoSistema("CodigoPortfolio");
                var codigoCarteira = GetItemInfoSistema("CodigoCarteira");
                _usuarioLogado.CodigoPortfolio = codigoPortfolio == null ? 0 : int.Parse(codigoPortfolio.ToString());
                _usuarioLogado.CodigoEntidade = int.Parse(GetItemInfoSistema("CodigoEntidade").ToString());
                _usuarioLogado.CodigoCarteira = codigoCarteira == null ? 0 : int.Parse(codigoCarteira.ToString());
                return _usuarioLogado;
            }
        }
        public dados CDados
        {
            set;
            get;
            //get
            //{
            //    if (_cDados == null)
            //    {
            //        OrderedDictionary listaParametrosDados = new OrderedDictionary();

            //        listaParametrosDados["RemoteIPUsuario"] = Session["RemoteIPUsuario"] + "";
            //        listaParametrosDados["NomeUsuario"] = Session["NomeUsuario"] + "";

            //        _cDados = CdadosUtil.GetCdados(listaParametrosDados);
            //    }
            //    return _cDados;
            //}        
        }
        private dados _cDados;

        public string TelaResolucao
        {
            get
            {
                return GetItemInfoSistema("ResolucaoCliente").ToString();
            }
        }

        public int TelaAltura
        {
            get
            {
                return TelaResolucao == null ? 0 : int.Parse(TelaResolucao.ToLower().Split('x')[1]);
            }
        }

        public int TelaLargura
        {
            get
            {
                return TelaResolucao == null ? 0 : int.Parse(TelaResolucao.ToLower().Split('x')[0]);
            }
        }

        /// <summary>
        /// 
        /// </summary>    
        public UserBrowser GetUserBrowser()
        {
            string userBrowser = Request.Browser.Browser;

            return userBrowser.Equals("InternetExplorer")
                   ? UserBrowser.InternetExplorer
                   : userBrowser.Equals("Chrome")
                     ? UserBrowser.Chrome
                     : userBrowser.Equals("Firefox")
                       ? UserBrowser.Firefox
                       : UserBrowser.Others;
        }

        /// <summary>
        /// Verificar a autenticação do usuário
        /// </summary>    
        public void VerificarAuth()
        {
            try
            {
                if (Session["infoSistema"] == null)
                {
                    Response.Redirect("~/erros/erroInatividade.aspx");
                }
            }
            catch
            {
                Response.RedirectLocation = CDados.getPathSistema() + "erros/erroInatividade.aspx";
                Response.End();
            }
        }

        /// <summary>
        /// construtor
        /// </summary>
        private UnitOfWorkApplication _uowApplication;

        /// <summary>
        /// UowApplication
        /// </summary>
        public UnitOfWorkApplication UowApplication
        {
            get
            {

                if (_uowApplication == null)
                {
                    string strCon = Cdis.Brisk.Infra.Core.Secutiry.ConnectString.GetStringConexao();
                    _uowApplication = new UnitOfWorkApplication(strCon);
                }
                return _uowApplication;
            }
        }

        /// <summary>
        /// Buscar a string de conexão
        /// </summary>    
        private string GetStringConexao()
        {
            CDIS_Crypto oCrypto = new CDIS_Crypto(CDIS_Crypto.SymmProvEnum.Rijndael);
            try
            {
                string chave = System.Configuration.ConfigurationManager.AppSettings["IDProduto"].ToString();
                string pathDB = System.Configuration.ConfigurationManager.AppSettings["pathDB"].ToString();
                chave = chave.Replace("{", "");
                chave = chave.Replace("-", "");
                chave = chave.Substring(0, 16);

                return oCrypto.descriptografaString(pathDB, chave);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Buscar 
        /// </summary>      
        public object GetItemInfoSistema(string chave)
        {
            if (InfoSistema == null || !InfoSistema.Contains(chave))
                return null;
            else
                return InfoSistema[chave];
        }

        /// <summary>
        /// Buscar 
        /// </summary>      
        public string GetSiglaEntidade()
        {
            object sigla = GetItemInfoSistema("SiglaUnidadeNegocio");
            return sigla == null ? "" : sigla.ToString();
        }

        /// <summary>
        /// Obter o código do idioma atual
        /// </summary>      
        public string GetCurrentLangCode()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
        }
    }

}