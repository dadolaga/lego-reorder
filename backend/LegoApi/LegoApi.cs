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
                Dictionary<int, LegoThemeApi> legoThemeDictonary = new Dictionary<int, LegoThemeApi>();

                foreach (var theme in await GetAllThemeInternal()) {
                    legoThemeDictonary.Add(theme.Id, theme);
                }

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

                        var legoSetResult = content.Results
                            .Select(legoSet => {
                                var theme = legoThemeDictonary.GetAbsoluteParent(legoSet.ThemeId);

                                return new IN.LegoSet {
                                    ApiId = legoSet.SetNum,
                                    Name = legoSet.Name,
                                    Year = legoSet.Year,
                                    ImageUrl = legoSet.SetImgUrl,
                                    LegoCode = legoSet.SetNum.Split("-")[0],
                                    Theme = new LegoTheme {
                                        ApiId = $"{theme.Id}",
                                        Name = theme.Name
                                    }
                                };
                            } ).ToList();

                        return legoSetResult.AsEnumerable();
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
                                Quantity = (uint)p.Quantity,
                                isSpare = p.IsSpare,
                                Color = new IN.LegoColor {
                                    ApiId = $"{p.Color.Id}",
                                    Name = p.Color.Name,
                                    Trasparent = p.Color.IsTrans,
                                    Value = Convert.ToInt32(p.Color.Rgb, 16)
                                }
                            };

                            if (p.Part?.PartImgUrl != null) {
                                var matchOriginal = Regex.Match(p.Part.PartImgUrl, @"elements/([\d]+)", RegexOptions.IgnoreCase);
                                var matchSecondary = Regex.Match(p.Part.PartImgUrl, @"ldraw/[\d]+/([\d]+)", RegexOptions.IgnoreCase);

                                if (matchOriginal.Success) {
                                    piece.LegoId = matchOriginal.Groups[1].Value;
                                } else if (matchSecondary.Success) {
                                    piece.LegoId = matchSecondary.Groups[1].Value;
                                }
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

        public Task<IEnumerable<LegoTheme>> GetAllTheme() {
            return Task.Run(async () => {
                return (await GetAllThemeInternal()).Select(t => new LegoTheme {
                    ApiId = $"{t.Id}",
                    Name = t.Name
                });
            });
        }

        private Task<IEnumerable<LegoThemeApi>> GetAllThemeInternal() {
            return Task.Run(async () => {
                using (HttpClient client = new HttpClient()) {
                    try {
                        BaseResponse<API.LegoThemeApi>? content = null;
                        List<Models.LegoThemeApi> themes = new List<API.LegoThemeApi>();

                        do {
                            var request = new HttpRequestMessage(HttpMethod.Get, content == null ? $"https://rebrickable.com/api/v3/lego/themes/" : content.Next);
                            request.Headers.Add("Authorization", $"key {token_}");

                            var response = await client.SendAsync(request);
                            response.EnsureSuccessStatusCode();
                            content = await response.Content.ReadFromJsonAsync<BaseResponse<API.LegoThemeApi>>();

                            if (content == null || content.Results == null) {
                                throw new NullReferenceException("Json parsing failing");
                            }

                            themes.AddRange(content.Results);

                        } while (content != null && content.Next != null);

                        if (themes.Count() != content.Count) {
                            throw new Exception("Expected different number of piece");
                        }

                        return themes;

                    } catch (HttpRequestException e) {
                        MyLogger.Log.Error($"API SearchSetByCode raise an error: {e.Message}");
                    } catch (NullReferenceException e) {
                        MyLogger.Log.Error($"API SearchSetByCode raise an error: {e.Message}");
                    }

                    return new LinkedList<LegoThemeApi>().AsEnumerable();
                }
            });
        }

        public Task<LegoTheme?> GetTheme(string apiId) {
            return Task.Run(async () => {
                using (HttpClient client = new HttpClient()) {
                    try {
                        var request = new HttpRequestMessage(HttpMethod.Get, $"https://rebrickable.com/api/v3/lego/themes/{apiId}/");
                        request.Headers.Add("Authorization", $"key {token_}");

                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadFromJsonAsync<API.LegoThemeApi>();

                        if (content == null) {
                            throw new NullReferenceException("Json parsing failing");
                        }

                        return new LegoTheme {
                            ApiId = $"{content.Id}",
                            Name = content.Name
                        };
                    } catch (HttpRequestException e) {
                        MyLogger.Log.Error($"API SearchSetByCode raise an error: {e.Message}");
                    } catch (NullReferenceException e) {
                        MyLogger.Log.Error($"API SearchSetByCode raise an error: {e.Message}");
                    }

                    return null;
                }
            });
        }
    }


}
