using UnityEngine;
using System.Collections;

public class Spiral 
{

    private int m_currentX;
    private int m_currentY;
    private int m_direction = 1;
    private int m_stepLenght = 1;
    private int m_step = 1;
    private bool m_increaseStepLenght = false;
    private bool m_XY = true;
    private bool m_first = true;

    public void Next(out int x, out int y)
    {
        if (m_first) { x = 0; y = 0; m_first = false; return; }

        if (m_XY)
            m_currentX += m_direction;
        else
            m_currentY += m_direction;

        m_step++;

        if (m_step > m_stepLenght)
        {
            m_XY = !m_XY;
            m_step = 1;
            if (m_increaseStepLenght)
            {
                m_stepLenght++;
                m_direction *= -1;
            }
            m_increaseStepLenght = !m_increaseStepLenght;
        }

        x = m_currentX;
        y = m_currentY;
    }

}
