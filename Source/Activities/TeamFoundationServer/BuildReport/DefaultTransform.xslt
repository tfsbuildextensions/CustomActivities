<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:template match="/">
        <html>
            <style>
                th, td{text-align:left;padding:1px;border:1px solid #fff;}
                th {background:#328aa4;color:#fff;}
                td {background:#e5f1f4;}
                h2 { 	font-weight: normal;font-size: 20px;
                font-family: Sans-Serif, "Lucida Sans Unicode", "Lucida Grande"; }
                h3 {	font-weight: normal;font-size: 16px;
                font-family: Sans-Serif, "Lucida Sans Unicode", "Lucida Grande"; }

                #buildreport
                {
                font-family: "Lucida Sans Unicode", "Lucida Grande", Sans-Serif;
                font-size: 12px;
                margin: 15px;
                width: 90%;
                border-collapse: collapse;
                margin: 0 auto;
                margin-left: auto;
                margin-right: auto;
                }

                #buildreport th
                {
                font-size: 13px;
                font-weight: normal;
                padding: 1px;
                background: #b9c9fe;
                border-top: 4px solid #99CCCC;
                border-bottom: 1px solid #fff;
                color: #039;
                }

                #buildreport td
                {
                padding: 2px;
                background: #e8edff;
                border-bottom: 1px solid #fff;
                color: #669;
                border-top: 1px solid transparent;
                }
            </style>
            <body>
                <h2>
                    Build Report: <xsl:value-of select="/buildreport/@buildName"/> (<xsl:value-of select="/buildreport/@buildNumber"/>)
                </h2>
                <table border="0" id="buildreport">
                    <tr>
                        <th>Team Project</th>
                        <th>Compilation Status</th>
                        <th>Test Status</th>
                        <th>Build Reason</th>
                    </tr>
                    <tr align="middle">
                        <td>
                            <xsl:value-of select="/buildreport/@teamProject" />
                        </td>
                        <td>
                            <xsl:value-of select="/buildreport/@compilationStatus" />
                        </td>
                        <td>
                            <xsl:value-of select="/buildreport/@testStatus" />
                        </td>
                        <td>
                            <xsl:value-of select="/buildreport/@buildReason" />
                        </td>
                    </tr>
                    <tr>
                        <th>Requested By</th>
                        <th>Requested For</th>
                        <th>Start Time</th>
                        <th>Report Time</th>
                    </tr>
                    <tr align="middle">
                        <td>
                            <xsl:value-of select="/buildreport/@requestedBy" />
                        </td>
                        <td>
                            <xsl:value-of select="/buildreport/@requestedFor" />
                        </td>
                        <td>
                            <xsl:value-of select="/buildreport/@startTime" />
                        </td>
                        <td>
                            <xsl:value-of select="/buildreport/@reportTime" />
                        </td>
                    </tr>
                    <tr>
                        <th>Build Agent</th>
                        <th>Build Agent Uri</th>
                        <th>Build Controller</th>
                        <th>Source Get Version</th>
                    </tr>
                    <tr align="middle">
                        <td>
                            <xsl:value-of select="/buildreport/@buildAgent" />
                        </td>
                        <td>
                            <xsl:value-of select="/buildreport/@buildAgentUri" />
                        </td>
                        <td>
                            <xsl:value-of select="/buildreport/@buildController" />
                        </td>
                        <td>
                            <xsl:value-of select="/buildreport/@sourceGetVersion" />
                        </td>
                    </tr>
                    <tr>
                        <th colspan="2">Drop Location</th>
                        <th colspan="2">Log Location</th>
                    </tr>
                    <tr align="middle">
                        <td colspan="2">
                            <xsl:value-of select="/buildreport/@dropLocation" />
                        </td>
                        <td colspan="2">
                            <xsl:value-of select="/buildreport/@logLocation" />
                        </td>
                    </tr>
                </table>
                <h3>
                    Changesets (<xsl:value-of select="/buildreport/Changesets/@count"/>)
                </h3>
                <table id="buildreport">
                    <tr>
                        <th>Id</th>
                        <th>Committer</th>
                        <th>Owner</th>
                        <th>Comment</th>
                    </tr>
                    <xsl:for-each select="/buildreport/Changesets/changeset">
                        <xsl:variable name="cid" select="@id" />
                        <tr align="middle">
                            <td>
                                <xsl:value-of select="@id" />
                            </td>
                            <td>
                                <xsl:value-of select="@committer" />
                            </td>
                            <td>
                                <xsl:value-of select="@owner" />
                            </td>
                            <td>
                                <xsl:value-of select="." />
                            </td>
                        </tr>
                        <tr>
                            <td/>
                            <td/>
                            <td/>
                            <td colspan="3">
                                <Table id="buildreport">
                                    <tr>
                                        <th colspan="5">
                                            Work Items (<xsl:value-of select="/buildreport/Changesets/changeset[@id = $cid]/WorkItems/@count" />)
                                        </th>
                                    </tr>
                                    <tr>
                                        <td>
                                            <b>Type</b>
                                        </td>
                                        <td>
                                            <b>Id</b>
                                        </td>
                                        <td>
                                            <b>Title</b>
                                        </td>
                                        <td>
                                            <b>State</b>
                                        </td>
                                        <td>
                                            <b>Reason</b>
                                        </td>
                                    </tr>
                                    <xsl:for-each select="/buildreport/Changesets/changeset[@id = $cid]/WorkItems/workitem">
                                        <tr align="middle">
                                            <td>
                                                <xsl:value-of select="@type" />
                                            </td>
                                            <td>
                                                <xsl:value-of select="@id" />
                                            </td>
                                            <td>
                                                <xsl:value-of select="@title" />
                                            </td>
                                            <td>
                                                <xsl:value-of select="@state" />
                                            </td>
                                            <td>
                                                <xsl:value-of select="@reason" />
                                            </td>
                                        </tr>
                                    </xsl:for-each>
                                </Table>
                            </td>
                        </tr>
                        <tr>
                            <td/>
                            <td/>
                            <td/>
                            <td colspan="3">
                                <Table id="buildreport">
                                    <tr>
                                        <th colspan="2">
                                            Files (<xsl:value-of select="/buildreport/Changesets/changeset[@id = $cid]/Files/@count" />)
                                        </th>
                                    </tr>
                                    <tr>
                                        <td>
                                            <b>Type</b>
                                        </td>
                                        <td>
                                            <b>Name</b>
                                        </td>
                                    </tr>
                                    <xsl:for-each select="/buildreport/Changesets/changeset[@id = $cid]/Files/file">
                                        <tr align="middle">
                                            <td>
                                                <xsl:value-of select="@change" />
                                            </td>
                                            <td>
                                                <xsl:value-of select="@name" />
                                            </td>
                                        </tr>
                                    </xsl:for-each>
                                </Table>
                            </td>
                        </tr>
                    </xsl:for-each>
                </table>
                <h3>
                    Output Files (<xsl:value-of select="/buildreport/OutputFiles/@count"/>)
                </h3>
                <table id="buildreport">
                    <tr>
                        <th>Name</th>
                        <th>Created</th>
                        <th>Size</th>
                    </tr>
                    <xsl:for-each select="/buildreport/OutputFiles/file">
                        <tr>
                            <td>
                                <xsl:value-of select="." />
                            </td>
                            <td>
                                <xsl:value-of select="@creationTimeUtc" />
                            </td>
                            <td>
                                <xsl:value-of select="@size" />
                            </td>
                        </tr>
                    </xsl:for-each>
                </table>
            </body>
        </html>
    </xsl:template>
</xsl:stylesheet>