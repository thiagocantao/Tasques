<%@ Page Language="C#" MasterPageFile="~/novaCdis.master" EnableViewState="false" AutoEventWireup="true" CodeBehind="visaoCorporativa.aspx.cs" Inherits="BriskPtf._Projetos._Projetos_visaoCorporativa" Title="Portal da Estratégia" %>
<%@ MasterType VirtualPath="~/novaCdis.master"   %>
<asp:Content ID="Content1" ContentPlaceHolderID="AreaTrabalho" Runat="Server">
    <script type="text/javascript" language="JavaScript"> 
    var refreshinterval=600; 
    var starttime; 
    var nowtime; 
    var reloadseconds=0; 
    var secondssinceloaded=0; 
    
    function starttime() 
    { 
        starttime=new Date(); 
        starttime=starttime.getTime(); 
        countdown(); 
    } 
 
    function countdown() 
    { 
        nowtime= new Date(); 
        nowtime=nowtime.getTime(); 
        secondssinceloaded=(nowtime-starttime)/1000; 
        reloadseconds=Math.round(refreshinterval-secondssinceloaded); 
        if (refreshinterval>=secondssinceloaded) 
        { 
            var timer=setTimeout("countdown()",1000);         
            
        } 
        else 
        { 
            clearTimeout(timer); 
            window.location.reload(true); 
        } 
    }
    window.onload = starttime;

    function verificaVisao() {
        var tipoVisao = ddlOpcaoVisao.GetValue();

        if (tipoVisao == "0") {
            document.getElementById('frmVC').src = hfVisaoCorporativa.Get('UrlVC');
            document.getElementById("tdFin1").style.display = "";
            document.getElementById("tdFin2").style.display = "";
            document.getElementById("tdBtn").style.display = "";
        }
        else {

            document.getElementById("tdFin1").style.display = "none";
            document.getElementById("tdFin2").style.display = "none";
            document.getElementById("tdBtn").style.display = "none";

            if (tipoVisao == "1") {
                document.getElementById('frmVC').src = '../_Public/Gantt/paginas/projetometa/Default.aspx'
                    //'./VisaoCorporativa/vc_gantt.aspx';
            }
            else
            {
               document.getElementById('frmVC').src = '../_Portfolios/VisaoMetas/visaoMetas_01.aspx';
            }
        }
    }
    
