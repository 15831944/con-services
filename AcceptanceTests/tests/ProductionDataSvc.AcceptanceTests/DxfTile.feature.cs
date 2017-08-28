﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.42000
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace ProductionDataSvc.AcceptanceTests
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class DxfTileFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "DxfTile.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "DxfTile", "I should be able to request DXF tiles", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute()]
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute()]
        public virtual void TestInitialize()
        {
            if (((TechTalk.SpecFlow.FeatureContext.Current != null) 
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "DxfTile")))
            {
                ProductionDataSvc.AcceptanceTests.DxfTileFeature.FeatureSetup(null);
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Dxf Tile - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DxfTile")]
        public virtual void DxfTile_GoodRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Dxf Tile - Good Request", ((string[])(null)));
#line 4
this.ScenarioSetup(scenarioInfo);
#line 5
 testRunner.Given("the Dxf Tile service URI \"/api/v2/compaction/lineworktiles\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 6
 testRunner.And("a projectUid \"ff91dd40-1569-4765-a2bc-014321f76ace\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 7
  testRunner.And("a bbox \"-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625\"" +
                    " and a width \"256\" and a height \"256\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 8
  testRunner.And("a fileType \"linework\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 9
 testRunner.When("I request a Dxf Tile", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 10
 testRunner.Then("the Dxf Tile result should be", "{\n  \"tileData\": \"iVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAYAAABccqhmAAAAAXNSR0IArs4c6QAA" +
                    "AARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAABWkSURBVHhe7d1brt02sgbgHm4yh4wk" +
                    "cwngaeQ1QAIYCYzgoNGv67CoKu2ftUiKkqgLtf4PqBbFOylKtneM9n+IiIiIiIiIiIiIiIiIiIiIiIiI" +
                    "iIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiI" +
                    "iIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiI" +
                    "iIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiI" +
                    "iIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiI" +
                    "iIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIjocN++fXtJ6C0RfQp98WPwI0D0QfDlt8CPgKQ/5qPwev35" +
                    "er3++IzFEgW1DwCWPf4j8Hr9CC//3xr8CNDnyL3omGfx6I/A6/VvXNz0u4A/n7tQogx5ufEF/4gPgF/0" +
                    "6/Wdv/rTxwqHPzn7+BF45MsfLsnipj8GyEfge8x73KKJKvwHQDzyPdAFJSF5r9f/hfj39ddffyX54Ur0" +
                    "eLkPgCjlD6v8Afjx+vHjx+uXX355KwtXokf7mA+AwI8AvuCW/+uvv2bLiZ7s4z4C/uWW/wz4/fv312+/" +
                    "/fb66aef+PLTR/moD4DBxdnfB/j999/jy49lRE9XOu+Pfg/Ki5a/FMQPAH2O8rvwQR+A6XcB/w0h/0mQ" +
                    "HwD6HKXz/uj3ILe46SMg/1mQHwD6HKXz/uj3wC/O/kKQ3j578USgdNYf/w7UFvj4xROp0ll//DtQW+Dj" +
                    "F0+Ptub8luqu6WNItQU+fvH0WHZ2W89wqV5r+2EtLfDxG0CPZOe29fzW6rX2MaSlxT168fRIcmbtb7sa" +
                    "LSqq1WlpP7SPXjw9ipxX/WvsMdb8rdZSvdb2w6ot8PGLp0fBl9+i9SNQqtPSdmhLC3z8BlB38tLpy3ga" +
                    "Oae1D4AI91m1+dbaPcLSAh+/AZSovQwttG2MPf2sJedU+PGFlcvVW5pvqd2j1Bb5ERswODm4ucO71tLL" +
                    "sATbW/SY1xI5o0jGlMjRJlHLfH2bR6ot8iM2YGB4iP3hXaP0Mqyx1Ee4v5XSfMN1dsd5d7e0yI/YhAG1" +
                    "HGBPnmXOlr5ysB/fXoeq0qpV0q/1jekttG2MXD+tcxra0iI/YhMOsPdw1sgzwcNrIXk1oU4R9rdn3tJ2" +
                    "a3udZkKLoswcMb1Jbb5+/EeqbYBYKqd3ul8x1u5dPPULtOqucXKkjx799KRLtnkl0Wv90r8m39TKhre0" +
                    "gb02eDSy1q3rxT2zwL7kQNVotWZ75jqSyr5i+s3S/ui2b2o7NF1YErjYpfKnwnWvWa+eI2uThOSZcH9L" +
                    "Msc16y3p1U+O9jvvKaZRyIsy9d9gfdTSdmi4QAtc6FL5lWQeR8yltOYWoe4M+zlinlvIPEpz6TXfXv3U" +
                    "SL/WN6aRPA/NTwLrxocW6G1iqe1j4EJzC1wqv0KvOenzT2DfFlvHkHZ75tcTrsvPCcsstsy7Vz+9lOaD" +
                    "Ql7W3dZyKFlYbXFWntswK6u170nHScKPHSfaQKu/wTHOXNdRY+F6LGws2YdS+Vpr+wnlh8M5abrZnrbD" +
                    "aXkg8akFeouH9rBN0iFnOJ6F5KGQtxus7XC4pl5j6lZE2L+F5ImQjnrNYU0/cQKOFjWTMZbGaalTsqft" +
                    "LZUW1Lr58SkF0E8Spc3SZqtp8wSOWxpvFK17qNvRTJvNWvZM8ktla+zpR6c/0+wsHSNGj3mX6FQO6/80" +
                    "pQ1buzipL+2xPwvJy/szjvF6/RHSU8j9VjD+7bTOTXZF6yUheV7I3611Xneiy09IfmnfwvUQNu6wShtm" +
                    "wn2z6f9C/I/4T4lJyL8sLKH/xHgkdaar/DsD0z9BPrWTf3nofzEvVjyRrPfIQyJwn3F/c2KDwLeJmVQk" +
                    "e4d7ZnHk3sUHFkhaxhnmOem8mzdMfqWe4u9Y9nX/9Sv39ELLizz90+L//PNPDHypv+pIW/nXhiRt99MH" +
                    "QaueAte/9eGFSVe17nGO1Ns6r0+F+y1pFCt0Jv36MWPBFeIqG2j1KDf56Vdj+dXZXnRJS0wv89e9vcAS" +
                    "8kLbvyIkIfdTe2mzh8yr98biui1wjDDxJlo9C+adRO+1UAr2PaGPbKbZu5Se7+nPeM+C/ITtV+PSy5y+" +
                    "8FLvf81jr90crRtjTbuSMNkI+7WQPBPud7Mx/Br0ni6mj3rXs8Bna+Ge+3iml15e8vSfBsuxD4HeVuFm" +
                    "tWxOZXNnYfBVtFm0dj6tdKjYp43h07Ei3YI+rkizVrHnKoFpu7cI9+PoPWntK4lc//ocolIbFPJ2kf5y" +
                    "80A6VDNtFln/OsbiWHQtfYSRZjXBZ4yBeUM8e7+QvZPWvSxujhfyE5m57J6T0OGaaJPNZL4WmkUD0Me/" +
                    "6pnZGZXAtMvrzw4YhhY1g3ZJ+L50X5pokwj7XjO/3Lz2zElos1NdNS7tEw+M0iw8k2+sTMuTKLXZzQbD" +
                    "QdcMlmtvIXko5G0mfbXMK4wiI8U4ek5nGmmu9E6eH57HpbPs65pY2FnxRQnXKqnjJ+rSu9iLvCa06az3" +
                    "nK4SFjfs3Ck9hxZL51HKfZ0jzkHy4lpI3tJg1g7bS1rvE/aCrgltupvNSWjWkEaf/yfDd8Qi9560iAc5" +
                    "0Nvsh6KZTQwnaJ3pOPHeD+DbWR17eX3ERhUtdXoIkzllnCOMPHdKPwKajs9USHotaZfrcxVp5EOLIr1/" +
                    "G8DnW8TCjfgRWDby3OnrfdPbmTxXobdNtJ8k9r6DidIAlo/lku7xAvMjUDfqvKnNmudbej/DtY/SAJhv" +
                    "9xL8ABxv1HlTO3nGQm+r/LvY2q6ZHyBmBrn8Xi9vr36WdN+sk4w6b1qn9TnL+4fvZvfzYQPgIMLybMDw" +
                    "v90G7tlXTffNOsGIc6Zt5FkLvW22pU2Rvvgx/EdA2GDhf/sNGvTur6TrZp1kxDnTdvK8hd420Sb7zgm+" +
                    "/Bb+IyD3ltf7pe3dX87uTbrAiHOm/fC543tXs+us6ABJ4KBYbvm9X9re/eXs2qSLjDhn2k+ee+69q5E2" +
                    "Qm/XyQwWr5iP5SN+AMTmDbrIaPOlPkrvXbgu2nxmci98aSISI34EZHPOGKeXOFv6OKX3LlybxGMe6G2b" +
                    "2suO93Dt/tKe9RHQ5BBGmy/14d+7LedgdZvMyx5J2srcddCPwPFj9LLlwdMzwHsXnXIWbFAczPJ0MjE0" +
                    "PdwHwPo/ZTM7GWmudKxTzwIOlnv5jb1UvfTuT0if2O+pG7nTSHOl48l5EHp7HD+I/S4gN3jI6Tqhnv2V" +
                    "+sqt465Gmiud45QzURoklx9yuk6oR39LfZTWdzejzJPOJedC6G1/pc7L+ff5CLS2La3lbkaZJ53v0LNR" +
                    "6lzy7Y8FmhXyrv8ASJs17Urru5tR5knXkPMh9LYf6dS/6ELvY9zlI7B17EM2rrMR5kjX635Oci865llc" +
                    "/RHYO+YIL9gIc9xCzg6eH9qn2zkpveil/HCd7X0hvVJ/kt9jLNm03nPuLc7wYfAs+TPkwdmjBV3OCj4c" +
                    "C3sAWJZ7KEe8TL7P3mOM8IKNMMdWtfPllc6bpEttqIPSxgvb/NKh7P2CCuvziL7F3V+wu89vDTxbFnae" +
                    "TMgr1sN8Ta8G/VCJbZI9kJxSWcjtvrlH9Ilq67yDu89vDX35YuReRFkr1rGo5DVbGpsyaoevVNbrhZV+" +
                    "evVVI+s4Y5yt4uweIG5zIC/f0gvoX1a8x/yy9OyU2ofrIxy6lridBbky3PitevSxRm2NV7vz3FptWYMc" +
                    "ajzY+BIvHXgZD2Fbi6U+RgFrO45soiYT5fztE9rTdo+rxm1R2ucR+LnLgfWhRbO1+S20XYytfdwRrOs4" +
                    "tQNYKlv7Qkn9tW16sXFr67zSXedVI3MWehvZYZWrj6nG+4uKZXthfzq7bn1fqeceFcXtcl6vv+fNjBlO" +
                    "6wZf9SBkXBy7tI47uPPcvNxc9ZDOL7Xd1/JcehPs2/cj8xR6O6zT1oADvV5/hLv/hvg7xHcpyk4i5FYn" +
                    "t1R+lNK4pXVc7a7z8krz1JcPX8QkML9WDvUWufpzP7EQjLK3OafP3Q/4ev0Zcn5IdnYiIbc4wVrZkZbG" +
                    "La3lanCYbycegEBvs+xFxBfSAstK5Ziv6SKrj20ssC3W2xLazel0u88f3w86/eovvwuQyE8o5L7l5/KO" +
                    "JmO2jBsXcsH8avSwxbjy4OXE3WqAL45fD+aVyu0e88M1y9qU2llbrLclpI+zte73YfwE7HcBUzo/OXyh" +
                    "MH2WtWOW1nEFPWhJXHX4vLX7hGuBl2hOL+VhaF5Wqa2l9eo+FLKW5UjbnOs25xInMn0AJOR3AfKzgfwk" +
                    "Q66UXLBp28YsraNG2iDN3kUPWhJXHD6ky9s0B3h5khfJ58sV61Ty3lhdrFfKwzChWpTLx/pxsJNM49+I" +
                    "TOhrg+SPAhb5iYbc0xewd8zSWnLiwgO9ne+FZm2mhy3G2QfP27MeOy8hmYStCctzaQxX543VkbB6WN/d" +
                    "z/Wm9aV/gcjnY4Tyw01j34wuHjZIfhgo/2XgHh+BHmPFhTT0M9Wb2MEQWBYr7oD9nrmPKI68kc1fItwm" +
                    "AflJPQxXb75C+o2vb/UsbWUYtfxwmcuh3qH27PlhdOFJSN708v8bov5HAU0eQvrvOUZpHUbKDe7LtB9T" +
                    "W7v20ru/JTKe0NuZrNFCs4psb6D+fA/XN1Yf61kaQ/PeWH0rt7aQl4TPt/utEfrYLLfnt6ALS0Lyvn4e" +
                    "8D1OPLeAkCO5b/k9HNdv+UFYWWlPwrXafqsj+swpjSNrwzXbPeQnfLlFKMJ0EbbRukkstTfWNteXpKf1" +
                    "fn3Msd6WCH2sFicRSLrWXy7vNDpwDJyEfAA0qQuZPgYoLE2W95a/R+/+vFL/skK54n5Y2L5Ynd6O6teU" +
                    "+pd1VdaL6QS2szTkLcq0wbE292MRukhiqbwUaZt1cM+1vfWDaf8MrmGTKB2U6WcC4o+38tCi28R79pUj" +
                    "/Qu9TcQCBQ8k2Re79hYHOGDtU7/5Odszx7Va+HxNZ184V7+Jbwft53yp1+K9D1nve/g6ppZO27Sb2k+0" +
                    "rfWRBPRtcT2cvJh+HjD9HQFfZkLu7sn36KNE+sb+y+uY8uVqDwfzhKSP0Lvvpf7s4PlDiPmYh/XwHvJX" +
                    "0TZzfy7dzMa3CF3MgXwdU0unbdpMbSfQdr5aWJnLvwdchLAPwJT+2phYGITa0mLTAva0bVHqW9ahyZnk" +
                    "Gc2KNOuwOZpeY7T0g4fP0ngv11IZlru8LCv3dSCvWu7zEdaZ6snap0ClOrVI2yyTcTQZaTtrP199HuTf" +
                    "By4Gfx6QWUQUakuL+b7F2vpr1fqX9ZXKp7KUFh1KR9o81tS+fa52+GoRqhXTdu/SiUL9mZVbPt5viTDE" +
                    "HMjXMbV02qZuapOCtklfGFgWG91FaUHhkgROPLSY00vW1N2ipf/cGq8mc9qyN7HVBnb4Ss/W8nNpu8fQ" +
                    "vKiljrB6pQhV5nZLEarNgXwdU0unbcqm+nnYx1Jok/vwC9NJJuEnHlok9zktdfZY03/t4V1l7Zz2rsEd" +
                    "whiWxnxLY1h9C83L9mkB+XM9zMewNhaY5+u2RmjaFGmbvKW9d33MV0u78vvxC/QLiJlOaFHML5X1sLV/" +
                    "v8aryXxa1jHV6zt3OIzJ4ayF1MX61o/Lm+8xH+99vuVZ+HvLWxuhWVOkbVK69W/5Odre+kn6xYiV70gW" +
                    "ipPEdG4TQo7kJvn+vrc9/csajp7fWktziqUHsee7NbCPkPRlyT3mt5RZGvO+6smeTOG11sMybDO1m0z1" +
                    "1vF95UKr3o9OLoafaGkzQm7Ml6ulj9Kj/y0P9Wgyp9zhOHquNubaCE3tGvlyH6FKksb7XFh5S91ahKZz" +
                    "eFiWtpnGm+rs5/vV9P3YJDH8ZEubEnKz+b1I/z3HkHVc/TBkDgb3XtImVjwY7MO8J7l7y8vUi7AOlhXy" +
                    "5/SREYaZw8OytE3+7O/h+r8nXLyFTfin6T6aNu9dyM3m73VEv7jWKx5KPIGBpGv7fhYZz+Zh6aWwurED" +
                    "5evsidDdnJ72Kn1RW0LaWHhYlrZZfgZYv1bP6JAzzb4fXUx2A674CPTuT+AaLVoeYk/xFAQy7h3mI2wu" +
                    "LRGqYzpRqDOncxGKs/kSUmaRK+8dNtfS2cM6coV01vSkZf7J3twXTtImbs78CPTqx9O1JXHmQ7F9w3n4" +
                    "dGlvryDzsflZGvISkD/X99dchKIYpfxceejvDZbvCe1ORk3G0TKrkwS2Q/i8LWLBCPxBxA+A8OXCb9pa" +
                    "0n5vH0vwAbY8EHx4LfVrbM+0nzmw79y+XgnmVtwDy7d6EniP5T5kxRYeluXalgLbeViWtnkXakkLrDNf" +
                    "MbC91cUI2fM1VhrFtGFfjvxdwNZ2a9gY8GCK6xCZhze328LGkqvvE8vO2IslMqdchCK7zjDf1ZnzSiEr" +
                    "tvCwLNe2FNjOw7K0TZmWW735amlrb2lf7usNZdq0L0d8BNbW36I0RmkN+ODkirH1QcpYyA7FO6m6P3TY" +
                    "1WxeFiErCciPsK6PUOzbuPIwUw0Py0ptcuVbQtbhhZ5lFlgnaVPKl3sMrBOu45kexpeeH4HWeltJ/0tj" +
                    "+DXgg6w80E1kLIHpVL/9sLWvCVs7rt/Sdg/X//z888/xivUs7SNXFrLm8LAM2xhrg2VbQ9aAQu9zXq6+" +
                    "RSiOkcsrlI1JNlqT0dJHADewpKXOHmv6xzXYQ/RXS+99kDJWjhZfxtaGEbKL9/by1z4CmN8rjOyk3Mt4" +
                    "GL5+yGsmZyZ3bmwcf7W0j1zZNOdB+cnv+aGg5JfKetnSv60BHyI+QJ8vdfc4Yx/WwLUthc3d5u8/AhKY" +
                    "L9dcnS0Rd05juo9zmOO9fhtbC8r0lVxbAutKn8MKTztZwJaPQG6Te5L+t46B88cHiBGKYmh6kz1zrHHz" +
                    "XA3azv2UYmoxsfVY2Ituch8BAX0l/ebyv/LCEMrSVjdXX8sWybw1OYP2xb4tsNzS2A6uY5MN12S05ucB" +
                    "uU3uqUf/tgZ7mO7hYboJ9iPzO2IPcIxwW50j1s3VyZQn95qXwBdcrrZOXK+vA33NV0i/lX/dhy4Vpn2d" +
                    "9L4M54igbRLYb64O5hXS45MN12TU84eCW/Xs39aAD8+uax4i1Md+urIxcBwLHA/rYV2sI3w9Lcd0ln/B" +
                    "RemDgH1rn0nYOL7elC/PZjnSNnmhZrbMtU3Cly3VwXqafgZ5SXBRPX4ouNURfdsabI241hZQP4k1fSyx" +
                    "/mtj5SJXL1xnWM/Svk5O7SMgJJ3rW9IYkJ/Zf3kuy5G2SYUa8nTf8o22wfbzPeZj+Dp4b3nfvn17/T8N" +
                    "iZDLtGioPAAAAABJRU5ErkJggg==\",\n  \"tileOutsideProjectExtents\": false,\n  \"Code\": 0" +
                    ",\n  \"Message\": \"success\"\n}", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Dxf Tile - No Imported Files")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DxfTile")]
        public virtual void DxfTile_NoImportedFiles()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Dxf Tile - No Imported Files", ((string[])(null)));
#line 20
this.ScenarioSetup(scenarioInfo);
#line 21
 testRunner.Given("the Dxf Tile service URI \"/api/v2/compaction/lineworktiles\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 22
 testRunner.And("a projectUid \"0fa94210-0d7a-4015-9eee-4d9956f4b250\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 23
  testRunner.And("a bbox \"-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625\"" +
                    " and a width \"256\" and a height \"256\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 24
  testRunner.And("a fileType \"linework\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 25
 testRunner.When("I request a Dxf Tile", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 26
  testRunner.Then("the Dxf Tile result should be", @"{
  ""tileData"": ""iVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAYAAABccqhmAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEVSURBVHhe7cExAQAAAMKg9U/tawggAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABODQE8AAH/Cno5AAAAAElFTkSuQmCC"",
  ""tileOutsideProjectExtents"": false,
  ""Code"": 0,
  ""Message"": ""success""
}", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Dxf Tile - No FileType")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DxfTile")]
        public virtual void DxfTile_NoFileType()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Dxf Tile - No FileType", ((string[])(null)));
#line 36
this.ScenarioSetup(scenarioInfo);
#line 37
 testRunner.Given("the Dxf Tile service URI \"/api/v2/compaction/lineworktiles\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 38
 testRunner.And("a projectUid \"ff91dd40-1569-4765-a2bc-014321f76ace\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 39
  testRunner.And("a bbox \"-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625\"" +
                    " and a width \"256\" and a height \"256\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 40
  testRunner.And("a fileType \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 41
 testRunner.When("I request a Dxf Tile Expecting BadRequest", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 42
 testRunner.Then("I should get error code -1 and message \"Missing file type\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Dxf Tile - Bad FileType")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "DxfTile")]
        public virtual void DxfTile_BadFileType()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Dxf Tile - Bad FileType", ((string[])(null)));
#line 44
this.ScenarioSetup(scenarioInfo);
#line 45
 testRunner.Given("the Dxf Tile service URI \"/api/v2/compaction/lineworktiles\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 46
 testRunner.And("a projectUid \"ff91dd40-1569-4765-a2bc-014321f76ace\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 47
  testRunner.And("a bbox \"-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625\"" +
                    " and a width \"256\" and a height \"256\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 48
  testRunner.And("a fileType \"SurveyedSurface\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 49
 testRunner.When("I request a Dxf Tile Expecting BadRequest", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 50
  testRunner.Then("I should get error code -1 and message \"Unsupported file type SurveyedSurface\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
