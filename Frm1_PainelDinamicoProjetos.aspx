<%@ Page Language="C#" EnableViewState="false"  AutoEventWireup="true" CodeBehind="Frm1_PainelDinamicoProjetos.aspx.cs" Inherits="BriskPtf._Projetos._Projetos_Frm1_PainelDinamicoProjetos" Title="Portal da Estratégia" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Untitled Page</title>
    <script type="text/javascript" src="../scripts/CDIS.js" language="javascript"></script>
    <link href="../estilos/cdisEstilos.css" rel="stylesheet" type="text/css" />   
</head>
<body style="margin:0px">
    <form id="form1" runat="server">
    <div>
    <table>
       
        <tr>
            <td>
            </td>
        </tr>
        <tr>
            <td>   
        <dx:ASPxSplitter ID="spl1" runat="server" Orientation="Vertical" Width="100%" 
            ClientInstanceName="spl1" FullscreenMode="True" Height="100%">
            <Panes>
                <dxtv:SplitterPane AllowResize="False" MaxSize="40px" Size="40px">
                    <PaneStyle Font-Size="15pt" HorizontalAlign="Center">
                        <Paddings PaddingTop="8px" />
                    </PaneStyle>
                    <ContentCollection>
                        <dxtv:SplitterContentControl runat="server">
                            <dxtv:ASPxLabel ID="lblNomeProjeto" runat="server" Font-Size="15pt">
                            </dxtv:ASPxLabel>
                        </dxtv:SplitterContentControl>
                    </ContentCollection>
                </dxtv:SplitterPane>
                <dx:SplitterPane>
                    <Panes>
                        <dx:SplitterPane Size="25%">
                            <ContentCollection>
                                <dx:SplitterContentControl runat="server" SupportsDisabledAttribute="True">
                                    <div style="margin: 0px; width:100%; text-align:center">Desempenho Físico</div>
                                    <div id="chartdiv1" style="WIDTH: 100%; height:100%" align="center">
                                                </div>                                     
                                </dx:SplitterContentControl>
                            </ContentCollection>
                        </dx:SplitterPane>
                        <dx:SplitterPane>
                            <ContentCollection>
                                <dx:SplitterContentControl runat="server" SupportsDisabledAttribute="True">
                                    <div id="chartdiv2" style="WIDTH: 100%; height:100%" align="center">
                                                </div>
                                </dx:SplitterContentControl>
                            </ContentCollection>
                        </dx:SplitterPane>
                    </Panes>
                    <ContentCollection>
<dx:SplitterContentControl runat="server" SupportsDisabledAttribute="True"></dx:SplitterContentControl>
</ContentCollection>
                </dx:SplitterPane>
                <dx:SplitterPane>
                    <ContentCollection>
