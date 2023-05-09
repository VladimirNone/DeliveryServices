import { ChartData } from "chart.js";
import { FC, useEffect, useState } from "react";
import { Line } from "react-chartjs-2";
import ChartDataLabels from 'chartjs-plugin-datalabels';

import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
  } from 'chart.js';

  ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
  );

const LineChart: FC<{ query:statisticQueryInfo }> = ({ query }) => {
    const [chartsData, setChartsData] = useState<ChartData<"line">[]>([]);

    useEffect(() => {
        const fetchData = async () => {
            const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/statistic/${query.linkToQuery}`, {
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                },
            });
            const resInfo = await resp.json() as statisticQueryDataItem[];
    
            const datasets:ChartData<"line">[] = [];
            
            for( let i = 0; i < (query.nameDatasets?.length ?? 1); i++)
            {
                datasets.push({
                    labels: resInfo.map((value) => value.x),
                    datasets: [
                        {
                            label: query.nameDatasets == null ? query.nameQuery : query.nameDatasets[i] ,
                            data: resInfo.map((value) => value.y[i]),
                            borderColor: 'rgb(255, 99, 132)',
                            backgroundColor: '#7E07A9',
                        },
                    ],
                });
            };

            setChartsData(datasets);
        }
        fetchData();
    }, [query]);

    const options = {
        plugins: {
            datalabels: {
                color: '#000',
                font: {
                    weight: 'bold' as 'bold'
                },
                align: 'left' as 'left'
              }
        },
    };

    return (
        <>
            {chartsData.map((chartData, i) => <Line key={i} plugins={[ChartDataLabels]} options={options} data={chartData}/>) }
        </>    
    );
}

export default LineChart;