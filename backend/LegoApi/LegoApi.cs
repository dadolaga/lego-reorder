using LegoApi.Models;
using Logic.Models;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

using API = LegoApi.Models;
using IN = Logic.Models;

namespace LegoApi {
    public class LegoApi : ILegoApi {
        public string token_;

        public LegoApi(string token) {
            token_ = token;
        }

        public Task<IEnumerable<IN.LegoSet>> SearchLegoSetFromCode(string code) {
            return Task.Run(async () => {
                using (HttpClient client = new HttpClient()) {
                    try {
                        var request = new HttpRequestMessage(HttpMethod.Get, $"https://rebrickable.com/api/v3/lego/sets/?search={code}");
                        request.Headers.Add("Authorization", $"key {token_}");

                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadFromJsonAsync<BaseResponse<Models.LegoSet>>();

                        if (content == null || content.Results == null) {
                            throw new NullReferenceException("Json parsing failing");
                        }

                        return content.Results
                            .Select(legoSet => new IN.LegoSet {
                                ApiId = legoSet.SetNum,
                                Name = legoSet.Name,
                                Year = legoSet.Year,
                                ImageUrl = legoSet.SetImgUrl,
                                LegoCode = legoSet.SetNum.Split("-")[0]
                            });
                    } catch (HttpRequestException e) {
                        MyLogger.Log.Error($"API SearchSetByCode raise an error: {e.Message}");
                    } catch (NullReferenceException e) {
                        MyLogger.Log.Error($"API SearchSetByCode raise an error: {e.Message}");
                    }

                    return new LinkedList<IN.LegoSet>();
                }
            });

        }

        public Task<IEnumerable<IN.LegoPiece>> GetAllPieceFromSet(string apiId) {
            return Task.Run(async () => {
                using (HttpClient client = new HttpClient()) {
                    try {
                        BaseResponse<API.LegoPart>? content = null;
                        List<Models.LegoPart> parts = new List<API.LegoPart>();

                        do {
                            var request = new HttpRequestMessage(HttpMethod.Get, content == null ? $"https://rebrickable.com/api/v3/lego/sets/{apiId}/parts/" : content.Next);
                            request.Headers.Add("Authorization", $"key {token_}");

                            var response = await client.SendAsync(request);
                            response.EnsureSuccessStatusCode();
                            content = await response.Content.ReadFromJsonAsync<BaseResponse<API.LegoPart>>();

                            if (content == null || content.Results == null) {
                                throw new NullReferenceException("Json parsing failing");
                            }

                            parts.AddRange(content.Results);

                        } while (content != null && content.Next != null);

                        if (parts.Count() != content.Count) {
                            throw new Exception("Expected different number of piece");
                        }

                        return parts.Select(p => {
                            var piece = new IN.LegoPiece {
                                ApiId = $"{p.Id}",
                                Name = p.Part.Name,
                                ImageUrl = p.Part.PartImgUrl,
                                Color = new IN.LegoColor {
                                    ApiId = $"{p.Color.Id}",
                                    Name = p.Color.Name,
                                    Trasparent = p.Color.IsTrans,
                                    Value = Convert.ToInt32(p.Color.Rgb, 16)
                                }
                            };

                            var match = Regex.Match(p.Part.PartImgUrl, @"elements/([\d]+)", RegexOptions.IgnoreCase);
                            if (match.Success) {
                                piece.LegoId = match.Groups[1].Value;
                            } else if (p.ElementId != null) {
                                piece.LegoId = p.ElementId;
                            } else {
                                piece.LegoId = p.Part.ExternalIds.LEGO?[0];
                            }

                            return piece;
                        });

                    } catch (HttpRequestException e) {
                        MyLogger.Log.Error($"API SearchSetByCode raise an error: {e.Message}");
                    } catch (NullReferenceException e) {
                        MyLogger.Log.Error($"API SearchSetByCode raise an error: {e.Message}");
                    }

                    return new LinkedList<IN.LegoPiece>().AsEnumerable();
                }
            });
        }
    }


}