<dx:SplitterContentControl runat="server" SupportsDisabledAttribute="True">
    <dxtv:ASPxGridView ID="gvEntregas" runat="server" AutoGenerateColumns="False" ClientInstanceName="gvEntregas"  KeyFieldName="CodigoTarefa" Width="100%">
        <Templates>
            <FooterRow>
                <table>
                    <tr>                        
                        <td align="left">
                            <dxtv:ASPxImage ID="ASPxImage3" runat="server" Height="16px" ImageUrl="~/imagens/Verde.gif">
                            </dxtv:ASPxImage>
                        </td>
                        <td align="left" style="padding-left: 3px; padding-right: 10px;">
                            <dxtv:ASPxLabel ID="ASPxLabel4" runat="server"  Text="No Prazo/Adiantado">
                            </dxtv:ASPxLabel>
                        </td>
                        <td align="left">
                            <dxtv:ASPxImage ID="ASPxImage4" runat="server" Height="16px" ImageUrl="~/imagens/Amarelo.gif">
                            </dxtv:ASPxImage>
                        </td>
                        <td align="left" style="padding-left: 3px; padding-right: 10px;">
                            <dxtv:ASPxLabel ID="ASPxLabel5" runat="server"  Text="Tendência de Atraso">
                            </dxtv:ASPxLabel>
                        </td>
                        <td align="left">
                            <dxtv:ASPxImage ID="ASPxImage5" runat="server" Height="16px" ImageUrl="~/imagens/Vermelho.gif">
                            </dxtv:ASPxImage>
                        </td>
                        <td align="left" style="padding-left: 3px; padding-right: 10px">
                            <dxtv:ASPxLabel ID="ASPxLabel6" runat="server"  Text="Atrasado">
                            </dxtv:ASPxLabel>
                        </td>
                    </tr>
                </table>
            </FooterRow>
        </Templates>
        <SettingsPager Mode="ShowAllRecords" Visible="False">
        </SettingsPager>
        <Settings ShowFooter="True" VerticalScrollableHeight="190" VerticalScrollBarMode="Visible" />
        <SettingsCommandButton>
            <ShowAdaptiveDetailButton RenderMode="Image">
            </ShowAdaptiveDetailButton>
            <HideAdaptiveDetailButton RenderMode="Image">
            </HideAdaptiveDetailButton>
        </SettingsCommandButton>
        <Columns>
            <dxtv:GridViewDataTextColumn VisibleIndex="0" Width="40px" Caption=" "
                FieldName="Status" Name="col_Status">
                <PropertiesTextEdit DisplayFormatString="&lt;img src='../imagens/{0}.gif' /&gt;">
                </PropertiesTextEdit>
                <CellStyle HorizontalAlign="Center">
                </CellStyle>
            </dxtv:GridViewDataTextColumn>
            <dxtv:GridViewDataTextColumn Caption="Entrega" ExportWidth="400" FieldName="NomeTarefa" ShowInCustomizationForm="True" VisibleIndex="1">
                <PropertiesTextEdit DisplayFormatString="{0}">
                </PropertiesTextEdit>
                <CellStyle HorizontalAlign="Left">
                </CellStyle>
                <HeaderTemplate>
                    <table>
                        <tr>
                            <td align="left">
                                <dxtv:ASPxLabel ID="ASPxLabel7" runat="server"  Text="Entrega">
                                </dxtv:ASPxLabel>
                            </td>
                            <td align="right" style="margin: 0px;"><%# "Total de Entregas: " + gvEntregas.VisibleRowCount %></td>
                        </tr>
                    </table>
                </HeaderTemplate>
            </dxtv:GridViewDataTextColumn>
            <dxtv:GridViewDataTextColumn Caption="Previsto" ExportWidth="100" FieldName="TerminoPrevisto" ShowInCustomizationForm="True" VisibleIndex="2" Width="85px">
                <PropertiesTextEdit DisplayFormatString="dd/MM/yyyy">
                </PropertiesTextEdit>
                <HeaderStyle HorizontalAlign="Center" />
                <CellStyle HorizontalAlign="Center">
                </CellStyle>
            </dxtv:GridViewDataTextColumn>
            <dxtv:GridViewDataTextColumn Caption="Tendência" ExportWidth="100" FieldName="Termino" ShowInCustomizationForm="True" VisibleIndex="3" Width="85px">
                <PropertiesTextEdit DisplayFormatString="dd/MM/yyyy" EncodeHtml="False">
                </PropertiesTextEdit>
                <HeaderStyle HorizontalAlign="Center" />
                <CellStyle HorizontalAlign="Center">
                </CellStyle>
            </dxtv:GridViewDataTextColumn>
        </Columns>
        <Styles>
            <Header >
            </Header>
            <Footer>
                <Paddings Padding="0px" />
            </Footer>
        </Styles>
    </dxtv:ASPxGridView>
                        </dx:SplitterContentControl>
</ContentCollection>
                </dx:SplitterPane>
            </Panes>
            <ClientSideEvents PaneResizeCompleted="function(s, e) {	
    grafico001.resizeTo('100%', '100%');
    grafico002.resizeTo('100%', '100%');
    gvEntregas.SetHeight(s.GetPane(2).offsetHeight - 30);
}" Init="function(s, e) {
	    getGrafico('../../Flashs/AngularGauge.swf', 'grafico001', '100%', '100%',hfGeral.Get('grafico_xml1'), 'chartdiv1');
        getGrafico('../../Flashs/MSLine.swf', 'grafico002', '100%', '100%', hfGeral.Get('grafico_xml2'), 'chartdiv2');                

}" />
            <Styles>
                <Pane>
                    <Paddings Padding="0px" />
                </Pane>
            </Styles>
        </dx:ASPxSplitter>   
        <dxhf:ASPxHiddenField ID="hfGeral" runat="server" ClientInstanceName="hfGeral">
        </dxhf:ASPxHiddenField>
    
                <dxcp:ASPxGlobalEvents ID="ASPxGlobalEvents1" runat="server">
                    <ClientSideEvents ControlsInitialized="function(s, e) {
	gvEntregas.SetHeight(spl1.GetPane(2).offsetHeight - 30);
}" />
                </dxcp:ASPxGlobalEvents>
    
            </td>
        </tr>
    </table>
</div>
        </form>
    </body>
    </html>