</script>

    <table cellpadding="0" cellspacing="0" style="width: 100%">
        <tr>
            <td>
                <dxcb:ASPxCallback ID="callBackVC" runat="server" ClientInstanceName="callBackVC"
                    OnCallback="callBackVC_Callback" EnableViewState="False">
                    <ClientSideEvents EndCallback="function(s, e) {
	if(statusVC != 1)
	{
		document.getElementById('frmVC').src = '../_Public/Gantt/paginas/projetometa/Default.aspx';
                        //'./VisaoCorporativa/vc_gantt.aspx';
	}
	else
	{
		document.getElementById('frmVC').src = s.cp_UrlVC;
	}
}" />
                </dxcb:ASPxCallback>
                <dxhf:ASPxHiddenField ID="hfVisaoCorporativa" runat="server" 
                    ClientInstanceName="hfVisaoCorporativa">
                </dxhf:ASPxHiddenField>
                
                <dxlp:ASPxLoadingPanel ID="lp1" runat="server" ClientInstanceName="lp1" 
                    HorizontalAlign="Center" Modal="True" Text="" VerticalAlign="Middle">
                    
                    <Image Url="~/imagens/carregando.gif">
                    </Image>
                </dxlp:ASPxLoadingPanel>
                
            </td>
        </tr>
        <tr>
            <td>
                <table border="0" cellpadding="0" cellspacing="0" style="background-image: url(../imagens/titulo/back_Titulo_Desktop.gif);
                    width: 100%">
                    <tr style="height:26px">
                        <td valign="middle" style="padding-left: 10px">
                            <table cellpadding="0" cellspacing="0">
                                <tr>
                                    <td style="padding-right: 10px;">
                            <asp:Label ID="lblTituloTela" runat="server" EnableViewState="False" Font-Bold="True"
                                Font-Overline="False" Font-Strikeout="False"
                                Text="Painel de Bordo"></asp:Label>
                                    </td>
                                    <td>
                            <dxe:ASPxComboBox ID="ddlOpcaoVisao" runat="server" ClientInstanceName="ddlOpcaoVisao"
                                SelectedIndex="0" 
                                ShowImageInEditBox="True" Width="210px" EnableViewState="False">
                                <Items>
                                    <dxe:ListEditItem ImageUrl="~/imagens/graficos.PNG" Selected="True" Text="Monitor de Projetos"
                                        Value="0" />
                                    <dxe:ListEditItem ImageUrl="~/imagens/botoes/btnGantt.png" Text="Gantt de Projetos" Value="1" />
                                    <dxe:ListEditItem ImageUrl="~/imagens/meta.PNG" Text="Metas de Projetos" Value="2" />
                                </Items>
                                <ClientSideEvents SelectedIndexChanged="function(s, e) {
	verificaVisao();
}" Init="function(s, e) {
	verificaVisao();
}" />
                               
                            </dxe:ASPxComboBox>
                                    </td>
                                </tr>
                            </table>
                        </td>
                         <td align="right" style="width: 70px; " id="tdFin1">
                            <dxe:ASPxLabel ID="lblFinanceiro" runat="server"
                                Text="Financeiro:" EnableViewState="False"></dxe:ASPxLabel>
                        </td>
                         <td align="right" style="width: 100px; padding-right: 10px;" id="tdFin2">
                            <dxe:ASPxComboBox ID="ddlFinanceiro" runat="server" 
                                 ClientInstanceName="ddlFinanceiro" Width="100%" EnableViewState="False" 
                                 SelectedIndex="0">
                                <Items>
                                    <dxe:ListEditItem Selected="True" Text="Todos" Value="T" />
                                    <dxe:ListEditItem Text="Ano Atual" Value="A" />
                                </Items>
                             </dxe:ASPxComboBox>
                        </td>
                         <td align="right" style="width: 55px; display: none;">
                            <dxe:ASPxLabel ID="ASPxLabel1" runat="server"
                                Text="Status:" visible="False" EnableViewState="False"></dxe:ASPxLabel>
                        </td>
                        <td align="left" style="width: 145px; display: none;">
                            <dxe:ASPxComboBox ID="ddlStatus" runat="server" ClientInstanceName="ddlStatus" Width="145px" visible="False" EnableViewState="False"></dxe:ASPxComboBox>
                        </td>
                        <td align="left" style="width: 10px; display: none;">
                            &nbsp;</td>
                        <td align="left" style="width: 80px; " id="tdBtn">
                            <dxe:ASPxButton ID="btnSelecionar" runat="server" AutoPostBack="False" Text="Selecionar" EnableViewState="False">
<Paddings Padding="0px"></Paddings>

<ClientSideEvents Click="function(s, e) 
{
	callBackVC.PerformCallback('AtualizarVC');
}"></ClientSideEvents>
</dxe:ASPxButton>
                        </td>
                        <td style="width: 8px; padding-right: 5px;">
                                <asp:ImageButton ID="imgExportaParaPDF" runat="server" 
                                    ImageUrl="~/imagens/botoes/btnPdf.png" OnClick="ImageButton1_Click" 
                                    AlternateText="Imprimir Painel" ToolTip="Imprimir painel" />
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td style="height: 2px">
            </td>
        </tr>
        <tr>
            <td>
                <iframe frameborder="0" name="graficos" scrolling="auto" src=""
                    width="100%" style="height: <%=alturaTela %> !important" id="frmVC"></iframe>
            </td>
        </tr>
    </table>
</asp:Content>

