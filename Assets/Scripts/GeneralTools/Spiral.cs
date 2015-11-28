using UnityEngine;
using System.Collections;

public class Spiral
{
	private Vector2i m_currentOffset = Vector2i.Zero;
	private int m_direction = 1;
	private int m_stepLenght = 1;
	private int m_step = 1;
	private bool m_increaseStepLenght = false;
	private bool m_XY = true;
	private bool m_first = true;

	public void Reset()
	{
		m_currentOffset = Vector2i.Zero;
		m_direction = 1;
		m_stepLenght = 1;
		m_step = 1;
		m_increaseStepLenght = false;
		m_XY = true;
		m_first = true;
	}

	public Vector2i GetNextOffset()
	{
		if (m_first)
		{
			m_first = false;
			return m_currentOffset;
		}

		if (m_XY)
			m_currentOffset.x += m_direction;
		else
			m_currentOffset.y += m_direction;

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

		return m_currentOffset;
	}

}
