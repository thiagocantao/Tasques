<%@ Page Language="C#" MasterPageFile="~/novaCdis.master" AutoEventWireup="true" CodeBehind="PainelDinamicoProjetos.aspx.cs" Inherits="BriskPtf._Projetos._Projetos_PainelDinamicoProjetos" Title="Portal da Estratégia" %>
<%@ MasterType VirtualPath="~/novaCdis.master"   %>
<asp:Content ID="Content1" ContentPlaceHolderID="AreaTrabalho" Runat="Server">
    <script type="text/javascript" language="JavaScript"> 
    
</script>

    <table>
        <tr>
            <td>
                <table border="0" cellpadding="0" cellspacing="0" style="background-image: url(../imagens/titulo/back_Titulo_Desktop.gif);
                    width: 100%">
                    <tr style="height:26px">
                        <td valign="middle" style="padding-left: 10px">
                            <asp:Label ID="lblTituloTela" runat="server" EnableViewState="False" Font-Bold="True"
                                Font-Overline="False" Font-Strikeout="False"
                                Text="<%$ Resources:traducao, PainelDinamicoProjetos_painel_de_projetos_e_metas_por_carteira %>"></asp:Label></td>
                        <td valign="middle" style="padding-left: 10px" align="right">
                            <dxcp:ASPxImage ID="ASPxImage1" runat="server" Cursor="pointer" ImageUrl="~/imagens/botoes/btnEngrenagem.png" ShowLoadingImage="True" ToolTip="Abrir configurações do painel">
                                <ClientSideEvents Click="function(s, e) {
	pcConfiguracoes.Show();
}" />
                            </dxcp:ASPxImage>
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
                <iframe frameborder="0" name="frmVC" scrolling="auto" src=""
                    width="100%" style="height: <%=alturaTela %>" id="frmVC"></iframe>
                
            </td>
        </tr>
    </table>
    <dxcp:ASPxPopupControl ID="pcConfiguracoes" runat="server" ClientInstanceName="pcConfiguracoes"  HeaderText="Configurar" Modal="True" PopupHorizontalAlign="WindowCenter" PopupVerticalAlign="WindowCenter" ShowOnPageLoad="True">
                <ContentCollection>
<dxcp:PopupControlContentControl runat="server">
    <table cellpadding="0" cellspacing="0" class="auto-style1">
        <tr>
            <td>
                <dxtv:ASPxLabel ID="lblCarteira" runat="server" ClientInstanceName="lblCarteira" EnableViewState="False"  Text="Carteira de Projetos:">
                </dxtv:ASPxLabel>
            </td>
        </tr>
        <tr>
            <td style="padding-bottom: 10px">
                <dxtv:ASPxComboBox ID="ddlCarteirasProjetos" runat="server" ClientInstanceName="ddlCarteirasProjetos"  Width="100%" DropDownStyle="DropDown">
                    <ClientSideEvents SelectedIndexChanged="function(s, e) {
	gvDados.PerformCallback(s.GetValue());
}" />
                </dxtv:ASPxComboBox>
            </td>
        </tr>
        <tr>
            <td style="padding-bottom: 10px">
                <dxtv:ASPxRoundPanel ID="ASPxRoundPanel1" runat="server"  HeaderText="Configuração do Tempo de Execução de Cada Painel" ShowCollapseButton="True" Width="100%">
                    <ContentPaddings Padding="3px" />
                    <PanelCollection>
                        <dxtv:PanelContent runat="server">
                            <table cellpadding="0" cellspacing="0" class="auto-style1">
                                <tr>
                                    <td>
                                        <dxtv:ASPxLabel ID="lblCarteira0" runat="server" ClientInstanceName="lblCarteira" ClientVisible="False" EnableViewState="False"  Text="Tempo (em segundos):">
                                        </dxtv:ASPxLabel>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td style="padding-right: 10px">
                                        <dxtv:ASPxSpinEdit ID="txtTempo" runat="server" AllowMouseWheel="False" ClientInstanceName="txtTempo"  Number="0" NumberType="Integer" Width="135px">
                                            <SpinButtons ClientVisible="False" Enabled="False" ShowIncrementButtons="False">
                                            </SpinButtons>
                                            <ClientSideEvents Validation="function(s, e) {
	if(s.GetValue() &lt;= 0)
	{
		e.isValid = false;
		e.errorText = traducao.PainelDinamicoProjetos_o_tempo_de_execu__o_de_um_painel_n_o_pode_ser_inferior_a_0__zero__;
}
}" />
                                            <ValidationSettings Display="Dynamic" ErrorDisplayMode="ImageWithTooltip" ValidationGroup="MKE">
                                            </ValidationSettings>
                                        </dxtv:ASPxSpinEdit>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                            </table>
                        </dxtv:PanelContent>
                    </PanelCollection>
                </dxtv:ASPxRoundPanel>
            </td>
        </tr>
        <tr>
            <td style="padding-bottom: 10px">
                <dxtv:ASPxGridView ID="gvDados" runat="server" AutoGenerateColumns="False" ClientInstanceName="gvDados"  KeyFieldName="CodigoProjeto" Width="800px" OnCustomCallback="gvDados_CustomCallback">
                    <ClientSideEvents EndCallback="function(s, e) {
