<?xml version="1.0" encoding="UTF-8" ?>
<xsl:transform version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output indent="yes" />
	<xsl:variable name="guidStub">
		<xsl:call-template name="testRunGuid">
			<xsl:with-param name="date" select="/test-results/@date"/>
			<xsl:with-param name="time" select="/test-results/@time"/>
		</xsl:call-template>
	</xsl:variable>

	<xsl:template match="/">
		<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
			<xsl:attribute name="id">
				<xsl:value-of select="concat($guidStub,'30db1d215203')"/>
			</xsl:attribute>
			<xsl:attribute name="runUser">
				<xsl:value-of select="concat(//environment/@machine-name,'\',//environment/@user)"/>
			</xsl:attribute>
			<xsl:attribute name="name">
				<xsl:value-of select="concat(//environment/@user,'@',//environment/@machine-name,' ',/test-results/@date,' ',/test-results/@time)"/>
			</xsl:attribute>
			<TestSettings name="Default Test Settings" id="8dfb34aa-91bc-45e3-8609-d0a4e732d982">
				<Deployment>
					<xsl:attribute name="runDeploymentRoot">
						<xsl:value-of select="//environment/@cwd" />
					</xsl:attribute>
					<DeploymentItem>
						<xsl:attribute name="filename">
							<xsl:value-of select="/test-results/test-suite/@name"/>
						</xsl:attribute>
					</DeploymentItem>
				</Deployment>
				<Execution>
					<TestTypeSpecific />
					<AgentRule name="Execution Agents">
					</AgentRule>
				</Execution>
			</TestSettings>
			<Times creation="2008-01-01T00:00:00.0000000+10:00" queuing="2008-01-01T00:00:00.0000000+10:00" start="2008-01-01T00:00:00.0000000+10:00" finish="2008-01-01T00:00:00.0000000+10:00" />
			<xsl:variable name="pass_count" select="count(//test-case[@success='True'])"/>
			<xsl:variable name="failed_count" select="count(//test-case[@success='False' and (count(./failure) + count(./reason) &gt; 0)])"/>
			<xsl:variable name="inconclusive_count" select="count(//test-case[@success='False' and (count(./failure) + count(./reason) = 0)])"/>
			<xsl:variable name="executed_count" select="count(//test-case[@executed='True'])"/>
			<xsl:variable name="notExecuted_count" select="count(//test-case[@executed='False'])"/>
			<ResultSummary>
				<xsl:attribute name="outcome">
					<xsl:choose>
						<xsl:when test="$failed_count &gt; 0">Failed</xsl:when>
						<xsl:when test="$inconclusive_count &gt; 0">Inconclusive</xsl:when>
						<xsl:otherwise>Completed</xsl:otherwise>
					</xsl:choose>
				</xsl:attribute>
				<Counters error="0" timeout="0" aborted="0" passedButRunAborted="0" notRunnable="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0">
					<xsl:attribute name="total">
						<xsl:value-of select="$executed_count + $notExecuted_count"/>
					</xsl:attribute>
					<xsl:attribute name="executed">
						<xsl:value-of select="$executed_count"/>
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
					<xsl:attribute name="inconclusive">
						<xsl:value-of select="$inconclusive_count"/>
					</xsl:attribute>
				</Counters>
			</ResultSummary>
			<TestDefinitions>
				<xsl:for-each select="//test-case">
					<xsl:variable name="testName">
						<xsl:call-template name="getTestName">
							<xsl:with-param name="nunitName" select="@name" />
						</xsl:call-template>
					</xsl:variable>
					<xsl:variable name="pos" select="position()" />
					<UnitTest>
						<xsl:attribute name="name">
							<xsl:value-of select="$testName"/>
						</xsl:attribute>
						<xsl:attribute name="storage">
							<xsl:value-of select="concat(//environment/@cwd,'\',/test-results/test-suite/@name)"/>
						</xsl:attribute>
						<xsl:attribute name="id">
							<xsl:call-template name="testIdGuid">
								<xsl:with-param name="value" select="$pos"/>
							</xsl:call-template>
						</xsl:attribute>
						<xsl:if test="@description">
							<Description><xsl:value-of select="@description" /></Description>
						</xsl:if>
						<Execution>
							<xsl:attribute name="id">
								<xsl:call-template name="executionIdGuid">
									<xsl:with-param name="value" select="$pos"/>
								</xsl:call-template>
							</xsl:attribute>
						</Execution>
						<TestMethod adapterTypeName="Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" >
							<xsl:attribute name="name">
								<xsl:value-of select="$testName"/>
							</xsl:attribute>
							<xsl:attribute name="codeBase">
								<xsl:value-of select="concat(//environment/@cwd,'\',/test-results/test-suite/@name)"/>
							</xsl:attribute>
							<xsl:attribute name="className">
								<xsl:variable name="testClassName">
									<xsl:call-template name="getTestClassName">
										<xsl:with-param name="nunitName" select="@name"/>
									</xsl:call-template>
								</xsl:variable>
								<xsl:value-of select="concat(@name, ', ', $testClassName)" />
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
				<xsl:for-each select="//test-case">
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
				<xsl:for-each select="//test-case">
					<xsl:variable name="testName">
						<xsl:call-template name="getTestName">
							<xsl:with-param name="nunitName" select="@name" />
						</xsl:call-template>
					</xsl:variable>
					<xsl:variable name="pos" select="position()" />
					<UnitTestResult startTime="2008-01-01T00:00:01.0000000+10:00" endTime="2008-01-01T00:00:02.0000000+10:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d">
						<xsl:attribute name="testName">
							<xsl:value-of select="$testName"/>
						</xsl:attribute>
						<xsl:attribute name="computerName">
							<xsl:value-of select="//environment/@machine-name"/>
						</xsl:attribute>
						<xsl:attribute name="duration">
							<xsl:call-template name="secondsToDuration">
								<xsl:with-param name="seconds" select="@time"/>
							</xsl:call-template>
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
							<xsl:choose>
								<xsl:when test="@executed='True'">
									<xsl:choose>
										<xsl:when test="@success='True'">
											<xsl:value-of select="'Passed'"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:choose>
												<!--
													If the test has failed it will have a child <failure> element for ResultStates of
													Failure, Error or Cancelled. For ResultStates of NotRunnable, Skipped or Ignored it
													will have a child <reason> element. Therefore if it has neither it can only have a
													ResultState of Inconclusive.
												-->
												<xsl:when test="count(./failure) + count(./reason) &gt; 0">
													<xsl:value-of select="'Failed'"/>
												</xsl:when>
												<xsl:otherwise>
													<xsl:value-of select="'Inconclusive'"/>
												</xsl:otherwise>
											</xsl:choose>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="'NotExecuted'"/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:attribute>
						<xsl:if test="./failure">
							<Output>
								<xsl:for-each select="./failure">
									<ErrorInfo>
										<Message>
											<xsl:value-of select="./message"/>
										</Message>
										<StackTrace>
											<xsl:value-of select="./stack-trace"/>
										</StackTrace>
									</ErrorInfo>
								</xsl:for-each>
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

	<xsl:template name="getTestName">
		<xsl:param name="nunitName" />

		<xsl:choose>
			<xsl:when test="contains($nunitName, '(')">
				<!-- NUnit name contains an open bracket; must be a test case name, e.g. MyTest("MyParameter"). Do a substring-after-last
				     on everything before the '(' to get the method name. -->
				<xsl:variable name="MethodName">
					<xsl:call-template name="substring-after-last">
						<xsl:with-param name="string" select="substring-before($nunitName, '(')" />
						<xsl:with-param name="delimiter" select="'.'" />
					</xsl:call-template>
				</xsl:variable>
				<!-- Now we need to append the stuff after the bracket. Normalize and trim to 255 characters at the same time. -->
				<xsl:value-of select="substring(normalize-space(concat($MethodName, '(', substring-after($nunitName, '('))), 0, 255)" />
			</xsl:when>
			<xsl:otherwise>
				<!-- No open bracket, method name is everything after the last '.'. -->
				<xsl:variable name="MethodName">
					<xsl:call-template name="substring-after-last">
						<xsl:with-param name="string" select="$nunitName" />
						<xsl:with-param name="delimiter" select="'.'" />
					</xsl:call-template>
				</xsl:variable>
				<!-- Trim the name to 255 characters and return. -->
				<xsl:value-of select="substring($MethodName, 0, 255)" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="getTestClassName">
		<xsl:param name="nunitName" />

		<xsl:variable name="MethodName">
			<xsl:choose>
				<xsl:when test="contains($nunitName, '(')">
					<!-- NUnit name contains an open bracket; must be a test case name, e.g. MyTest("MyParameter"). Do a substring-after-last
				     on everything before the '(' to get the method name. -->
					<xsl:call-template name="substring-after-last">
						<xsl:with-param name="string" select="substring-before($nunitName, '(')" />
						<xsl:with-param name="delimiter" select="'.'" />
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<!-- No open bracket, method name is everything after the last '.'. -->
					<xsl:call-template name="substring-after-last">
						<xsl:with-param name="string" select="$nunitName" />
						<xsl:with-param name="delimiter" select="'.'" />
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<!-- Now get the class name, i.e. everything before the method name. Trim to 255 characters at the same time. -->
		<xsl:value-of select="substring(substring-before($nunitName, concat('.', $MethodName)), 0, 255)" />
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
	
	<xsl:template name="secondsToDuration">
		<xsl:param name="seconds" />
		<xsl:variable name="duration">
			<xsl:choose>
				<xsl:when test="$seconds">
					<xsl:variable name="hours" select="floor($seconds div 3600)" />
					<xsl:variable name="mins" select="floor(($seconds - ($hours * 3600)) div 60)" />
					<xsl:variable name="secs" select="floor($seconds - ($hours * 3600) - ($mins * 60))" />
					<xsl:variable name="frac" select="substring($seconds - floor($seconds), 3, 7)" />

					<xsl:value-of select="substring(concat('00', $hours), string-length($hours) + 1, 2)" />
					<xsl:text>:</xsl:text>
					<xsl:value-of select="substring(concat('00', $mins) ,string-length($mins) + 1, 2)" />
					<xsl:text>:</xsl:text>
					<xsl:value-of select="substring(concat('00', $secs), string-length($secs) + 1, 2)" />
					<xsl:text>.</xsl:text>
					<xsl:value-of select="substring(concat($frac, '0000000'), string-length($frac) + 1, 7)" />
				</xsl:when>
				<xsl:otherwise>00:00:00.0000000</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:value-of select="$duration" />
	</xsl:template>

</xsl:transform>
