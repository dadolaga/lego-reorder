using LegoApi.Models;
using Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace LegoApi {
    public class LegoApi : ILegoApi {
        public string token_;

        public LegoApi(string token) {
            token_ = token;
        }

        public Task<IEnumerable<Logic.Models.LegoSet>> SearchLegoSetFromCode(string code) {
            return Task.Run(async () => {
                using (HttpClient client = new HttpClient()) {
                    try {

                        var request = new HttpRequestMessage(HttpMethod.Get, $"https://rebrickable.com/api/v3/lego/sets/?search={code}");
                        request.Headers.Add("Authorization", $"key {token_}");

                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadFromJsonAsync<BaseResponse<Models.LegoSet>>();

                        if(content == null || content.Results == null) {
                            throw new NullReferenceException("Json parsing failing");
                        }

                        return content.Results.Select(legoSet => new Logic.Models.LegoSet { ApiId = legoSet.SetNum, Name = legoSet.Name, ImageUrl = legoSet.SetImgUrl, LegoCode = legoSet.SetNum.Split("-")[0] });
                    } catch (HttpRequestException e) {
                        MyLogger.Log.Error($"API SearchSetByCode raise an error: {e.Message}");
                    } catch (NullReferenceException e) {
                        MyLogger.Log.Error($"API SearchSetByCode raise an error: {e.Message}");
                    }

                    return new LinkedList<Logic.Models.LegoSet>();
                }
            });

        }
    }


}
