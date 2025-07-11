using UnityEngine;
using System.Collections;

public class DiagnosticSpriteSetter : MonoBehaviour {
    
    public SpriteRenderer boxFrontSprite;
    public SpriteRenderer boxBackSprite;

    public void SetBoxFrontSprite(Sprite sprite)
    {
        boxFrontSprite.sprite = sprite;
    }

    public void SetBoxBackSprite(Sprite sprite)
    {
        boxBackSprite.sprite = sprite;
    }
}