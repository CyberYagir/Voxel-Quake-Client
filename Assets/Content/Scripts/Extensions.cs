using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Content.Scripts.Game.IO.Structures;
using LightServer.Base.PlayersModule;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Content.Scripts
{
    public static class Extensions
    {
        #region BASETYPES

        public static string ToSpaceCapLetters(this string str)
        {
            return Regex.Replace(str, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
        }

        #endregion

        #region VECTORS

        public static float ToAngle(this Vector2 vec)
        {
            return Mathf.Atan2(vec.x, vec.y) * Mathf.Rad2Deg;
        }

        public static float ToAngle(this Vector3 vec)
        {
            return Mathf.Atan2(vec.x, vec.y) * Mathf.Rad2Deg;
        }

        public static float ToDistance(this Vector3 startPoint, Vector3 lastPoint)
        {
            return Vector3.Distance(startPoint, lastPoint);
        }


        public static Vector3 ToGlobalPos(this Transform obj, Vector3 point)
        {
            return obj.TransformPoint(point);
        }

        public static Vector3 ToLocalPos(this Transform obj, Vector3 point)
        {
            return obj.InverseTransformPoint(point);
        }

        public static Vector3 ToLocalVelocity(this Rigidbody rb)
        {
            return rb.transform.InverseTransformDirection(rb.velocity);
        }

        public static Vector3 ToXZ(this Vector2 dir) => new Vector3(dir.x, 0, dir.y); 
        
        public static Vector3 RemoveY(this Vector3 dir) => new Vector3(dir.x, 0, dir.z); 

        #region Vector2

        public static Vector2 MinusVector2(this ref Vector2 vector2, Vector2 value)
        {
            vector2.x -= value.x;
            vector2.y -= value.y;
            return vector2;
        }

        public static Vector2 PlusVector2(this ref Vector2 vector2, Vector2 value)
        {
            vector2.x += value.x;
            vector2.y += value.y;
            return vector2;
        }

        public static Vector2 DevideVector2(this ref Vector2 vector2, float value)
        {
            vector2.x /= value;
            vector2.y /= value;
            return vector2;
        }

        public static Vector2 MultiplyVector2(this ref Vector2 vector2, float value)
        {
            vector2.x *= value;
            vector2.y *= value;
            
            return vector2;
        }

        #endregion

        #region Vector3

        public static Vector3 MinusVector3(this ref Vector3 vector3, Vector3 value)
        {
            vector3.x -= value.x;
            vector3.y -= value.y;
            vector3.z -= value.z;
            return vector3;
        }

        public static Vector3 PlusVector3(this ref Vector3 vector3, Vector3 value)
        {
            vector3.x += value.x;
            vector3.y += value.y;
            vector3.z += value.z;

            return vector3;
        }

        public static Vector3 DevideVector3(this ref Vector3 vector3, float value)
        {
            vector3.x /= value;
            vector3.y /= value;
            vector3.z /= value;
            return vector3;
        }
        
        public static Vector3 DevideVector3(this ref Vector3 vector3, Vector3 value)
        {
            vector3.x /= value.x;
            vector3.y /= value.y;
            vector3.z /= value.z;
            return vector3;
        }
        
        public static Vector3 MultiplyVector3(this Vector3 vector3, Vector3 value)
        {
            return new Vector3(vector3.x * value.x, vector3.y * value.y, vector3.z * value.z);
        }

        public static Vector3 MultiplyVector3(this ref Vector3 vector3, float value)
        {
            vector3.x *= value;
            vector3.y *= value;
            vector3.z *= value;

            return vector3;
        }

        #endregion

        #endregion
        
        #region ARRAYS OPERATIONS

        public static void Add<T>(this List<T> _array, params T[] items)
        {
            if (items.Length != 0)
                _array.AddRange(items);
        }

        public static int GetRandomIndex<T>(this List<T> _array)
        {
            return Random.Range(0, _array.Count);
        }
        
        public static int GetRandomIndex<T>(this List<T> _array, System.Random random)
        {
            return random.Next(0, _array.Count);
        }

        public static int GetRandomIndex<T>(this T[] _array)
        {
            return Random.Range(0, _array.Length);
        }

        public static T GetRandomItem<T>(this List<T> _array)
        {
            return _array[Random.Range(0, _array.Count)];
        }
        
        public static T GetRandomItem<T>(this List<T> _array, System.Random random)
        {
            return _array[_array.GetRandomIndex(random)];
        }

        public static T GetRandomItem<T>(this T[] _array)
        {
            return _array[Random.Range(0, _array.Length)];
        }

        public static T GetItemNOrZero<T>(this List<T> _array, int itemIndex)
        {
            return _array[itemIndex >= _array.Count ? 0 : itemIndex];
        }

        public static T GetItemNOrZero<T>(this T[] _array, int itemIndex)
        {
            return _array[itemIndex >= _array.Length ? 0 : itemIndex];
        }

        public static T GetItemNOrLast<T>(this List<T> _array, int itemIndex)
        {
            return _array[itemIndex >= _array.Count ? _array.Count - 1 : itemIndex];
        }

        public static T GetItemNOrLast<T>(this T[] _array, int itemIndex)
        {
            return _array[itemIndex >= _array.Length ? _array.Length - 1 : itemIndex];
        }

        public static T GetItemNOrRandom<T>(this List<T> _array, int itemIndex)
        {
            return _array[itemIndex >= _array.Count ? Random.Range(0, _array.Count) : itemIndex];
        }

        public static T GetItemNOrRandom<T>(this T[] _array, int itemIndex)
        {
            return _array[itemIndex >= _array.Length ? Random.Range(0, _array.Length) : itemIndex];
        }

        public static T GetRandomItemExpectNIndex<T>(this T[] _array, int expectedItemIndex)
        {
            int random = Random.Range(0, _array.Length);

            if (random == expectedItemIndex)
                return random == _array.Length - 1 ? _array[random - 1] : _array[random + 1];

            return _array[random];
        }

        public static T GetRandomItemExpectNIndex<T>(this List<T> _array, int expectedItemIndex)
        {
            int random = Random.Range(0, _array.Count);

            if (random == expectedItemIndex)
                return random == _array.Count - 1 ? _array[random - 1] : _array[random + 1];

            return _array[random];
        }

        public static T GetRandomItemExpectNItem<T>(this T[] _array, T expectedItem)
        {
            List<T> _temp = new List<T>(_array.Length);

            for (int i = 0; i < _array.Length; i++)
                if (!EqualityComparer<T>.Default.Equals(_array[i], expectedItem))
                    _temp.Add(_array[i]);

            if (_temp.Count == 0)
            {
                Debug.LogWarning("GetRandomItemExpectNItem<T> -> No Item Available!");
                return _array[Random.Range(0, _array.Length)];
            }

            return _temp[Random.Range(0, _temp.Count)];
        }

        public static T GetRandomItemExpectNItem<T>(this List<T> _array, T expectedItem)
        {
            List<T> _temp = new List<T>(_array.Count);

            for (int i = 0; i < _array.Count; i++)
                if (!EqualityComparer<T>.Default.Equals(_array[i], expectedItem))
                    _temp.Add(_array[i]);

            if (_temp.Count == 0)
            {
                Debug.LogWarning("GetRandomItemExpectNItem<T> -> No Item Available!");
                return _array[Random.Range(0, _array.Count)];
            }

            return _temp[Random.Range(0, _temp.Count)];
        }

        public static int GetRandomIndexExpectN<T>(this T[] _array, int expectedItemIndex)
        {
            int random = Random.Range(0, _array.Length);

            if (random != expectedItemIndex)
                return random;

            if (random == _array.Length - 1)
                return random - 1;

            return random + 1;
        }

        public static int GetRandomIndexExpectN<T>(this List<T> _array, int expectedItemIndex)
        {
            int random = Random.Range(0, _array.Count);

            if (random != expectedItemIndex)
                return random;

            if (random == _array.Count - 1)
                return random - 1;

            return random + 1;
        }

        public static void Shuffle<T>(this T[] deck)
        {
            for (int i = 0; i < deck.Length; i++)
            {
                T temp = deck[i];
                int randomIndex = deck.GetRandomIndex();
                deck[i] = deck[randomIndex];
                deck[randomIndex] = temp;
            }
        }

        public static void Shuffle<T>(this List<T> deck)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                T temp = deck[i];
                int randomIndex = deck.GetRandomIndex();
                deck[i] = deck[randomIndex];
                deck[randomIndex] = temp;
            }
        }
        
        public static void Shuffle<T>(this List<T> deck, System.Random random)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                T temp = deck[i];
                int randomIndex = deck.GetRandomIndex(random);
                deck[i] = deck[randomIndex];
                deck[randomIndex] = temp;
            }
        }

        public static T[] GetRandomItemsWithoutRepeats<T>(this T[] _array, int numRequired)
        {
            T[] result = new T[numRequired];

            int numToChoose = numRequired;

            for (int numLeft = _array.Length; numLeft > 0; numLeft--)
            {
                float prob = numToChoose / (float) numLeft;

                if (Random.value <= prob)
                {
                    numToChoose--;
                    result[numToChoose] = _array[numLeft - 1];

                    if (numToChoose == 0)
                        break;
                }
            }

            return result;
        }

        public static T[] GetRandomItemsWithoutRepeats<T>(this List<T> _array, int numRequired)
        {
            T[] result = new T[numRequired];

            int numToChoose = numRequired;

            for (int numLeft = _array.Count; numLeft > 0; numLeft--)
            {
                float prob = numToChoose / (float) numLeft;

                if (Random.value <= prob)
                {
                    numToChoose--;
                    result[numToChoose] = _array[numLeft - 1];

                    if (numToChoose == 0)
                        break;
                }
            }

            return result;
        }

        public static int ChooseRandomIndexFromWeights(this float[] probs)
        {
            float total = 0;

            for (int i = 0; i < probs.Length; i++)
                total += probs[i];

            float randomPoint = Random.value * total;

            for (int i = 0; i < probs.Length; i++)
            {
                if (randomPoint < probs[i] && randomPoint >= 0)
                    return i;

                randomPoint -= probs[i];
            }

            return probs.Length - 1;
        }

        public static int ChooseRandomIndexFromWeights(this List<float> probs)
        {
            float total = 0;

            for (int i = 0; i < probs.Count; i++)
                total += probs[i];

            float randomPoint = Random.value * total;

            for (int i = 0; i < probs.Count; i++)
            {
                if (randomPoint < probs[i] && randomPoint >= 0)
                    return i;

                randomPoint -= probs[i];
            }

            return probs.Count - 1;
        }
        
        public static int ChooseRandomIndexFromWeights(this List<float> probs, System.Random rnd)
        {
            float total = 0;

            for (int i = 0; i < probs.Count; i++)
                total += probs[i];

            float randomPoint = (float)rnd.NextDouble() * total;

            for (int i = 0; i < probs.Count; i++)
            {
                if (randomPoint < probs[i] && randomPoint >= 0)
                    return i;

                randomPoint -= probs[i];
            }

            return probs.Count - 1;
        }

        public static void RecalculateWeights(this List<float> weights) // общая сумма весов = 1
        {
            float _summ = 0;

            for (int i = 0; i < weights.Count; i++)
                _summ += weights[i];

            for (int i = 0; i < weights.Count; i++)
                weights[i] /= _summ;
        }

        public static void RecalculateWeights(this float[] weights) // общая сумма весов = 1
        {
            float _summ = 0;

            for (int i = 0; i < weights.Length; i++)
                _summ += weights[i];

            for (int i = 0; i < weights.Length; i++)
                weights[i] /= _summ;
        }

        #endregion

        #region COLORS

        public static void SetAlpha(this TMP_Text t, float a)
        {
            Color c = t.color;
            c.a = a;
            t.color = c;
        }


        public static void SetAlpha(this Image i, float a)
        {
            Color c = i.color;
            c.a = a;
            i.color = c;
        }

        public static float GetAlpha(this Image i)
        {
            Color c = i.color;
            return c.a;
        }

        public static void SetAlpha(this SpriteRenderer sr, float a)
        {
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }

        #endregion

        #region TRANSFORM OPERATIONS

        public static void SetXPosition(this Transform t, float x)
        {
            Vector3 p = t.position;
            p.x = x;
            t.position = p;
        }

        public static void SetYPosition(this Transform t, float y)
        {
            Vector3 p = t.position;
            p.y = y;
            t.position = p;
        }

        public static void SetZPosition(this Transform t, float z)
        {
            Vector3 p = t.position;
            p.z = z;
            t.position = p;
        }

        public static void SetXLocalPosition(this Transform t, float x)
        {
            Vector3 p = t.localPosition;
            p.x = x;
            t.localPosition = p;
        }

        public static void SetYLocalPosition(this Transform t, float y)
        {
            Vector3 p = t.localPosition;
            p.y = y;
            t.localPosition = p;
        }

        public static void SetZLocalPosition(this Transform t, float z)
        {
            Vector3 p = t.localPosition;
            p.z = z;
            t.localPosition = p;
        }

        public static void SetXLocalEulerAngles(this Transform t, float x)
        {
            Vector3 p = t.localEulerAngles;
            p.x = x;
            t.localEulerAngles = p;
        }

        public static void SetYLocalEulerAngles(this Transform t, float y)
        {
            Vector3 p = t.localEulerAngles;
            p.y = y;
            t.localEulerAngles = p;
        }

        public static void SetZLocalEulerAngles(this Transform t, float z)
        {
            Vector3 p = t.localEulerAngles;
            p.z = z;
            t.localEulerAngles = p;
        }

        public static void SetXEulerAngles(this Transform t, float x)
        {
            Vector3 p = t.eulerAngles;
            p.x = x;
            t.eulerAngles = p;
        }

        public static void SetYEulerAngles(this Transform t, float y)
        {
            Vector3 p = t.eulerAngles;
            p.y = y;
            t.eulerAngles = p;
        }

        public static void SetZEulerAngles(this Transform t, float z)
        {
            Vector3 p = t.eulerAngles;
            p.z = z;
            t.eulerAngles = p;
        }

        public static void SetXLocalScale(this Transform t, float x)
        {
            Vector3 p = t.localScale;
            p.x = x;
            t.localScale = p;
        }

        public static void SetYLocalScale(this Transform t, float y)
        {
            Vector3 p = t.localScale;
            p.y = y;
            t.localScale = p;
        }

        public static void SetZLocalScale(this Transform t, float z)
        {
            Vector3 p = t.localScale;
            p.z = z;
            t.localScale = p;
        }
        
        public static float ToDot(this Transform attacker, Vector3 target)
        {
            return Vector3.Dot(attacker.forward, target - attacker.position);
        }

        public static float ToDistance(this Transform firstPoint, Transform secondPoint)
        {
            return firstPoint.position.ToDistance(secondPoint.position);
        }

        public static void ChangeLayerWithChilds(this Transform transform, int layer)
        {
            ChangeLayer(transform, layer);
        }
        public static void ChangeLayerWithChilds(this GameObject transform, int layer)
        {
            ChangeLayer(transform.transform, layer);
        }
        private static void ChangeLayer(Transform obj, int layer)
        {
            obj.gameObject.layer = layer;
            foreach (Transform chld in obj)
            {
                ChangeLayer(chld, layer);
            }
        }
        
        #endregion

        #region TIME SCALE OPERATIONS
        
        public static void SetTimeScale(float changeValue)
        {
            Time.timeScale = 1f * changeValue;
            Time.fixedDeltaTime = 0.02f * changeValue;
        }

        public static void SetNormalTimeScale()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
        
        #endregion
        
        #region CAMERA

        public static bool ObjectIsVisible(Camera camera, Vector3 target, out Vector3 screenPoint, float border = 0)
        {
            screenPoint = camera.WorldToViewportPoint(target);
            return screenPoint.z > 0 &&
                   screenPoint.x > 0 + border &&
                   screenPoint.x < 1 - border &&
                   screenPoint.y > 0 + border &&
                   screenPoint.y < 1 - border;
        }

        public static RaycastHit MouseRaycast(this Camera camera, out bool isHit, Vector2 mousePos, float maxDist = Mathf.Infinity, int layerMask = ~0, QueryTriggerInteraction triggers = QueryTriggerInteraction.UseGlobal)
        {
            Ray ray = camera.ScreenPointToRay(mousePos);
            RaycastHit hit;
            isHit = Physics.Raycast(ray, out hit, maxDist, layerMask, triggers);
            if (hit.collider == null)
            {
                isHit = false;
            }
            
            return hit;
        }


        #endregion

        #region ENUM OPERATIONS
        
        public static T GetRandomEnum<T>()
        {
            Array A = Enum.GetValues(typeof(T));
            return (T)A.GetValue(Random.Range(0, A.Length));
        }
        
        public static T GetRandomEnum<T>(System.Random rnd)
        {
            Array A = Enum.GetValues(typeof(T));
            return (T)A.GetValue(rnd.Next(0, A.Length));
        }
		
        public static T GetRandomEnumExceptFirst<T>()
        {
            Array A = Enum.GetValues(typeof(T));
            return (T)A.GetValue(Random.Range(1, A.Length));
        }
        
        #endregion

        #region LOGIC

        public static T With<T>(this T self, Action<T> set)
        {
            set.Invoke(self);
            return self;
        }

        public static T With<T>(this T self, Action<T> apply, Func<bool> when)
        {
            if (when())
                apply?.Invoke(self);

            return self;
        }

        public static T With<T>(this T self, Action<T> apply, bool when)
        {
            if (when)
                apply?.Invoke(self);

            return self;
        }
        
        public static T Get<T>(this GameObject self)
        {
            return self.GetComponent<T>();
        }
        public static T Get<T>(this Component self)
        {
            return self.GetComponent<T>();
        }
        
        public static void Destroy(this Object self)
        {
            Object.Destroy(self);
        }
        
        #endregion

        #region Addressables

