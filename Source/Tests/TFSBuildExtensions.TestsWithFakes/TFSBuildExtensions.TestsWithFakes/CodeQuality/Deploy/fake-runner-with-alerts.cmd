REM BEWARE OF THE ENCODING OF THIS FILE
REM MUST BE Windows Western European
REM Visual Studio tends to change it to UTF8
@echo I'm the fake Sonar runner
@echo I'm going to display fake alerts to the console error output and the build break alert stamp on the std output
@echo
@echo 17:21:10.747 INFO  - - Clean MyCompany.MyProject1 [id=355]
@echo 17:21:10.753 INFO  - - Clean MyCompany.MyProject2 [id=356]
@echo 17:21:10.760 INFO  - - Clean MyCompany.MyProject3 [id=18771]
@echo 17:21:10.765 INFO  - - Clean MyCompany.MyProject4 [id=18772]
@echo 17:21:10.771 INFO  - - Clean MyCompany.MyProject5 [id=20846]
@echo 17:21:10.820 INFO  - Executing post-job class org.sonar.plugins.buildbreaker.AlertBreaker
@echo 17:21:10.821 ERROR - [BUILD BREAKER] Major violations variation  1 since previous analysis (2013 oct. 20)
@echo INFO: ------------------------------------------------------------------------
@echo INFO: EXECUTION FAILURE
@echo INFO: ------------------------------------------------------------------------
@echo Total time: 1:05.173s
@echo Final Memory: 16M/258M
@echo INFO: ------------------------------------------------------------------------
@echo ERROR: Error during Sonar runner execution 1>&2
@echo ERROR: Unable to execute Sonar 1>&2
@echo ERROR: Caused by: Alert thresholds have been hit (1 times). 1>&2
@echo ERROR: 1>&2
@echo ERROR: To see the full stack trace of the errors, re-run Sonar Runner with the -e switch. 1>&2
@echo ERROR: Re-run Sonar Runner using the -X switch to enable full debug logging. 1>&2



