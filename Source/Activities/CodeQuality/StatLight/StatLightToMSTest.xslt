<?xml version="1.0" encoding="UTF-8" ?>
<xsl:transform version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output indent="yes" />
  <xsl:param name="runUser"></xsl:param>
  <xsl:param name="machineName"></xsl:param>

  <xsl:variable name="dateRun" select="//StatLightTestResults/@dateRun"/>

  <xsl:variable name="datePart" select="substring-before(//StatLightTestResults/@dateRun, ' ')"/>
  <xsl:variable name="timePart" select="substring-after(//StatLightTestResults/@dateRun, ' ')"/>

  <xsl:variable name="guidStub">
    <xsl:call-template name="testRunGuid">
      <xsl:with-param name="date" select="$datePart"/>
      <xsl:with-param name="time" select="$timePart"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:template match="/">
    <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">

      <xsl:variable name="pass_count" select="//StatLightTestResults/@total - //StatLightTestResults/@failed" />
      <xsl:variable name="failed_count" select="StatLightTestResults/@failed"/>
      <xsl:variable name="total_count" select="//StatLightTestResults/@total"/>
      <xsl:variable name="notExecuted_count" select="//StatLightTestResults/@ignored"/>
      <xsl:variable name="storage" select="//tests/@xapFileName"/>

      <xsl:attribute name="id">
        <xsl:value-of select="concat($guidStub,'30db1d215203')"/>
      </xsl:attribute>
      <xsl:attribute name="runUser">
        <xsl:value-of select="concat($machineName,'\',$runUser)"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="concat($runUser,'@',$machineName,' ',$dateRun)"/>
      </xsl:attribute>

      <TestSettings name="Default Test Settings" id="8dfb34aa-91bc-45e3-8609-d0a4e732d982">
        <Deployment enabled="false">
          <xsl:attribute name="runDeploymentRoot">
            <xsl:value-of select="concat($runUser,'_',$machineName,' ',translate($dateRun,':','_') )" />
          </xsl:attribute>
        </Deployment>
        <Execution>
          <TestTypeSpecific />
          <AgentRule name="Execution Agents">
          </AgentRule>
        </Execution>
      </TestSettings>

      <Times>
        <xsl:attribute name="creation">
          <xsl:value-of select="$dateRun"/>
        </xsl:attribute>
        <xsl:attribute name="queuing">
          <xsl:value-of select="$dateRun"/>
        </xsl:attribute>
        <xsl:attribute name="start">
          <xsl:value-of select="$dateRun"/>
        </xsl:attribute>
        <xsl:attribute name="finish">
          <xsl:value-of select="$dateRun"/>
        </xsl:attribute>
      </Times>

      <ResultSummary>
        <xsl:attribute name="outcome">
          <xsl:choose>
            <xsl:when test="$failed_count &gt; 0">Failed</xsl:when>
            <xsl:otherwise>Completed</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>

        <Counters error="0" timeout="0" aborted="0" passedButRunAborted="0" notRunnable="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" inconclusive="0">
          <xsl:attribute name="total">
            <xsl:value-of select="$total_count"/>
          </xsl:attribute>
          <xsl:attribute name="executed">
            <xsl:value-of select="$total_count - $notExecuted_count"/>
          </xsl:attribute>
          <xsl:attribute name="notExecuted">
            <xsl:value-of select="$notExecuted_count"/>
          </xsl:attribute>
          <xsl:attribute name="passed">
            <xsl:value-of select="$pass_count"/>
          </xsl:attribute>
          <xsl:attribute name="failed">
            <xsl:value-of select="$failed_count"/>
          </xsl:attribute>
        </Counters>

      </ResultSummary>

      <TestDefinitions>
        <xsl:for-each select="//test">
          <xsl:variable name="testName">
            <xsl:value-of select="@name"/>
          </xsl:variable>
          <xsl:variable name="pos" select="position()" />

          <UnitTest>
            <xsl:attribute name="name">
              <xsl:call-template name="getTestMethodName">
                <xsl:with-param name="testName" select="$testName"/>
              </xsl:call-template>
            </xsl:attribute>
            <xsl:attribute name="storage">
              <xsl:value-of select="$storage"/>
            </xsl:attribute>
            <xsl:attribute name="id">
              <xsl:call-template name="testIdGuid">
                <xsl:with-param name="value" select="$pos"/>
              </xsl:call-template>
            </xsl:attribute>

            <Execution>
              <xsl:attribute name="id">
                <xsl:call-template name="executionIdGuid">
                  <xsl:with-param name="value" select="$pos"/>
                </xsl:call-template>
              </xsl:attribute>
            </Execution>

            <TestMethod adapterTypeName="Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" >
              <xsl:attribute name="name">
                <xsl:call-template name="getTestMethodName">
                  <xsl:with-param name="testName" select="$testName"/>
                </xsl:call-template>
              </xsl:attribute>
              <xsl:attribute name="codeBase">
                <xsl:value-of select="$storage"/>
              </xsl:attribute>
              <xsl:attribute name="className">
                <xsl:variable name="testClassName">
                  <xsl:call-template name="getTestClassName">
                    <xsl:with-param name="testName" select="$testName"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:value-of select="$testClassName" />
              </xsl:attribute>
            </TestMethod>

          </UnitTest>
        </xsl:for-each>

      </TestDefinitions>

      <TestLists>
        <TestList name="Results Not in a List" id="8c84fa94-04c1-424b-9868-57a2d4851a1d" />
        <TestList name="All Loaded Results" id="19431567-8539-422a-85d7-44ee4e166bda" />
      </TestLists>

      <TestEntries>
        <xsl:for-each select="//test">
          <xsl:variable name="pos" select="position()" />
          <TestEntry testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d">
            <xsl:attribute name="testId">
              <xsl:call-template name="testIdGuid">
                <xsl:with-param name="value" select="$pos"/>
              </xsl:call-template>
            </xsl:attribute>
            <xsl:attribute name="executionId">
              <xsl:call-template name="executionIdGuid">
                <xsl:with-param name="value" select="$pos"/>
              </xsl:call-template>
            </xsl:attribute>
          </TestEntry>
        </xsl:for-each>
      </TestEntries>

      <Results>
        <xsl:for-each select="//test">
          <xsl:variable name="testName">
            <xsl:call-template name="getTestMethodName">
              <xsl:with-param name="testName" select="@name"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="pos" select="position()" />

          <UnitTestResult startTime="2008-01-01T00:00:01.0000000+10:00" endTime="2008-01-01T00:00:02.0000000+10:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d">
            <xsl:attribute name="testName">
              <xsl:value-of select="$testName"/>
            </xsl:attribute>
            <xsl:attribute name="computerName">
              <xsl:value-of select="$machineName"/>
            </xsl:attribute>
            <xsl:attribute name="duration">
              <xsl:value-of select="@timeToComplete"/>
            </xsl:attribute>
            <xsl:attribute name="testId">
              <xsl:call-template name="testIdGuid">
                <xsl:with-param name="value" select="$pos"/>
              </xsl:call-template>
            </xsl:attribute>
            <xsl:attribute name="executionId">
              <xsl:call-template name="executionIdGuid">
                <xsl:with-param name="value" select="$pos"/>
              </xsl:call-template>
            </xsl:attribute>
            <xsl:attribute name="outcome">
              <xsl:value-of select="@resulttype"/>
            </xsl:attribute>
            <xsl:if test="@resulttype='Failed'">
              <Output>
                <ErrorInfo>
                  <Message>
                    <xsl:value-of select="exceptionInfo/message"/>
                  </Message>
                  <StackTrace>
                    <xsl:value-of select="exceptionInfo/stackTrace"/>
                  </StackTrace>
                </ErrorInfo>
              </Output>
            </xsl:if>
          </UnitTestResult>
        </xsl:for-each>
      </Results>
    </TestRun>
  </xsl:template>

  <xsl:template name="substring-after-last">
    <xsl:param name="string" />
    <xsl:param name="delimiter" />
    <xsl:choose>
      <xsl:when test="contains($string, $delimiter)">
        <xsl:call-template name="substring-after-last">
          <xsl:with-param name="string" select="substring-after($string, $delimiter)" />
          <xsl:with-param name="delimiter" select="$delimiter" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$string" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="getTestClassName">
    <xsl:param name="testName" />

    <xsl:variable name="MethodName">
      <xsl:call-template name="substring-after-last">
        <xsl:with-param name="string" select="$testName" />
        <xsl:with-param name="delimiter" select="'.'" />
      </xsl:call-template>
    </xsl:variable>
    <!-- Now get the class name, i.e. everything before the method name. Trim to 255 characters at the same time. -->
    <xsl:value-of select="substring(substring-before($testName, concat('.', $MethodName)), 0, 255)" />
  </xsl:template>

  <xsl:template name="getTestMethodName">
    <xsl:param name="testName" />

    <xsl:variable name="MethodName">
      <xsl:call-template name="substring-after-last">
        <xsl:with-param name="string" select="$testName" />
        <xsl:with-param name="delimiter" select="'.'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="$MethodName" />
  </xsl:template>

  <xsl:template name="testIdGuid">
    <xsl:param name="value" />
    <xsl:variable name="id">
      <xsl:call-template name="dec_to_hex">
        <xsl:with-param name="value" select="$value"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="concat($guidStub,substring(concat('000000000000', $id),string-length($id) + 1, 12))"/>
  </xsl:template>

  <xsl:template name="executionIdGuid">
    <xsl:param name="value" />
    <xsl:variable name="id">
      <xsl:call-template name="dec_to_hex">
        <xsl:with-param name="value" select="$value"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="concat($guidStub,substring(concat('000000000000', $id),string-length($id) + 1, 12))"/>
  </xsl:template>

  <xsl:template name="testRunGuid">
    <xsl:param name="date" />
    <xsl:param name="time" />
    <xsl:variable name="year">
      <xsl:value-of select="substring($date,1,4)"/>
    </xsl:variable>
    <xsl:variable name="month">
      <xsl:value-of select="substring($date,6,2)"/>
    </xsl:variable>
    <xsl:variable name="day">
      <xsl:value-of select="substring($date,9,2)"/>
    </xsl:variable>
    <xsl:variable name="hour">
      <xsl:value-of select="substring($time,1,2)"/>
    </xsl:variable>
    <xsl:variable name="minute">
      <xsl:value-of select="substring($time,4,2)"/>
    </xsl:variable>
    <xsl:variable name="second">
      <xsl:value-of select="substring($time,7,2)"/>
    </xsl:variable>
    <xsl:variable name="hexYear">
      <xsl:call-template name="dec_to_hex">
        <xsl:with-param name="value" select="$year"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="hexMonth">
      <xsl:call-template name="dec_to_hex">
        <xsl:with-param name="value" select="$month"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="hexDay">
      <xsl:call-template name="dec_to_hex">
        <xsl:with-param name="value" select="$day"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="hexHour">
      <xsl:call-template name="dec_to_hex">
        <xsl:with-param name="value" select="$hour"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="hexMinute">
      <xsl:call-template name="dec_to_hex">
        <xsl:with-param name="value" select="$minute"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="hexSecond">
      <xsl:call-template name="dec_to_hex">
        <xsl:with-param name="value" select="$second"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="padYear">
      <xsl:value-of select="substring(concat('0000', $hexYear),string-length($hexYear) + 1, 4)"/>
    </xsl:variable>
    <xsl:variable name="padMonth">
      <xsl:value-of select="substring(concat('00', $hexMonth),string-length($hexMonth) + 1, 2)"/>
    </xsl:variable>
    <xsl:variable name="padDay">
      <xsl:value-of select="substring(concat('00', $hexDay),string-length($hexDay) + 1, 2)"/>
    </xsl:variable>
    <xsl:variable name="padHour">
      <xsl:value-of select="substring(concat('00', $hexHour),string-length($hexHour) + 1, 2)"/>
    </xsl:variable>
    <xsl:variable name="padMinute">
      <xsl:value-of select="substring(concat('00', $hexMinute),string-length($hexMinute) + 1, 2)"/>
    </xsl:variable>
    <xsl:variable name="padSecond">
      <xsl:value-of select="substring(concat('00', $hexSecond),string-length($hexSecond) + 1, 2)"/>
    </xsl:variable>
    <xsl:value-of select="concat($padYear,$padMonth,$padDay,'-',$padHour,$padMinute,'-',$padSecond,'00-91c4-')"/>
  </xsl:template>

  <xsl:variable name="hex_digits" select="'0123456789ABCDEF'" />

  <xsl:template name="dec_to_hex">
    <xsl:param name="value" />
    <xsl:if test="$value >= 16">
      <xsl:call-template name="dec_to_hex">
        <xsl:with-param name="value" select="floor($value div 16)" />
      </xsl:call-template>
    </xsl:if>
    <xsl:value-of select="substring($hex_digits, ($value mod 16) + 1, 1)" />
  </xsl:template>

</xsl:transform>