#if ADDRESSABLES
        public static T Spawn<T>(this Object prefab)
        {
            return prefab.Spawn().GetComponent<T>();
        }
        public static GameObject Spawn(this Object prefab)
        {
            return Object.Instantiate(prefab) as GameObject;
        }
#endif
        #endregion
        
        #region AI

        public static bool IsArrived(this NavMeshAgent agent)
        {
            return agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending;
        }

        #endregion

        #region UI

        public static void ChangeSizeDeltaX(this RectTransform rectTransform, float value)
        {
            rectTransform.sizeDelta = new Vector2(value, rectTransform.sizeDelta.y);
        }

        public static void ChangeSizeDeltaY(this RectTransform rectTransform, float value)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, value);
        }
        #endregion

        public static string RemoveSpecialCharacters(this string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_. ]+", "", RegexOptions.Compiled);
        }

        public static string RemovePartItem(this string str)
        {
            if (str != null)
            {
                return str.Replace("item_", String.Empty);
            }
            else
            {
                return String.Empty;
            }
        }

        public static Quaternion ToRotation(this Vector3 normal, Transform transform)
        {
            return Quaternion.FromToRotation(transform.up, normal) * transform.rotation;
        }
        
        public static Guid GenerateSeededGuid(System.Random rnd)
        {
            var guid = new byte[16];
            rnd.NextBytes(guid);
            return new Guid(guid);
        }

        public static Vector3 GetRandomPoint(this Bounds bounds)
        {
            return new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), Random.Range(bounds.min.z, bounds.max.z));
        }


        // public static bool IsInRange(this Range range, float value)
        // {
        //     return value >= range.min && value <= range.max;
        // }
        
        public static List<string> LinesToList(this TextAsset asset)
        {
            var list = asset.text.Split("\n").ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list[i].RemoveSpecialCharacters();
            }
            
            return list;
        }
        
        
        
        public static float NextFloat(this System.Random rnd, float minimum, float maximum)
        { 
            return (float)rnd.NextDouble() * (maximum - minimum) + minimum;
        }
        
        public static Texture2D ToTexture2D(this RenderTexture renderTexture)
        {
            // Сохраняем текущую активную RenderTexture
            RenderTexture activeRT = RenderTexture.active;

            // Делаем переданную RenderTexture активной
            RenderTexture.active = renderTexture;

            // Создаем Texture2D с учетом формата RenderTexture
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();

            // Восстанавливаем предыдущую активную RenderTexture
            RenderTexture.active = activeRT;

            // Применяем гамма-коррекцию, если проект использует линейное цветовое пространство
            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                Color[] pixels = texture.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = pixels[i].gamma; // Применяем гамма-коррекцию
                }
                texture.SetPixels(pixels);
                texture.Apply();
            }

            return texture;
        }


