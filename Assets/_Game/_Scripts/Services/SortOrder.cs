

using System.Collections.Generic;
using System.Linq;

using UnityEngine;


public class SortOrder : MonoBehaviour
{
    /*
    public List<DecorData> _decorSprites;
    public bool _isPlayer;
    private GameObject _previousClosestSprite;


    private void Start()
    {
 
        DecorData[] decors = FindObjectsByType<DecorData>(FindObjectsSortMode.None);

        foreach (var decor in decors)
        {
            _decorSprites.Add(decor);
        }
    }
    private void Update()
    {
        var playerY = transform.position.y;

        var closestBelow = _decorSprites
            .Where(decor => DecorPosition(decor).y < playerY)
            .OrderBy(decor => Vector3.Distance(DecorPosition(decor), transform.position))
            .FirstOrDefault();

        if (_previousClosestSprite != null && _previousClosestSprite != closestBelow)
        {
            var decor = _previousClosestSprite.GetComponent<SpriteRenderer>();
                decor.sortingOrder = 0;
            
            var color = decor.color;
                color.a = 1f;
                decor.color = color;    
            
            
        }

    
        if (closestBelow != null)
        {

            closestBelow.GetComponent<SpriteRenderer>().sortingOrder = 2;
            if (_isPlayer)
            {
                var color = closestBelow.GetComponent<SpriteRenderer>().color;

                color.a = 0.5f;
                closestBelow.GetComponent<SpriteRenderer>().color = color;
                _previousClosestSprite = closestBelow.gameObject; 
            }
        }

    }
    
    private Vector3 DecorPosition(DecorData decor)
    {
        Vector3 decorPosition = decor.transform.position;
        switch (decor.DecorType) 
        {
            case DecorType.tree: return SumVector(decorPosition,-decor.transform.localScale.y*5);
            case DecorType.house: return decorPosition;
            default: return decorPosition;
        }
        Vector3 SumVector(Vector3 vector,float Y)
        {
            return vector+new Vector3(0,Y);
        }
    }*/
}
