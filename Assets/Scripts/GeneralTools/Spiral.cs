using UnityEngine;
using System.Collections;

public class Spiral 
{

    private int _currentX;
    private int _currentY;
    private int _direction = 1;
    private int _stepLenght = 1;
    private int _step = 1;
    private bool _increaseStepLenght = false;
    private bool _XY = true;
    private bool _first = true;

    public void Next(out int x, out int y)
    {
        if (_first) { x = 0; y = 0; _first = false; return; }

        if (_XY)
            _currentX += _direction;
        else
            _currentY += _direction;

        _step++;

        if (_step > _stepLenght)
        {
            _XY = !_XY;
            _step = 1;
            if (_increaseStepLenght)
            {
                _stepLenght++;
                _direction *= -1;
            }
            _increaseStepLenght = !_increaseStepLenght;
        }

        x = _currentX;
        y = _currentY;
    }

}