vrfHabilitacaoBtnOk();
}" />
                    <Settings VerticalScrollableHeight="180" VerticalScrollBarMode="Auto" />
                    <SettingsBehavior AllowSelectByRowClick="True" AllowSelectSingleRowOnly="True" />
                    <SettingsCommandButton>
                        <ShowAdaptiveDetailButton RenderMode="Image">
                        </ShowAdaptiveDetailButton>
                        <HideAdaptiveDetailButton RenderMode="Image">
                        </HideAdaptiveDetailButton>
                    </SettingsCommandButton>
                    <Columns>
                        <dxtv:GridViewDataTextColumn Caption="Nome do Projeto" FieldName="NomeProjeto" ShowInCustomizationForm="True" VisibleIndex="1">
                        </dxtv:GridViewDataTextColumn>
                        <dxtv:GridViewCommandColumn Caption="Inicial" ShowInCustomizationForm="True" ShowSelectCheckbox="True" VisibleIndex="0" Width="55px">
                            <HeaderStyle HorizontalAlign="Center" />
                            <CellStyle HorizontalAlign="Center">
                            </CellStyle>
                        </dxtv:GridViewCommandColumn>
                    </Columns>
                </dxtv:ASPxGridView>
            </td>
        </tr>
        <tr>
            <td align="right">
                <dxtv:ASPxCallback ID="callBack" runat="server" ClientInstanceName="callBack" OnCallback="callBack_Callback">
                </dxtv:ASPxCallback>
                <table>
                    <tr>
                        <td>
                            <dxtv:ASPxButton ID="btnOk" runat="server" AutoPostBack="False"  Text="Iniciar Apresentação" ValidationGroup="MKE" Width="145px" ClientInstanceName="btnOk" EnableClientSideAPI="True">
                                <ClientSideEvents Click="function(s, e) {
	executaEventoFecharConfiguracoes();
}" />
                            </dxtv:ASPxButton>
                        </td>
                        <td style="padding-left: 10px">
                            <dxtv:ASPxButton ID="btnFechar" runat="server" AutoPostBack="False"  Text="Fechar" ValidationGroup="MKE" Width="140px">
                                <ClientSideEvents Click="function(s, e) {
	pcConfiguracoes.Hide();
}" />
                            </dxtv:ASPxButton>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
                    </dxcp:PopupControlContentControl>
</ContentCollection>
                </dxcp:ASPxPopupControl>
   
</asp:Content>

<asp:Content ID="Content2" runat="server" contentplaceholderid="HeadContent">
</asp:Content>