#if UNITY_EDITOR
        [MenuItem("Tools/Open Persistant Path")]
        public static void OpenPersistantPath()
        {
            Application.OpenURL(Application.persistentDataPath);
        }
#endif
        
        public static Vector3IntData Convert(this Vector3Int vector3Int)
        {
            return new Vector3IntData(vector3Int.x, vector3Int.y, vector3Int.z);
        }

        public static Vector3Int Convert(this Vector3IntData vector3Int)
        {
            return new Vector3Int(vector3Int.x, vector3Int.y, vector3Int.z);
        }
        
        public static IEnumerable<ValueDropdownItem<int>> GetNamedLayers()
        {
            for (int i = 0; i < 32; i++)
            {
                string name = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(name))
                {
                    yield return new ValueDropdownItem<int>(name, i);
                }
            }
        }

        public static Vector3 Convert(this NetVector3 vector3)
        {
            return new Vector3(vector3.x, vector3.y, vector3.z);
        }
        
        public static NetVector3 Convert(this Vector3 vector3)
        {
            return new NetVector3(vector3.x, vector3.y, vector3.z);
        }
        
        public static Vector3Int Convert(this NetVector3Int vector3)
        {
            return new Vector3Int(vector3.x, vector3.y, vector3.z);
        }
    }
}
