using QuizCanners.Utils;
using System;
using System.Collections;
using System.Text;
using UnityEngine;


namespace QuizCanners.SpecialEffects
{

    public class TextAnimation
    {

        public IEnumerator AnimateAsync(string fullText, Action<string> feed, float characterFadeInSpeed = 10)
        {
            var segments = fullText.Split(' ');

            int i = 0;

            float fadeInTimer = 1f;

            float fadeInSpeed = characterFadeInSpeed * 0.1f;

            var visibleBuilder = new StringBuilder(fullText.Length + 50);
            var resultBuilder = new StringBuilder(fullText.Length + 50);

            while (i < segments.Length)
            {

                fadeInTimer -= Time.deltaTime * fadeInSpeed;

                if (fadeInTimer < 0)
                {

                    string fadingSegment = segments[i];

                    if (fadingSegment.Length > 0)
                    {
                        var lastChar = fadingSegment[fadingSegment.Length - 1];

                        float waitFor = 0;

                        switch (lastChar)
                        {
                            case '.': waitFor = 0.4f; break;
                            case '!': waitFor = 0.6f; break;
                            case ',': waitFor = 0.2f; break;
                            case ':': waitFor = 0.15f; break;
                        }

                        while (waitFor > 0)
                        {
                            waitFor -= Time.deltaTime;
                            yield return null;
                        }
                    }


                    visibleBuilder.Append(segments[i]).Append(' ');
                    i++;
                    //newSegment = true;
                    fadeInTimer += 1f;

                    if (i < segments.Length)
                    {

                        fadingSegment = segments[i];

                        var dontFade = true;

                        while (dontFade && i < segments.Length)
                        {
                            dontFade = fadingSegment.Length == 0 || fadingSegment[0] == '<';

                            if (dontFade)
                            {
                                visibleBuilder.Append(fadingSegment).Append(' ');
                                i++;
                            }

                            if (i < segments.Length)
                            {
                                fadingSegment = segments[i];
                            }
                        }

                        fadeInSpeed = characterFadeInSpeed / (1f + fadingSegment.Length);
                    }

                }

                if (i >= segments.Length)
                {
                    feed.Invoke(fullText);
                    yield break;
                }

                string visible = visibleBuilder.ToString();

                string fading = segments[i];

                resultBuilder.Clear();

                resultBuilder.Append(visible)
                    .Append(QcSharp.HtmlTagAlpha(1f - fadeInTimer))
                    .Append(fading)
                    .Append(QcSharp.HtmlTagAlpha(0f))
                    .Append(fullText.Substring(visible.Length + fading.Length));

                feed.Invoke(resultBuilder.ToString()); //visible + HtmlTagAlpha(fadeInTimer) + fading + HtmlTagAlpha(1f) + fullText.Substring(visible.Length + fading.Length));

                yield return null;
            }
        }
    }
}