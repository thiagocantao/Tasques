<%@ Page Language="C#" MasterPageFile="~/novaCdis.master" AutoEventWireup="true" CodeBehind="visaoDemandas.aspx.cs" Inherits="BriskPtf._Projetos._Projetos_visaoDemandas" Title="Portal da Estratégia" %>
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
</script>
    <table>
        <tr>
            <td>
            </td>
        </tr>
        <tr>
            <td>
                <table border="0" cellpadding="0" cellspacing="0" style="background-image: url(../imagens/titulo/back_Titulo_Desktop.gif);
                    width: 100%">
                    <tr style="height:26px">
                        <td valign="middle" style="width: 165px">
                            &nbsp;
                            <asp:Label ID="lblTitulo" runat="server" EnableViewState="False" Font-Bold="True"
                                Font-Overline="False" Font-Strikeout="False"
                                Text="Monitor de Demandas"></asp:Label></td>
                        <td valign="middle">
                            <dxe:ASPxComboBox ID="ddlOpcaoVisao" runat="server" ClientInstanceName="ddlOpcaoVisao"
                                 SelectedIndex="0" ShowImageInEditBox="True"
                                ValueType="System.String" Width="123px" ClientVisible="False">
                                <Items>
                                    <dxe:ListEditItem ImageUrl="~/imagens/graficos.PNG" Selected="True" Text="Gr&#225;ficos"
                                        Value="0" />
                                    <dxe:ListEditItem ImageUrl="~/imagens/olap.PNG" Text="Tabela" Value="1" />
                                </Items>
                                <ClientSideEvents SelectedIndexChanged="function(s, e) {
	verificaVisao();
}" />
                            </dxe:ASPxComboBox>
                        </td>
                        <td style="width: 5px">
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td>
            </td>
        </tr>
        <tr>
            <td>
                <iframe frameborder="0" name="graficos" scrolling="no" src="./VisaoDemandas/visaoDemandas_01.aspx"
                    width="100%" style="height: <%=alturaTela %>" id="frmVC"></iframe>
            </td>
        </tr>
    </table>
</asp:Content>
