<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Frm2_PainelDinamicoProjetos.aspx.cs" Inherits="BriskPtf._Projetos._Projetos_Frm2_PainelDinamicoProjetos" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Untitled Page</title>
    <script type="text/javascript" language="javascript" src="../../scripts/CDIS.js"></script>
    <link href="../../estilos/cdisEstilos.css" rel="stylesheet" type="text/css" />
    <style type="text/css">
        .style3
        {
            width: 100px;
        }
        .style4
        {
            width: 10px;
            height: 10px;
        }
        .lk
        {
            font-weight: normal;
            text-decoration: underline;
            cursor: pointer;
            color: #0E008C;
        }
        </style>
</head>
<body style="margin: 0px">
    <form id="form1" enableviewstate="false" runat="server">
    <table cellpadding="0" cellspacing="0" width="100%">
        <tr>
            <td class="style4">
                &nbsp;
            </td>
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
                <dx:SplitterPane AllowResize="False" ScrollBars="Auto">
                    <Panes>
                        <dxtv:SplitterPane AllowResize="False">
                            <ContentCollection>
                                <dxtv:SplitterContentControl runat="server">
                                    <dxtv:ASPxNavBar ID="nb01" runat="server" EnableViewState="False" EncodeHtml="False"  GroupSpacing="4px" Width="100%">
                                        <Paddings PaddingLeft="0px" PaddingRight="0px" />
                                        <GroupHeaderStyle Wrap="True">
                                            <Paddings Padding="2px" PaddingLeft="10px" />
                                        </GroupHeaderStyle>
                                        <GroupContentStyle>
                                            <Paddings PaddingBottom="1px" PaddingTop="1px" />
                                        </GroupContentStyle>
                                        <ItemStyle BackColor="White" />
                                        <DisabledStyle ForeColor="Black">
                                        </DisabledStyle>
                                    </dxtv:ASPxNavBar>
                                </dxtv:SplitterContentControl>
                            </ContentCollection>
                        </dxtv:SplitterPane>
                        <dxtv:SplitterPane AllowResize="False">
                            <ContentCollection>
                                <dxtv:SplitterContentControl runat="server">
                                    <dxtv:ASPxNavBar ID="nb2" runat="server" EnableViewState="False" EncodeHtml="False"  GroupSpacing="4px" Width="100%">
                                        <Paddings PaddingLeft="0px" PaddingRight="0px" />
                                        <GroupHeaderStyle Wrap="True">
                                            <Paddings Padding="2px" PaddingLeft="10px" />
                                        </GroupHeaderStyle>
                                        <GroupContentStyle>
                                            <Paddings PaddingBottom="1px" PaddingTop="1px" />
                                        </GroupContentStyle>
                                        <ItemStyle BackColor="White" />
                                        <DisabledStyle ForeColor="Black">
                                        </DisabledStyle>
                                    </dxtv:ASPxNavBar>
                                </dxtv:SplitterContentControl>
                            </ContentCollection>
                        </dxtv:SplitterPane>
                    </Panes>
                    <ContentCollection>
<dx:SplitterContentControl runat="server" SupportsDisabledAttribute="True">
                        </dx:SplitterContentControl>
</ContentCollection>
                </dx:SplitterPane>
            </Panes>
            
            <Styles>
                <Pane>
                    <Paddings Padding="0px" />
                </Pane>
            </Styles>
        </dx:ASPxSplitter>   

    <dxcp:ASPxPopupControl runat="server" PopupHorizontalAlign="WindowCenter" PopupVerticalAlign="WindowCenter" CloseAction="CloseButton" HeaderText=" " ShowHeader="False" Width="272px" Height="27px" ID="popUpStatusTela">
<ContentStyle HorizontalAlign="Center">
<Paddings PaddingTop="15px" PaddingBottom="15px"></Paddings>
</ContentStyle>
<ContentCollection>
<dxcp:PopupControlContentControl runat="server">
                <dxcp:ASPxLabel runat="server" Text="Nenhuma informa&#231;&#227;o a ser apresentada." Font-Bold="False" Font-Italic="False"  ID="ASPxLabel1"></dxcp:ASPxLabel>

            </dxcp:PopupControlContentControl>
</ContentCollection>
</dxcp:ASPxPopupControl>

            </td>
        </tr>
    </table>
    </form>
</body>
</html>
