import React, { useEffect, useRef } from 'react';
import mermaid from 'mermaid';

interface MermaidDiagramProps {
  chart: string;
  id?: string;
  className?: string;
}

const MermaidDiagram: React.FC<MermaidDiagramProps> = ({ 
  chart, 
  id = `mermaid-${Math.random().toString(36).substr(2, 9)}`, 
  className = '' 
}) => {
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    // Initialize mermaid
    mermaid.initialize({
      startOnLoad: false,
      theme: 'default',
      securityLevel: 'loose',
      fontFamily: 'Arial, sans-serif'
    });

    if (ref.current) {
      ref.current.innerHTML = chart;
      mermaid.init(undefined, ref.current);
    }
  }, [chart]);

  return <div ref={ref} id={id} className={className} />;
};

export default MermaidDiagram;
