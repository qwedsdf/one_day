using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class back : MonoBehaviour
{
    private static float UpValue = 0.1f;
    private Color _color;

    private bool isUp = false;

    private int cont = 0;
    
    private Image _image;

    // Start is called before the first frame update
    void Start()
    {
        _image = GetComponent<Image>();
        _color = new Color(0,0,0,0.4f);
    }

    // Update is called once per frame
    void Update()
    {
        if(sekiyuking._on){
            setColor();
            _image.color = _color;
        }
    }

    private void setColor(){
        if(isUp) {
            if(_color.r < 1){
                _color.r += UpValue;
                return;
            }
            if(_color.g < 1){
                _color.g += UpValue;
                return;
            }
            if(_color.b < 1){
                _color.b += UpValue;
                return;
            }
            isUp = false;
        }
        if(_color.r > 0){
            _color.r -= UpValue;
            return;
        }
        if(_color.g > 0){
            _color.g -= UpValue;
            return;
        }
        if(_color.b > 0){
            _color.b -= UpValue;
            return;
        }
        isUp = true;
    }
}
